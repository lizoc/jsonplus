using System;
using Xunit;

namespace Lizoc.JsonPlus.Tests
{
    public class ArrayTests
    {
        [Fact]
        public void CanHaveObjectInArray()
        {
            var source = "a = [32, { b : 1 }]";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal(1, 
                root.Value.GetObject()["a"]
                    .GetArray()[1]
                    .GetObject()["b"].Value
                    .GetInt32());
        }

        [Fact]
        public void CanHaveArrayInArray()
        {
            var source = "a = [1, [ 2 ]]";
            var root = JsonPlusParser.Parse(source);

            Assert.Equal(1, root.Value.GetObject()["a"].GetArray()[0].GetValue().GetInt32());

            Assert.Equal(2, root.Value.GetObject()["a"]
                .GetArray()[1]
                .GetArray()[0].GetValue().GetInt32());
        }
    }
}
