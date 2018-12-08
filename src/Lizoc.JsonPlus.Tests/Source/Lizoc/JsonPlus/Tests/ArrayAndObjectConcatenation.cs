// -----------------------------------------------------------------------
// <copyright file="ArrayAndObjectConcatenation.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Lizoc.JsonPlus.Tests
{
    /// <summary>
    /// Container concat tests.
    /// </summary>
    public class ArrayAndObjectConcatenation
    {
        private readonly ITestOutputHelper _output;

        public ArrayAndObjectConcatenation(ITestOutputHelper output)
        {
            _output = output;
        }

        /// <summary>
        /// Arrays can be concatenated with arrays, and objects with objects, but it is an error if they are mixed.
        /// </summary>
        [Fact]
        public void CanConcatenateArray()
        {
            var source = @"a=[1,2] [3,4]";
            Assert.True(
                new[] { 1, 2, 3, 4 }.SequenceEqual(
                    JsonPlusParser.Parse(source).GetInt32List("a")));
        }

        [Fact]
        public void CanConcatenateObjectsViaValueConcatenation_1()
        {
            var source = "a : { b : 1 } { c : 2 }";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal(1, root.GetInt32("a.b"));
            Assert.Equal(2, root.GetInt32("a.c"));
        }

        [Fact]
        public void CanConcatenateObjectsViaValueConcatenation_2()
        {
            var source = @"
data-center-generic = { cluster-size = 6 }
data-center-east = ${data-center-generic} { name = ""east"" }";

            var root = JsonPlusParser.Parse(source);

            Assert.Equal(6, root.GetInt32("data-center-generic.cluster-size"));

            Assert.Equal(6, root.GetInt32("data-center-east.cluster-size"));
            Assert.Equal("east", root.GetString("data-center-east.name"));
        }

        [Fact]
        public void CanConcatenateObjectsViaValueConcatenation_3()
        {
            var source = @"
data-center-generic = { cluster-size = 6 }
data-center-east = { name = ""east"" } ${data-center-generic}";

            var root = JsonPlusParser.Parse(source);

            Assert.Equal(6, root.GetInt32("data-center-generic.cluster-size"));

            Assert.Equal(6, root.GetInt32("data-center-east.cluster-size"));
            Assert.Equal("east", root.GetString("data-center-east.name"));
        }

        [Fact]
        public void CanConcatenateObjectsWhenMerged()
        {
            var source = @"
a : { b : 1 } 
a : { c : 2 }";

            var root = JsonPlusParser.Parse(source);
            Assert.Equal(1, root.GetInt32("a.b"));
            Assert.Equal(2, root.GetInt32("a.c"));
        }

        #region Array and object concatenation exception spec

        [Fact]
        public void ThrowsWhenArrayAndObjectAreConcatenated_1()
        {
            var source = @"a : [1,2] { c : 2 }";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");

        }

        [Fact]
        public void ThrowsWhenArrayAndObjectAreConcatenated_2()
        {
            var source = @"a : { c : 2 } [1,2]";
            
            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenArrayAndObjectSubstitutionAreConcatenated_1()
        {
            var source = @"
a : { c : 2 }
b : [1,2] ${a}";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenArrayAndObjectSubstitutionAreConcatenated_2()
        {
            var source = @"
a : { c : 2 }
b : ${a} [1,2]";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenObjectAndArraySubstitutionAreConcatenated_1()
        {
            var source = @"
a : [1,2]
b : { c : 2 } ${a}";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenObjectAndArraySubstitutionAreConcatenated_2()
        {
            var source = @"
a : [1,2]
b : ${a} { c : 2 }";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        #endregion // Array and object concatenation exception spec

        #region String and object concatenation exception spec

        [Fact]
        public void ThrowsWhenStringAndObjectAreConcatenated_1()
        {
            var source = @"a : literal { c : 2 }";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenStringAndObjectAreConcatenated_2()
        {
            var source = @"a : { c : 2 } literal";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenStringAndObjectSubstitutionAreConcatenated_1()
        {
            var source = @"
a : { c : 2 }
b : literal ${a}";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenStringAndObjectSubstitutionAreConcatenated_2()
        {
            var source = @"
a : { c : 2 }
b : ${a} literal";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenObjectAndStringSubstitutionAreConcatenated_1()
        {
            var source = @"
a : literal
b : ${a} { c : 2 }";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenObjectAndStringSubstitutionAreConcatenated_2()
        {
            var source = @"
a : literal
b : { c : 2 } ${a}";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        #endregion // String and object concatenation exception spec

        #region String and array concatenation exception spec

        [Fact]
        public void ThrowsWhenArrayAndStringAreConcatenated_1()
        {
            var source = @"a : [1,2] literal";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenArrayAndStringAreConcatenated_2()
        {
            var source = @"a : literal [1,2]";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenArrayAndStringSubstitutionAreConcatenated_1()
        {
            var source = @"
a : literal
b : ${a} [1,2]";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenArrayAndStringSubstitutionAreConcatenated_2()
        {
            var source = @"
a : literal
b : [1,2] ${a}";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenStringAndArraySubstitutionAreConcatenated_1()
        {
            var source = @"
a : [1,2]
b : ${a} literal";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsWhenStringAndArraySubstitutionAreConcatenated_2()
        {
            var source = @"
a : [1,2]
b : literal ${a}";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        #endregion // String and array concatenation exception spec

        [Fact]
        public void ArrayTypeCanMixObjectAndString()
        {
            var source = "b = [{a = 1}, test]";
            var root = JsonPlusParser.Parse(source);

            var array = root.GetValue("b").GetArray();

            var item0 = array[0].GetObject();
            Assert.Equal(JsonPlusType.Object, item0.Type);
            Assert.True(item0.ContainsKey("a"));
            Assert.Equal(JsonPlusType.Literal, item0.GetMember("a").Type);
            Assert.Equal(JsonPlusLiteralType.Integer, item0.GetMember("a").GetValue().GetLiteralType());
            Assert.Equal(1, item0.GetMember("a").GetValue().GetInt32());

            var item1 = array[1].GetString();
            Assert.Equal("test", item1);
        }

        [Fact]
        public void ArrayTypeCanMixStringAndObject()
        {
            var source = "b = [test, {a = 1}]";
            var root = JsonPlusParser.Parse(source);

            var array = root.GetValue("b").GetArray();

            var item0 = array[0].GetString();
            Assert.Equal("test", item0);

            var item1 = array[1].GetObject();
            Assert.Equal(JsonPlusType.Object, item1.Type);
            Assert.True(item1.ContainsKey("a"));
            Assert.Equal(JsonPlusType.Literal, item1.GetMember("a").Type);
            Assert.Equal(JsonPlusLiteralType.Integer, item1.GetMember("a").GetValue().GetLiteralType());
            Assert.Equal(1, item1.GetMember("a").GetValue().GetInt32());
        }
    }
}
