// -----------------------------------------------------------------------
// <copyright file="ConvertFromJsonPlusCommand.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Threading.Tasks;
using System.Net;
using Lizoc.JsonPlus;
using Lizoc.PowerShell.JsonPlus;
using System.Collections.ObjectModel;
using System.Text;

namespace Lizoc.PowerShell.Commands
{
    /// <summary>
    /// Converts a Json+ formatted string to a custom object.
    /// </summary>
    [Cmdlet(
        VerbsData.ConvertFrom, "JsonPlus", 
        HelpUri = "http://docs.lizoc.com/ps/convertfromjsonplus",
        RemotingCapability = RemotingCapability.None
    ), OutputType(typeof(PSObject))]
    public class ConvertFromJsonPlusCommand : Cmdlet
    {
        private const string JsonPlusNewLine = "\n";

        private List<string> _inputObjectBuffer = new List<string>();

        private bool _allowResolveEnv = true;

        private int _recurseLevel = -1; // this will become 0 when called by the root node 
        private int _maxRecurseLevel = -1;

        /// <summary>
        /// Specifies the Json+ strings to convert to Json+ objects. Enter a variable that contains the string, or type a command or expression that gets the string.
        /// </summary>
        [AllowEmptyString, Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
        public string InputObject { get; set; }

        /// <summary>
        /// Search environment variables when resolving substitutions.
        /// </summary>
        public SwitchParameter AllowEnvSubstitution
        {
            get { return _allowResolveEnv; }
            set { _allowResolveEnv = value; }
        }

        /// <summary>
        /// A script to resolve `include` and `include?` directives. The default implementation can resolve local files and web URL.
        /// </summary>
        [Parameter()]
        public ScriptBlock Include { get; set; }

        /// <summary>
        /// Restricts the recursion level to convert. If you do not specify this parameter, the recursion level will not be limited.
        /// </summary>
        [Parameter()]
        [ValidateRange(0, int.MaxValue)]
        public int Depth
        {
            get { return _maxRecurseLevel; }
            set { _maxRecurseLevel = value; }
        }

        /// <see cref="Cmdlet.BeginProcessing()" />
        protected override void BeginProcessing()
        {
        }

        /// <see cref="Cmdlet.ProcessRecord()" />
        protected override void ProcessRecord()
        {
            _inputObjectBuffer.Add(this.InputObject);
        }

        /// <see cref="Cmdlet.EndProcessing()" />
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
                // this is the default include callback behavior
                async Task<string> includeCallback(string path)
                {
                    if (Include != null)
                    {
                        Collection<PSObject> psResult = Include.Invoke(path);

                        StringBuilder sb = new StringBuilder();
                        foreach (PSObject psobj in psResult)
                        {
                            sb.AppendLine(psobj.ToString());
                        }

                        return sb.ToString();
                    }

                    // default implement

                    if (string.IsNullOrEmpty(path))
                        return "{}";

                    if (path.StartsWith("http://") ||
                        path.StartsWith("https://"))
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(path);
                        using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                        using (Stream stream = response.GetResponseStream())
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            return await reader.ReadToEndAsync();
                        }
                    }
                    else
                    {
                        if (path.StartsWith("file://"))
                            path = path.Substring("file://".Length);

                        File.ReadAllText(path);
                    }

                    // this will result in an error unless using `include?` directive
                    return string.Empty;
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
                throw new ArgumentException(string.Format(RS.InternalErrorStopCode, "POPULATE_NOT_OBJECT"));

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
                throw new ArgumentException(string.Format(RS.InternalErrorStopCode, "POPULATE_NOT_LITERAL"));

            // node could could contain substitution, or unit based value like 
            // timespan or data size
            JsonPlusLiteralType literalType = jv.GetLiteralType();

            switch (literalType)
            {
                case JsonPlusLiteralType.Null:
                    return null;

                case JsonPlusLiteralType.Boolean:
                    try
                    {
                        return jv.GetBoolean();
                    }
                    catch (Exception e)
                    {
                        error = new ErrorRecord(e, "BadJsonPlusLiteralValue", ErrorCategory.ParserError, null);
                        return null;
                    }

                case JsonPlusLiteralType.Integer:
                case JsonPlusLiteralType.Hexadecimal:
                case JsonPlusLiteralType.Octet:
                    // octet and hex could be byte representation. try it first!
                    if (literalType == JsonPlusLiteralType.Hexadecimal || 
                        literalType == JsonPlusLiteralType.Octet)
                    {
                        try
                        {
                            return jv.GetByte();
                        }
                        catch (JsonPlusException e)
                        {
                            if (e.InnerException is OverflowException)
                            {
                                // do nothing
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

                case JsonPlusLiteralType.Decimal:
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

                case JsonPlusLiteralType.TimeSpan:
                    try
                    {
                        return jv.GetTimeSpan();
                    }
                    catch (Exception e)
                    {
                        error = new ErrorRecord(e, "BadJsonPlusLiteralValue", ErrorCategory.ParserError, null);
                        return null;
                    }


                case JsonPlusLiteralType.ByteSize:
                    try
                    {
                        return jv.GetByteSize();
                    }
                    catch (Exception e)
                    {
                        error = new ErrorRecord(e, "BadJsonPlusLiteralValue", ErrorCategory.ParserError, null);
                        return null;
                    }

                case JsonPlusLiteralType.String:
                case JsonPlusLiteralType.UnquotedString:
                case JsonPlusLiteralType.QuotedString:
                case JsonPlusLiteralType.TripleQuotedString:
                case JsonPlusLiteralType.Whitespace:
                    try
                    {
                        return jv.GetString();
                    }
                    catch (Exception e)
                    {
                        error = new ErrorRecord(e, "BadJsonPlusLiteralValue", ErrorCategory.ParserError, null);
                        return null;
                    }

                default:
                    error = new ErrorRecord(new JsonPlusException(string.Format(RS.JsonPlusUnsupportedType)), "BadJsonPlusLiteralValue", ErrorCategory.ParserError, null);
                    return null;
            }

            // this shouldn't happen
            throw new ArgumentException(string.Format(RS.InternalErrorStopCode, "POPULATE_LITERAL_FALLTHROUGH"));
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
