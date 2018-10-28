using System;
using Xunit;

namespace Lizoc.JsonPlus.Tests
{
    public class DataUnitTests
    {
        [Theory]
        // null
        //[InlineData("", null)]
        // byte
        [InlineData("11byte", 11L)]
        [InlineData("11bytes", 11L)]
        [InlineData("11 byte", 11L)]
        [InlineData("11 bytes", 11L)]
        // kb
        [InlineData("11kB", 11L * 1000L)]
        [InlineData("11kilobyte", 11L * 1000L)]
        [InlineData("11kilobytes", 11L * 1000L)]
        [InlineData("11kb", 11L * 1024L)]
        [InlineData("11kibibyte", 11L * 1024L)]
        [InlineData("11kibibytes", 11L * 1024L)]
        [InlineData("11 kB", 11L * 1000L)]
        [InlineData("11 kilobyte", 11L * 1000L)]
        [InlineData("11 kilobytes", 11L * 1000L)]
        [InlineData("11 kb", 11L * 1024L)]
        [InlineData("11 kibibyte", 11L * 1024L)]
        [InlineData("11 kibibytes", 11L * 1024L)]
        // mb
        [InlineData("11mB", 11L * 1000L * 1000L)]
        [InlineData("11megabyte", 11L * 1000L * 1000L)]
        [InlineData("11megabytes", 11L * 1000L * 1000L)]
        [InlineData("11mb", 11L * 1024L * 1024L)]
        [InlineData("11mebibyte", 11L * 1024L * 1024L)]
        [InlineData("11mebibytes", 11L * 1024L * 1024L)]
        [InlineData("11 mB", 11L * 1000L * 1000L)]
        [InlineData("11 megabyte", 11L * 1000L * 1000L)]
        [InlineData("11 megabytes", 11L * 1000L * 1000L)]
        [InlineData("11 mb", 11L * 1024L * 1024L)]
        [InlineData("11 mebibyte", 11L * 1024L * 1024L)]
        [InlineData("11 mebibytes", 11L * 1024L * 1024L)]
        // gb
        [InlineData("11gB", 11L * 1000L * 1000L * 1000L)]
        [InlineData("11gigabyte", 11L * 1000L * 1000L * 1000L)]
        [InlineData("11gigabytes", 11L * 1000L * 1000L * 1000L)]
        [InlineData("11gb", 11L * 1024L * 1024L * 1024L)]
        [InlineData("11gibibyte", 11L * 1024L * 1024L * 1024L)]
        [InlineData("11gibibytes", 11L * 1024L * 1024L * 1024L)]
        [InlineData("11 gB", 11L * 1000L * 1000L * 1000L)]
        [InlineData("11 gigabyte", 11L * 1000L * 1000L * 1000L)]
        [InlineData("11 gigabytes", 11L * 1000L * 1000L * 1000L)]
        [InlineData("11 gb", 11L * 1024L * 1024L * 1024L)]
        [InlineData("11 gibibyte", 11L * 1024L * 1024L * 1024L)]
        [InlineData("11 gibibytes", 11L * 1024L * 1024L * 1024L)]
        // tb
        [InlineData("11tB", 11L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11terabyte", 11L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11terabytes", 11L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11tb", 11L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11tebibyte", 11L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11tebibytes", 11L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11 tB", 11L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11 terabyte", 11L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11 terabytes", 11L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11 tb", 11L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11 tebibyte", 11L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11 tebibytes", 11L * 1024L * 1024L * 1024L * 1024L)]
        // pb
        [InlineData("11pB", 11L * 1000L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11petabyte", 11L * 1000L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11petabytes", 11L * 1000L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11pb", 11L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11pebibyte", 11L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11pebibytes", 11L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11 pB", 11L * 1000L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11 petabyte", 11L * 1000L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11 petabytes", 11L * 1000L * 1000L * 1000L * 1000L * 1000L)]
        [InlineData("11 pb", 11L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11 pebibyte", 11L * 1024L * 1024L * 1024L * 1024L * 1024L)]
        [InlineData("11 pebibytes", 11L * 1024L * 1024L * 1024L * 1024L * 1024L)]
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
            var value = "1234 ns";
            var source = $"timespan = {value}";

            var res = JsonPlusParser.Parse(source).GetTimeSpan("timespan");
            Assert.Equal(expected, res);
        }

        [Fact]
        public void CanParseMicroseconds()
        {
            var expected = TimeSpan.FromTicks((long)Math.Round(TimeSpan.TicksPerMillisecond * 0.123));
            var value = "123 microseconds";
            var source = $"timespan = {value}";

            var res = JsonPlusParser.Parse(source).GetTimeSpan("timespan");
            Assert.Equal(expected, res);
        }
    }
}
