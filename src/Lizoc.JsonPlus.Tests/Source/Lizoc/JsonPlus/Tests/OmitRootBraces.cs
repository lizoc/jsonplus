// -----------------------------------------------------------------------
// <copyright file="OmitRootBraces.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Lizoc.JsonPlus.Tests
{
    /// <summary>
    /// Root node tests.
    /// </summary>
    public class OmitRootBraces
    {
        private readonly ITestOutputHelper _output;

        public OmitRootBraces(ITestOutputHelper output)
        {
            _output = output;
        }

        /// <summary>
        /// Empty files are invalid documents.
        /// </summary>
        [Fact]
        public void EmptyFilesShouldThrows()
        {
            var ex = Record.Exception(() => JsonPlusParser.Parse(""));
            Assert.NotNull(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
            Assert.IsType<JsonPlusParserException>(ex);

            ex = Record.Exception(() => JsonPlusParser.Parse(null));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        /// <summary>
        /// Files containing only a non-array non-object value such as a string are invalid.
        /// </summary>
        [Fact]
        public void FileWithLiteralOnlyShouldThrows()
        {
            var ex = Record.Exception(() => JsonPlusParser.Parse("literal"));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");

            ex = Record.Exception(() => JsonPlusParser.Parse("${?path}"));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        /// <summary>
        /// If the file does not begin with a square bracket or curly brace, 
        /// it is parsed as if it were enclosed with {} curly braces.
        /// </summary>
        [Fact]
        public void CanParseJson()
        {
            var source = @"{
  ""root"" : {
    ""int"" : 1,
    ""string"" : ""foo"",
    ""object"" : {
      ""hasContent"" : true
    },
    ""array"" : [1,2,3],
    ""null"" : null,
    ""double"" : 1.23,
    ""bool"" : true
  },
  ""root_2"" : 1234
}";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal("1", root.GetString("root.int"));
            Assert.Equal("1.23", root.GetString("root.double"));
            Assert.True(root.GetBoolean("root.bool"));
            Assert.True(root.GetBoolean("root.object.hasContent"));
            Assert.Null(root.GetString("root.null"));
            Assert.Equal("foo", root.GetString("root.string"));
            Assert.True(new[] { 1, 2, 3 }.SequenceEqual(JsonPlusParser.Parse(source).GetInt32List("root.array")));
            Assert.Equal("1234", root.GetString("root_2"));
        }

        [Fact]
        public void CanParseJsonWithNoRootBraces()
        {
            var source = @"
""root"" : {
  ""int"" : 1,
  ""string"" : ""foo"",
  ""object"" : {
    ""hasContent"" : true
  },
  ""array"" : [1,2,3],
  ""null"" : null,
  ""double"" : 1.23,
  ""bool"" : true
},
""root_2"" : 1234";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal("1", root.GetString("root.int"));
            Assert.Equal("1.23", root.GetString("root.double"));
            Assert.True(root.GetBoolean("root.bool"));
            Assert.True(root.GetBoolean("root.object.hasContent"));
            Assert.Null(root.GetString("root.null"));
            Assert.Equal("foo", root.GetString("root.string"));
            Assert.True(new[] { 1, 2, 3 }.SequenceEqual(JsonPlusParser.Parse(source).GetInt32List("root.array")));
            Assert.Equal("1234", root.GetString("root_2"));
        }

        [Fact]
        public void CanParseJsonPlus()
        {
            var source = @"
root {
  int = 1
  quoted-string = ""foo""
  unquoted-string = bar
  concat-string = foo bar
  object {
    hasContent = true
  }
  array = [1,2,3,4]
  array-concat = [[1,2] [3,4]]
  array-single-element = [1 2 3 4]
  array-newline-element = [
    1
    2
    3
    4
  ]
  null = null
  double = 1.23
  bool = true
}
root_2 = 1234
";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal("1", root.GetString("root.int"));
            Assert.Equal("1.23", root.GetString("root.double"));
            Assert.True(root.GetBoolean("root.bool"));
            Assert.True(root.GetBoolean("root.object.hasContent"));
            Assert.Null(root.GetString("root.null"));
            Assert.Equal("foo", root.GetString("root.quoted-string"));
            Assert.Equal("bar", root.GetString("root.unquoted-string"));
            Assert.Equal("foo bar", root.GetString("root.concat-string"));
            Assert.True(
                new[] { 1, 2, 3, 4 }.SequenceEqual(JsonPlusParser.Parse(source).GetInt32List("root.array")));
            Assert.True(
                new[] { 1, 2, 3, 4 }.SequenceEqual(
                    JsonPlusParser.Parse(source).GetInt32List("root.array-newline-element")));
            Assert.True(
                new[] { "1 2 3 4" }.SequenceEqual(
                    JsonPlusParser.Parse(source).GetStringList("root.array-single-element")));
            Assert.Equal("1234", root.GetString("root_2"));
        }

        [Fact]
        public void CanParseJsonPlusWithRootBraces()
        {
            var source = @"
{
  root {
    int = 1
    quoted-string = ""foo""
    unquoted-string = bar
    concat-string = foo bar
    object {
      hasContent = true
    }
    array = [1,2,3,4]
    array-concat = [[1,2] [3,4]]
    array-single-element = [1 2 3 4]
    array-newline-element = [
      1
      2
      3
      4
    ]
    null = null
    double = 1.23
    bool = true
  }
  root_2 : 1234
}";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal("1", root.GetString("root.int"));
            Assert.Equal("1.23", root.GetString("root.double"));
            Assert.True(root.GetBoolean("root.bool"));
            Assert.True(root.GetBoolean("root.object.hasContent"));
            Assert.Null(root.GetString("root.null"));
            Assert.Equal("foo", root.GetString("root.quoted-string"));
            Assert.Equal("bar", root.GetString("root.unquoted-string"));
            Assert.Equal("foo bar", root.GetString("root.concat-string"));
            Assert.True(
                new[] { 1, 2, 3, 4 }.SequenceEqual(JsonPlusParser.Parse(source).GetInt32List("root.array")));
            Assert.True(
                new[] { 1, 2, 3, 4 }.SequenceEqual(
                    JsonPlusParser.Parse(source).GetInt32List("root.array-newline-element")));
            Assert.True(
                new[] { "1 2 3 4" }.SequenceEqual(
                    JsonPlusParser.Parse(source).GetStringList("root.array-single-element")));
            Assert.Equal("1234", root.GetString("root_2"));
        }

        /// <summary>
        /// A Json+ file is invalid if it omits the opening { but still has a closing }
        /// the curly braces must be balanced.
        /// </summary>
        [Fact]
        public void ThrowsParserExceptionOnUnterminatedFile()
        {
            var source = "{ root { string : \"hello\" }";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsParserExceptionOnInvalidTerminatedFile()
        {
            var source = "root { string : \"hello\" }}";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsParserExceptionOnUnterminatedObject()
        {
            var source = " root { string : \"hello\" ";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsParserExceptionOnUnterminatedNestedObject()
        {
            var source = " root { bar { string : \"hello\" } ";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

    }
}
