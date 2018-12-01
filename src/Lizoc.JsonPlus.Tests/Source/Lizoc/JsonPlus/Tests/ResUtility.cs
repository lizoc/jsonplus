// -----------------------------------------------------------------------
// <copyright file="ResUtility.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;

namespace Lizoc.JsonPlus.Tests
{
    internal static class ResUtility
    {
        /// <summary>
        /// Parse Json+ source code embedded in an assembly resource.
        /// </summary>
        /// <param name="resourceName">The name of the resource that contains the source text.</param>
        /// <param name="assembly">The assembly that contains the given resource.</param>
        /// <returns>The <see cref="JsonPlusRoot"/> obtained by parsing the source code embedded in the assembly as a resource.</returns>
        public static string GetResource(string resourceName, Assembly assembly)
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Parse Json+ source code embedded in an assembly resource.
        /// </summary>
        /// <typeparam name="TAssembly">A type which is defined in the assembly that contains the Json+ source code.</typeparam>
        /// <param name="resourceName">The name of the resource that contains the source code.</param>
        /// <returns>The <see cref="JsonPlusRoot"/> obtained by parsing the source code embedded in the assembly as a resource.</returns>
        public static string GetResource<TAssembly>(string resourceName)
        {
            return GetResource(resourceName, GetAssemblyByType(typeof(TAssembly)));
        }

        /// <summary>
        /// Parse Json+ source code embedded in a resource of the assembly that contains the instance object specified.
        /// </summary>
        /// <param name="resourceName">The name of the resource that contains the source code.</param>
        /// <param name="instanceInAssembly">An instance of the type defined in the assembly that contains the Json+ source code.</param>
        /// <returns>The <see cref="JsonPlusRoot"/> obtained by parsing the source code embedded in the assembly 
        /// that contains the type definition for <paramref name="instanceInAssembly"/>.</returns>
        public static string GetResource(string resourceName, object instanceInAssembly)
        {
            if (instanceInAssembly is Type t)
                return GetResource(resourceName, GetAssemblyByType(t));

            if (instanceInAssembly is Assembly a)
                return GetResource(resourceName, a);

            return GetResource(resourceName, GetAssemblyByType(instanceInAssembly.GetType()));
        }

        /// <summary>
        /// Retrieves a Json+ source text defined in the current executing assembly.
        /// </summary>
        /// <param name="resourceName">The name of the resource that contains the source code.</param>
        /// <returns>The <see cref="JsonPlusRoot"/> obtained by parsing the source code defined in the current 
        /// executing assembly.</returns>
        internal static string GetResource(string resourceName)
        {
#if NETSTANDARD
            Assembly assembly = typeof(JsonPlusParser).GetTypeInfo().Assembly;
#else
            Assembly assembly = Assembly.GetExecutingAssembly();
#endif

            return GetResource(resourceName, assembly);
        }

        private static Assembly GetAssemblyByType(Type type)
        {
#if NETSTANDARD
            return type.GetTypeInfo().Assembly;
#else
            return type.Assembly;
#endif           
        }

        public static string GetEmbedString(string fileName)
        {
            string resourceName = string.Format("Lizoc.JsonPlus.Tests.Resource.{0}", fileName);
            return GetResource<WhitespaceDef>(resourceName);
        }

        public static JsonPlusRoot GetEmbed(string fileName)
        {
            return JsonPlusParser.Parse(GetEmbedString(fileName));
        }
    }
}
