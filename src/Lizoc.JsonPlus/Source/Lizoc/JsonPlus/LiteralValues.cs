// -----------------------------------------------------------------------
// <copyright file="LiteralValues.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code documented in this file is subject to the MIT license.
//     See the LICENSE file in the project root for more information.
// </copyright>
// -----------------------------------------------------------------------
 
using System;
using System.Collections.Generic;

namespace Lizoc.JsonPlus
{
    /// <summary>
    /// An abstract implementation of the <see cref="JsonPlusType.Literal"/> data type.
    /// </summary>
    public abstract class JsonPlusLiteralValue : IJsonPlusNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPlusLiteralValue"/> class.
        /// </summary>
        /// <param name="parent">The container object.</param>
        /// <param name="value">The underlying <see cref="string"/> of this value.</param>
        protected JsonPlusLiteralValue(IJsonPlusNode parent, string value)
        {
            Parent = parent;
            Value = value;
        }

        /// <see cref="IJsonPlusNode.Parent"/>
        public IJsonPlusNode Parent { get; }

        /// <see cref="IJsonPlusNode.Type"/>
        public JsonPlusType Type
        {
            get { return JsonPlusType.Literal; }
        }

        /// <summary>
        /// Gets the sub-type of a literal data type.
        /// </summary>
        public abstract JsonPlusLiteralType LiteralType { get; }

        /// <summary>
        /// Gets or sets the underlying string value of this literal value.
        /// </summary>
        public virtual string Value { get; }

        /// <see cref="IJsonPlusNode.Source"/>
        public virtual string Source
        {
            get { return Value; }
        }

        /// <summary>
        /// Returns this value as a <see cref="JsonPlusObject"/>.
        /// </summary>
        /// <exception cref="JsonPlusException">This value is a <see cref="JsonPlusLiteralValue"/> and not a <see cref="JsonPlusObject"/>. Therefore, calling this method will always result in an exception.</exception>
        /// <returns>Calling this method will result in an <see cref="JsonPlusException"/>.</returns>
        public JsonPlusObject GetObject()
        {
            throw new JsonPlusException(RS.CannotConvertLiteralToObject);
        }

        /// <see cref="JsonPlusValue.GetString()"/>
        public string GetString()
        {
            return Value;
        }

        /// <summary>
        /// Returns this value as a <see cref="JsonPlusArray"/>.
        /// </summary>
        /// <exception cref="JsonPlusException">This value is a <see cref="JsonPlusLiteralValue"/> and not a <see cref="JsonPlusArray"/>. Therefore, calling this method will always result in an exception.</exception>
        /// <returns>Calling this method will result in an <see cref="JsonPlusException"/>.</returns>

        public List<IJsonPlusNode> GetArray()
        {
            throw new JsonPlusException(RS.CannotConvertLiteralToArray);
        }

        /// <summary>
        /// Returns the value of this node as a <see cref="JsonPlusValue"/>.
        /// </summary>
        /// <exception cref="JsonPlusException">This value is a <see cref="JsonPlusLiteralValue"/>. It cannot be casted into a <see cref="JsonPlusValue"/>. Therefore, calling this method will always result in an exception.</exception>
        /// <returns>Calling this method will result in an <see cref="JsonPlusException"/>.</returns>
        public JsonPlusValue GetValue()
        {
            throw new JsonPlusException(RS.CannotConvertLiteralToValue);
        }

        /// <summary>
        /// Returns a string representation of this <see cref="JsonPlusObject"/>.
        /// </summary>
        /// <returns>A string representation of this <see cref="JsonPlusObject"/>.</returns>
        public override string ToString()
        {
            return Source;
        }

        /// <see cref="JsonPlusValue.ToString(int, int)"/>
        public string ToString(int indent, int indentSize)
        {
            return Source;
        }

        /// <see cref="IJsonPlusNode.Clone(IJsonPlusNode)"/>
        public abstract IJsonPlusNode Clone(IJsonPlusNode newParent);

