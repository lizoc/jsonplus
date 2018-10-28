using System;
using Xunit;
using Lizoc.JsonPlus;

namespace Lizoc.JsonPlus.Tests
{
    public class HelloTests
    {
        [Fact]
        public void CanParseHelloFile()
        {
            var ctx = ResUtility.GetEmbed("Hello.jsonp");
            var val = ctx.GetString("root.simple-string");
            Assert.Equal("Hello world", val);
        }
    }
}
