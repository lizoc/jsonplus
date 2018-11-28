// -----------------------------------------------------------------------
// <copyright file="SelfReferentialSubstitution.cs" repo="Json+">
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
    /// Self assignment tests.
    /// </summary>
    public class SelfReferentialSubstitution
    {
        private readonly ITestOutputHelper _output;

        public SelfReferentialSubstitution(ITestOutputHelper output)
        {
            _output = output;
        }

        /// <summary>
        /// Allow a new value for a field to be based on the older value.
        /// </summary>
        [Fact]
        public void CanValueConcatenateOlderValue()
        {
            var source = @"
path : ""a:b:c""
path : ${path}"":d""";

            var root = JsonPlusParser.Parse(source);
            Assert.Equal("a:b:c:d", root.GetString("path"));
        }

        [Fact]
        public void CanValueConcatenateOlderArray()
        {
            var source = @"
path : [ /usr/etc, /usr/home ]
path : ${path} [ /usr/bin ]";

            var root = JsonPlusParser.Parse(source);
            Assert.True(new []{"/usr/etc", "/usr/home", "/usr/bin"}.SequenceEqual(root.GetStringList("path")));
        }

        /// <summary>
        /// In isolation (with no merges involved), a self-referential field is an error because the substitution 
        /// cannot be resolved.
        /// </summary>
        [Fact]
        public void ThrowsWhenThereAreNoOldValue()
        {
            var source = "foo : ${foo}";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        /// <summary>
        /// When merging two objects, the self-reference in the overriding field refers to the overridden field.
        /// </summary>
        [Fact]
        public void CanReferToOverriddenField()
        {
            var source = @"
foo : { a : 1 }
foo : ${foo}";

            var root = JsonPlusParser.Parse(source);
            Assert.Equal("1", root.GetString("foo.a"));
        }

        /// <summary>
        /// It would be an error if these two fields were reversed
        /// </summary>
        [Fact]
        public void ThrowsWhenThereAreNoOverriddenValueAvailable()
        {
            var source = @"
foo : ${foo}
foo : { a : 1 }
";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        /// <summary>
        /// The optional substitution syntax ${?foo} does not create a cycle
        /// </summary>
        [Fact]
        public void OptionalSubstitutionCycleShouldBeIgnored()
        {
            var source = "foo : ${?foo}";

            JsonPlusRoot root = null;
            var ex = Record.Exception(() => root = JsonPlusParser.Parse(source));
            Assert.Null(ex);
            // should not create a field
            Assert.False(root.HasPath("foo"));
        }

        /// <summary>
        /// The optional substitution syntax ${?foo} does not create a cycle
        /// </summary>
        [Fact]
        public void HiddenSubstitutionShouldNeverBeEvaluated()
        {
            var source = @"
foo : ${does-not-exist}
foo : 42";

            JsonPlusRoot root = null;
            var ex = Record.Exception(() => root = JsonPlusParser.Parse(source));
            Assert.Null(ex);
            Assert.Equal(42, root.GetInt32("foo"));
        }

        /// <summary>
        /// Fields may have += as a separator rather than : or =. A field with += transforms into an optional self-referential 
        /// array concatenation {a += b} becomes {a = ${?a} [b]}
        /// </summary>
        [Fact]
        public void PlusEqualOperatorShouldExpandToSelfReferencingArrayConcatenation()
        {
            var source = @"
a = [ 1, 2 ]
a += 3
a += ${b}
b = [ 4, 5 ]
";

            JsonPlusRoot root = null;
            var ex = Record.Exception(() => root = JsonPlusParser.Parse(source));
            Assert.Null(ex);
            Assert.True( new []{1, 2, 3, 4, 5}.SequenceEqual(root.GetInt32List("a")) );
        }

        /// <summary>
        /// A self-reference resolves to the value "below" even if it's part of a path expression.
        /// Here, ${foo.a} would refer to { c : 1 } rather than 2 and so the final merge would be { a : 2, c : 1 }
        /// </summary>
        [Fact]
        public void MergedSubstitutionShouldAlwaysResolveToOlderValue()
        {
            var source = @"
foo : { a : { c : 1 } }
foo : ${foo.a}
foo : { a : 2 }";

            JsonPlusRoot root = null;
            var ex = Record.Exception(() => root = JsonPlusParser.Parse(source));
            Assert.Null(ex);

            Assert.Equal(2, root.GetInt32("foo.a"));
            Assert.Equal(1, root.GetInt32("foo.c"));
            Assert.False(root.HasPath("foo.a.c"));
        }

        /// <summary>
        /// Implementations must be careful to allow objects to refer to paths within themselves.
        /// The test below is NOT a self reference nor a cycle, the final value for bar.foo 
        /// and bar.baz should be 43 (forward checking)
        /// </summary>
        [Fact]
        public void SubstitutionToAnotherMemberOfTheSameObjectAreResolvedNormally()
        {
            var source = @"
bar : { foo : 42,
        baz : ${bar.foo}
      }
bar : { foo : 43 }";

            JsonPlusRoot root = null;
            var ex = Record.Exception(() => root = JsonPlusParser.Parse(source));
            Assert.Null(ex);

            Assert.Equal(43, root.GetInt32("bar.foo"));
            Assert.Equal(43, root.GetInt32("bar.baz"));
        }

        /// <summary>
        /// Mutually-referring objects should also work, and are not self-referential
        /// </summary>
        [Fact]
        public void MutuallyReferringObjectsAreResolvedNormally()
        {
            var source = @"
// bar.a should end up as 4
bar : { a : ${foo.d}, b : 1 }
bar.b = 3
// foo.c should end up as 3
foo : { c : ${bar.b}, d : 2 }
foo.d = 4";

            JsonPlusRoot root = null;
            var ex = Record.Exception(() => root = JsonPlusParser.Parse(source));
            Assert.Null(ex);

            Assert.Equal(4, root.GetInt32("bar.a"));
            Assert.Equal(3, root.GetInt32("foo.c"));
        }

        /// <summary>
        /// Value concatenated optional substitution should be ignored and dropped silently
        /// </summary>
        [Fact]
        public void SelfReferenceOptionalSubstitutionInValueConcatenationShouldBeIgnored()
        {
            var source = "a = ${?a}foo";

            JsonPlusRoot root = null;
            var ex = Record.Exception(() => root = JsonPlusParser.Parse(source));
            Assert.Null(ex);

            Assert.Equal("foo", root.GetString("a"));
        }

        /// <summary>
        /// A cyclic or circular loop substitution should be detected as invalid.
        /// </summary>
        [Fact]
        public void ThrowsOnCyclicSubstitutionDetection_1()
        {
            var source = @"
bar : ${foo}
foo : ${bar}";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsOnCyclicSubstitutionDetection_2()
        {
            var source = @"
a : ${b}
b : ${c}
c : ${a}";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

        [Fact]
        public void ThrowsOnCyclicSubstitutionDetection_3()
        {
            var source = @"
a : 1
b : 2
a : ${b}
b : ${a}";

            var ex = Record.Exception(() => JsonPlusParser.Parse(source));
            Assert.NotNull(ex);
            Assert.IsType<JsonPlusParserException>(ex);
            _output.WriteLine($"Exception message: {ex.Message}");
        }

    }
}
