// -----------------------------------------------------------------------
// <copyright file="JsonPlusRoot.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code documented in this file is subject to the MIT license.
//     See the LICENSE file in the project root for more information.
// </copyright>
// -----------------------------------------------------------------------
 
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lizoc.JsonPlus
{
    /// <summary>
    /// The root node in a Json+ context tree.
    /// </summary>
    public class JsonPlusRoot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPlusRoot"/> class.
        /// </summary>
        public JsonPlusRoot() 
            : this(new JsonPlusValue(null), Enumerable.Empty<JsonPlusSubstitution>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPlusRoot"/> class.
        /// </summary>
        /// <param name="value">The value to associate with this node.</param>
        public JsonPlusRoot(JsonPlusValue value) 
            : this(value, Enumerable.Empty<JsonPlusSubstitution>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPlusRoot"/> class.
        /// </summary>
        /// <param name="value">The value to associate with this instance.</param>
        /// <param name="substitutions">An enumeration of substitutions to associate with this instance.</param>
        public JsonPlusRoot(JsonPlusValue value, IEnumerable<JsonPlusSubstitution> substitutions)
        {
            Value = value;
            Substitutions = substitutions;
        }

        /// <summary>
        /// Gets the value associated with this instance.
        /// </summary>
        public JsonPlusValue Value { get; protected set; }

        /// <summary>
        /// Gets an enumeration of associated substitutions.
        /// </summary>
        public IEnumerable<JsonPlusSubstitution> Substitutions { get; }

        /// <summary>
        /// Determines if this root node contains any value.
        /// </summary>
        public bool IsEmpty
        {
            get { return Value == null || Value.Type == JsonPlusType.Empty; }
        }

        /// <summary>
        /// Returns a node in the Json+ tree using Json+ path query expression.
        /// </summary>
        /// <param name="path">The Json+ path query expression.</param>
        /// <returns>The value of a node selected by <paramref name="path"/>.</returns>
        protected virtual JsonPlusValue GetNode(JsonPlusPath path)
        {
            if (Value.Type != JsonPlusType.Object)
                throw new JsonPlusException(RS.RootNotAnObject);

            return Value.GetObject().GetValue(path);
        }

        /// <see cref="HasPath(JsonPlusPath)"/>
        public bool HasPath(string path)
        {
            return HasPath(JsonPlusPath.Parse(path));
        }

        /// <summary>
        /// Determine whether a node exists at the specified path.
        /// </summary>
        /// <param name="path">A Json+ query path that specifies a node.</param>
        /// <returns>`true` if a node exists at the <paramref name="path"/> specified. Otherwise, `false`.</returns>
        public bool HasPath(JsonPlusPath path)
        {
            JsonPlusValue node;
            try
            {
                node = GetNode(path);
            }
            catch
            {
                return false;
            }
            return node != null;
        }

        /// <summary>
        /// Normalize <see cref="Value"/> and any of its children items, such that only simple literals are used 
        /// where possible.
        /// </summary>
        /// <remarks>
        /// This is an irreversible action. The original <see cref="Value"/> cannot be restored after this method is called.
        /// </remarks>
        public void Normalize()
        {
            Flatten(Value);
        }

        private static void Flatten(IJsonPlusNode node)
        {
            if (!(node is JsonPlusValue v))
                return;

            switch (v.Type)
            {
                case JsonPlusType.Object:
                    JsonPlusObject o = v.GetObject();
                    v.Clear();
                    v.Add(o);
                    foreach (JsonPlusObjectMember item in o.Values)
                    {
                        Flatten(item);
                    }
                    break;

                case JsonPlusType.Array:
                    List<IJsonPlusNode> a = v.GetArray();
                    v.Clear();
                    JsonPlusArray newArray = new JsonPlusArray(v);
                    foreach (IJsonPlusNode item in a)
                    {
                        Flatten(item);
                        newArray.Add(item);
                    }
                    v.Add(newArray);
                    break;

                case JsonPlusType.Literal:
                    if (v.Count == 1)
                        return;

                    string value = v.GetString();
                    v.Clear();
                    if (value == null)
                        v.Add(new NullValue(v));
                    else if (value.NeedTripleQuotes())
                        v.Add(new TripleQuotedStringValue(v, value));
                    else if (value.NeedQuotes())
                        v.Add(new QuotedStringValue(v, value));
                    else
                        v.Add(new UnquotedStringValue(v, value));
                    break;
            }
        }

        #region Typecasting methods

        /// <see cref="GetString(JsonPlusPath, string)"/>
        public string GetString(string path, string defaultValue = null)
        {
            return GetString(JsonPlusPath.Parse(path), defaultValue);
        }

        /// <summary>
        /// Returns a node as a <see cref="string"/>.
        /// </summary>
        /// <param name="path">A Json+ query path that identifies a node in the current context.</param>
        /// <param name="defaultValue">The default value to return if the node specified by <paramref name="path"/> does not exist. Defaults to `null`.</param>
        /// <returns>The <see cref="string"/> value of the node specified by <paramref name="path"/>, or <paramref name="defaultValue"/> if the node does not exist.</returns>
        public string GetString(JsonPlusPath path, string defaultValue = null)
        {
            JsonPlusValue value = GetNode(path);
            return ReferenceEquals(value, JsonPlusValue.Undefined) ? defaultValue : value.GetString();
        }

        /// <see cref="GetBoolean(JsonPlusPath, bool)"/>
        public bool GetBoolean(string path, bool defaultValue = false)
        {
            return GetBoolean(JsonPlusPath.Parse(path), defaultValue);
        }

        /// <summary>
        /// Return a node as a <see cref="bool"/>.
        /// </summary>
        /// <param name="path">A Json+ query path that identifies a node in the current context.</param>
        /// <param name="defaultValue">The default value to return if the node specified by <paramref name="path"/> does not exist.</param>
        /// <returns>The <see cref="bool"/> value of the node specified by <paramref name="path"/>, or <paramref name="defaultValue"/> if the node does not exist.</returns>
        public bool GetBoolean(JsonPlusPath path, bool defaultValue = false)
        {
            JsonPlusValue value = GetNode(path);
            if (ReferenceEquals(value, JsonPlusValue.Undefined))
                return defaultValue;

            return value.GetBoolean();
        }

        /// <see cref="GetByteSize(JsonPlusPath)"/>
        public long? GetByteSize(string path)
        {
            return GetByteSize(JsonPlusPath.Parse(path));
        }

        /// <summary>
        /// Returns a node as a <see cref="Nullable{Int64}"/> object by parsing the value as a number with data size unit.
        /// </summary>
        /// <param name="path">A Json+ query path that identifies a node in the current context.</param>
        /// <returns>The <see cref="Nullable{Int64}"/> value of the node specified by <paramref name="path"/>.</returns>
        /// <see cref="JsonPlusValue.GetByteSize()"/>
        public long? GetByteSize(JsonPlusPath path)
        {
            JsonPlusValue value = GetNode(path);
            if (ReferenceEquals(value, JsonPlusValue.Undefined))
                return null;
            return value.GetByteSize();
        }

        /// <see cref="GetInt32(JsonPlusPath, int)"/>
        public int GetInt32(string path, int defaultValue = 0)
        {
            return GetInt32(JsonPlusPath.Parse(path), defaultValue);
        }

        /// <summary>
        /// Returns a node as an <see cref="int"/>.
        /// </summary>
        /// <param name="path">A Json+ query path that identifies a node in the current context.</param>
        /// <param name="defaultValue">The default value to return if the node specified by <paramref name="path"/> does not exist. Defaults to zero.</param>
        /// <returns>The <see cref="Int32"/> value of the node specified by <paramref name="path"/>, or <paramref name="defaultValue"/> if the node does not exist.</returns>
        public int GetInt32(JsonPlusPath path, int defaultValue = 0)
        {
            JsonPlusValue value = GetNode(path);
            if (ReferenceEquals(value, JsonPlusValue.Undefined))
                return defaultValue;

            return value.GetInt32();
        }

        /// <see cref="GetInt64(JsonPlusPath, long)"/>
        public long GetInt64(string path, long defaultValue = 0)
        {
            return GetInt64(JsonPlusPath.Parse(path), defaultValue);
        }

        /// <summary>
        /// Returns a node as an <see cref="long"/>.
        /// </summary>
        /// <param name="path">A Json+ query path that identifies a node in the current context.</param>
        /// <param name="defaultValue">The default value to return if the node specified by <paramref name="path"/> does not exist. Defaults to zero.</param>
        /// <returns>The <see cref="Int64"/> value of the node specified by <paramref name="path"/>, or <paramref name="defaultValue"/> if the node does not exist.</returns>
        public long GetInt64(JsonPlusPath path, long defaultValue = 0)
        {
            JsonPlusValue value = GetNode(path);
            if (ReferenceEquals(value, JsonPlusValue.Undefined))
                return defaultValue;

            return value.GetInt64();
        }

        /// <see cref="GetByte(JsonPlusPath, byte)"/>
        public byte GetByte(string path, byte defaultValue = 0)
        {
            return GetByte(JsonPlusPath.Parse(path), defaultValue);
        }

        /// <summary>
        /// Returns a node as a <see cref="byte"/>.
        /// </summary>
        /// <param name="path">A Json+ query path that identifies a node in the current context.</param>
        /// <param name="defaultValue">The default value to return if the node specified by <paramref name="path"/> does not exist. Defaults to zero.</param>
        /// <returns>The <see cref="Byte"/> value of the node specified by <paramref name="path"/>, or <paramref name="defaultValue"/> if the node does not exist.</returns>
        public byte GetByte(JsonPlusPath path, byte defaultValue = 0)
        {
            JsonPlusValue value = GetNode(path);
            if (ReferenceEquals(value, JsonPlusValue.Undefined))
                return defaultValue;

            return value.GetByte();
        }

        /// <see cref="GetSingle(JsonPlusPath, float)"/>
        public float GetSingle(string path, float defaultValue = 0)
        {
            return GetSingle(JsonPlusPath.Parse(path), defaultValue);
        }

        /// <summary>
        /// Returns a node as a <see cref="float"/>.
        /// </summary>
        /// <param name="path">A Json+ query path that identifies a node in the current context.</param>
        /// <param name="defaultValue">The default value to return if the node specified by <paramref name="path"/> does not exist. Defaults to zero.</param>
        /// <returns>The <see cref="float"/> value of the node specified by <paramref name="path"/>, or <paramref name="defaultValue"/> if the node does not exist.</returns>
        public float GetSingle(JsonPlusPath path, float defaultValue = 0)
        {
            JsonPlusValue value = GetNode(path);
            if (ReferenceEquals(value, JsonPlusValue.Undefined))
                return defaultValue;

            return value.GetSingle();
        }

        /// <see cref="GetDecimal(JsonPlusPath, decimal)"/>
        public decimal GetDecimal(string path, decimal defaultValue = 0)
        {
            return GetDecimal(JsonPlusPath.Parse(path), defaultValue);
        }

        /// <summary>
        /// Returns a node as a <see cref="decimal"/>.
        /// </summary>
        /// <param name="path">A Json+ query path that identifies a node in the current context.</param>
        /// <param name="defaultValue">The default value to return if the node specified by <paramref name="path"/> does not exist. Defaults to zero.</param>
        /// <returns>The <see cref="decimal"/> value of the node specified by <paramref name="path"/>, or <paramref name="defaultValue"/> if the node does not exist.</returns>
        public decimal GetDecimal(JsonPlusPath path, decimal defaultValue = 0)
        {
            JsonPlusValue value = GetNode(path);
            if (ReferenceEquals(value, JsonPlusValue.Undefined))
                return defaultValue;

            return value.GetDecimal();
        }

        /// <see cref="GetDouble(JsonPlusPath, double)"/>
        public double GetDouble(string path, double defaultValue = 0)
        {
            return GetDouble(JsonPlusPath.Parse(path), defaultValue);
        }

        /// <summary>
        /// Returns a node as a <see cref="double"/>.
        /// </summary>
        /// <param name="path">A Json+ query path that identifies a node in the current context.</param>
        /// <param name="defaultValue">The default value to return if the node specified by <paramref name="path"/> does not exist. Defaults to zero.</param>
        /// <returns>The <see cref="double"/> value of the node specified by <paramref name="path"/>, or <paramref name="defaultValue"/> if the node does not exist.</returns>
        public double GetDouble(JsonPlusPath path, double defaultValue = 0)
        {
            JsonPlusValue value = GetNode(path);
            if (ReferenceEquals(value, JsonPlusValue.Undefined))
                return defaultValue;

            return value.GetDouble();
        }

        /// <see cref="GetBooleanList(JsonPlusPath)"/>
        public IList<Boolean> GetBooleanList(string path)
        {
            return GetBooleanList(JsonPlusPath.Parse(path));
        }

        /// <summary>
        /// Returns a node as an enumerable collection <see cref="bool"/>.
        /// </summary>
        /// <param name="path">A Json+ query path that identifies a node in the current context.</param>
        /// <exception cref="JsonPlusParserException">The node cannot be casted into an array.</exception>
        /// <returns>The <see cref="IList{Boolean}"/> value of the node specified by <paramref name="path"/>.</returns>
        public IList<Boolean> GetBooleanList(JsonPlusPath path)
        {
            JsonPlusValue value = GetNode(path);
            if (ReferenceEquals(value, JsonPlusValue.Undefined))
                throw new JsonPlusParserException(string.Format(RS.ErrCastNodeByPathToArray, path));

            return value.GetBooleanList();
        }

        /// <see cref="GetDecimalList(JsonPlusPath)"/>
        public IList<decimal> GetDecimalList(string path)
        {
            return GetDecimalList(JsonPlusPath.Parse(path));
        }

        /// <summary>
        /// Returns a node as an enumerable collection <see cref="decimal"/>.
        /// </summary>
        /// <param name="path">A Json+ query path that identifies a node in the current context.</param>
        /// <exception cref="JsonPlusParserException">The node cannot be casted into an array.</exception>
        /// <returns>The <see cref="IList{Decimal}"/> value of the node specified by <paramref name="path"/>.</returns>
        public IList<decimal> GetDecimalList(JsonPlusPath path)
        {
            JsonPlusValue value = GetNode(path);
            if (ReferenceEquals(value, JsonPlusValue.Undefined))
                throw new JsonPlusParserException(string.Format(RS.ErrCastNodeByPathToArray, path));

            return value.GetDecimalList();
        }

        /// <see cref="GetSingleList(JsonPlusPath)"/>
        public IList<float> GeSingleList(string path)
        {
            return GetSingleList(JsonPlusPath.Parse(path));
        }

        /// <summary>
        /// Returns a node as an enumerable collection <see cref="float"/>.
        /// </summary>
        /// <param name="path">A Json+ query path that identifies a node in the current context.</param>
        /// <exception cref="JsonPlusParserException">The node cannot be casted into an array.</exception>
        /// <returns>The <see cref="IList{Single}"/> value of the node specified by <paramref name="path"/>.</returns>
        public IList<float> GetSingleList(JsonPlusPath path)
        {
            JsonPlusValue value = GetNode(path);
            if (ReferenceEquals(value, JsonPlusValue.Undefined))
                throw new JsonPlusParserException(string.Format(RS.ErrCastNodeByPathToArray, path));

            return value.GetSingleList();
        }

        /// <see cref="GetDoubleList(JsonPlusPath)"/>
        public IList<double> GetDoubleList(string path)
        {
            return GetDoubleList(JsonPlusPath.Parse(path));
        }

        /// <summary>
        /// Returns a node as an enumerable collection <see cref="double"/>.
        /// </summary>
        /// <param name="path">A Json+ query path that identifies a node in the current context.</param>
        /// <exception cref="JsonPlusParserException">The node cannot be casted into an array.</exception>
        /// <returns>The <see cref="IList{Double}"/> value of the node specified by <paramref name="path"/>.</returns>
        public IList<double> GetDoubleList(JsonPlusPath path)
        {
            JsonPlusValue value = GetNode(path);
            if (ReferenceEquals(value, JsonPlusValue.Undefined))
                throw new JsonPlusParserException(string.Format(RS.ErrCastNodeByPathToArray, path));

            return value.GetDoubleList();
        }

        /// <see cref="GetInt32List(JsonPlusPath)"/>
        public IList<int> GetInt32List(string path)
        {
            return GetInt32List(JsonPlusPath.Parse(path));
        }

        /// <summary>
        /// Returns a node as an enumerable collection <see cref="int"/>.
        /// </summary>
        /// <param name="path">A Json+ query path that identifies a node in the current context.</param>
        /// <exception cref="JsonPlusParserException">The node cannot be casted into an array.</exception>
        /// <returns>The <see cref="IList{Int32}"/> value of the node specified by <paramref name="path"/>.</returns>
        public IList<int> GetInt32List(JsonPlusPath path)
        {
            JsonPlusValue value = GetNode(path);
            if (ReferenceEquals(value, JsonPlusValue.Undefined))
                throw new JsonPlusParserException(string.Format(RS.ErrCastNodeByPathToArray, path));

            return value.GetInt32List();
        }

        /// <see cref="GetInt64List(JsonPlusPath)"/>
        public IList<long> GetInt64List(string path)
        {
            return GetInt64List(JsonPlusPath.Parse(path));
        }

        /// <summary>
        /// Returns a node as an enumerable collection <see cref="long"/>.
        /// </summary>
        /// <param name="path">A Json+ query path that identifies a node in the current context.</param>
        /// <exception cref="JsonPlusParserException">The node cannot be casted into an array.</exception>
        /// <returns>The <see cref="IList{Int64}"/> value of the node specified by <paramref name="path"/>.</returns>
        public IList<long> GetInt64List(JsonPlusPath path)
        {
            JsonPlusValue value = GetNode(path);
            if (ReferenceEquals(value, JsonPlusValue.Undefined))
                throw new JsonPlusParserException(string.Format(RS.ErrCastNodeByPathToArray, path));

            return value.GetInt64List();
        }

        /// <see cref="GetByteList(JsonPlusPath)"/>
        public IList<byte> GetByteList(string path)
        {
            return GetByteList(JsonPlusPath.Parse(path));
        }

        /// <summary>
        /// Returns a node as an enumerable collection <see cref="byte"/>.
        /// </summary>
        /// <param name="path">A Json+ query path that identifies a node in the current context.</param>
        /// <exception cref="JsonPlusParserException">The node cannot be casted into an array.</exception>
        /// <returns>The <see cref="IList{Byte}"/> value of the node specified by <paramref name="path"/>.</returns>
        public IList<byte> GetByteList(JsonPlusPath path)
        {
            JsonPlusValue value = GetNode(path);
            if (ReferenceEquals(value, JsonPlusValue.Undefined))
                throw new JsonPlusParserException(string.Format(RS.ErrCastNodeByPathToArray, path));

            return value.GetByteList();
        }

        /// <see cref="GetStringList(JsonPlusPath)"/>
        public IList<string> GetStringList(string path)
        {
            return GetStringList(JsonPlusPath.Parse(path));
        }

        /// <summary>
        /// Returns a node as an enumerable collection <see cref="string"/>.
        /// </summary>
        /// <param name="path">A Json+ query path that identifies a node in the current context.</param>
        /// <exception cref="JsonPlusParserException">The node cannot be casted into an array.</exception>
        /// <returns>The <see cref="IList{String}"/> value of the node specified by <paramref name="path"/>.</returns>
        public IList<string> GetStringList(JsonPlusPath path)
        {
            JsonPlusValue value = GetNode(path);
            if (ReferenceEquals(value, JsonPlusValue.Undefined))
                throw new JsonPlusParserException(string.Format(RS.ErrCastNodeByPathToArray, path));

            return value.GetStringList();
        }

        /// <see cref="GetValue(JsonPlusPath)"/>
        public JsonPlusValue GetValue(string path)
        {
            return GetValue(JsonPlusPath.Parse(path));
        }

        /// <summary>
        /// Returns a node as a generic <see cref="JsonPlusValue"/>.
        /// </summary>
        /// <param name="path">A Json+ query path that identifies a node in the current context.</param>
        /// <returns>The <see cref="JsonPlusValue"/> value of the node specified by <paramref name="path"/>, or `null` if the node does not exist.</returns>
        public JsonPlusValue GetValue(JsonPlusPath path)
        {
            JsonPlusValue value = GetNode(path);
            return value;
        }

        /// <see cref="GetTimeSpan(JsonPlusPath, TimeSpan?, bool)"/>
        public TimeSpan GetTimeSpan(string path, TimeSpan? defaultValue = null, bool allowInfinite = true)
        {
            return GetTimeSpan(JsonPlusPath.Parse(path), defaultValue, allowInfinite);
        }

        /// <summary>
        /// Returns a node as a <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="path">A Json+ query path that identifies a node in the current context.</param>
        /// <param name="defaultValue">The default value to return if the node specified by <paramref name="path"/> does not exist. Defaults to `null`.</param>
        /// <param name="allowInfinite">Set to `true` to allow the keyword `infinite`. Otherwise, `false`. Defaults to `true`.</param>
        /// <returns>The <see cref="TimeSpan"/> value of the node specified by <paramref name="path"/>, or <paramref name="defaultValue"/> if the node does not exist.</returns>
        public TimeSpan GetTimeSpan(JsonPlusPath path, TimeSpan? defaultValue = null, bool allowInfinite = true)
        {
            JsonPlusValue value = GetNode(path);
            if (value == null)
                return defaultValue.GetValueOrDefault();

            return value.GetTimeSpan(allowInfinite);
        }

        #endregion // Typecasting methods

        /// <summary>
        /// Returns this instance as an enumerable collection of key-value pairs.
        /// </summary>
        /// <returns>An enumerable collection of key-value pairs.</returns>
        public virtual IEnumerable<KeyValuePair<string, JsonPlusObjectMember>> AsEnumerable()
        {
            return Value.GetObject();
        }

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public override string ToString()
        {
            return Value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Returns a string representation of this instance, formatted for improved readibility.
        /// </summary>
        /// <param name="indentSize">The number of spaces to use for each indent level.</param>
        /// <returns>A string representation of this instance, formatted for improved readibility.</returns>
        public string ToString(int indentSize)
        {
            return Value.ToString(1, indentSize);
        }
    }
}

