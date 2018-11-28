// -----------------------------------------------------------------------
// <copyright file="JsonPlusTokenizerException.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code documented in this file is subject to the MIT license.
//     See the LICENSE file in the project root for more information.
// </copyright>
// -----------------------------------------------------------------------
 
using System;

namespace Lizoc.JsonPlus
{
    /// <summary>
    /// Represents an error when parsing Json+ source code into tokens.
    /// </summary>
    public sealed class JsonPlusTokenizerException : JsonPlusException, ISourceLocation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPlusTokenizerException"/> class.
        /// </summary>
        /// <see cref="JsonPlusException.JsonPlusException(string)"/>
        internal JsonPlusTokenizerException(string message, Token token) 
            : base(message)
        {
            Line = token.Line;
            Column = token.Column;
            Value = token.Value;
        }

        /// <see cref="ISourceLocation.Line"/>
        public int Line { get; }

        /// <see cref="ISourceLocation.Column"/>
        public int Column { get; }

        /// <summary>
        /// A string representation of the token.
        /// </summary>
        public string Value { get; }
    }
}