        internal static JsonPlusLiteralValue Create(IJsonPlusNode owner, Token token)
        {
            switch (token.LiteralType)
            {
                case LiteralTokenType.Null:
                    return (new NullValue(owner));

                case LiteralTokenType.Boolean:
                    return (new BooleanValue(owner, token.Value));

                case LiteralTokenType.Whitespace:
                    return (new WhitespaceValue(owner, token.Value));

                case LiteralTokenType.UnquotedLiteralValue:
                    return (new UnquotedStringValue(owner, token.Value));

                case LiteralTokenType.QuotedLiteralValue:
                    return (new QuotedStringValue(owner, token.Value));

                case LiteralTokenType.TripleQuotedLiteralValue:
                    return (new TripleQuotedStringValue(owner, token.Value));

                case LiteralTokenType.Integer:
                    return (new IntegerValue(owner, token.Value));

                case LiteralTokenType.Decimal:
                    return (new DecimalValue(owner, token.Value));

                case LiteralTokenType.Hexadecimal:
                    return (new HexadecimalValue(owner, token.Value));

                case LiteralTokenType.Octet:
                    return (new OctetValue(owner, token.Value));

                default:
                    throw new JsonPlusException(string.Format(RS.UnknownLiteralToken, token.Value));
            }
        }

        /// <see cref="JsonPlusValue.Equals(IJsonPlusNode)"/>
        public bool Equals(IJsonPlusNode other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Type == other.Type && string.Equals(Value, other.GetString());
        }

        /// <see cref="JsonPlusValue.Equals(object)"/>
        public override bool Equals(object obj)
        {
            // Needs to be cast to IJsonPlusNode because there are cases 
            // where a JsonPlusLiteralValue can be the same as a JsonPlusValue
            return obj is IJsonPlusNode other && Equals(other);
        }

        /// <see cref="JsonPlusValue.GetHashCode()"/>
        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="JsonPlusObject"/> are equal.
        /// </summary>
        /// <param name="val1">The first object to compare.</param>
        /// <param name="val2">The second object to compare.</param>
        /// <returns>`true` if <paramref name="val1"/> and <paramref name="val2"/> represent the same value. Otherwise, `false`.</returns>
        public static bool operator ==(JsonPlusLiteralValue val1, JsonPlusLiteralValue val2)
        {
            return Equals(val1, val2);
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="JsonPlusObject"/> are not equal.
        /// </summary>
        /// <param name="val1">The first object to compare.</param>
        /// <param name="val2">The second object to compare.</param>
        /// <returns>`true` if <paramref name="val1"/> and <paramref name="val2"/> do not represent the same value. Otherwise, `false`.</returns>
        public static bool operator !=(JsonPlusLiteralValue val1, JsonPlusLiteralValue val2)
        {
            return !Equals(val1, val2);
        }
    }

    /// <summary>
    /// Implements the <see cref="JsonPlusLiteralType.Null"/> data type.
    /// </summary>
    public sealed class NullValue : JsonPlusLiteralValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NullValue"/> class.
        /// </summary>
        /// <param name="parent">The container object.</param>
        public NullValue(IJsonPlusNode parent) 
            : base(parent, "null")
        {
        }

        /// <see cref="JsonPlusLiteralValue.LiteralType"/>
        public override JsonPlusLiteralType LiteralType
        {
            get { return JsonPlusLiteralType.Null; }
        }

        /// <see cref="JsonPlusLiteralValue.Source"/>
        public override string Source
        {
            get { return "null"; }
        }

        /// <summary>
        /// Get the underlying string value of this literal value, which is `null`.
        /// </summary>
        public override string Value
        {
            get { return null; }
        }

