// -----------------------------------------------------------------------
// <copyright file="JsonPlusLiteralType.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

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

        /// <summary>
        /// An expression that represents time span.
        /// </summary>
        TimeSpan,

        /// <summary>
        /// A number with data size unit.
        /// </summary>
        ByteSize,

        /// <summary>
        /// A string that may or may not be quoted.
        /// </summary>
        String
    }
}
