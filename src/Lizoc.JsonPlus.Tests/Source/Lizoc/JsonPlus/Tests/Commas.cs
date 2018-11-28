// -----------------------------------------------------------------------
// <copyright file="Commas.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code documented in this file is subject to the MIT license.
//     See the LICENSE file in the project root for more information.
// </copyright>
// -----------------------------------------------------------------------
 
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Lizoc.JsonPlus.Tests
{
    /// <summary>
    /// Array separator tests.
    /// </summary>
    public class Commas
    {
        private readonly ITestOutputHelper _output;

        public Commas(ITestOutputHelper output)
        {
            _output = output;
        }

        // Values in arrays, and fields in objects, need not have a comma between them 
        // as long as they have at least one ASCII newline (\n, decimal value 10) between them.
        // 
        // The last element in an array or last field in an object may be followed by a single comma. This extra comma is ignored.

        /// <summary>
        /// [1, 2, 3,] and [1, 2, 3] are the same array.
        /// </summary>
        [Fact]
        public void ExtraCommaAtTheEndOfArraysIsIgnored()
        {
            var source = @"
array_1 : [1, 2, 3, ]
array_2 : [1, 2, 3]
";
            var root = JsonPlusParser.Parse(source);
            Assert.True(
                root.GetInt32List("array_1").SequenceEqual(
                    root.GetInt32List("array_2")));
        }

        /// <summary>
        /// [1\n2\n3] and[1, 2, 3] are the same array.
        /// </summary>
        [Fact]
        public void NewLineCanReplaceCommaInArrays()
        {
            var source = @"
array_1 : [
  1
  2
  3 ]
array_2 : [1, 2, 3]
";
            var root = JsonPlusParser.Parse(source);
            Assert.True(
                root.GetInt32List("array_1").SequenceEqual(
                    root.GetInt32List("array_2")));
        }

        /// <summary>
        /// [1, 2, 3,,] is invalid because it has two trailing commas.
        /// </summary>
        [Fact]
        public void ThrowsParserExceptionOnMultipleTrailingCommasInArray()
        {
            var source = @"array : [1, 2, 3,, ]";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        /// <summary>
        /// [, 1, 2, 3] is invalid because it has an initial comma.
        /// </summary>
        [Fact]
        public void ThrowsParserExceptionOnIllegalCommaInFrontOfArray()
        {
            var source = @"array : [, 1, 2, 3]";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        /// <summary>
        /// [1,, 2, 3] is invalid because it has two commas in a row.
        /// </summary>
        [Fact]
        public void ThrowsParserExceptionOnMultipleCommasInArray()
        {
            var source = @"array : [1,, 2, 3]";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        // these same comma rules apply to fields in objects.

        /// <summary>
        /// {a:1, b:2, c:3,} and {a:1, b:2, c:3} are the same object.
        /// </summary>
        [Fact]
        public void ExtraCommaAtTheEndIgnored()
        {
            var src1 = @"a:1, b:2, c:3,";
            var src2 = @"a:1, b:2, c:3";

            Assert.True(
                JsonPlusParser.Parse(src1).AsEnumerable().SequenceEqual(
                    JsonPlusParser.Parse(src2).AsEnumerable()));
        }

        /// <summary>
        /// {a:1\nb:2\nc:3} and {a:1, b:2, c:3} are the same object.
        /// </summary>
        [Fact]
        public void NewLineCanReplaceComma()
        {
            var src1 = @"
a:1
b:2
c:3";
            var src2 = @"a:1, b:2, c:3";

            Assert.True(
                JsonPlusParser.Parse(src1).AsEnumerable().SequenceEqual(
                    JsonPlusParser.Parse(src2).AsEnumerable()));
        }

        /// <summary>
        /// {a:1, b:2, c:3,,} is invalid because it has two trailing commas.
        /// </summary>
        [Fact]
        public void ThrowsParserExceptionOnMultipleTrailingCommas()
        {
            var source = @"{a:1, b:2, c:3,,}";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        /// <summary>
        /// {, a:1, b:2, c:3} is invalid because it has an initial comma.
        /// </summary>
        [Fact]
        public void ThrowsParserExceptionOnIllegalCommaInFront()
        {
            var source = @"{, a:1, b:2, c:3}";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        /// <summary>
        /// {a:1,, b:2, c:3} is invalid because it has two commas in a row.
        /// </summary>
        [Fact]
        public void ThrowsParserExceptionOnMultipleCommas()
        {
            var source = @"{a:1,, b:2, c:3}";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }
    }
}