        /// <see cref="IJsonPlusNode.Clone(IJsonPlusNode)"/>
        public override IJsonPlusNode Clone(IJsonPlusNode newParent)
        {
            return new NullValue(newParent);
        }
    }

    /// <summary>
    /// Implements the <see cref="JsonPlusLiteralType.Boolean"/> data type.
    /// </summary>
    public sealed class BooleanValue : JsonPlusLiteralValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NullValue"/> class.
        /// </summary>
        /// <param name="parent">The container object.</param>
        /// <param name="value">The underlying <see cref="string"/> of this value.</param>
        public BooleanValue(IJsonPlusNode parent, string value) 
            : base(parent, value)
        {
        }

        /// <see cref="JsonPlusLiteralValue.LiteralType"/>
        public override JsonPlusLiteralType LiteralType
        {
            get { return JsonPlusLiteralType.Boolean; }
        }

        /// <see cref="IJsonPlusNode.Clone(IJsonPlusNode)"/>
        public override IJsonPlusNode Clone(IJsonPlusNode newParent)
        {
            return new BooleanValue(newParent, Value);
        }
    }

    /// <summary>
    /// Implements the <see cref="JsonPlusLiteralType.Decimal"/> data type.
    /// </summary>
    public sealed class DecimalValue : JsonPlusLiteralValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecimalValue"/> class.
        /// </summary>
        /// <param name="parent">The container object.</param>
        /// <param name="value">The underlying <see cref="string"/> of this value.</param>
        public DecimalValue(IJsonPlusNode parent, string value) 
            : base(parent, value)
        {
        }

        /// <see cref="JsonPlusLiteralValue.LiteralType"/>
        public override JsonPlusLiteralType LiteralType
        {
            get { return JsonPlusLiteralType.Decimal; }
        }

        /// <see cref="IJsonPlusNode.Clone(IJsonPlusNode)"/>
        public override IJsonPlusNode Clone(IJsonPlusNode newParent)
        {
            return new DecimalValue(newParent, Value);
        }
    }

    /// <summary>
    /// Implements the <see cref="JsonPlusLiteralType.Integer"/> data type.
    /// </summary>
    public sealed class IntegerValue : JsonPlusLiteralValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerValue"/> class.
        /// </summary>
        /// <param name="parent">The container object.</param>
        /// <param name="value">The underlying <see cref="string"/> of this value.</param>
        public IntegerValue(IJsonPlusNode parent, string value) 
            : base(parent, value)
        {
        }

        /// <see cref="JsonPlusLiteralValue.LiteralType"/>
        public override JsonPlusLiteralType LiteralType
        {
            get { return JsonPlusLiteralType.Integer; }
        }

        /// <see cref="IJsonPlusNode.Clone(IJsonPlusNode)"/>
        public override IJsonPlusNode Clone(IJsonPlusNode newParent)
        {
            return new IntegerValue(newParent, Value);
        }
    }

    /// <summary>
    /// Implements the <see cref="JsonPlusLiteralType.Hexadecimal"/> data type.
    /// </summary>
    public sealed class HexadecimalValue : JsonPlusLiteralValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HexadecimalValue"/> class.
        /// </summary>
        /// <param name="parent">The container object.</param>
        /// <param name="value">The underlying <see cref="string"/> of this value.</param>
        public HexadecimalValue(IJsonPlusNode parent, string value) 
            : base(parent, value)
        {
        }

        /// <see cref="JsonPlusLiteralValue.LiteralType"/>
        public override JsonPlusLiteralType LiteralType
        {
            get { return JsonPlusLiteralType.Hexadecimal; }
        }

        /// <see cref="IJsonPlusNode.Clone(IJsonPlusNode)"/>
        public override IJsonPlusNode Clone(IJsonPlusNode newParent)
        {
            return new HexadecimalValue(newParent, Value);
        }
    }

    /// <summary>
    /// Implements the <see cref="JsonPlusLiteralType.Octet"/> data type.
    /// </summary>
    public sealed class OctetValue : JsonPlusLiteralValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OctetValue"/> class.
        /// </summary>
        /// <param name="parent">The container object.</param>
        /// <param name="value">The underlying <see cref="string"/> of this value.</param>
        public OctetValue(IJsonPlusNode parent, string value) 
            : base(parent, value)
        {
        }

        /// <see cref="JsonPlusLiteralValue.LiteralType"/>
        public override JsonPlusLiteralType LiteralType
        {
            get { return JsonPlusLiteralType.Integer; }
        }

        /// <see cref="IJsonPlusNode.Clone(IJsonPlusNode)"/>
        public override IJsonPlusNode Clone(IJsonPlusNode newParent)
        {
            return new OctetValue(newParent, Value);
        }
    }

    /// <summary>
    /// Implements the <see cref="JsonPlusLiteralType.UnquotedString"/> data type.
    /// </summary>
    public sealed class UnquotedStringValue : JsonPlusLiteralValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnquotedStringValue"/> class.
        /// </summary>
        /// <param name="parent">The container object.</param>
        /// <param name="value">The underlying <see cref="string"/> of this value.</param>
        public UnquotedStringValue(IJsonPlusNode parent, string value)
            : base(parent, value)
        {
        }

        /// <see cref="JsonPlusLiteralValue.LiteralType"/>
        public override JsonPlusLiteralType LiteralType
        {
            get { return JsonPlusLiteralType.UnquotedString; }
        }

        /// <see cref="IJsonPlusNode.Clone(IJsonPlusNode)"/>
        public override IJsonPlusNode Clone(IJsonPlusNode newParent)
        {
            return new UnquotedStringValue(newParent, Value);
        }
    }

    /// <summary>
    /// Implements the <see cref="JsonPlusLiteralType.QuotedString"/> data type.
    /// </summary>
    public sealed class QuotedStringValue : JsonPlusLiteralValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuotedStringValue"/> class.
        /// </summary>
        /// <param name="parent">The container object.</param>
        /// <param name="value">The underlying <see cref="string"/> of this value.</param>
        public QuotedStringValue(IJsonPlusNode parent, string value) 
            : base(parent, value)
        {
        }

        /// <see cref="JsonPlusLiteralValue.LiteralType"/>
        public override JsonPlusLiteralType LiteralType
        {
            get { return JsonPlusLiteralType.QuotedString; }
        }

        /// <see cref="IJsonPlusNode.Source"/>
        public override string Source
        {
            get
            {
                return JPlusConstants.QuoteChar.ToString() + Value + JPlusConstants.QuoteChar.ToString();
            }
        }

        /// <see cref="IJsonPlusNode.Clone(IJsonPlusNode)"/>
        public override IJsonPlusNode Clone(IJsonPlusNode newParent)
        {
            return new QuotedStringValue(newParent, Value);
        }
    }

    /// <summary>
    /// Implements the <see cref="JsonPlusLiteralType.TripleQuotedString"/> data type.
    /// </summary>
    public sealed class TripleQuotedStringValue : JsonPlusLiteralValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TripleQuotedStringValue"/> class.
        /// </summary>
        /// <param name="parent">The container object.</param>
        /// <param name="value">The underlying <see cref="string"/> of this value.</param>
        public TripleQuotedStringValue(IJsonPlusNode parent, string value) 
            : base(parent, value)
        {
        }

        /// <see cref="JsonPlusLiteralValue.LiteralType"/>
        public override JsonPlusLiteralType LiteralType
        {
            get { return JsonPlusLiteralType.TripleQuotedString; }
        }

        /// <see cref="IJsonPlusNode.Source"/>
        public override string Source
        {
            get
            {
                return JPlusConstants.TripleQuote + Value + JPlusConstants.TripleQuote;
            }
        }

        /// <see cref="IJsonPlusNode.Clone(IJsonPlusNode)"/>
        public override IJsonPlusNode Clone(IJsonPlusNode newParent)
        {
            return new TripleQuotedStringValue(newParent, Value);
        }
    }

    /// <summary>
    /// Implements the <see cref="JsonPlusLiteralType.Whitespace"/> data type.
    /// </summary>
    public sealed class WhitespaceValue : JsonPlusLiteralValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WhitespaceValue"/> class.
        /// </summary>
        /// <param name="parent">The container object.</param>
        /// <param name="value">The underlying <see cref="string"/> of this value.</param>
        public WhitespaceValue(IJsonPlusNode parent, string value) 
            : base(parent, value)
        {
        }

        /// <see cref="JsonPlusLiteralValue.LiteralType"/>
        public override JsonPlusLiteralType LiteralType
        {
            get { return JsonPlusLiteralType.Whitespace; }
        }

        /// <see cref="IJsonPlusNode.Clone(IJsonPlusNode)"/>
        public override IJsonPlusNode Clone(IJsonPlusNode newParent)
        {
            return new WhitespaceValue(newParent, Value);
        }
    }
}
