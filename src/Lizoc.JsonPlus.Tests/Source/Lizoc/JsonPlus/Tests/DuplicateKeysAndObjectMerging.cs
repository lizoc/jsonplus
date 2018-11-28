// -----------------------------------------------------------------------
// <copyright file="DuplicateKeysAndObjectMerging.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code documented in this file is subject to the MIT license.
//     See the LICENSE file in the project root for more information.
// </copyright>
// -----------------------------------------------------------------------
 
using System;
using Xunit;

namespace Lizoc.JsonPlus.Tests
{
    /// <summary>
    /// Key dup tests.
    /// </summary>
    public class DuplicateKeysAndObjectMerging
    {
        /// <summary>
        /// Duplicate keys that appear later override those that appear earlier, unless both values are objects
        /// </summary>
        [Fact]
        public void CanOverrideLiteral()
        {
            var source = @"
foo : literal
foo : 42
";
            var root = JsonPlusParser.Parse(source);
            Assert.Equal(42, root.GetInt32("foo"));
        }

        /*
         * If both values are objects, then the objects are merged.
         */

        /// <summary>
        /// Add fields present in only one of the two objects to the merged object.
        /// </summary>
        [Fact]
        public void CanMergeObject_DifferentFields()
        {
            var source = @"
foo : { a : 42 },
foo : { b : 43 }
";

            var root = JsonPlusParser.Parse(source);
            Assert.Equal(42, root.GetInt32("foo.a"));
            Assert.Equal(43, root.GetInt32("foo.b"));
        }

        /// <summary>
        /// For non-object-valued fields present in both objects, the field found in the second object must be used.
        /// </summary>
        [Fact]
        public void CanMergeObject_SameField()
        {
            var source = @"
foo : { a : 42 },
foo : { a : 43 }
";

            var root = JsonPlusParser.Parse(source);
            Assert.Equal(43, root.GetInt32("foo.a"));
        }

        /// <summary>
        /// For object-valued fields present in both objects, the object values should be recursively merged according 
        /// to these same rules.
        /// </summary>
        [Fact]
        public void CanMergeObject_RecursiveMerging()
        {
            var source = @"
foo
{
  bar 
  {
    a : 42
    b : 43 
  }
},
foo
{ 
  bar
  {
    b : 44
    c : 45
    baz
    {
      a : 9000
    }
  }
}
";

            var root = JsonPlusParser.Parse(source);
            Assert.Equal(42, root.GetInt32("foo.bar.a"));
            Assert.Equal(44, root.GetInt32("foo.bar.b"));
            Assert.Equal(45, root.GetInt32("foo.bar.c"));
            Assert.Equal(9000, root.GetInt32("foo.bar.baz.a"));
        }

        /// <summary>
        /// Assigning an object field to a literal and then to another object would prevent merging.
        /// </summary>
        [Fact]
        public void CanOverrideObject()
        {
            var source = @"
{
    foo : { a : 42 },
    foo : null,
    foo : { b : 43 }
}";
            var root = JsonPlusParser.Parse(source);
            Assert.False(root.HasPath("foo.a"));
            Assert.Equal(43, root.GetInt32("foo.b"));
        }

    }
}
