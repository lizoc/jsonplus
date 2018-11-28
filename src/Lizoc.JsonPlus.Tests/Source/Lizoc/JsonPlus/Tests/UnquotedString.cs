// -----------------------------------------------------------------------
// <copyright file="UnquotedString.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code documented in this file is subject to the MIT license.
//     See the LICENSE file in the project root for more information.
// </copyright>
// -----------------------------------------------------------------------
 
using System;
using Xunit;

namespace Lizoc.JsonPlus.Tests
{
    /// <summary>
    /// String value concatenation tests.
    /// </summary>
    public class UnquotedString
    {
        /// <summary>
        /// Whitespace before the first and after the last simple value must be discarded.
        /// </summary>
        [Fact]
        public void ShouldRemoveAllLeadingWhitespace()
        {
            var source = $"a = {WhitespaceDef.Whitespaces}literal value";
            var root = JsonPlusParser.Parse(source);

            Assert.Equal("literal value", root.GetString("a"));
        }

        /// <summary>
        /// Whitespace before the first and after the last simple value must be discarded.
        /// </summary>
        [Fact]
        public void ShouldRemoveAllTrailingWhitespace()
        {
            var source = $"a = literal value{WhitespaceDef.Whitespaces}";
            var root = JsonPlusParser.Parse(source);

            Assert.Equal("literal value", root.GetString("a"));
        }

        /// <summary>
        /// As long as simple values are separated only by non-newline whitespace, 
        /// the whitespace between them is preserved and the values, along with the whitespace, 
        /// are concatenated into a string.
        /// </summary>
        [Fact]
        public void ShouldPreserveWhitespacesInTheMiddle()
        {
            var source = $"a = literal{WhitespaceDef.Whitespaces}value";
            var root = JsonPlusParser.Parse(source);

            Assert.Equal($"literal{WhitespaceDef.Whitespaces}value", root.GetString("a"));
        }
    }
}
