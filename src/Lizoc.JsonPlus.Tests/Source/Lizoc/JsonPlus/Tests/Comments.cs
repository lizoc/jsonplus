using System;
using Xunit;

namespace Lizoc.JsonPlus.Tests
{
    /// <summary>
    /// Tests on comments.
    /// </summary>
    public class Comments
    {
        /// <summary>
        /// Anything between // or # and the next newline is considered a comment and ignored, 
        /// unless the // or # is inside a quoted string.
        /// </summary>
        [Fact]
        public void CommentsShouldBeIgnored()
        {
            var source = @"a = 1
// This should be ignored
b = 2 // This should be ignored
# This should be ignored
c = 3 # This should be ignored";
            Assert.Equal("2", JsonPlusParser.Parse(source).GetString("b"));
            Assert.Equal("3", JsonPlusParser.Parse(source).GetString("c"));
        }

        [Fact]
        public void DoubleSlashOrPoundsInQuotedTextAreNotIgnored()
        {
            var source = @"a = 1
b = ""2 // This should not be ignored"" 
c = ""3 # This should not be ignored"" 
";
            Assert.Equal("2 // This should not be ignored", JsonPlusParser.Parse(source).GetString("b"));
            Assert.Equal("3 # This should not be ignored", JsonPlusParser.Parse(source).GetString("c"));
        }
    }
}
