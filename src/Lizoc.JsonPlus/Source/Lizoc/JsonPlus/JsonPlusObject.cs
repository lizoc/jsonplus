// -----------------------------------------------------------------------
// <copyright file="JsonPlusObject.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code documented in this file is subject to the MIT license.
//     See the LICENSE file in the project root for more information.
// </copyright>
// -----------------------------------------------------------------------
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lizoc.JsonPlus
{
    /// <summary>
    /// This class represents a <see cref="JsonPlusType.Object"/> data type.
    /// </summary>
    /// <remarks>
    /// You can define an `object` in Json+ like this:
    /// 
    /// ```json+
    /// foo {  
    ///   child {
    ///     grandchild {  
    ///       receive = on 
    ///       autoreceive = on
    ///       lifecycle = on
    ///       event-stream = on
    ///       unhandled = on
    ///     }
    ///   }
    /// }
    /// ```
    /// </remarks>
    public class JsonPlusObject : Dictionary<string, JsonPlusObjectMember>, IJsonPlusNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPlusObject"/> class.
        /// </summary>
        public JsonPlusObject(IJsonPlusNode parent)
        {
            Parent = parent;
        }

        /// <see cref="IJsonPlusNode.Parent"/>
        public IJsonPlusNode Parent { get; }

        /// <see cref="IJsonPlusNode.Type"/>
        public JsonPlusType Type
        {
            get { return JsonPlusType.Object; }
        }

        /// <see cref="IJsonPlusNode.Source"/>
        public string Source
        {
            get
            {
                throw new JsonPlusException(RS.CannotConvertObjectToString);
            }
        }

        /// <summary>
        /// Returns the underlying map that contains the barebone object values.
        /// </summary>
        public IDictionary<string, object> Unwrapped
        {
            get
            {
                return this.ToDictionary(
                    k => k.Key,
                    v => v.Value.Type == JsonPlusType.Object
                        ? (object)v.Value.GetObject().Unwrapped
                        : v.Value);
            }
        }

        /// <see cref="IJsonPlusNode.GetObject()"/>
        public JsonPlusObject GetObject()
        {
            return this;
        }

        /// <summary>
        /// Returns this value as a <see cref="string"/>.
        /// </summary>
        /// <exception cref="JsonPlusException">This value is a <see cref="JsonPlusObject"/> and not a <see cref="string"/>. Therefore, calling this method will always result in an exception.</exception>
        /// <returns>Calling this method will result in an <see cref="JsonPlusException"/>.</returns>
        public string GetString()
        {
            throw new JsonPlusException(RS.CannotConvertObjectToString);
        }

        /// <summary>
        /// Returns this value as a <see cref="JsonPlusArray"/>.
        /// </summary>
        /// <exception cref="JsonPlusException">This value is a <see cref="JsonPlusObject"/> and not a <see cref="JsonPlusArray"/>. Therefore, calling this method will always result in an exception.</exception>
        /// <returns>Calling this method will result in an <see cref="JsonPlusException"/>.</returns>
        public List<IJsonPlusNode> GetArray()
        {
            throw new JsonPlusException(RS.CannotConvertObjectToArray);
        }

        /// <summary>
        /// Returns the value of this node as a <see cref="JsonPlusValue"/>.
        /// </summary>
        /// <exception cref="JsonPlusException">This value is a <see cref="JsonPlusObject"/>. It cannot be casted into a <see cref="JsonPlusValue"/>. Therefore, calling this method will always result in an exception.</exception>
        /// <returns>Calling this method will result in an <see cref="JsonPlusException"/>.</returns>
        public JsonPlusValue GetValue()
        {
            throw new JsonPlusException(RS.CannotConvertObjectToValue);
        }

        /// <summary>
        /// Returns the <see cref="JsonPlusObjectMember"/> associated with the key specified.
        /// </summary>
        /// <param name="key">The key associated with the member to return.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> specified is `null`.</exception>
        /// <exception cref="KeyNotFoundException">The <paramref name="key"/> does not exist in the <see cref="JsonPlusObject"/> instance.</exception>
        /// <returns>The <see cref="JsonPlusObjectMember"/> associated with <paramref name="key"/>.</returns>
        public JsonPlusObjectMember GetMember(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (!TryGetValue(key, out JsonPlusObjectMember item))
            {
                IJsonPlusNode p = Parent;
                while (p != null && !(p is JsonPlusObjectMember))
                {
                    p = p.Parent;
                }

                string currentPath = (p != null)
                    ? ((JsonPlusObjectMember)p).Path.Value + "." + key
                    : key;
                string errorMessage = string.Format(RS.ObjectMemberNotFoundByPath, key, currentPath);
                throw new KeyNotFoundException(errorMessage);
            }

            return item;
        }

        /// <summary>
        /// Returns the <see cref="JsonPlusObjectMember"/> associated with the key specified.
        /// </summary>
        /// <param name="key">The key associated with the member to return.</param>
        /// <param name="result">If the member is returned successfully, this parameter will contain the result <see cref="JsonPlusObjectMember"/>. This parameter should be passed uninitialized.</param>
        /// <returns>`true` if the <see cref="JsonPlusObject"/> contains the <see cref="JsonPlusObjectMember"/> associated with <paramref name="key"/>. Otherwise, `false`.</returns>
        public bool TryGetMember(string key, out JsonPlusObjectMember result)
        {
            return TryGetValue(key, out result);
        }

        /// <summary>
        /// Returns the <see cref="JsonPlusObjectMember"/> associated with the <see cref="JsonPlusPath"/> specified.
        /// </summary>
        /// <param name="path">The path to the field to return.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="path"/> specified is `null`.</exception>
        /// <exception cref="ArgumentException">The <paramref name="path"/> specified is empty.</exception>
        /// <exception cref="KeyNotFoundException">The key does not exist in the <see cref="JsonPlusObject"/> instance.</exception>
        /// <exception cref="JsonPlusException">The <paramref name="path"/> specified is invalid.</exception>
        /// <returns>The <see cref="JsonPlusObjectMember"/> associated with <paramref name="path"/>.</returns>
        public JsonPlusObjectMember GetMember(JsonPlusPath path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path.Count == 0)
                throw new ArgumentException(RS.PathIsEmpty, nameof(path));

            int pathIndex = 0;
            JsonPlusObject currentObject = this;
            while (true)
            {
                string key = path[pathIndex];

                if (!currentObject.TryGetValue(key, out JsonPlusObjectMember field))
                {
                    throw new KeyNotFoundException(string.Format(RS.ObjectMemberNotFoundByPath, 
                        key, new JsonPlusPath(path.GetRange(0, pathIndex + 1)).Value));
                }

                if (pathIndex >= path.Count - 1)
                    return field;

                if (field.Type != JsonPlusType.Object)
                {
                    throw new JsonPlusException(string.Format(RS.ObjectMemberInPathNotObject, 
                        new JsonPlusPath(path.GetRange(0, pathIndex + 1)).Value));

                }

                currentObject = field.GetObject();
                pathIndex = pathIndex + 1;
            }
        }

        /// <summary>
        /// Returns the <see cref="JsonPlusObjectMember"/> associated with the <see cref="JsonPlusPath"/> specified.
        /// </summary>
        /// <param name="path">The path to the member to return.</param>
        /// <param name="result">If the member is returned successfully, this parameter will contain the result <see cref="JsonPlusObjectMember"/>. This parameter should be passed uninitialized.</param>
        /// <returns>`true` if the <see cref="JsonPlusObject"/> contains the <see cref="JsonPlusObjectMember"/> associated with <paramref name="path"/>. Otherwise, `false`.</returns>
        public bool TryGetMember(JsonPlusPath path, out JsonPlusObjectMember result)
        {
            result = null;
            if (path == null || path.Count == 0)
                return false;

            int pathIndex = 0;
            JsonPlusObject currentObject = this;
            while (true)
            {
                string key = path[pathIndex];

                if (!currentObject.TryGetValue(key, out var field))
                    return false;

                if (pathIndex >= path.Count - 1)
                {
                    result = field;
                    return true;
                }

                if (field.Type != JsonPlusType.Object)
                    return false;

                currentObject = field.GetObject();
                pathIndex = pathIndex + 1;
            }
        }

        /// <summary>
        /// Returns the merged <see cref="JsonPlusObject"/> that backs the <see cref="JsonPlusObjectMember"/> field associated with the key specified.
        /// </summary>
        /// <param name="key">The key associated with the field to return.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="key"/> specified is `null`.</exception>
        /// <exception cref="KeyNotFoundException">The <paramref name="key"/> does not exist in the <see cref="JsonPlusObject"/> instance.</exception>
        /// <exception cref="JsonPlusException">The <see cref="JsonPlusObjectMember.Type"/> property is not of type <see cref="JsonPlusType.Object"/>.</exception>
        /// <returns>The <see cref="JsonPlusObject"/> backing the member that is associated with <paramref name="key"/>.</returns>
        public JsonPlusObject GetObject(string key)
        {
            return GetMember(key).GetObject();
        }

        /// <summary>
        /// Returns the merged <see cref="JsonPlusObject"/> that backs the <see cref="JsonPlusObjectMember"/> associated with the key specified.
        /// </summary>
        /// <param name="key">The key associated with the member to return.</param>
        /// <param name="result">If the member is returned successfully, this parameter will contain the result <see cref="JsonPlusObjectMember"/>. This parameter should be passed uninitialized.</param>
        /// <returns>`true` if the <see cref="JsonPlusObject"/> contains the <see cref="JsonPlusObjectMember"/> associated with <paramref name="key"/>, and the <see cref="JsonPlusObjectMember.Type"/> property 
        /// is <see cref="JsonPlusType.Object"/>. Otherwise, `false`.</returns>
        public bool TryGetObject(string key, out JsonPlusObject result)
        {
            result = null;
            if (!TryGetMember(key, out JsonPlusObjectMember field))
                return false;

            result = field.GetObject();
            return true;
        }

        /// <summary>
        /// Returns the backing <see cref="JsonPlusValue"/> of the <see cref="JsonPlusObjectMember"/> associated with 
        /// the <see cref="JsonPlusPath"/> specified. The path is relative to the this instance.
        /// </summary>
        /// <param name="path">The relative <see cref="JsonPlusPath"/> path associated with the <see cref="JsonPlusObjectMember"/> of the <see cref="JsonPlusValue"/>.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="path"/> is `null`.</exception>
        /// <exception cref="ArgumentException">The <paramref name="path"/> is empty.</exception>
        /// <exception cref="KeyNotFoundException">The key specified does not exist in the <see cref="JsonPlusObject"/>.</exception>
        /// <returns>The <see cref="JsonPlusValue"/> backing the <see cref="JsonPlusObjectMember"/> associated with the <paramref name="path"/> specified.</returns>
        public JsonPlusValue GetValue(JsonPlusPath path)
        {
            return GetMember(path).Value;
        }

        /// <summary>
        /// Returns the backing <see cref="JsonPlusValue"/> value of the <see cref="JsonPlusObjectMember"/> associated with 
        /// the <see cref="JsonPlusPath"/> specified. The path is relative to the this instance.
        /// </summary>
        /// <param name="path">The relative <see cref="JsonPlusPath"/> path associated with the <see cref="JsonPlusObjectMember"/> of the <see cref="JsonPlusValue"/>.</param>
        /// <param name="result">If the field is returned successfully, this parameter will contain the backing <see cref="JsonPlusValue"/> of the <see cref="JsonPlusObjectMember"/> associated 
        /// with <paramref name="path"/> (if the path is resolvable). This parameter should be passed uninitialized.</param>
        /// <returns>`true` if the <see cref="JsonPlusObject"/> contains the <see cref="JsonPlusObjectMember"/> resolvable by the <paramref name="path"/> specified. Otherwise, `false`.</returns>
        public bool TryGetValue(JsonPlusPath path, out JsonPlusValue result)
        {
            result = null;

            if (!TryGetMember(path, out JsonPlusObjectMember field))
                return false;

            result = field.Value;
            return true;
        }

        /// <summary>
        /// Returns the value associated with the supplied key. If the supplied key is not found, one is created with a blank value.
        /// </summary>
        /// <param name="path">The path associated with the value.</param>
        /// <returns>The value associated with <paramref name="path"/>.</returns>
        private JsonPlusObjectMember GetOrCreateKey(JsonPlusPath path)
        {
            if (TryGetValue(path.Key, out JsonPlusObjectMember child))
                return child;

            child = new JsonPlusObjectMember(path, this);
            Add(path.Key, child);
            return child;
        }

        internal List<JsonPlusObjectMember> TraversePath(JsonPlusPath path)
        {
            List<JsonPlusObjectMember> result = new List<JsonPlusObjectMember>();
            int pathLength = 1;
            JsonPlusObject currentObject = this;
            while (true)
            {
                JsonPlusObjectMember child = currentObject.GetOrCreateKey(new JsonPlusPath(path.GetRange(0, pathLength)));
                result.Add(child);

                pathLength++;
                if (pathLength > path.Count)
                    return result;

                child.EnsureMemberIsObject();
                currentObject = child.GetObject();
            }
        }

        /// <summary>
        /// Returns a string representation of this <see cref="JsonPlusObject"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="JsonPlusObject"/>.</returns>
        public override string ToString()
        {
            return ToString(0, 2);
        }

        /// <see cref="IJsonPlusNode.ToString(int, int)"/>
        public string ToString(int indent, int indentSize)
        {
            string i = new string(' ', indent * indentSize);
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, JsonPlusObjectMember> field in this)
            {
                sb.Append(string.Format("{0}{1} : {2}{3}",
                    i,
                    field.Key,
                    field.Value.ToString(indent + 1, indentSize),
                    Environment.NewLine));
            }

            return sb.ToString(0, sb.Length - Environment.NewLine.Length - 1);
        }

        /// <summary>
        /// Merge the members of a <see cref="JsonPlusObject"/> into this instance.
        /// </summary>
        /// <param name="other">The <see cref="JsonPlusObject"/> to merge with this instance.</param>
        public virtual void Merge(JsonPlusObject other)
        {
            string[] keys = other.Keys.ToArray();
            foreach (string key in keys)
            {
                if (ContainsKey(key))
                {
                    JsonPlusObjectMember thisItem = this[key];
                    JsonPlusObjectMember otherItem = other[key];
                    if (thisItem.Type == JsonPlusType.Object && 
                        otherItem.Type == JsonPlusType.Object)
                    {
                        thisItem.GetObject().Merge(otherItem.GetObject());
                        continue;
                    }
                }
                this[key] = other[key];
            }
        }

        internal void ResolveValue(JsonPlusObjectMember child)
        {
            if (child.Value.Count == 0)
            {
                if (child.HasOldValues)
                    child.RestoreOldValue();
                else
                    Remove(child.Key);
            }
        }

        /// <see cref="IJsonPlusNode.Clone(IJsonPlusNode)"/>
        public IJsonPlusNode Clone(IJsonPlusNode newParent)
        {
            JsonPlusObject clone = new JsonPlusObject(newParent);
            foreach (KeyValuePair<string, JsonPlusObjectMember> kvp in this)
            {
                clone[kvp.Key] = (JsonPlusObjectMember)kvp.Value.Clone(clone);
            }
            return clone;
        }

        /// <see cref="JsonPlusValue.Equals(IJsonPlusNode)"/>
        public bool Equals(IJsonPlusNode other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (other.Type != JsonPlusType.Object)
                return false;

            return this.AsEnumerable().SequenceEqual(other.GetObject().AsEnumerable());
        }

        /// <see cref="JsonPlusValue.Equals(object)"/>
        public override bool Equals(object obj)
        {
            return obj is IJsonPlusNode element && Equals(element);
        }

        /// <see cref="JsonPlusValue.GetHashCode()"/>
        public override int GetHashCode()
        {
            const int modifier = 43;
            int result = 587;
            unchecked
            {
                foreach (JsonPlusObjectMember value in Values)
                {
                    result = result * modifier + value.GetHashCode();
                }
            }
            return result;
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="JsonPlusObject"/> are equal.
        /// </summary>
        /// <param name="val1">The first object to compare.</param>
        /// <param name="val2">The second object to compare.</param>
        /// <returns>`true` if <paramref name="val1"/> and <paramref name="val2"/> represent the same value. Otherwise, `false`.</returns>
        public static bool operator ==(JsonPlusObject val1, JsonPlusObject val2)
        {
            return Equals(val1, val2);
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="JsonPlusObject"/> are not equal.
        /// </summary>
        /// <param name="val1">The first object to compare.</param>
        /// <param name="val2">The second object to compare.</param>
        /// <returns>`true` if <paramref name="val1"/> and <paramref name="val2"/> do not represent the same value. Otherwise, `false`.</returns>
        public static bool operator !=(JsonPlusObject val1, JsonPlusObject val2)
        {
            return !Equals(val1, val2);
        }
    }
}
