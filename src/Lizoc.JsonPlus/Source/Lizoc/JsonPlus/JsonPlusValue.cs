using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;

namespace Lizoc.JsonPlus
{
    /// <summary>
    /// Basic data type for a Json+ value. All value types in Json+ inherits from this class.
    /// </summary>
    public class JsonPlusValue : List<IJsonPlusNode>, IJsonPlusNode
    {
        private static readonly Regex TimeSpanRegex = new Regex(
            @"^(?<value>([0-9]+(\.[0-9]+)?))(?<unit>(" + 
            "ns|us|ms|" + 
            "s|m|h|d" +
            "))$", RegexOptions.Compiled);

        private static readonly Regex ByteSizeRegex = new Regex(
            @"^(?<value>([0-9]+(\.[0-9]+)?))(?<unit>(" +
            "kB|kb|" +
            "mB|mb|" +
            "gB|gb|" +
            "tB|tb|" +
            "pB|pb|" +
            "))$", RegexOptions.Compiled);

        /// <summary>
        /// Represents an undefined Json+ value.
        /// </summary>
        public static readonly JsonPlusValue Undefined;

        static JsonPlusValue()
        {
            Undefined = new EmptyValue(null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPlusValue"/> class.
        /// </summary>
        /// <param name="parent">The parent container.</param>
        public JsonPlusValue(IJsonPlusNode parent)
        {
            Parent = parent;
        }

        /// <see cref="IJsonPlusNode.Parent"/>
        public IJsonPlusNode Parent { get; }

        /// <see cref="IJsonPlusNode.Type"/>
        public virtual JsonPlusType Type { get; private set; } = JsonPlusType.Empty;

        /// <see cref="IJsonPlusNode.Source"/>
        public virtual string Source
        {
            get
            {
                return Type != JsonPlusType.Literal ? null : ConcatRawString();
            }
        }

        /// <summary>
        /// Gets all children nodes contained by this <see cref="JsonPlusValue"/>.
        /// </summary>
        public ReadOnlyCollection<IJsonPlusNode> Children
        {
            get { return AsReadOnly(); }
        }

        /// <summary>
        /// Removes all children nodes contained by this <see cref="JsonPlusValue"/>.
        /// </summary>
        public new void Clear()
        {
            Type = JsonPlusType.Empty;
            base.Clear();
        }

        /// <summary>
        /// Merge the specified <see cref="IJsonPlusNode"/> into this <see cref="JsonPlusValue"/>.
        /// </summary>
        /// <param name="value">The <see cref="IJsonPlusNode"/> to merge with this <see cref="JsonPlusValue"/>.</param>
        /// <exception cref="JsonPlusParserException">The merged <see cref="IJsonPlusNode.Type"/> type does not match <see cref="JsonPlusValue.Type"/>. This exception will not 
        /// occur if <see cref="JsonPlusValue.Type"/> is set to <see cref="JsonPlusType.Empty"/>.</exception>
        public new virtual void Add(IJsonPlusNode value)
        {
            if (Type == JsonPlusType.Empty)
            {
                Type = value.Type;
            }
            else
            {
                if (!value.IsSubstitution() && Type != value.Type)
                    throw new JsonPlusException(string.Format(RS.MergeTypeMismatch, Type, value.Type));
            }

            base.Add(value);
        }

        /// <summary>
        /// Merge a enumerable collection of <see cref="IJsonPlusNode"/> into this <see cref="JsonPlusValue"/>.
        /// </summary>
        /// <param name="values">An enumerable collection of <see cref="IJsonPlusNode"/> to merge with this <see cref="JsonPlusValue"/>.</param>
        /// <exception cref="JsonPlusParserException">The <see cref="JsonPlusValue.Type"/> property of an item in <paramref name="values"/> type does not match <see cref="JsonPlusValue.Type"/>. This exception will not 
        /// occur if <see cref="JsonPlusValue.Type"/> is set to <see cref="JsonPlusType.Empty"/>.</exception>
        public new virtual void AddRange(IEnumerable<IJsonPlusNode> values)
        {
            foreach (IJsonPlusNode value in values)
            {
                Add(value);
            }
        }

        /// <see cref="IJsonPlusNode.GetObject()"/>
        public virtual JsonPlusObject GetObject()
        {
            List<JsonPlusObject> objects = this.Select(value => value.GetObject()).ToList();

            switch (objects.Count)
            {
                case 0:
                    return null;
                case 1:
                    return objects[0];
                default:
                    return new JsonPlusMergedObject(Parent, objects);
            }
        }

        /// <summary>
        /// Returns the underlying <see cref="JsonPlusLiteralType"/> of a literal value.
        /// </summary>
        /// <exception cref="JsonPlusException">The <see cref="Type"/> property must be of type <see cref="JsonPlusType.Literal"/>.</exception>
        /// <returns>
        /// The underlying <see cref="JsonPlusLiteralType"/> of an instance of this <see cref="JsonPlusValue"/> which is a literal value.
        /// </returns>
        public virtual JsonPlusLiteralType GetLiteralType()
        {
            if (Type != JsonPlusType.Literal)
                throw new JsonPlusException(RS.ExpectLiteralType);

            if (Count > 1)
            {
                bool containsSubstitution = false;
                foreach (IJsonPlusNode node in Children)
                {
                    if (node is JsonPlusSubstitution)
                    {
                        containsSubstitution = true;
                        break;
                    }
                }

                if (containsSubstitution)
                    return JsonPlusLiteralType.String;

                 if (IsTimeSpan())
                    return JsonPlusLiteralType.TimeSpan;

                 if (IsByteSize())
                    return JsonPlusLiteralType.ByteSize;

                return JsonPlusLiteralType.String;
            }

            if (Count == 0)
                return JsonPlusLiteralType.Null;

            // timespan and bytesize expressions will have at least 2 children, so here onwards node cannot be either.
            IJsonPlusNode firstChild = Children[0];

            if (firstChild is JsonPlusSubstitution)
            {
                JsonPlusValue substChild = ((JsonPlusSubstitution)firstChild).ResolvedValue;
                if (substChild.Type != JsonPlusType.Literal)
                    throw new JsonPlusException(string.Format(RS.InternalErrorStopCode, "ONLY_SUBST_TYPE_NOT_LITERAL"));

                return substChild.GetLiteralType();
            }

            if (firstChild is NullValue)
                return JsonPlusLiteralType.Null;
            else if (firstChild is BooleanValue)
                return JsonPlusLiteralType.Boolean;
            else if (firstChild is DecimalValue)
                return JsonPlusLiteralType.Decimal;
            else if (firstChild is IntegerValue)
                return JsonPlusLiteralType.Integer;
            else if (firstChild is HexadecimalValue)
                return JsonPlusLiteralType.Hexadecimal;
            else if (firstChild is OctetValue)
                return JsonPlusLiteralType.Octet;
            else if (firstChild is UnquotedStringValue)
                return JsonPlusLiteralType.UnquotedString;
            else if (firstChild is QuotedStringValue)
                return JsonPlusLiteralType.QuotedString;
            else if (firstChild is TripleQuotedStringValue)
                return JsonPlusLiteralType.TripleQuotedString;
            else if (firstChild is WhitespaceValue)
                return JsonPlusLiteralType.Whitespace;
            else
                throw new JsonPlusException(string.Format(RS.UnknownLiteralToken, firstChild.GetType().ToString()));
        }

        /// <see cref="IJsonPlusNode.GetString()"/>
        public virtual string GetString()
        {
            return Type != JsonPlusType.Literal ? null : ConcatString();
        }

        private string ConcatString()
        {
            string[] array = this.Select(l => l.GetString()).ToArray();
            if (array.All(value => value == null))
                return null;

            StringBuilder sb = new StringBuilder();
            foreach (string s in array)
            {
                sb.Append(s);
            }

            return sb.ToString();
        }

        private string ConcatRawString()
        {
            string[] array = this.Select(l => l.Source).ToArray();
            if (array.All(value => value == null))
                return null;

            StringBuilder sb = new StringBuilder();
            foreach (string s in array)
            {
                sb.Append(s);
            }

            return sb.ToString();
        }

        /// <see cref="IJsonPlusNode.GetArray()"/>
        public virtual List<IJsonPlusNode> GetArray()
        {
            IEnumerable<IJsonPlusNode> x = from value in this
                where value.Type == JsonPlusType.Array
                from e in value.GetArray()
                select e;

            return x.ToList();
        }

        /// <see cref="IJsonPlusNode.GetValue()"/>
        public virtual JsonPlusValue GetValue()
        {
            return this;
        }

        internal List<JsonPlusSubstitution> GetAllSubstitution()
        {
            IEnumerable<IJsonPlusNode> x = from v in this
                where v is JsonPlusSubstitution
                select v;

            return x.Cast<JsonPlusSubstitution>().ToList();
        }

        #region Typecasting methods

        /// <summary>
        /// Returns this value as a <see cref="bool"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">The value is not one of the following keywords: true, false, yes, no</exception>
        /// <returns>
        /// The result of this value casted to a <see cref="bool"/>.
        /// </returns>
        public bool GetBoolean()
        {
            string value = GetString();

            switch (value)
            {
                case JPlusConstants.TrueKeyword:
                case JPlusConstants.AltTrueKeyword:
                    return true;
                case JPlusConstants.FalseKeyword:
                case JPlusConstants.AltFalseKeyword:
                    return false;
                default:
                    throw new NotSupportedException(string.Format(RS.BadBooleanName, value));
            }
        }

        /// <summary>
        /// Returns this value as a <see cref="decimal"/>.
        /// </summary>
        /// <exception cref="JsonPlusException">The value cannot be converted to a <see cref="decimal"/>, or the keywords `infinity` or `NaN` were used.</exception>
        /// <returns>
        /// The result of this value casted to a <see cref="decimal"/>.
        /// </returns>
        public decimal GetDecimal()
        {
            string value = GetString();

            switch (value)
            {
                case JPlusConstants.InfinityPositiveKeyword:
                case JPlusConstants.InfinityKeyword:
                case JPlusConstants.InfinityNegativeKeyword:
                case JPlusConstants.NanKeyword:
                    throw new JsonPlusException(string.Format(RS.NoInfinityOrNanInDecimal, value));
                default:
                    try
                    {
                        return decimal.Parse(value, NumberStyles.Any, NumberFormatInfo.InvariantInfo);
                    }
                    catch (Exception e)
                    {
                        throw new JsonPlusException(string.Format(RS.ErrConvertToDecimal, value), e);
                    }
            }
        }

        /// <summary>
        /// Returns this value as a <see cref="float"/>.
        /// </summary>
        /// <exception cref="JsonPlusException">The value cannot be converted to a <see cref="float"/>.</exception>
        /// <returns>
        /// The result of this value casted to a <see cref="float"/>.
        /// </returns>
        public float GetSingle()
        {
            string value = GetString();

            switch (value)
            {
                case JPlusConstants.InfinityPositiveKeyword:
                case JPlusConstants.InfinityKeyword:
                    return float.PositiveInfinity;
                case JPlusConstants.InfinityNegativeKeyword:
                    return float.NegativeInfinity;
                case JPlusConstants.NanKeyword:
                    return float.NaN;
                default:
                    try
                    {
                        return float.Parse(value, NumberStyles.Any, NumberFormatInfo.InvariantInfo);
                    }
                    catch (Exception e)
                    {
                        throw new JsonPlusException(string.Format(RS.ErrConvertToSingle, value), e);
                    }
            }
        }

        /// <summary>
        /// Returns this value as a <see cref="double"/>.
        /// </summary>
        /// <exception cref="JsonPlusException">The value cannot be converted to a <see cref="double"/>.</exception>
        /// <returns>
        /// The result of this value casted to a <see cref="double"/>.
        /// </returns>
        public double GetDouble()
        {
            string value = GetString();

            switch (value)
            {
                case JPlusConstants.InfinityPositiveKeyword:
                case JPlusConstants.InfinityKeyword:
                    return double.PositiveInfinity;
                case JPlusConstants.InfinityNegativeKeyword:
                    return double.NegativeInfinity;
                case JPlusConstants.NanKeyword:
                    return double.NaN;
                default:
                    try
                    {
                        return double.Parse(value, NumberStyles.Any, NumberFormatInfo.InvariantInfo);
                    }
                    catch (Exception e)
                    {
                        throw new JsonPlusException(string.Format(RS.ErrConvertToDouble, value), e);
                    }
            }
        }

        /// <summary>
        /// Returns this value as a <see cref="long"/>.
        /// </summary>
        /// <exception cref="JsonPlusException">Unable to convert this value to <see cref="long"/>.</exception>
        /// <returns>
        /// The result of this value casted to a <see cref="long"/>.
        /// </returns>
        public long GetInt64()
        {
            string value = GetString();

            if (value.StartsWith("0x"))
            {
                try
                {
                    return Convert.ToInt64(value, 16);
                }
                catch (Exception e)
                {
                    throw new JsonPlusException(string.Format(RS.ErrConvertToHexLong, value), e);
                }
            }

            if (value.StartsWith("0"))
            {
                try
                {
                    return Convert.ToInt64(value, 8);
                }
                catch (Exception e)
                {
                    throw new JsonPlusException(string.Format(RS.ErrConvertToOctetLong, value), e);
                }
            }

            try
            {
                return long.Parse(value, NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw new JsonPlusException(string.Format(RS.ErrConvertToLong, value), e);
            }
        }

        /// <summary>
        /// Returns this value as an <see cref="int"/>.
        /// </summary>
        /// <exception cref="JsonPlusException">Unable to convert this value to <see cref="int"/>.</exception>
        /// <returns>
        /// The result of this value casted to an <see cref="int"/>.
        /// </returns>
        public int GetInt32()
        {
            string value = GetString();

            if (value.StartsWith("0x"))
            {
                try
                {
                    return Convert.ToInt32(value, 16);
                }
                catch (Exception e)
                {
                    throw new JsonPlusException(string.Format(RS.ErrConvertToHexInt, value), e);
                }
            }

            if (value.StartsWith("0"))
            {
                try
                {
                    return Convert.ToInt32(value, 8);
                }
                catch (Exception e)
                {
                    throw new JsonPlusException(string.Format(RS.ErrConvertToOctetInt, value), e);
                }
            }

            try
            {
                return int.Parse(value, NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw new JsonPlusException(string.Format(RS.ErrConvertToInt, value), e);
            }
        }

        /// <summary>
        /// Returns this value as a <see cref="byte"/>.
        /// </summary>
        /// <exception cref="JsonPlusException">Unable to convert this value to <see cref="byte"/>.</exception>
        /// <returns>
        /// The result of this value casted to a <see cref="byte"/>.
        /// </returns>
        public byte GetByte()
        {
            string value = GetString();

            if (value.StartsWith("0x"))
            {
                try
                {
                    return Convert.ToByte(value, 16);
                }
                catch (Exception e)
                {
                    throw new JsonPlusException(string.Format(RS.ErrConvertToHexByte, value), e);
                }
            }

            if (value.StartsWith("0"))
            {
                try
                {
                    return Convert.ToByte(value, 8);
                }
                catch (Exception e)
                {
                    throw new JsonPlusException(string.Format(RS.ErrConvertToOctetByte, value), e);
                }
            }

            try
            {
                return byte.Parse(value, NumberStyles.Integer);
            }
            catch (Exception e)
            {
                throw new JsonPlusException(string.Format(RS.ErrConvertToByte, value), e);
            }
        }

        /// <summary>
        /// Returns this value as an enumerable collection <see cref="byte"/> objects.
        /// </summary>
        /// <returns>
        /// The result of this value casted to a <see cref="IList{Byte}"/>.
        /// </returns>
        public IList<byte> GetByteList()
        {
            return GetArray().Select(v => ((JsonPlusValue)v).GetByte()).ToList();
        }

        /// <summary>
        /// Returns this value as an enumerable collection <see cref="int"/> objects.
        /// </summary>
        /// <returns>
        /// The result of this value casted to a <see cref="IList{Int32}"/>.
        /// </returns>
        public IList<int> GetInt32List()
        {
            return GetArray().Select(v => ((JsonPlusValue)v).GetInt32()).ToList();
        }

        /// <summary>
        /// Returns this value as an enumerable collection <see cref="long"/> objects.
        /// </summary>
        /// <returns>
        /// The result of this value casted to a <see cref="IList{Int64}"/>.
        /// </returns>
        public IList<long> GetInt64List()
        {
            return GetArray().Select(v => ((JsonPlusValue)v).GetInt64()).ToList();
        }

        /// <summary>
        /// Returns this value as an enumerable collection <see cref="bool"/> objects.
        /// </summary>
        /// <returns>
        /// The result of this value casted to a <see cref="IList{Boolean}"/>.
        /// </returns>
        public IList<bool> GetBooleanList()
        {
            return GetArray().Select(v => ((JsonPlusValue)v).GetBoolean()).ToList();
        }

        /// <summary>
        /// Returns this value as an enumerable collection <see cref="float"/> objects.
        /// </summary>
        /// <returns>
        /// The result of this value casted to a <see cref="IList{Single}"/>.
        /// </returns>
        public IList<float> GetSingleList()
        {
            return GetArray().Select(v => ((JsonPlusValue)v).GetSingle()).ToList();
        }

        /// <summary>
        /// Returns this value as an enumerable collection <see cref="double"/> objects.
        /// </summary>
        /// <returns>
        /// The result of this value casted to a <see cref="IList{Double}"/>.
        /// </returns>
        public IList<double> GetDoubleList()
        {
            return GetArray().Select(v => ((JsonPlusValue)v).GetDouble()).ToList();
        }

        /// <summary>
        /// Returns this value as an enumerable collection <see cref="decimal"/> objects.
        /// </summary>
        /// <returns>
        /// The result of this value casted to a <see cref="IList{Decimal}"/>.
        /// </returns>
        public IList<decimal> GetDecimalList()
        {
            return GetArray().Select(v => ((JsonPlusValue)v).GetDecimal()).ToList();
        }

        /// <summary>
        /// Returns this value as an enumerable collection <see cref="string"/> objects.
        /// </summary>
        /// <returns>
        /// The result of this value casted to a <see cref="IList{String}"/>.
        /// </returns>
        public IList<string> GetStringList()
        {
            return GetArray().Select(v => v.GetString()).ToList();
        }

        private bool IsTimeSpan(bool allowInfinite = true)
        {
            string res = GetString();

            if (allowInfinite && res.Equals(JPlusConstants.InfiniteTimeKeyword, StringComparison.Ordinal))
                return true;

            Match match = TimeSpanRegex.Match(res);
            return match.Success;
        }

        /// <summary>
        /// Returns this value as a <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="allowInfinite">Set to `true` to allow the keyword `infinite`, which will return <see cref="Timeout.InfiniteTimeSpan"/>. Otherwise, `false`. Defaults to `true`.</param>
        /// <returns>
        /// The result of this value casted to a <see cref="TimeSpan"/>.
        /// </returns>
        public TimeSpan GetTimeSpan(bool allowInfinite = true)
        {
            string res = GetString();

            if (allowInfinite && res.Equals(JPlusConstants.InfiniteTimeKeyword, StringComparison.Ordinal))
                return Timeout.InfiniteTimeSpan;

            // do a regex match
            Match match = TimeSpanRegex.Match(res);
            if (match.Success)
            {
                string unit = match.Groups["unit"].Value;
                double numeric = ParsePositiveValue(match.Groups["value"].Value);

                switch (unit)
                {
                    case "ns":
                        return TimeSpan.FromTicks((long)Math.Round(TimeSpan.TicksPerMillisecond * numeric / 1000000.0));

                    case "us":
                        return TimeSpan.FromTicks((long)Math.Round(TimeSpan.TicksPerMillisecond * numeric / 1000.0));

                    case "ms":
                        return TimeSpan.FromMilliseconds(numeric);

                    case "s":
                        return TimeSpan.FromSeconds(numeric);

                    case "m":
                        return TimeSpan.FromMinutes(numeric);

                    case "h":
                        return TimeSpan.FromHours(numeric);

                    case "d":
                        return TimeSpan.FromDays(numeric);
                }
            }

            // fallback in case match fails, or nothing in match cases
            return TimeSpan.FromMilliseconds(ParsePositiveValue(res));
        }

        private static double ParsePositiveValue(string v)
        {
            double value = double.Parse(v, NumberFormatInfo.InvariantInfo);
            if (value < 0)
                throw new FormatException(string.Format(RS.ExpectPositiveNumber, value));

            return value;
        }

        private bool IsByteSize()
        {
            string res = GetString();

            Match match = ByteSizeRegex.Match(res);
            return match.Success;
        }

        /// <summary>
        /// Returns this value as a <see cref="Nullable{Int64}"/> by parsing the value as a number with data size unit.
        /// </summary>
        /// <exception cref="OverflowException">Maxiumum supported size is 7 exbibytes (e), or 9 exabytes (eb).</exception>
        /// <returns>
        /// The result of this value casted to a <see cref="Nullable{Int64}"/>.
        /// </returns>
        /// <remarks>
        /// This method returns a value of type <see cref="Int64"/>. Therefore, the maximum supported size is 7e (or 9eb).
        /// 
        /// To specify a byte size, append any of the following keywords to a number:
        /// 
        /// | Unit | Meaning                    |
        /// |------|----------------------------|
        /// | kB   | x1000                      |
        /// | kb   | x1024                      |
        /// | mB   | x1000^2                    |
        /// | mb   | x1024^2                    |
        /// | gB   | x1000^3                    |
        /// | gb   | x1024^3                    |
        /// | tB   | x1000^4                    |
        /// | tb   | x1024^4                    |
        /// | pB   | x1000^5                    |
        /// | pb   | x1024^5                    |
        /// </remarks>
        public long? GetByteSize()
        {
            string res = GetString();

            // do a regex match
            Match match = ByteSizeRegex.Match(res);
            if (match.Success)
            {
                string unit = match.Groups["unit"].Value;
                string v = match.Groups["value"].Value;

                switch (unit)
                {
                    case "kB":
                        return (long.Parse(v) * 1000L);
                    case "kb":
                        return (long.Parse(v) * 1024L);
                    case "mB":
                        return (long.Parse(v) * 1000L * 1000L);
                    case "mb":
                        return (long.Parse(v) * 1024L * 1024L);
                    case "gB":
                        return (long.Parse(v) * 1000L * 1000L * 1000L);
                    case "gb":
                        return (long.Parse(v) * 1024L * 1024L * 1024L);
                    case "tB":
                        return (long.Parse(v) * 1000L * 1000L * 1000L * 1000L);
                    case "tb":
                        return (long.Parse(v) * 1024L * 1024L * 1024L * 1024L);
                    case "pB":
                        return (long.Parse(v) * 1000L * 1000L * 1000L * 1000L * 1000L);
                    case "pb":
                        return (long.Parse(v) * 1024L * 1024L * 1024L * 1024L * 1024L);
                }
            }

            return long.Parse(res);
        }

        #endregion // Typecasting methods

        internal void ResolveValue(IJsonPlusNode child)
        {
            if (child.Type == JsonPlusType.Empty)
            {
                Remove(child);
            }
            else if (Type == JsonPlusType.Empty)
            {
                Type = child.Type;
            }
            else if (Type != child.Type)
            {
                JsonPlusSubstitution sub = (JsonPlusSubstitution)child;
                throw JsonPlusParserException.Create(sub, sub.Path, string.Format(RS.SubstitutionSiblingTypeMismatch, Type, child.Type));
            }

            ((JsonPlusObjectMember)Parent).ResolveValue(this);
        }

        /// <summary>
        /// Returns a string representation of this <see cref="JsonPlusValue"/>.
        /// </summary>
        /// <returns>
        /// A string representation of this <see cref="JsonPlusValue"/>.
        /// </returns>
        public override string ToString()
        {
            return ToString(0, 2);
        }

        /// <see cref="IJsonPlusNode.ToString(int, int)"/>
        public virtual string ToString(int indent, int indentSize)
        {
            switch (Type)
            {
                case JsonPlusType.Literal:
                    return ConcatRawString();
                case JsonPlusType.Object:
                    return "{{" + Environment.NewLine +
                        GetObject().ToString(indent, indentSize) +
                        Environment.NewLine +
                        new string(' ', (indent - 1) * indentSize) +
                        "}}";
                case JsonPlusType.Array:
                    return "[" + string.Join(", ", GetArray().Select(e => e.ToString(indent, indentSize))) + "]";
                case JsonPlusType.Empty:
                    return "<<empty>>";
                default:
                    return null;
            }
        }

        /// <see cref="IJsonPlusNode.Clone(IJsonPlusNode)"/>
        public virtual IJsonPlusNode Clone(IJsonPlusNode newParent)
        {
            JsonPlusValue clone = new JsonPlusValue(newParent);
            foreach (IJsonPlusNode value in this)
            {
                clone.Add(value.Clone(clone));
            }
            return clone;
        }

        /// <summary>
        /// Returns a value indicating whether the value of this instance is equal to the value of the specified <see cref="JsonPlusValue"/> instance.
        /// </summary>
        /// <param name="other">The object to compare to this instance.</param>
        /// <returns>
        /// `true` if the <paramref name="other"/> parameter equals the value of this instance. Otherwise, `false`.
        /// </returns>
        protected bool Equals(JsonPlusValue other)
        {
            return Type == other.Type && GetString() == other.GetString();
        }

        /// <summary>
        /// Returns a value indicating whether the value of this instance is equal to the value of the specified <see cref="IJsonPlusNode"/> instance.
        /// </summary>
        /// <param name="other">The object to compare to this instance.</param>
        /// <exception cref="JsonPlusException">The Json+ data type is unsupported.</exception>
        /// <returns>
        /// `true` if the <paramref name="other"/> parameter equals the value of this instance. Otherwise, `false`.
        /// </returns>
        /// <remarks>
        /// A <see cref="JsonPlusValue"/> is an aggregate of objects of the same type. Therefore, there are possibilities where it can match with any other 
        /// objects of the same type. For example, a <see cref="JsonPlusLiteralValue"/> can have the same value as a <see cref="JsonPlusValue"/>.
        /// </remarks>
        public virtual bool Equals(IJsonPlusNode other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (Type != other.Type)
                return false;

            switch (Type)
            {
                case JsonPlusType.Empty:
                    return other.Type == JsonPlusType.Empty;
                case JsonPlusType.Array:
                    return GetArray().SequenceEqual(other.GetArray());
                case JsonPlusType.Literal:
                    return string.Equals(GetString(), other.GetString());
                case JsonPlusType.Object:
                    return GetObject().AsEnumerable().SequenceEqual(other.GetObject().AsEnumerable());
                default:
                    throw new JsonPlusException(string.Format(RS.UnsupportedType, Type));
            }
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">The object to compare to this instance.</param>
        /// <returns>
        /// `true` if <paramref name="obj"/> is an instance of <see cref="IJsonPlusNode"/> and equals the value of this instance. Otherwise, `false`.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is IJsonPlusNode value && Equals(value);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer hash code.
        /// </returns>
        public override int GetHashCode()
        {
            const int seed = 613;
            const int modifier = 41;

            unchecked
            {
                return this.Aggregate(seed, (current, item) => (current * modifier) + item.GetHashCode());
            }
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="JsonPlusValue"/> are equal.
        /// </summary>
        /// <param name="val1">The first object to compare.</param>
        /// <param name="val2">The second object to compare.</param>
        /// <returns>
        /// `true` if <paramref name="val1"/> and <paramref name="val2"/> represent the same value. Otherwise, `false`.
        /// </returns>
        public static bool operator ==(JsonPlusValue val1, JsonPlusValue val2)
        {
            return Equals(val1, val2);
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="JsonPlusValue"/> are not equal.
        /// </summary>
        /// <param name="val1">The first object to compare.</param>
        /// <param name="val2">The second object to compare.</param>
        /// <returns>
        /// `true` if <paramref name="val1"/> and <paramref name="val2"/> do not represent the same value. Otherwise, `false`.
        /// </returns>
        public static bool operator !=(JsonPlusValue val1, JsonPlusValue val2)
        {
            return !Equals(val1, val2);
        }
    }
}
