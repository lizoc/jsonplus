// -----------------------------------------------------------------------
// <copyright file="JsonPlusArray.cs" repo="Json+">
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
    /// This class represents an array node.
    /// </summary>
    /// <remarks>
    /// The following code demonstrates an array declaration in Json+:
    /// 
    /// ```json+
    /// items = [ "1", "2" ]
    /// ```
    /// </remarks>
    public sealed class JsonPlusArray : List<IJsonPlusNode>, IJsonPlusNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPlusArray"/> class.
        /// </summary>
        /// <param name="parent">The parent container.</param>
        public JsonPlusArray(IJsonPlusNode parent)
        {
            Parent = parent;
        }

        /// <see cref="IJsonPlusNode.Parent"/>
        public IJsonPlusNode Parent { get; }

        /// <see cref="IJsonPlusNode.Source"/>
        public string Source
        {
            get
            {
                throw new JsonPlusException(RS.CannotConvertArrayToString);
            }
        }

        /// <see cref="IJsonPlusNode.Type"/>
        public JsonPlusType Type
        {
            get { return JsonPlusType.Array; }
        }

        /// <exception cref="JsonPlusException">This node is an <see cref="JsonPlusArray"/> and not <see cref="JsonPlusObject"/>. Therefore, calling this method will always result in an exception.</exception>
        /// <returns>Calling this method will result in an <see cref="JsonPlusException"/>.</returns>
        /// <see cref="IJsonPlusNode.GetObject()"/>
        public JsonPlusObject GetObject()
        {
            throw new JsonPlusException(RS.CannotConvertArrayToObject);
        }

        /// <exception cref="JsonPlusException">This node is an <see cref="JsonPlusArray"/> and not <see cref="string"/>. Therefore, calling this method will always result in an exception.</exception>
        /// <returns>Calling this method will result in an <see cref="JsonPlusException"/>.</returns>
        /// <see cref="IJsonPlusNode.GetString()"/>
        public string GetString()
        {
            throw new JsonPlusException(RS.CannotConvertArrayToString);
        }

        /// <see cref="IJsonPlusNode.GetArray()"/>
        public List<IJsonPlusNode> GetArray()
        {
            List<IJsonPlusNode> result = new List<IJsonPlusNode>();
            foreach (IJsonPlusNode item in this)
            {
                switch (item)
                {
                    case JsonPlusValue value:
                        result.Add(value);
                        break;

                    case JsonPlusSubstitution sub:
                        if (sub.ResolvedValue != null)
                            result.AddRange(sub.ResolvedValue.GetArray());
                        break;

                    case JsonPlusObject obj:
                        result.Add(obj);
                        break;

                    default:
                        throw new JsonPlusException(RS.InternalError);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the value of this node as a <see cref="JsonPlusValue"/>.
        /// </summary>
        /// <exception cref="JsonPlusException">This value is a <see cref="JsonPlusArray"/>. It cannot be casted into a <see cref="JsonPlusValue"/>. Therefore, calling this method will always result in an exception.</exception>
        /// <returns>Calling this method will result in an <see cref="JsonPlusException"/>.</returns>
        public JsonPlusValue GetValue()
        {
            throw new JsonPlusException(RS.CannotConvertArrayToValue);
        }

        internal void ResolveValue(JsonPlusSubstitution sub)
        {
            if (sub.Type == JsonPlusType.Empty)
            {
                Remove(sub);
                return;
            }

            if (sub.Type != JsonPlusType.Array)
            {
                throw JsonPlusParserException.Create(sub, sub.Path, 
                    string.Format(RS.SubstitutionTypeMismatch, Type, sub.Type));
            }
        }

        /// <summary>
        /// Returns a string representation of this <see cref="JsonPlusArray"/> instance.
        /// </summary>
        /// <returns>A string representation of this <see cref="JsonPlusArray"/> instance.</returns>
        public override string ToString()
        {
            return ToString(0, 2);
        }

        /// <see cref="IJsonPlusNode.ToString(int, int)"/>
        public string ToString(int indent, int indentSize)
        {
            return "[" + string.Join(", ", this) + "]";
        }

        /// <see cref="IJsonPlusNode.Clone(IJsonPlusNode)"/>
        public IJsonPlusNode Clone(IJsonPlusNode newParent)
        {
            JsonPlusArray clone = new JsonPlusArray(newParent);
            clone.AddRange(this);
            return clone;
        }

        private bool Equals(JsonPlusArray other)
        {
            if (Count != other.Count)
                return false;

            for (int i = 0; i < Count; ++i)
            {
                if (!Equals(this[i], other[i]))
                    return false;
            }
            return true;
        }

        /// <see cref="JsonPlusValue.Equals(IJsonPlusNode)"/>
        public bool Equals(IJsonPlusNode other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return other is JsonPlusArray array && Equals(array);
        }

        /// <see cref="JsonPlusValue.Equals(object)"/>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return obj is IJsonPlusNode element && Equals(element);
        }

        /// <see cref="JsonPlusValue.GetHashCode()"/>
        public override int GetHashCode()
        {
            const int seed = 599;
            const int modifier = 37;

            unchecked
            {
                return this.Aggregate(seed, (current, item) => (current * modifier) + item.GetHashCode());
            }
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="JsonPlusArray"/> are equal.
        /// </summary>
        /// <param name="val1">The first object to compare.</param>
        /// <param name="val2">The second object to compare.</param>
        /// <returns>`true` if <paramref name="val1"/> and <paramref name="val2"/> represent the same <see cref="JsonPlusArray"/> value. Otherwise, `false`.</returns>
        public static bool operator ==(JsonPlusArray val1, JsonPlusArray val2)
        {
            return Equals(val1, val2);
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="JsonPlusArray"/> are not equal.
        /// </summary>
        /// <param name="val1">The first object to compare.</param>
        /// <param name="val2">The second object to compare.</param>
        /// <returns>`true` if <paramref name="val1"/> and <paramref name="val2"/> do not represent the same <see cref="JsonPlusArray"/> value. Otherwise, `false`.</returns>
        public static bool operator !=(JsonPlusArray val1, JsonPlusArray val2)
        {
            return !Equals(val1, val2);
        }
    }
}
