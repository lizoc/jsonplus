// -----------------------------------------------------------------------
// <copyright file="PathTests.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Lizoc.JsonPlus.Tests
{
    /// <summary>
    /// Tests on <see cref="JsonPlusPath"/>.
    /// </summary>
    public class PathTests
    {
        private readonly ITestOutputHelper _output;

        public PathTests(ITestOutputHelper output)
        {
            _output = output;
        }

        /// <summary>
        /// Convert string array to <see cref="JsonPlusPath" /> and back.
        /// </summary>
        /// <remarks><![CDATA[
        /// - foo, bar  =>  foo.bar
        /// - foo, bar.baz  =>  foo."bar.baz"
        /// - shoot, a "laser" beam  =>  shoot."a \"laser\" beam"
        /// - foo, bar`\n`baz  =>  foo."bar\nbaz"
        /// - foo, bar baz, ` `wis` `  =>  foo."bar baz"." wis "
        /// - foo, bar`\t`baz, foo.bar`t`baz  =>  foo."bar\tbaz"."foo.bar\tbaz"
        /// - foo, bar`\r\n`baz, x`\r`  =>  foo."bar\r\nbaz"."x\r"
        /// - foo, ``  =>  foo.""
        /// - foo, \  =>  foo."\\"
        /// - $"{}[]:=,#`^?!@*&\  =>  "$\"{}[]:=,#`^?!@*&\\"
        /// ]]></remarks>
        [Theory]
        [InlineData(new string[] { "foo", "bar" }, "foo.bar")]
        [InlineData(new string[] { "foo", "bar.baz" }, "foo.\"bar.baz\"")]
        [InlineData(new string[] { "shoot", "a \"laser\" beam" }, "shoot.\"a \\\"laser\\\" beam\"")]
        [InlineData(new string[] { "foo", "bar\nbaz" }, "foo.\"bar\\nbaz\"")]
        [InlineData(new string[] { "foo", "bar baz", " wis " }, "foo.\"bar baz\".\" wis \"")]
        [InlineData(new string[] { "foo", "bar\tbaz"}, "foo.\"bar\tbaz\"")]
        [InlineData(new string[] { "foo", "bar\r\nbaz", "x\r" }, "foo.\"bar\r\\nbaz\".\"x\r\"")]
        [InlineData(new string[] { "foo", "" }, "foo.\"\"")]
        [InlineData(new string[] { "foo", "\\" }, "foo.\"\\\\\"")]
        [InlineData(new string[] { "$\"{}[]:=,#`^?!@*&\\" }, "\"" + "$\\\"{}[]:=,#`^?!@*&\\\\" + "\"")]
        public void CanParseAndSerialize(string[] pathKeys, string path)
        {
            JsonPlusPath jpPath = new JsonPlusPath(pathKeys);
            JsonPlusPath jpPath2 = JsonPlusPath.Parse(path);

            // JsonPlusPath object created from constructor and `Parse` method should 
            // serialize to the same value.
            Assert.Equal(path, jpPath.Value);
            Assert.Equal(path, jpPath2.Value);

            // JsonPlusPath object created from constructor and `Parse` method should 
            // be equal (using equality comparer).
            Assert.Equal(jpPath, jpPath2);

            // JsonPlusPath object created from constructor and `Parse` method should 
            // have the same number of keys
            Assert.Equal(pathKeys.Length, jpPath.Count);
            Assert.Equal(pathKeys.Length, jpPath2.Count);

            // assert each key in JsonPlusPath objects created
            for (int i = 0; i < pathKeys.Length; i++)
            {
                Assert.Equal(pathKeys[i], jpPath[i]);
                Assert.Equal(pathKeys[i], jpPath2[i]);
            }
            
            _output.WriteLine(string.Format("Path [{0}] serialized from: {1}", path, string.Join(", ", pathKeys)));
        }

        [Fact]
        public void PathToStringQuoteKeysContainingDot()
        {
            var path1 = new JsonPlusPath(new string[]
            {
                "kong.fu",
                "panda"
            });

            var path2 = new JsonPlusPath(new string[]
            {
                "kong",
                "fu",
                "panda"
            });

            Assert.NotEqual(path1.Value, path2.Value);

            // "kong.fu".panda
            Assert.Equal("\"kong.fu\".panda", path1.Value);
            // kong.fu.panda
            Assert.Equal("kong.fu.panda", path2.Value);
        }
    }
}
