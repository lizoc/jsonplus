using System;
using System.Collections.Generic;

namespace Lizoc.JsonPlus
{
    /// <summary>
    /// This class implements the <see cref="JsonPlusType.Empty"/> type. It masquerades as all other types 
    /// and are usually used to represent empty or unresolved substitutions.
    /// </summary>
    public sealed class EmptyValue : JsonPlusValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyValue"/> class.
        /// </summary>
        /// <param name="parent">The parent container.</param>
        public EmptyValue(IJsonPlusNode parent)
            : base(parent)
        {
        }

        /// <see cref="IJsonPlusNode.Type"/>
        public override JsonPlusType Type
        {
            get { return JsonPlusType.Empty; }
        }

        /// <see cref="IJsonPlusNode.Source"/>
        public override string Source
        {
            get { return string.Empty; }
        }

        /// <see cref="JsonPlusValue.Add(IJsonPlusNode)"/>
        public override void Add(IJsonPlusNode value)
        {
            throw new JsonPlusException(string.Format(RS.ErrAddToNode, nameof(EmptyValue)));
        }

        /// <see cref="JsonPlusValue.AddRange(IEnumerable{IJsonPlusNode})"/>
        public override void AddRange(IEnumerable<IJsonPlusNode> values)
        {
            throw new JsonPlusException(string.Format(RS.ErrAddRangeToNode, nameof(EmptyValue)));
        }

        /// <see cref="IJsonPlusNode.GetObject()"/>
        public override JsonPlusObject GetObject()
        {
            return new JsonPlusObject(Parent);
        }

        /// <see cref="IJsonPlusNode.GetString()"/>
        public override string GetString()
        {
            return string.Empty;
        }

        /// <see cref="IJsonPlusNode.GetArray()"/>
        public override List<IJsonPlusNode> GetArray()
        {
            return new List<IJsonPlusNode>();
        }

        /// <see cref="JsonPlusValue.Equals(IJsonPlusNode)"/>
        public override bool Equals(IJsonPlusNode other)
        {
            if (other is null)
                return false;

            return other.Type == JsonPlusType.Empty;
        }

        /// <see cref="JsonPlusValue.GetHashCode()"/>
        public override int GetHashCode()
        {
            return 0;
        }

        /// <see cref="IJsonPlusNode.Clone(IJsonPlusNode)"/>
        public override IJsonPlusNode Clone(IJsonPlusNode newParent)
        {
            return new EmptyValue(newParent);
        }
    }
}
