namespace Lizoc.JsonPlus
{
    /// <summary>
    /// Literal data types defined under the Json+ specification. This enum represents the subtypes of <see cref="JsonPlusType.Literal"/>.
    /// </summary>
    public enum JsonPlusLiteralType
    {
        /// <summary>
        /// The value is `null`.
        /// </summary>
        Null,

        /// <summary>
        /// Any whitespace character.
        /// </summary>
        Whitespace,

        /// <summary>
        /// A string that is not quoted.
        /// </summary>
        UnquotedString,

        /// <summary>
        /// A string that is double or single quoted.
        /// </summary>
        QuotedString,

        /// <summary>
        /// A string that is triple quoted.
        /// </summary>
        TripleQuotedString,

        /// <summary>
        /// A boolean.
        /// </summary>
        Boolean,

        /// <summary>
        /// An integer number.
        /// </summary>
        Integer,

        /// <summary>
        /// A hexadecimal representation of a number.
        /// </summary>
        Hexadecimal,

        /// <summary>
        /// A octet representation of a number.
        /// </summary>
        Octet,

        /// <summary>
        /// A decimal number.
        /// </summary>
        Decimal,
    }
}
