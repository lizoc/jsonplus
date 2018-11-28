// -----------------------------------------------------------------------
// <copyright file="DataUnitTests.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code documented in this file is subject to the MIT license.
//     See the LICENSE file in the project root for more information.
// </copyright>
// -----------------------------------------------------------------------
 
using System;
using Xunit;

namespace Lizoc.JsonPlus.Tests
{
    public class DataUnitTests
    {
        [Theory]
        // null
        //[InlineData("", null)]
        // kb
        [InlineData("11kB", 11L * 1000L)]
        [InlineData("11kb", 11L * 1024L)]
        // mb
        [InlineData("11mB", 11L * 1000L * 1000L)]
        [InlineData("11mb", 11L * 1024L * 1024L)]
        // gb
        [InlineData("11gB", 11L * 1000L * 1000L * 1000L)]
        [InlineData("11gb", 11L * 1024L * 1024L * 1024L)]
        // tb
        [InlineData("11tB", 11L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11tb", 11L * 1024L * 1024L * 1024L * 1024L)]
        // pb
        [InlineData("11pB", 11L * 1000L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11pb", 11L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        public void CanParseByteSize(string value, long? expected)
        {
            var source = string.Format("byte-size = {0}", value);
            var root = JsonPlusParser.Parse(source);
            var actual = root.GetByteSize("byte-size");

            if (expected.HasValue)
                Assert.Equal(expected, actual);
            else
                Assert.Null(actual);
        }

        [Fact]
        public void CanParseNanoseconds()
        {
            var expected = TimeSpan.FromTicks(12);
            var value = "1234ns";
            var source = $"timespan = {value}";

            var res = JsonPlusParser.Parse(source).GetTimeSpan("timespan");
            Assert.Equal(expected, res);
        }

        [Fact]
        public void CanParseMicroseconds()
        {
            var expected = TimeSpan.FromTicks((long)Math.Round(TimeSpan.TicksPerMillisecond * 0.123));
            var value = "123us";
            var source = $"timespan = {value}";

            var res = JsonPlusParser.Parse(source).GetTimeSpan("timespan");
            Assert.Equal(expected, res);
        }
    }
}
