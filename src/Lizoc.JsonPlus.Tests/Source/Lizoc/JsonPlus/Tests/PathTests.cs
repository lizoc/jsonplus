// -----------------------------------------------------------------------
// <copyright file="PathTests.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code documented in this file is subject to the MIT license.
//     See the LICENSE file in the project root for more information.
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

        [Fact]
        public void PathToStringAutoQuoteKeysIfRequired()
        {
            var path1 = new JsonPlusPath(new string[]
            {
                "i am",
                "kong.fu",
                "panda"
            });

            var path2 = new JsonPlusPath(new string[]
            {
                "i am",
                "kong",
                "fu",
                "panda"
            });

            Assert.NotEqual(path1.Value, path2.Value);

            // "i am"."kong.fu".panda
            Assert.Equal("\"i am\".\"kong.fu\".panda", path1.Value);

            // "i am".kong.fu.panda
            Assert.Equal("\"i am\".kong.fu.panda", path2.Value);
        }

        [Fact]
        public void PathToStringAutoQuoteAndEscapeKeysIfRequired()
        {
            var path = new JsonPlusPath(new string[]
            {
                "i\"m",
                "panda"
            });

            // "i\"m".panda
            Assert.Equal("\"i\\\"m\".panda", path.Value);
        }
    }
}
