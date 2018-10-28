using System;
using System.Collections.Generic;
using System.Text;

namespace Lizoc.JsonPlus
{
    /// <summary>
    /// Represents a Json+ query path expression.
    /// </summary>
    public sealed class JsonPlusPath : List<string>, IEquatable<JsonPlusPath>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPlusPath"/> class.
        /// </summary>
        public JsonPlusPath()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPlusPath"/> class.
        /// </summary>
        /// <param name="path">A Json+ path query expression.</param>
        public JsonPlusPath(IEnumerable<string> path)
        {
            AddRange(path);
        }

        /// <summary>
        /// Determines whether the path contains any keys.
        /// </summary>
        public bool IsEmpty
        {
            get { return Count == 0; }

        }

        /// <summary>
        /// Gets the Json+ query path expression. #todo
        /// </summary>
        public string Value
        {
            get { return string.Join(".", this); }
        }

        /// <summary>
        /// Gets the last key in the query path.
        /// </summary>
        public string Key
        {
            get { return this[Count - 1]; }
        }

        /// <summary>
        /// Create a <see cref="JsonPlusPath"/> from a part of this <see cref="JsonPlusPath"/> instance, starting from the 
        /// first key.
        /// </summary>
        /// <param name="length">The number of keys to select.</param>
        /// <returns>A <see cref="JsonPlusPath"/> from a part of this <see cref="JsonPlusPath"/> instance.</returns>
        public JsonPlusPath SubPath(int length)
        {
            return new JsonPlusPath(GetRange(0, length));
        }

        /// <summary>
        /// Create a <see cref="JsonPlusPath"/> from a part of this <see cref="JsonPlusPath"/> instance.
        /// </summary>
        /// <param name="index">The index position of the key in this path where selection should begin.</param>
        /// <param name="count">The number of sections to select.</param>
        /// <returns>A <see cref="JsonPlusPath"/> from a part of this <see cref="JsonPlusPath"/> instance.</returns>
        public JsonPlusPath SubPath(int index, int count)
        {
            return new JsonPlusPath(GetRange(index, count));
        }

        internal bool IsChildPathOf(JsonPlusPath parentPath)
        {
            if (Count < parentPath.Count)
                return false;

            for (int i = 0; i < parentPath.Count; ++i)
            {
                if (this[i] != parentPath[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns a string representation of this <see cref="JsonPlusPath"/>.
        /// </summary>
        /// <returns>The string representation of this <see cref="JsonPlusPath"/>.</returns>
        public override string ToString()
        {
            return Value;
        }

        internal static JsonPlusPath FromTokens(TokenizeResult tokens)
        {
            if (tokens == null)
                throw new ArgumentNullException(nameof(tokens));

            List<string> result = new List<string>();
            StringBuilder sb = new StringBuilder();
            while (tokens.Current.Type == TokenType.LiteralValue)
            {
                switch (tokens.Current.LiteralType)
                {
                    case LiteralTokenType.TripleQuotedLiteralValue:
                        throw JsonPlusParserException.Create(tokens.Current, null, RS.TripleQuoteUnsupportedInPath);

                    case LiteralTokenType.QuotedLiteralValue:
                        // Normalize quoted keys, remove the quotes if the key doesn't need them.
                        //sb.Append(tokens.Current.Value.NeedQuotes() ? $"\"{tokens.Current.Value}\"" : tokens.Current.Value);
                        sb.Append(tokens.Current.Value);
                        break;

                    default:
                        string[] split = tokens.Current.Value.Split('.');
                        for (int i = 0; i < split.Length-1; ++i)
                        {
                            sb.Append(split[i]);
                            result.Add(sb.ToString());
                            sb.Clear();
                        }
                        sb.Append(split[split.Length-1]);
                        break;
                }
                tokens.Next();
            }
            result.Add(sb.ToString());
            return new JsonPlusPath(result);
        }

        /// <summary>
        /// Parse a Json+ path query expression into a <see cref="JsonPlusPath"/>.
        /// </summary>
        /// <param name="path">A Json+ path query expression.</param>
        /// <returns>The parsed result of <paramref name="path"/>.</returns>
        public static JsonPlusPath Parse(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return FromTokens(new JPlusTokenizer(path).Tokenize());
        }

        /// <summary>
        /// Returns a value indicating whether the value of this instance is equal to the value of the specified <see cref="JsonPlusPath"/> instance.
        /// </summary>
        /// <param name="other">The object to compare to this instance.</param>
        /// <returns>`true` if the <paramref name="other"/> parameter equals the value of this instance. Otherwise, `false`.</returns>
        public bool Equals(JsonPlusPath other)
        {
            if (other is null)
                return false;

            if (Count != other.Count)
                return false;

            for (int i = 0; i < Count; ++i)
            {
                if (this[i] != other[i])
                    return false;
            }
            return true;
        }

        /// <see cref="JsonPlusValue.Equals(object)"/>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return obj is JsonPlusPath other && Equals(other);
        }

        /// <see cref="JsonPlusValue.GetHashCode()"/>
        public override int GetHashCode()
        {
            const int modifier = 31;
            int result = 601;

            unchecked
            {
                foreach (string key in this)
                {
                    result = result * modifier + key.GetHashCode();
                }
            }
            return result;
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="JsonPlusPath"/> are equal.
        /// </summary>
        /// <param name="val1">The first object to compare.</param>
        /// <param name="val2">The second object to compare.</param>
        /// <returns>`true` if <paramref name="val1"/> and <paramref name="val2"/> represent the same <see cref="JsonPlusPath"/> value. Otherwise, `false`.</returns>
        public static bool operator ==(JsonPlusPath val1, JsonPlusPath val2)
        {
            return Equals(val1, val2);
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="JsonPlusPath"/> are not equal.
        /// </summary>
        /// <param name="val1">The first object to compare.</param>
        /// <param name="val2">The second object to compare.</param>
        /// <returns>`true` if <paramref name="val1"/> and <paramref name="val2"/> do not represent the same <see cref="JsonPlusPath"/> value. Otherwise, `false`.</returns>
        public static bool operator !=(JsonPlusPath val1, JsonPlusPath val2)
        {
            return !Equals(val1, val2);
        }
    }
}
