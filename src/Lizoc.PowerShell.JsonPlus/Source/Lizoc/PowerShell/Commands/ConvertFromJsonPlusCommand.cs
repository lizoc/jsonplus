using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Lizoc.JsonPlus;
using Lizoc.PowerShell.JsonPlus;

namespace Lizoc.PowerShell.Commands
{
    [Cmdlet(
        VerbsData.ConvertFrom, "JsonPlus", 
        HelpUri = "http://docs.lizoc.com/ps/convertfromjsonplus",
        RemotingCapability = RemotingCapability.None
    ), OutputType(typeof(PSObject))]
    public class ConvertFromJsonPlusCommand : Cmdlet
    {
        private const string JsonPlusNewLine = "\n";

        private List<string> _inputObjectBuffer = new List<string>();

        private string[] _allowInclude = new string[] { "Any" };
        private bool _allowIncludeFile = true;
        private bool _allowIncludeUrl = true;
        //private bool _allowIncludeAssembly = true;

        private bool _allowResolveEnv = true;

        private int _recurseLevel = -1; // this will become 0 when called by the root node 
        private int _maxRecurseLevel = -1;

        [AllowEmptyString, Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public string InputObject { get; set; }

        [Parameter()]
        [ValidateSet(new string[] { "Any", "File", "Url", "Environment" })]
        // [ValidateSet(new string[] { "Any", "File", "Url", "Assembly", "Environment" })]
        public string[] AllowInclude
        {
            get
            {
                return _allowInclude;
            }
            set
            {
                _allowInclude = value;

                if (_allowInclude.Contains("Any"))
                {
                    //_allowIncludeAssembly = true;
                    _allowIncludeUrl = true;
                    _allowIncludeFile = true;
                    _allowResolveEnv = true;
                    return;
                }

                if (_allowInclude.Contains("File"))
                    _allowIncludeFile = true;

                if (_allowInclude.Contains("Url"))
                    _allowIncludeUrl = true;

                //if (_allowInclude.Contains("Assembly"))
                //    _allowIncludeAssembly = true;

                if (_allowInclude.Contains("Environment"))
                    _allowResolveEnv = true;
            }
        }

        [Parameter()]
        [ValidateRange(0, int.MaxValue)]
        public int Depth
        {
            get { return _maxRecurseLevel; }
            set { _maxRecurseLevel = value; }
        }

        protected override void BeginProcessing()
        {
        }

        protected override void ProcessRecord()
        {
            _inputObjectBuffer.Add(this.InputObject);
        }

        protected override void EndProcessing()
        {
            // ignore empty entry
            if (_inputObjectBuffer.Count == 0)
                return;

            // It is not actually easy to write syntaxically wrong Json+.
            // So instead of trying to figure out if it is a list of source or 
            // just newlines, let's just join the list to a whole big string.

            JsonPlusRoot root;
            try
            {
                async Task<string> includeCallback(IncludeSource resType, string path)
                {
                    if (path == null)
                        return "{}";

                    // heuristically determine res type
                    if (resType == IncludeSource.Unspecified)
                    {
                        if (path.StartsWith("http://") || path.StartsWith("https://"))
                            resType = IncludeSource.Url;
                        else if (path.StartsWith("file://"))
                            resType = IncludeSource.Url; // defined by spec
                        else
                            resType = IncludeSource.File;
                    }

                    switch (resType)
                    {
                        case IncludeSource.File:
                            if (!_allowIncludeFile)
                                return "{}";

                            return File.ReadAllText(path);

                        case IncludeSource.Url:
                            if (!_allowIncludeUrl)
                                return "{}";

                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(path);
                            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                            using(Stream stream = response.GetResponseStream())
                            using(StreamReader reader = new StreamReader(stream))
                            {
                                return await reader.ReadToEndAsync();
                            }

                        //case IncludeSource.Resource:
                        //    return "{}";

                        default:
                            return "{}";
                    }
                }

                string source;
                if (_inputObjectBuffer.Count == 1)
                    source = _inputObjectBuffer[0];
                else
                    source = string.Format(JsonPlusNewLine, _inputObjectBuffer.ToArray());

                root = JsonPlusParser.Parse(source, includeCallback, _allowResolveEnv);

                // Handle empty situation
                if (root.IsEmpty)
                    return;

                object obj = TransverseJPlusRoot(root, out ErrorRecord populateError);
                if (populateError != null)
                    base.ThrowTerminatingError(populateError);

                base.WriteObject(obj);
            }
            catch (Exception e)
            {
                ErrorRecord errorRecord = new ErrorRecord(e, "JsonPlusConversionFailure", ErrorCategory.ParserError, null);
                base.ThrowTerminatingError(errorRecord);
            }
        }

        private object TransverseJPlusRoot(JsonPlusRoot root, out ErrorRecord error)
        {
            // internal exception catching
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            error = null;

            if (root.IsEmpty)
                return null;

            if (root.Value.Type != JsonPlusType.Object)
            {
                error = new ErrorRecord(new JsonPlusException(RS.JsonPlusUnsupportedRootNodeType), "UnsupportedRootNodeType", ErrorCategory.ParserError, null);
                return null;
            }

            return PopulateJPlusObject(root.Value.GetObject(), out error);
        }

        private PSObject PopulateJPlusObject(JsonPlusObject jv, out ErrorRecord error)
        {
            if (jv == null)
                throw new ArgumentNullException(nameof(jv));

            error = null;

            // do not recurse over the max
            if (_maxRecurseLevel > -1)
            {
                _recurseLevel += 1;
                if (_recurseLevel > _maxRecurseLevel)
                    return null;
            }

            if (jv.Type != JsonPlusType.Object)
                throw new ArgumentException("Internal error: PopulateJPlusObject encountered a value that is not an object.");

            PSObject psObject = new PSObject();

            foreach (string key in jv.Keys)
            {
                JsonPlusObjectMember child = jv[key];

                if (child.Type == JsonPlusType.Empty)
                {
                    psObject.Properties.Add(new PSNoteProperty(key, null));
                }
                else if (child.Type == JsonPlusType.Literal)
                {
                    psObject.Properties.Add(new PSNoteProperty(key, PopulateJPlusLeaf(child.Value, out error)));
                }
                else if (child.Type == JsonPlusType.Array)
                {
                    psObject.Properties.Add(new PSNoteProperty(key, PopulateJPlusArray(child.GetArray(), out error)));
                    _recurseLevel -= 1;
                }
                else if (child.Type == JsonPlusType.Object)
                {
                    psObject.Properties.Add(new PSNoteProperty(key, PopulateJPlusObject(child.GetObject(), out error)));
                    _recurseLevel -= 1;
                }
                else
                {
                    error = new ErrorRecord(new JsonPlusException(RS.JsonPlusAmbiguousType), "UnsupportedJsonPlusDataType", ErrorCategory.ParserError, null);
                }

                // terminate immediately if there's an error
                if (error != null)
                {
                    error.ErrorDetails = new ErrorDetails(string.Format(RS.JsonPlusErrorAtPath, child.Path.Value));
                    return null;
                }
            }

            return psObject;
        }

        private object PopulateJPlusLeaf(JsonPlusValue jv, out ErrorRecord error)
        {
            if (jv == null)
                throw new ArgumentNullException(nameof(jv));

            error = null;

            if (jv.Type != JsonPlusType.Literal)
                throw new ArgumentException("Internal error: Non-leaf object has entered `PopulateJPlusLeaf`.");

            // node could could contain substitution, or unit based value like 
            // timespan or data size
            if (jv.Count > 1)
            {
                // if contains substitution, always cast to string
                bool containsSubstitution = false;
                foreach (IJsonPlusNode node in jv)
                {
                    if (node is JsonPlusSubstitution)
                    {
                        containsSubstitution = true;
                        break;
                    }
                }

                if (!containsSubstitution)
                {
                    try
                    {
                        return jv.GetByteSize();
                    }
                    catch (FormatException)
                    {
                        // do nothing
                    }
                    catch (Exception e)
                    {
                        error = new ErrorRecord(e, "BadJsonPlusLiteralValue", ErrorCategory.ParserError, null);
                        return null;
                    }

                    try
                    {
                        return jv.GetTimeSpan();
                    }
                    catch (FormatException)
                    {
                        // do nothing
                    }
                    catch (Exception e)
                    {
                        error = new ErrorRecord(e, "BadJsonPlusLiteralValue", ErrorCategory.ParserError, null);
                        return null;
                    }
                }

                try
                {
                    return jv.GetString();
                }
                catch (Exception e)
                {
                    error = new ErrorRecord(e, "BadJsonPlusLiteralValue", ErrorCategory.ParserError, null);
                    return null;
                }
            }

            // node only contains 1 item, so it could only contain a keyword or string or number.
            if (jv[0] is NullValue)
            {
                return null;
            }
            else if (jv[0] is BooleanValue)
            {
                try
                {
                    return jv.GetBoolean();
                }
                catch (Exception e)
                {
                    error = new ErrorRecord(e, "BadJsonPlusLiteralValue", ErrorCategory.ParserError, null);
                    return null;
                }
            }
            else if (jv[0] is IntegerValue || jv[0] is HexadecimalValue || jv[0] is OctetValue)
            {
                // try to cast hex or octet to byte first
                if (jv[0] is HexadecimalValue || jv[0] is OctetValue)
                {
                    try
                    {
                        return jv.GetByte();
                    }
                    catch (JsonPlusException e)
                    {
                        if (e.InnerException is OverflowException)
                        {
                            // do nothing.
                        }
                        else
                        {
                            error = new ErrorRecord(e, "BadJsonPlusLiteralValue", ErrorCategory.ParserError, null);
                            return null;
                        }
                    }
                    catch (Exception e)
                    {
                        error = new ErrorRecord(e, "BadJsonPlusLiteralValue", ErrorCategory.ParserError, null);
                        return null;
                    }
                }

                try
                {
                    return jv.GetInt32();
                }
                catch (JsonPlusException e)
                {
                    if (e.InnerException is OverflowException)
                    {
                        try
                        {
                            return jv.GetInt64();
                        }
                        catch (Exception e2)
                        {
                            error = new ErrorRecord(e2, "BadJsonPlusLiteralValue", ErrorCategory.ParserError, null);
                            return null;
                        }
                    }
                    else
                    {
                        error = new ErrorRecord(e, "BadJsonPlusLiteralValue", ErrorCategory.ParserError, null);
                        return null;
                    }
                }
                catch (Exception e)
                {
                    error = new ErrorRecord(e, "BadJsonPlusLiteralValue", ErrorCategory.ParserError, null);
                    return null;
                }
            }
            else if (jv[0] is DecimalValue)
            {
                try
                {
                    return jv.GetDouble();
                }
                catch (JsonPlusException e)
                {
                    if (e.InnerException is OverflowException)
                    {
                        try
                        {
                            return jv.GetDecimal();
                        }
                        catch (Exception e2)
                        {
                            error = new ErrorRecord(e2, "BadJsonPlusLiteralValue", ErrorCategory.ParserError, null);
                            return null;
                        }
                    }
                }
                catch (Exception e)
                {
                    error = new ErrorRecord(e, "BadJsonPlusLiteralValue", ErrorCategory.ParserError, null);
                    return null;
                }
            }
            else if (jv[0] is UnquotedStringValue || jv[0] is QuotedStringValue || 
                jv[0] is TripleQuotedStringValue || jv[0] is WhitespaceValue)
            {
                try
                {
                    return jv.GetString();
                }
                catch (Exception e)
                {
                    error = new ErrorRecord(e, "BadJsonPlusLiteralValue", ErrorCategory.ParserError, null);
                    return null;
                }
            }

            error = new ErrorRecord(new JsonPlusException(string.Format(RS.JsonPlusUnsupportedType)), "BadJsonPlusLiteralValue", ErrorCategory.ParserError, null);
            return null;
        }

        private object[] PopulateJPlusArray(List<IJsonPlusNode> jv, out ErrorRecord error)
        {
            if (jv == null)
                throw new ArgumentNullException(nameof(jv));

            error = null;

            // do not recurse over the max
            if (_maxRecurseLevel > -1)
            {
                _recurseLevel += 1;
                if (_recurseLevel > _maxRecurseLevel)
                    return null;
            }

            List<object> results = new List<object>();

            for (int i = 0; i < jv.Count; i++)
            {
                IJsonPlusNode current = jv[i];

                if (current.Type == JsonPlusType.Empty)
                {
                    results.Add(null);
                }
                else if (current.Type == JsonPlusType.Literal)
                {
                    results.Add(PopulateJPlusLeaf(current.GetValue(), out error));
                }
                else if (current.Type == JsonPlusType.Object)
                {
                    results.Add(PopulateJPlusObject(current.GetObject(), out error));
                    _recurseLevel -= 1;
                }
                else if (current.Type == JsonPlusType.Array)
                {
                    results.Add(PopulateJPlusArray(current.GetArray(), out error));
                    _recurseLevel -= 1;
                }
                else
                {
                    error = new ErrorRecord(new JsonPlusException(string.Format(RS.JsonPlusAmbiguousTypeInArray, i)), "UnsupportedJsonPlusDataType", ErrorCategory.ParserError, null);
                }

                // terminate immediately if there's an error
                if (error != null)
                    return null;
            }

            return results.ToArray();
        }
    }
}
