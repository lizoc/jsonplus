// -----------------------------------------------------------------------
// <copyright file="JsonPlusObjectMember.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lizoc.JsonPlus
{
    /// <summary>
    /// This class represents a key and value tuple representing a member in an <see cref="JsonPlusObject"/>.
    /// </summary>
    /// <remarks>
    /// The following code demonstrates an object member:
    /// ```json+
    /// root {
    ///     items = [
    ///       "1",
    ///       "2"]
    /// }
    /// ```
    /// </remarks>
    public sealed class JsonPlusObjectMember : IJsonPlusNode
    {
        private readonly List<JsonPlusValue> _internalValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPlusObjectMember"/> class.
        /// </summary>
        /// <param name="path">The path to this member in the data tree.</param>
        /// <param name="parent">The parent container of this object instance.</param>
        public JsonPlusObjectMember(JsonPlusPath path, JsonPlusObject parent)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            Path = new JsonPlusPath(path);
            Parent = parent;
            _internalValues = new List<JsonPlusValue>();
        }

        /// <see cref="IJsonPlusNode.Parent"/>
        public IJsonPlusNode Parent { get; }

        /// <see cref="IJsonPlusNode.Type"/>
        public JsonPlusType Type
        {
            get { return Value.Type; }
        }

        /// <see cref="IJsonPlusNode.Source"/>
        public string Source
        {
            get { return Value.Source; }
        }

        /// <summary>
        /// Gets the path to this object instance in the data tree.
        /// </summary>
        public JsonPlusPath Path { get; }

        /// <summary>
        /// Gets the last key in the <see cref="Path"/> property.
        /// </summary>
        public string Key
        {
            get { return Path.Key; }
        }

        internal JsonPlusObjectMember ParentMember
        {
            get
            {
                IJsonPlusNode p = Parent;
                while (p != null && !(p is JsonPlusObjectMember))
                {
                    p = p.Parent;
                }
                return p as JsonPlusObjectMember;
            }
        }

        internal JsonPlusPath GetMemberPath()
        {
            if (ParentMember == null)
                return Path;

            return GetMemberPath(ParentMember, null);
        }

        private JsonPlusPath GetMemberPath(JsonPlusObjectMember member, JsonPlusPath subPath)
        {
            if (member.ParentMember == null && subPath == null)
                return new JsonPlusPath(new string[] { member.Key });

            if (subPath == null)
                return GetMemberPath(member.ParentMember, new JsonPlusPath(new string[] { member.Key }));

            subPath.Add(member.Key);

            if (member.ParentMember == null)
                return subPath;

            return GetMemberPath(member.ParentMember, subPath);
        }

        /// <summary>
        /// Returns true if there are old values stored.
        /// </summary>
        internal bool HasOldValues
        {
            get { return _internalValues.Count > 1; }
        }

        /// <summary>
        /// Returns the underlying value of this member.
        /// </summary>
        public JsonPlusValue Value
        {
            get
            {
                return _internalValues.Count > 0 
                    ? _internalValues.Last() 
                    : JsonPlusValue.Undefined;
            }
            //set => _internalValues.Add(value);
        }

        internal void EnsureMemberIsObject()
        {
            if (Type == JsonPlusType.Object)
                return;

            JsonPlusValue v = new JsonPlusValue(this);
            JsonPlusObject o = new JsonPlusObject(v);
            v.Add(o);
            _internalValues.Add(v);
        }

        internal List<JsonPlusSubstitution> SetValue(JsonPlusValue value)
        {
            List<JsonPlusSubstitution> removedSubs = new List<JsonPlusSubstitution>();
            if (value.Type == JsonPlusType.Array || value.Type == JsonPlusType.Literal)
            {
                List<JsonPlusSubstitution> subs = value.GetAllSubstitution();
                if (subs.All(sub => sub.Path != Path))
                {
                    foreach (JsonPlusValue item in _internalValues)
                    {
                        removedSubs.AddRange(item.GetAllSubstitution());
                    }
                    _internalValues.Clear();
                }
            }
            _internalValues.Add(value);
            return removedSubs;
        }

        internal void RestoreOldValue()
        {
            if (HasOldValues)
                _internalValues.RemoveAt(_internalValues.Count - 1);
        }

        internal JsonPlusValue OlderValueThan(IJsonPlusNode marker)
        {
            List<JsonPlusObject> objectList = new List<JsonPlusObject>();
            int index = 0;
            while (index < _internalValues.Count)
            {
                JsonPlusValue value = _internalValues[index];
                if (value.Any(v => ReferenceEquals(v, marker)))
                    break;

                switch (value.Type)
                {
                    case JsonPlusType.Object:
                        objectList.Add(value.GetObject());
                        break;
                    case JsonPlusType.Literal:
                    case JsonPlusType.Array:
                        objectList.Clear();
                        break;
                }

                index++;
            }

            if (objectList.Count == 0)
                return index == 0 
                    ? null 
                    : _internalValues[index - 1];

            JsonPlusValue result = new JsonPlusValue(null);
            JsonPlusObject o = new JsonPlusObject(result);
            result.Add(o);

            foreach (JsonPlusObject obj in objectList)
            {
                o.Merge(obj);
            }

            return result;
        }

        /// <see cref="IJsonPlusNode.GetObject()"/>
        public JsonPlusObject GetObject()
        {
            List<JsonPlusObject> objectList = new List<JsonPlusObject>();
            foreach (JsonPlusValue value in _internalValues)
            {
                switch (value.Type)
                {
                    case JsonPlusType.Object:
                        objectList.Add(value.GetObject());
                        break;
                    case JsonPlusType.Literal:
                    case JsonPlusType.Array:
                        objectList.Clear();
                        break;
                }
            }

            switch (objectList.Count)
            {
                case 0:
                    return null;
                case 1:
                    return objectList[0];
                default:
                    return new JsonPlusMergedObject(this, objectList);
            }
        }

        /// <see cref="IJsonPlusNode.GetString()"/>
        public string GetString()
        {
            return Value.GetString();
        }

        /// <see cref="IJsonPlusNode.GetArray()"/>
        public List<IJsonPlusNode> GetArray()
        {
            return Value.GetArray();
        }

        /// <see cref="IJsonPlusNode.GetValue()"/>
        public JsonPlusValue GetValue()
        {
            return Value;
        }

        internal void ResolveValue(JsonPlusValue value)
        {
            if (value.Type != JsonPlusType.Empty)
                return;

            ((JsonPlusObject)Parent).ResolveValue(this);
        }

        /// <see cref="IJsonPlusNode.Clone(IJsonPlusNode)"/>
        public IJsonPlusNode Clone(IJsonPlusNode newParent)
        {
            JsonPlusObjectMember newField = new JsonPlusObjectMember(Path, (JsonPlusObject)newParent);
            newField._internalValues.AddRange(_internalValues);
            return newField;
        }

        /// <summary>
        /// Returns a string representation of this <see cref="JsonPlusObjectMember"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="JsonPlusObjectMember"/>.</returns>
        public override string ToString()
        {
            return ToString(0, 2);
        }

        /// <see cref="IJsonPlusNode.ToString(int, int)"/>
        public string ToString(int indent, int indentSize)
        {
            return Value.ToString(indent, indentSize);
        }

        /// <see cref="JsonPlusValue.Equals(IJsonPlusNode)"/>
        public bool Equals(IJsonPlusNode other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return other is JsonPlusObjectMember field && 
                Path.Equals(field.Path) && 
                Value.Equals(other);
        }

        /// <see cref="JsonPlusValue.Equals(object)"/>
        public override bool Equals(object obj)
        {
            return obj is IJsonPlusNode element && Equals(element);
        }

        /// <see cref="JsonPlusValue.GetHashCode()"/>
        public override int GetHashCode()
        {
            unchecked
            {
                return Path.GetHashCode() + Value.GetHashCode();
            }
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="JsonPlusObject"/> are equal.
        /// </summary>
        /// <param name="val1">The first object to compare.</param>
        /// <param name="val2">The second object to compare.</param>
        /// <returns>`true` if <paramref name="val1"/> and <paramref name="val2"/> represent the same value. Otherwise, `false`.</returns>
        public static bool operator ==(JsonPlusObjectMember val1, JsonPlusObjectMember val2)
        {
            return Equals(val1, val2);
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="JsonPlusObject"/> are not equal.
        /// </summary>
        /// <param name="val1">The first object to compare.</param>
        /// <param name="val2">The second object to compare.</param>
        /// <returns>`true` if <paramref name="val1"/> and <paramref name="val2"/> do not represent the same value. Otherwise, `false`.</returns>
        public static bool operator !=(JsonPlusObjectMember val1, JsonPlusObjectMember val2)
        {
            return !Equals(val1, val2);
        }
    }
}
