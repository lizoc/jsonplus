﻿using System;

namespace Lizoc.JsonPlus
{
    internal static class StringExtensions
    {
        public static bool NeedQuotes(this string s)
        {
            foreach (char c in s)
            {
                if (JPlusConstants.NotInUnquotedText.Contains(c))
                    return true;
            }
            return false;
        }

        public static bool NeedTripleQuotes(this string s)
        {
            return s.NeedQuotes() && 
                s.Contains(JPlusConstants.NewLineChar);
        }

        public static string TrimWhitespace(this string value)
        {
            int index = 0;
            while (JPlusConstants.Whitespace.Contains(value[index]) && index < value.Length)
            {
                index++;
            }

            string trimmed = value.Substring(index);
            index = trimmed.Length - 1;
            while (JPlusConstants.Whitespace.Contains(value[index]) && index > 0)
            {
                index--;
            }
            string res = trimmed.Substring(0, index + 1);
            return res;
        }

        public static bool IsNotInUnquoted(this char c)
        {
            return JPlusConstants.NotInUnquotedText.Contains(c);
        }

        public static bool IsJsonPlusWhitespace(this char c)
        {
            return JPlusConstants.Whitespace.Contains(c);
        }

        public static bool IsJsonPlusWhitespaceExceptNewLine(this char c)
        {
            return JPlusConstants.WhitespaceExceptNewLine.Contains(c);
        }

        public static bool IsDigit(this char c)
        {
            return JPlusConstants.AllowedDigitChars.Contains(c);
        }

        public static bool IsOctet(this char c)
        {
            return JPlusConstants.AllowedOctetChars.Contains(c);
        }

        public static bool IsHexadecimal(this char c)
        {
            return JPlusConstants.AllowedHexadecimalChars.Contains(c);
        }

        public static string AddQuotesIfRequired(this string s)
        {
            return s.NeedTripleQuotes() 
                ? (JPlusConstants.TripleQuote + s + JPlusConstants.TripleQuote)
                : s.NeedQuotes() 
                    ? (JPlusConstants.QuoteChar.ToString() + s + JPlusConstants.QuoteChar.ToString()) 
                    : s;
        }

        public static bool Contains(this string s, char c)
        {
            return s.IndexOf(c) != -1;
        }
    }
}