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
