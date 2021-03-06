﻿// -----------------------------------------------------------------------
// <copyright file="JPlusInternalExtensions.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics;

namespace Lizoc.JsonPlus
{
    internal static class JPlusInternalExtensions
    {
        public static JsonPlusPath ToJsonPlusPath(this string path)
        {
            return JsonPlusPath.Parse(path);
        }

        [DebuggerStepThrough]
        public static bool IsSignificant(this Token token)
        {
            return token.Type != TokenType.Comment &&
               token.Type != TokenType.EndOfLine &&
               token.LiteralType != LiteralTokenType.Whitespace;
        }

        [DebuggerStepThrough]
        public static bool IsNonSignificant(this Token token)
        {
            return !token.IsSignificant();
        }

        public static bool IsSubstitution(this IJsonPlusNode value)
        {
            switch (value)
            {
                case JsonPlusValue v:
                    foreach (IJsonPlusNode val in v)
                    {
                        if (val is JsonPlusSubstitution)
                            return true;
                    }
                    return false;

                case JsonPlusObjectMember f:
                    foreach (IJsonPlusNode v in f.Value)
                    {
                        if (v is JsonPlusSubstitution)
                            return true;
                    }
                    return false;

                case JsonPlusSubstitution _:
                    return true;

                default:
                    return false;
            }
        }
    }
}
