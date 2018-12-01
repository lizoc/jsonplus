// -----------------------------------------------------------------------
// <copyright file="ResUtility.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Reflection;
using Lizoc.JsonPlus;

namespace Lizoc.JsonPlus.Tests
{
    internal static class ResUtility
    {
        public static string GetEmbedString(string fileName)
        {
            string resourceName = string.Format("Lizoc.JsonPlus.Tests.Resource.{0}", fileName);
            return JsonPlusUtility.GetResource<WhitespaceDef>(resourceName);
        }

        public static JsonPlusRoot GetEmbed(string fileName)
        {
            return JsonPlusParser.Parse(GetEmbedString(fileName));
        }
    }
}
