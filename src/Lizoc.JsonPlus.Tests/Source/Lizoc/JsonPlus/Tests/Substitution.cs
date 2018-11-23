using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Lizoc.JsonPlus.Tests
{
    /// <summary>
    /// Substitution tests.
    /// </summary>
    public class Substitution
    {
        private readonly ITestOutputHelper _output;

        public Substitution(ITestOutputHelper output)
        {
            _output = output;
        }

        /*
         * FACT:
         * The syntax is ${pathexpression} or ${?pathexpression}
         */

        /// <summary>
        /// The two characters ${ must be exactly like that, grouped together.
        /// </summary>
        [Fact]
        public void ThrowsOnInvalidSubstitutionStartToken()
        {
            var source = @"a{
    b = 1
    c = $ {a.b}
}";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        /// <summary>
        /// The ? in ${?pathexpression} must not have whitespace before it
        /// The three characters ${? must be exactly like that, grouped together.
        /// </summary>
        [Fact]
        public void ThrowsOnInvalidSubstitutionWithQuestionMarkStartToken_1()
        {
            var source = @"a{
    b = 1
    c = ${ ?a.b}
}";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsOnInvalidSubstitutionWithQuestionMarkStartToken_2()
        {
            var source = @"a{
    b = 1
    c = $ {?a.b}
}";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        /// <summary>
        /// For substitutions which are not found in the configuration tree, 
        /// implementations may try to resolve them by looking at system environment variables.
        /// </summary>
        [Fact]
        public void ShouldFallbackToEnvironmentVariables()
        {
            var source = @"a {
  b = ${MY_ENV_VAR}
}";
            var value = "Environment_Var";
            Environment.SetEnvironmentVariable("MY_ENV_VAR", value);
            try
            {
                Assert.Equal(value, JsonPlusParser.Parse(source, null, true).GetString("a.b"));
            }
            finally
            {
                Environment.SetEnvironmentVariable("MY_ENV_VAR", null);
            }
        }

        [Fact]
        public void QuestionMarkShouldFallbackToEnvironmentVariables()
        {
            var source = @"a {
  b = ${?MY_ENV_VAR}
}";
            var value = "Environment_Var";
            Environment.SetEnvironmentVariable("MY_ENV_VAR", value);
            try
            {
                Assert.Equal(value, JsonPlusParser.Parse(source, null, true).GetString("a.b"));
            }
            finally
            {
                Environment.SetEnvironmentVariable("MY_ENV_VAR", null);
            }
        }

        /*
         * FACT:
         * For substitutions which are not found in the configuration tree, 
         * implementations may try to resolve them by looking at other external sources of configuration.
         */
        // TODO: Create external config file lookup and loading implementation

        /// <summary>
        /// Substitutions are not parsed inside quoted strings.
        /// </summary>
        [Fact]
        public void DoNotParseSubstitutionInsideQuotedString()
        {
            var source = @"a{
    b = 5
    c = ""I have ${a.b} Tesla car(s).""
}";
            Assert.Equal("I have ${a.b} Tesla car(s).", JsonPlusParser.Parse(source).GetString("a.c"));
        }

        /// <summary>
        /// To get a string containing a substitution, you must use value concatenation with the substitution in the unquoted portion.
        /// </summary>
        [Fact]
        public void CanConcatenateUnquotedString()
        {
            var source = @"a {
  name = Roger
  c = Hello my name is ${a.name}
}";
            Assert.Equal("Hello my name is Roger", JsonPlusParser.Parse(source).GetString("a.c"));
        }

        /// <summary>
        /// Can use value concatenation of a substitution and a quoted string.
        /// </summary>
        [Fact]
        public void CanConcatenateQuotedString()
        {
            var source = @"a {
  name = Roger
  c = ""Hello my name is ""${a.name}
}";
            Assert.Equal("Hello my name is Roger", JsonPlusParser.Parse(source).GetString("a.c"));
        }

        /*
         * FACT:
         * Substitutions are resolved by looking up the path in the configuration. 
         * The path begins with the root configuration object, i.e. it is "absolute" rather than "relative."
         */
        // TODO: Is this test-able?

        /// <summary>
        /// Substitution processing is performed as the last parsing step, 
        /// so a substitution can look forward in the configuration. 
        /// If a configuration consists of multiple files, it may even end up retrieving a value from another file.
        /// </summary>
        [Fact]
        public void SubstitutionShouldLookForwardInConfiguration()
        {
            var source = @"a{
    b = ${a.c}
    c = 42
}";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal(42, root.GetInt32("a.b"));
        }

        [Fact]
        public void CanResolveSubstitutesInInclude()
        {
            var source = @"a {
    b { 
        include ""foo""
    }
}";
            var includeSrc = @"
