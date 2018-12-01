// -----------------------------------------------------------------------
// <copyright file="SimpleSubstitutionTests.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

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
