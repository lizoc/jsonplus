using System;
using Xunit;

namespace Lizoc.JsonPlus.Tests
{
    public class SimpleSubstitutionTests
    {
        [Fact]
        public void CanParseSimpleSubstitutionFile()
        {
            JsonPlusRoot ctx = ResUtility.GetEmbed("SimpleSub.jsonp");
            var val = ctx.GetString("root.simple-string");
            Assert.Equal("Hello world", val);
        }
    }
}
