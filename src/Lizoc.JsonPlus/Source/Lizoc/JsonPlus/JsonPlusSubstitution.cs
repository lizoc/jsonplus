using System;
using System.Collections.Generic;

namespace Lizoc.JsonPlus
{
    /// <summary>
    /// This class represents a substitution node.
    /// </summary>
    /// <remarks>
    /// The following code demonstrates a substitution in Json+:
    /// 
    /// ```json+
    /// foo {  
    ///   defaultInstances = 10
    ///   deployment {
    ///     /user/time {
    ///       nr-of-instances = ${defaultInstances}
    ///     }
    ///   }
    /// }
    /// ```
    /// </remarks>
    public sealed class JsonPlusSubstitution : IJsonPlusNode, ISourceLocation
    {
        private JsonPlusValue _resolvedValue;


        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPlusSubstitution"/> class.
        /// </summary>
        /// <param name="parent">The <see cref="JsonPlusValue"/> parent of this substitution.</param>
        /// <param name="path">The <see cref="JsonPlusPath"/> that this substitution is pointing to.</param>
        /// <param name="required">Indicates whether this is a lazy substitution. Lazy substitutions uses the `${?` notation.</param>
        /// <param name="location">The location of this substitution token in the source code, used for exception generation purposes.</param>
        internal JsonPlusSubstitution(IJsonPlusNode parent, JsonPlusPath path, ISourceLocation location, bool required)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent), RS.CannotSubstitutionRootNode);
            Column = location.Column;
            Line = location.Line;
            Required = required;
            Path = path;
        }

        /// <see cref="IJsonPlusNode.Parent"/>
        public IJsonPlusNode Parent { get; private set; }

        /// <see cref="ISourceLocation.Line"/>
        public int Line { get; }

        /// <see cref="ISourceLocation.Column"/>
        public int Column { get; }

        /// <summary>
        /// Determines whether this substitution used the `${?` to denote lazy substitution.
        /// </summary>
        public bool Required { get; }

        internal JsonPlusObjectMember ParentMember
        {
            get  {
                IJsonPlusNode p = Parent;
                while (p != null && !(p is JsonPlusObjectMember))
                {
                    p = p.Parent;
                }
                return p as JsonPlusObjectMember;
            }
        }

        /// <summary>
        /// The path to the value which should substitute this instance.
        /// </summary>
        public JsonPlusPath Path { get; }

        /// <summary>
        /// The evaluated value from the <see cref="Path"/> property.
        /// </summary>
        public JsonPlusValue ResolvedValue
        {
            get
            {
                return _resolvedValue;
            }
            internal set
            {
                _resolvedValue = value;
                switch (Parent)
                {
                    case JsonPlusValue v:
                        v.ResolveValue(this);
                        break;
                    case JsonPlusArray a:
                        a.ResolveValue(this);
                        break;
                }
            }
        }

        /// <see cref="IJsonPlusNode.Type"/>
        public JsonPlusType Type
        {
            get { return ResolvedValue?.Type ?? JsonPlusType.Empty; }
        }

        /// <see cref="IJsonPlusNode.Source"/>
        public string Source
        {
            get { return ResolvedValue?.Source; }
        }

        /// <see cref="IJsonPlusNode.GetString()"/>
        public string GetString()
        {
            return ResolvedValue?.GetString();
        }

        /// <see cref="IJsonPlusNode.GetArray()"/>
        public List<IJsonPlusNode> GetArray()
        {
            return ResolvedValue?.GetArray() ?? new List<IJsonPlusNode>();
        }

        /// <see cref="IJsonPlusNode.GetObject()"/>
        public JsonPlusObject GetObject()
        {
            return ResolvedValue?.GetObject() ?? new JsonPlusObject(Parent);
        }

        /// <see cref="IJsonPlusNode.GetValue()"/>
        public JsonPlusValue GetValue()
        {
            return ResolvedValue?.GetValue() ?? new JsonPlusValue(Parent);
        }

        /// <summary>
        /// Returns a string representation of this <see cref="JsonPlusSubstitution"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="JsonPlusSubstitution"/>.</returns>
        public override string ToString()
        {
            return ResolvedValue.ToString(0, 2);
        }

        /// <see cref="JsonPlusValue.ToString(int, int)"/>
        public string ToString(int indent, int indentSize)
        {
            return ResolvedValue.ToString(indent, indentSize);
        }

        /// <summary>
        /// Substitution cannot be cloned because it is resolved at the end of the parsing process.
        /// </summary>
        /// <see cref="IJsonPlusNode.Clone(IJsonPlusNode)"/>
        public IJsonPlusNode Clone(IJsonPlusNode newParent)
        {
            Parent = newParent;
            return this;
        }

        /// <see cref="JsonPlusValue.Equals(IJsonPlusNode)"/>
        public bool Equals(IJsonPlusNode other)
        {
            if (other is null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            if (other is JsonPlusSubstitution sub)
                return Path == sub.Path;

            return !(_resolvedValue is null) && _resolvedValue.Equals(other);
        }

        /// <see cref="JsonPlusValue.Equals(object)"/>
        public override bool Equals(object obj)
        {
            return obj is IJsonPlusNode element && Equals(element);
        }

        /// <see cref="JsonPlusValue.GetHashCode"/>
        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="JsonPlusSubstitution"/> are equal.
        /// </summary>
        /// <param name="val1">The first object to compare.</param>
        /// <param name="val2">The second object to compare.</param>
        /// <returns>`true` if <paramref name="val1"/> and <paramref name="val2"/> represent the same <see cref="JsonPlusSubstitution"/> value. Otherwise, `false`.</returns>
        public static bool operator ==(JsonPlusSubstitution val1, JsonPlusSubstitution val2)
        {
            return Equals(val1, val2);
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="JsonPlusSubstitution"/> are not equal.
        /// </summary>
        /// <param name="val1">The first object to compare.</param>
        /// <param name="val2">The second object to compare.</param>
        /// <returns>`true` if <paramref name="val1"/> and <paramref name="val2"/> do not represent the same <see cref="JsonPlusSubstitution"/> value. Otherwise, `false`.</returns>
        public static bool operator !=(JsonPlusSubstitution val1, JsonPlusSubstitution val2)
        {
            return !Equals(val1, val2);
        }
    }
}
