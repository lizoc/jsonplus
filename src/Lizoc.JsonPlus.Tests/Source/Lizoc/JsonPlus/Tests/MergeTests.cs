using System;
using Xunit;
using Lizoc.JsonPlus;

namespace Lizoc.JsonPlus.Tests
{
    public class MergeTests
    {
        [Fact]
        public void CanParseObjectMergeFile()
        {
            var ctx = ResUtility.GetEmbed("ObjectMerge.jsonp");

            var val1 = ctx.GetString("root.some-object.property1");
            var val2 = ctx.GetString("root.some-object.property2");
            var val3 = ctx.GetString("root.some-object.property3");

            Assert.Equal("123", val1);
            Assert.Equal("456", val2);
            Assert.Equal("789", val3);
        }
    }
}
