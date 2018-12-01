// -----------------------------------------------------------------------
// <copyright file="ExampleTest.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Reflection;
using System.Linq;
using System.IO;
using Xunit;

namespace Lizoc.JsonPlus.Tests
{
    public class ExampleTest
    {
        /*
        #todo
        [Fact]
        public void CanParseExternalRefFile()
        {
            string text = GetEmbedFileContent("ExternalRef.bsd").ToString();

            // in this example we use a file resolver as the include mechanism
            // but could be replaced with e.g. a resolver for assembly resources
            Func<string, ConfonRoot> fileResolver = null;

            fileResolver = fileName =>
                {
                    var content = GetEmbedFileContent(fileName).ToString();

                    //var content = File.ReadAllText(fileName);
                    var parsed = ConfonParser.Parse(content, fileResolver);
                    return parsed;
                };

            var config = ConfonFactory.ParseString(text, fileResolver);

            var val1 = config.GetInt32("root.some-property.foo");
            var val2 = config.GetInt32("root.some-property.bar");
            var val3 = config.GetInt32("root.some-property.baz");

            Assert.Equal(123, val1);
            Assert.Equal(234, val2);
            Assert.Equal(789, val3);
        }
        */
    }
}
