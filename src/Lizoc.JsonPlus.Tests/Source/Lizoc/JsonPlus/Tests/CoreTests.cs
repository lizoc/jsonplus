using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Lizoc.JsonPlus.Tests
{
    public class CoreTests
    {
        private readonly ITestOutputHelper _output;

        public CoreTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanUnwrapSubConfig() //undefined behavior in spec, this does not behave the same as JVM hocon.
        {
            var source = @"
a {
   b {
     c = 1
     d = true
   }
}";
            var root = JsonPlusParser.Parse(source).Value.GetObject().Unwrapped;
            var a = root["a"] as IDictionary<string, object>;
            var b = a["b"] as IDictionary<string, object>;
            Assert.Equal(1, (b["c"] as JsonPlusObjectMember).Value.GetInt32());
            Assert.True((b["d"] as JsonPlusObjectMember).Value.GetBoolean());
        }

        [Fact]
        public void ThrowsParserExceptionOnUnterminatedString() //undefined behavior in spec
        {
            var source = " string : \"hello";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsParserExceptionOnUnterminatedStringInObject() //undefined behavior in spec
        {
            var source = " root { string : \"hello }";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsParserExceptionOnUnterminatedArray() //undefined behavior in spec
        {
            var source = " array : [1,2,3";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsParserExceptionOnUnterminatedArrayInObject() //undefined behavior in spec
        {
            var source = " root { array : [1,2,3 }";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void GettingStringFromArrayReturnsNull() //undefined behavior in spec
        {
            var source = " array : [1,2,3]";
            Assert.Null(JsonPlusParser.Parse(source).GetString("array"));
        }

        //TODO: not sure if this is the expected behavior but it is what we have established in Akka.NET
        [Fact]
        public void GettingArrayFromLiteralsReturnsNull() //undefined behavior in spec
        {
            var source = " literal : a b c";
            var res = JsonPlusParser.Parse(source).GetStringList("literal");

            Assert.Empty(res);
        }

        //Added tests to conform to the HOCON spec https://github.com/typesafehub/config/blob/master/HOCON.md
        [Fact]
        public void CanUsePathsAsKeys_3_14()
        {
            var src1 = @"3.14 : 42";
            var src2 = @"3 { 14 : 42}";
            Assert.Equal(
                JsonPlusParser.Parse(src1).GetString("3.14"),
                JsonPlusParser.Parse(src2).GetString("3.14"));
        }

        [Fact]
        public void CanUsePathsAsKeys_3()
        {
            var src1 = @"3 : 42";
            var src2 = @"""3"" : 42";
            Assert.Equal(
                JsonPlusParser.Parse(src1).GetString("3"),
                JsonPlusParser.Parse(src2).GetString("3"));
        }

        [Fact]
        public void CanUsePathsAsKeys_true()
        {
            var src1 = @"true : 42";
            var src2 = @"""true"" : 42";
            Assert.Equal(
                JsonPlusParser.Parse(src1).GetString("true"),
                JsonPlusParser.Parse(src2).GetString("true"));
        }

        [Fact]
        public void CanUsePathsAsKeys_FooBar()
        {
            var src1 = @"foo.bar : 42";
            var src2 = @"foo { bar : 42 }";
            Assert.Equal(
                JsonPlusParser.Parse(src1).GetString("foo.bar"),
                JsonPlusParser.Parse(src2).GetString("foo.bar"));
        }

        [Fact]
        public void CanUsePathsAsKeys_FooBarBaz()
        {
            var src1 = @"foo.bar.baz : 42";
            var src2 = @"foo { bar { baz : 42 } }";
            Assert.Equal(
                JsonPlusParser.Parse(src1).GetString("foo.bar.baz"),
                JsonPlusParser.Parse(src2).GetString("foo.bar.baz"));
        }

        [Fact]
        public void CanUsePathsAsKeys_AX_AY()
        {
            var src1 = @"a.x : 42, a.y : 43";
            var src2 = @"a { x : 42, y : 43 }";

            Assert.Equal(
                JsonPlusParser.Parse(src1).GetString("a.x"),
                JsonPlusParser.Parse(src2).GetString("a.x"));

            Assert.Equal(
                JsonPlusParser.Parse(src1).GetString("a.y"),
                JsonPlusParser.Parse(src2).GetString("a.y"));
        }

        [Fact]
        public void CanUsePathsAsKeys_A_B_C()
        {
            var src1 = @"a b c : 42";
            var src2 = @"""a b c"" : 42";
            Assert.Equal(
                JsonPlusParser.Parse(src1).GetString("a b c"),
                JsonPlusParser.Parse(src2).GetString("a b c"));
        }


        [Fact]
        public void CanMergeObject()
        {
            var source = @"
a.b.c = {
        x = 1
        y = 2
    }
a.b.c = {
        z = 3
    }
";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal("1", root.GetString("a.b.c.x"));
            Assert.Equal("2", root.GetString("a.b.c.y"));
            Assert.Equal("3", root.GetString("a.b.c.z"));
        }

        [Fact]
        public void CanOverrideObject()
        {
            var source = @"
a.b = 1
a = null
a.c = 3
";
            var root = JsonPlusParser.Parse(source);
            Assert.False(root.HasPath("a.b"));
            Assert.Equal("3", root.GetString("a.c"));
        }

        [Fact]
        public void CanParseObject()
        {
            var source = @"
a {
  b = 1
}
";
            Assert.Equal("1", JsonPlusParser.Parse(source).GetString("a.b"));
        }

        [Fact]
        public void CanTrimValue()
        {
            var source = "a= \t \t 1 \t \t,";
            Assert.Equal("1", JsonPlusParser.Parse(source).GetString("a"));
        }

        [Fact]
        public void CanTrimConcatenatedValue()
        {
            var source = "a= \t \t 1 2 3 \t \t,";
            Assert.Equal("1 2 3", JsonPlusParser.Parse(source).GetString("a"));
        }

        [Fact]
        public void CanConsumeCommaAfterValue()
        {
            var source = "a=1,";
            Assert.Equal("1", JsonPlusParser.Parse(source).GetString("a"));
        }

        [Fact]
        public void CanAssignIpAddressToField()
        {
            var source = @"a=127.0.0.1";
            Assert.Equal("127.0.0.1", JsonPlusParser.Parse(source).GetString("a"));
        }

        [Fact]
        public void CanAssignConcatenatedValueToField()
        {
            var source = @"a=1 2 3";
            Assert.Equal("1 2 3", JsonPlusParser.Parse(source).GetString("a"));
        }

        [Fact]
        public void CanAssignValueToQuotedField()
        {
            var source = @"""a""=1";
            Assert.Equal(1L, JsonPlusParser.Parse(source).GetInt64("a"));
        }

        [Fact]
        public void CanAssignValueToPathExpression()
        {
            var source = @"a.b.c=1";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal(1L, root.GetInt64("a.b.c"));
        }

        [Fact]
        public void CanAssignValuesToPathExpressions()
        {
            var source = @"
a.b.c=1
a.b.d=2
a.b.e.f=3
";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal(1L, root.GetInt64("a.b.c"));
            Assert.Equal(2L, root.GetInt64("a.b.d"));
            Assert.Equal(3L, root.GetInt64("a.b.e.f"));
        }

        [Fact]
        public void CanAssignLongToField()
        {
            var source = @"a=1";
            Assert.Equal(1L, JsonPlusParser.Parse(source).GetInt64("a"));
        }

        [Fact]
        public void CanAssignArrayToField()
        {
            var source = @"a=
[
    1
    2
    3
]";
            Assert.True(new[] { 1, 2, 3 }.SequenceEqual(JsonPlusParser.Parse(source).GetInt32List("a")));

            //source = @"a= [ 1, 2, 3 ]";
            //Assert.True(new[] { 1, 2, 3 }.SequenceEqual(Parser.Parse(source).GetIntList("a")));
        }

        [Fact]
        public void CanConcatenateArray()
        {
            var source = @"a=[1,2] [3,4]";
            Assert.True(new[] { 1, 2, 3, 4 }.SequenceEqual(JsonPlusParser.Parse(source).GetInt32List("a")));
        }

        [Fact]
        public void CanAssignDoubleToField()
        {
            var source = @"a=1.1";
            Assert.Equal(1.1, JsonPlusParser.Parse(source).GetDouble("a"));
        }

        [Fact]
        public void CanAssignNumbersToField()
        {
            var source = @"
a = 1000.05
b = infinity
c = -infinity
d = +infinity
e = NaN
f = 255
g = 0xff
h = 0377
";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal(1000.05, root.GetDouble("a"));
            Assert.Equal(double.PositiveInfinity, root.GetDouble("b"));
            Assert.Equal(double.NegativeInfinity, root.GetDouble("c"));
            Assert.Equal(double.PositiveInfinity, root.GetDouble("d"));
            Assert.Equal(double.NaN, root.GetDouble("e"));

            Assert.Equal(1000.05f, root.GetSingle("a"));
            Assert.Equal(float.PositiveInfinity, root.GetSingle("b"));
            Assert.Equal(float.NegativeInfinity, root.GetSingle("c"));
            Assert.Equal(float.PositiveInfinity, root.GetSingle("d"));
            Assert.Equal(float.NaN, root.GetSingle("e"));

            Assert.Equal(1000.05m, root.GetDecimal("a"));
            Assert.Throws<JsonPlusException>(() => root.GetDecimal("b"));
            Assert.Throws<JsonPlusException>(() => root.GetDecimal("c"));
            Assert.Throws<JsonPlusException>(() => root.GetDecimal("d"));

            Assert.Equal(255, root.GetInt64("f"));
            Assert.Equal(255, root.GetInt64("g"));
            Assert.Equal(255, root.GetInt64("h"));

            Assert.Equal(255, root.GetInt32("f"));
            Assert.Equal(255, root.GetInt32("g"));
            Assert.Equal(255, root.GetInt32("h"));

            Assert.Equal(255, root.GetByte("f"));
            Assert.Equal(255, root.GetByte("g"));
            Assert.Equal(255, root.GetByte("h"));
        }

        [Fact]
        public void CanAssignNullToField()
        {
            var source = @"a=null";
            Assert.Null(JsonPlusParser.Parse(source).GetString("a"));
        }

        [Fact]
        public void CanAssignBooleanToField()
        {
            var source = @"a=true";
            Assert.True(JsonPlusParser.Parse(source).GetBoolean("a"));
            source = @"a=false";
            Assert.False(JsonPlusParser.Parse(source).GetBoolean("a"));

            // deprecated keywords on/off
            //source = @"a=on";
            //Assert.True(JsonPlusParser.Parse(source).GetBoolean("a"));
            //source = @"a=off";
            //Assert.False(JsonPlusParser.Parse(source).GetBoolean("a"));

            source = @"a=yes";
            Assert.True(JsonPlusParser.Parse(source).GetBoolean("a"));
            source = @"a=no";
            Assert.False(JsonPlusParser.Parse(source).GetBoolean("a"));
        }

        [Fact]
        public void FailedIncludeParsingShouldBeParsedAsLiteralInstead()
        {
            var source = @"{
  include = include required file(not valid)
  include file = not an include
}";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal("include required file(not valid)", root.GetString("include"));
            Assert.Equal("not an include", root.GetString("include file"));
        }

        [Fact]
        public void CanAssignQuotedStringToField()
        {
            var source = @"a=""hello""";
            Assert.Equal("hello", JsonPlusParser.Parse(source).GetString("a"));
        }

        [Fact]
        public void CanAssignUnQuotedStringToField()
        {
            var source = @"a=hello";
            Assert.Equal("hello", JsonPlusParser.Parse(source).GetString("a"));
        }

        [Fact]
        public void CanAssignTripleQuotedStringToField()
        {
            var source = "a=\"\"\"hello\"\"\"";
            Assert.Equal("hello", JsonPlusParser.Parse(source).GetString("a"));
        }

        [Fact]
        public void CanOverwriteValue()
        {
            var source = @"
test {
  value  = 123
}
test.value = 456
";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal(456, root.GetInt32("test.value"));
        }

        [Fact]
        public void CanAssignNullStringToField()
        {
            var source = @"a=null";
            Assert.Null(JsonPlusParser.Parse(source).GetString("a"));
        }

        [Fact]
        public void CanAssignQuotedNullStringToField()
        {
            var source = @"a=""null""";
            Assert.Equal("null", JsonPlusParser.Parse(source).GetString("a"));
        }
    }
}
