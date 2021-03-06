﻿// -----------------------------------------------------------------------
// <copyright file="IncludeTests.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Lizoc.JsonPlus.Tests
{
    public class IncludeTests
    {
        [Fact]
        public void CanParseInclude()
        {
            var source = @"a {
    include ""foo""
    b : include ""foo""
}";
            var includeSrc = @"
x = 123
y = hello
";
            Task<string> IncludeCallback(string s)
                => Task.FromResult(includeSrc);

            var root = JsonPlusParser.Parse(source, IncludeCallback);

            Assert.Equal(123, root.GetInt32("a.x"));
            Assert.Equal("hello", root.GetString("a.y"));
            Assert.Equal(123, root.GetInt32("a.b.x"));
            Assert.Equal("hello", root.GetString("a.b.y"));
        }

        [Fact]
        public void CanParseArrayInclude()
        {
            var source = @"a : include ""foo""";
            var includeSrc = @"[1, 2, 3]";

            Task<string> IncludeCallback(string s)
                => Task.FromResult(includeSrc);

            var config = JsonPlusParser.Parse(source, IncludeCallback);
            Assert.True(new[] { 1, 2, 3 }.SequenceEqual(config.GetInt32List("a")));
        }

        [Fact]
        public void CanParseArrayIncludeInsideArray()
        {
            var source = @"a : [ include ""foo"" ]";
            var includeSrc = @"[1, 2, 3]";

            Task<string> IncludeCallback(string s)
                => Task.FromResult(includeSrc);

            var config = JsonPlusParser.Parse(source, IncludeCallback);
            // TODO: need to figure a better way to retrieve array inside array
            var array = config.GetValue("a").GetArray()[0].GetArray().Select(v => v.GetValue().GetInt32());
            Assert.True(new[] { 1, 2, 3 }.SequenceEqual(array));
        }

        [Fact]
        public void CanIncludeAnywhereAtRoot()
        {
            // inserting things between the 2 includes or before the first include
            var source = @"
hello = world
include ""foo""
banana = true
include ""bar""
cat = meow
";
            var includeSrc = @"a = 32";
            var includeSrc2 = @"b { foo = bar }";

            Task<string> IncludeCallback(string s)
                => Task.FromResult(s == "foo" ? includeSrc : includeSrc2);

            var config = JsonPlusParser.Parse(source, IncludeCallback);
            Assert.Equal(32, config.GetInt32("a"));
            Assert.Equal("bar", config.GetString("b.foo"));
            Assert.Equal("meow", config.GetString("cat"));
        }

        [Fact]
        public void CanParseIncludeAtRoot()
        {
            var source = @"
include ""foo""
a : include ""foo""
";
            var includeSrc = @"
x = 123
y = hello
";
            Task<string> includeCallback(string path)
                => Task.FromResult(includeSrc);

            var config = JsonPlusParser.Parse(source, includeCallback);
            Assert.Equal(123, config.GetInt32("x"));
            Assert.Equal("hello", config.GetString("y"));
            Assert.Equal(123, config.GetInt32("a.x"));
            Assert.Equal("hello", config.GetString("a.y"));
        }

        [Fact]
        public void CanIncludeFromAssembly()
        {
            var source = @"a { include ""dll://Hello.jsonp"" }";

            Task<string> includeAssemblyCallback(string path)
            {
                if (string.IsNullOrEmpty(path))
                    return Task.FromResult("{}");

                if (path.StartsWith("dll://"))
                    return Task.FromResult(ResUtility.GetEmbedString(path.Substring("dll://".Length)));

                return Task.FromResult("{}");
            }

            var config = JsonPlusParser.Parse(source, includeAssemblyCallback);

            Assert.Equal("Hello world", config.GetString("a.root.simple-string"));
        }

        [Fact]
        public void OptionalIncludeSuppressError()
        {
            Task<string> includeCallback(string path)
                => Task.FromResult(string.Empty);

            var source = @"
include ""foo""
";
            Assert.Throws<JsonPlusParserException>(() => JsonPlusParser.Parse(source, includeCallback));

            var source2 = @"
include? ""foo""
";

            var config = JsonPlusParser.Parse(source2, includeCallback);
            Assert.True(config.IsEmpty);
        }
    }
}
