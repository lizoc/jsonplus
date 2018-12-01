// -----------------------------------------------------------------------
// <copyright file="KeyValueSeparator.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Lizoc.JsonPlus.Tests
{
    /// <summary>
    /// Assignment operator tests.
    /// </summary>
    public class KeyValueSeparator
    {
        /// <summary>
        /// The = character can be used anywhere JSON allows :, i.e. to separate keys from values.
        /// </summary>
        [Fact]
        public void CanParseJsonPlusWithEqualsOrColonSeparator()
        {
            var source = @"
root = {
  int = 1
  quoted-string = ""foo""
  unquoted-string = bar
  concat-string = foo bar
  object = {
    hasContent = true
  }
  array = [1,2,3,4]
  array-concat = [[1,2] [3,4]]
  array-single-element : [1 2 3 4]
  array-newline-element : [
    1
    2
    3
    4
  ]
  null : null
  double : 1.23
  bool : true
}
root_2 : 1234
";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal("1", root.GetString("root.int"));
            Assert.Equal("1.23", root.GetString("root.double"));
            Assert.True(root.GetBoolean("root.bool"));
            Assert.True(root.GetBoolean("root.object.hasContent"));
            Assert.Null(root.GetString("root.null"));
            Assert.Equal("foo", root.GetString("root.quoted-string"));
            Assert.Equal("bar", root.GetString("root.unquoted-string"));
            Assert.Equal("foo bar", root.GetString("root.concat-string"));
            Assert.True(new[] { 1, 2, 3, 4 }.SequenceEqual(root.GetInt32List("root.array")));
            Assert.True(new[] { 1, 2, 3, 4 }.SequenceEqual(root.GetInt32List("root.array-newline-element")));
            Assert.True(new[] { "1 2 3 4" }.SequenceEqual(root.GetStringList("root.array-single-element")));
            Assert.Equal("1234", root.GetString("root_2"));
        }

        /// <summary>
        /// If a key is followed by {, the : or = may be omitted. So "foo" {} means "foo" : {}
        /// </summary>
        [Fact]
        public void CanParseJsonPlusWithSeparatorForObjectFieldAssignment()
        {
            var source = @"
root {
  int = 1
}
root_2 {
  unquoted-string = bar
}
";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal("1", root.GetString("root.int"));
            Assert.Equal("bar", root.GetString("root_2.unquoted-string"));
        }

    }
}