x = 123
y = ${x}
";
            Task<string> IncludeCallback(IncludeSource t, string s)
                => Task.FromResult(includeSrc);

            var config = JsonPlusParser.Parse(source, IncludeCallback);

            Assert.Equal(123, config.GetInt32("a.b.x"));
            Assert.Equal(123, config.GetInt32("a.b.y"));
        }

        [Fact]
        public void CanResolveSubstitutesInNestedIncludes()
        {
            var source = @"a.b.c {
    d { 
        include ""fallback1""
    }
}";
            var includeSrc = @"
f = 123
e {
      include ""fallback2""
}";

            var includeSrc2 = @"
x = 123
y = ${x}
";

            Task<string> Include(IncludeSource t, string s)
            {
                switch (s)
                {
                    case "fallback1":
                        return Task.FromResult(includeSrc);
                    case "fallback2":
                        return Task.FromResult(includeSrc2);
                    default:
                        return Task.FromResult("{}");
                }
            }

            var root = JsonPlusParser.Parse(source, Include);

            Assert.Equal(123, root.GetInt32("a.b.c.d.e.x"));
            Assert.Equal(123, root.GetInt32("a.b.c.d.e.y"));
        }

        /*
         * FACT:
         * If a key has been specified more than once, 
         * the substitution will always evaluate to its latest-assigned value 
         * (that is, it will evaluate to the merged object, or the last non-object value that was set, 
         * in the entire document being parsed including all included files).
         */
        // TODO: Need test implementation.

        /// <summary>
        /// If a configuration sets a value to null then it should not be looked up in the external source.
        /// </summary>
        [Fact]
        public void NullValueSubstitutionShouldNotLookUpExternalSource()
        {
            var source = @"
MY_ENV_VAR = null
a {
  b = ${MY_ENV_VAR}
}";
            var value = "Environment_Var";
            Environment.SetEnvironmentVariable("MY_ENV_VAR", value);
            try
            {
                Assert.Null(JsonPlusParser.Parse(source).GetString("a.b"));
            }
            finally
            {
                Environment.SetEnvironmentVariable("MY_ENV_VAR", null);
            }
        }

        [Fact]
        public void NullValueQuestionMarkSubstitutionShouldNotLookUpExternalSource()
        {
            var source = @"
MY_ENV_VAR = null
a {
  b = ${?MY_ENV_VAR}
}";
            var value = "Environment_Var";
            Environment.SetEnvironmentVariable("MY_ENV_VAR", value);
            try
            {
                Assert.Null(JsonPlusParser.Parse(source).GetString("a.b"));
            }
            finally
            {
                Environment.SetEnvironmentVariable("MY_ENV_VAR", null);
            }
        }

        /// <summary>
        /// If a substitution does not match any value present in the configuration 
        /// and is not resolved by an external source, then it is undefined. 
        /// An undefined substitution with the ${foo} syntax is invalid and should generate an error.
        /// </summary>
        [Fact]
        public void ThrowsWhenSubstituteIsUndefined()
        {
            var source = @"a{
    b = ${foo}
}";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        /// <summary>
        /// If a substitution with the ${?foo} syntax is undefined:
        /// If it is the value of an object field then the field should not be created.
        /// </summary>
        [Fact]
        public void UndefinedQuestionMarkSubstitutionShouldNotCreateField()
        {
            var source = @"a{
    b = 1
    c = ${?foo}
}";
            var root = JsonPlusParser.Parse(source);
            Assert.False(root.HasPath("a.c"));
        }

        [Fact]
        public void UndefinedQuestionMarkShouldFailSilently()
        {
            var source = @"a {
  b = ${?a.c}
}";
            JsonPlusParser.Parse(source);
        }

        /// <summary>
        /// If a substitution with the ${?foo} syntax is undefined:
        /// If the field would have overridden a previously-set value for the same field, 
        /// then the previous value remains.
        /// </summary>
        [Fact]
        public void UndefinedQuestionMarkSubstitutionShouldNotChangeFieldValue()
        {
            var source = @"a{
    b = 2
    b = ${?foo}
}";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal(2, root.GetInt32("a.b"));
        }

        /// <summary>
        /// If a substitution with the ${?foo} syntax is undefined:
        /// If it is an array element then the element should not be added.
        /// </summary>
        [Fact]
        public void UndefinedQuestionMarkSubstitutionShouldNotAddArrayElement()
        {
            var source = @"a{
    b = [ 1, ${?foo}, 3, 4 ]
}";
            var root = JsonPlusParser.Parse(source);
            Assert.True(new[] { 1, 3, 4 }.SequenceEqual(root.GetInt32List("a.b")));
        }

        /// <summary>
        /// If a substitution with the ${?foo} syntax is undefined:
        /// if it is part of a value concatenation with another string then it should become an empty string
        /// </summary>
        [Fact]
        public void UndefinedQuestionMarkSubstitutionShouldResolveToEmptyString()
        {
            var source = @"a{
    b = My name is ${?foo}
}";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal("My name is ", root.GetString("a.b"));
        }

        /// <summary>
        /// If a substitution with the ${?foo} syntax is undefined:
        /// if part of a value concatenation with an object or array it should become an empty object or array.
        /// </summary>
        [Fact]
        public void UndefinedQuestionMarkSubstitutionShouldResolveToEmptyArray()
        {
            var source = @"a {
  c = ${?a.b} [4,5,6]
}";
            Assert.True(new[] { 4, 5, 6 }.SequenceEqual(JsonPlusParser.Parse(source).GetInt32List("a.c")));
        }

        [Fact]
        public void UndefinedQuestionMarkSubstitutionShouldResolveToEmptyObject()
        {
            var source = @"
foo : { a : 42 },
foo : ${?bar}
";

            var root = JsonPlusParser.Parse(source);
            Assert.NotNull(root.GetValue("foo"));
            Assert.Equal(42, root.GetInt32("foo.a"));
        }

        /// <summary>
        /// foo : ${?bar} would avoid creating field foo if bar is undefined.
        /// foo : ${?bar}${?baz} would also avoid creating the field if both bar and baz are undefined.
        /// </summary>
        [Fact]
        public void UndefinedQuestionMarkSubstitutionAndResolvedQuestionMarkSubstitutionShouldResolveToTheResolvedSubtitude()
        {
            var source = @"
bar : { a : 42 },
foo : ${?bar}${?baz}
";

            var root = JsonPlusParser.Parse(source);
            Assert.NotNull(root.GetValue("foo"));
            Assert.Equal(42, root.GetInt32("foo.a"));
        }

        [Fact]
        public void TwoUndefinedQuestionMarkSubstitutionShouldNotCreateField()
        {
            var source = @"a{
    foo = ${?bar}${?baz}
}";
            var root = JsonPlusParser.Parse(source);
            Assert.False(root.HasPath("a.foo"));
        }

        /// <summary>
        /// Substitutions are not allowed in keys or nested inside other substitutions (path expressions)
        /// </summary>
        [Fact]
        public void ThrowsOnSubstitutionInKeys()
        {
            var source = @"a{
    b = 1
    c = b
    ${a.c} = 2;
}";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsOnSubstitutionWithQuestionMarkInKeys()
        {
            var source = @"a{
    b = 1
    c = b
    ${?a.c} = 2;
}";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsOnSubstitutionInSubstitution()
        {
            var source = @"a{
    bar = foo
    foo = ${?a.${?bar}}
}";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void SubPathWithDotAreQuoted()
        {
            var source = @"
foo { ""bar.dada"" = 123 }
baz = ${foo.""bar.dada""}
";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal(123, root.GetInt32("baz"));
        }

        [Fact]
        public void SubPathWithDotCanUseAltQuoted()
        {
            var source = @"
foo { 'bar.d\'ada' = 123 }
baz = ${foo.'bar.d\'ada'}
";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal(123, root.GetInt32("baz"));
        }

        [Fact]
        public void SubPathWithDotAndQuoteAreEscaped()
        {
            var source = @"
foo { ""bar.da\""da"" = 123 }
baz = ${foo.""bar.da\""da""}
";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal(123, root.GetInt32("baz"));
        }

        /// <summary>
        /// A substitution is replaced with any value type (number, object, string, array, true, false, null)
        /// </summary>
        [Fact]
        public void CanAssignSubstitutionToField()
        {
            var source = @"a{
    int = 1
    number = 10.0
    string = string
    boolean = true
    null = null

    b = ${a.number}
    c = ${a.string}
    d = ${a.boolean}
    e = ${a.null}
    f = ${a.int}
}";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal(10.0, root.GetSingle("a.b"));
            Assert.Equal("string", root.GetString("a.c"));
            Assert.True(root.GetBoolean("a.d"));
            Assert.Null(root.GetString("a.e"));
            Assert.Equal(1, JsonPlusParser.Parse(source).GetInt32("a.f"));
        }

        [Fact]
        public void CanCSubstituteObject()
        {
            var source = @"a {
  b {
      foo = hello
      bar = 123
  }
  c {
     d = xyz
     e = ${a.b}
  }  
}";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal("hello", root.GetString("a.c.e.foo"));
            Assert.Equal(123, root.GetInt32("a.c.e.bar"));
        }

        [Fact]
        public void CanConcatenateArray_1()
        {
            var source = @"a {
  b = [1,2,3]
  c = ${a.b} [4,5,6]
}";
            var root = JsonPlusParser.Parse(source);
            Assert.True(new[] { 1, 2, 3, 4, 5, 6 }.SequenceEqual(root.GetInt32List("a.c")));
        }

        [Fact]
        public void CanConcatenateArray_2()
        {
            var source = @"a {
  b = [4,5,6]
  c = [1,2,3] ${a.b}
}";
            var root = JsonPlusParser.Parse(source);
            Assert.True(new[] { 1, 2, 3, 4, 5, 6 }.SequenceEqual(root.GetInt32List("a.c")));
        }

        /// <summary>
        /// If the substitution is the only part of a value, then the type is preserved. Otherwise, it is value-concatenated 
        /// to form a string.
        /// </summary>
        [Fact]
        public void FieldSubstitutionShouldPreserveType()
        {
            var source = @"a{
    b = 1
    c = ${a.b}23
}";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal(1, root.GetInt32("a.b"));
            Assert.Equal(123, root.GetInt32("a.c"));
        }

        [Fact]
        public void FieldSubstitutionWithDifferentTypesShouldConcatenateToString()
        {
            var source = @"a{
    b = 1
    c = ${a.b}foo
}";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal(1, root.GetInt32("a.b"));
            Assert.Equal("1foo", root.GetString("a.c"));
        }
    }
}
