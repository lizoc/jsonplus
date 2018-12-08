// -----------------------------------------------------------------------
// <copyright file="JPlusConstants.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Lizoc.JsonPlus
{
    internal static class JPlusConstants
    {
        /*
         * These are all the characters defined as whitespace by spec.
         * Json+ only recognizes NewLine as an end of line marker.
         *
         * Unicode space separator (Zs category):
         *   - Space = '\u0020';
         *   - NoBreakSpace = '\u00A0';
         *   - OghamSpaceMark = '\u1680';
         *   - EnQuad = '\u2000';
         *   - EmQuad = '\u2001';
         *   - EnSpace = '\u2002';
         *   - EmSpace = '\u2003';
         *   - ThreePerEmSpace = '\u2004';
         *   - FourPerEmSpace = '\u2005';
         *   - SixPerEmSpace = '\u2006';
         *   - FigureSpace = '\u2007';
         *   - PunctuationSpace = '\u2008';
         *   - ThinSpace = '\u2009';
         *   - HairSpace = '\u200A';
         *   - NarrowNoBreakSpace = '\u202F';
         *   - MediumMathematicalSpace = '\u205F';
         *   - IdeographicSpace = '\u3000';
         * 
         * Unicode line separator(Zl category):
         *   - LineSeparator = '\u2028';
         *
         * Unicode paragraph separator (Zp category):
         *   - ParagraphSeparator = '\u2029';
         *
         * Unicode BOM
         *   - BOM = '\uFEFF';
         *
         * Other Unicode whitespaces
         *   - Tab = '\u0009'; // \t
         *   - NewLine = '\u000A'; // \n
         *   - VerticalTab = '\u000B'; // \v
         *   - FormFeed = '\u000C'; // \f
         *   - CarriageReturn = '\u000D'; // \r
         *   - FileSeparator = '\u001C';
         *   - GroupSeparator = '\u001D';
         *   - RecordSeparator = '\u001E';
         *   - UnitSeparator = '\u001F';
         */

        public const string WhitespaceExceptNewLine =
            "\u0020\u00A0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A" +
            "\u202F\u205F\u3000\u2028\u2029\uFEFF\u0009\u000B\u000C\u000D\u001C\u001D\u001E\u001F";

        public const string Whitespace =
            "\u0020\u00A0\u1680\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A" +
            "\u202F\u205F\u3000\u2028\u2029\uFEFF\u0009\u000A\u000B\u000C\u000D\u001C\u001D\u001E\u001F";

        public const string NotInUnquotedText = "$'{}[]:=,#`^?!@*&" + "\"" + "\\";
        public const char NewLineChar = '\u000A';
        public const string NewLine = "\u000A";
        public const string AllowedOctetChars = "12345670";
        public const string AllowedDigitChars = "1234567890";
        public const string AllowedHexadecimalChars = "1234567890abcdefABCDEF";

        public const string TripleQuote = "\"\"\"";
        public const string AltTripleQuote = "'''";

        public const char QuoteChar = '\"';
        public const string Quote = "\"";
        public const char AltQuoteChar = '\'';
        public const string AltQuote = "'";

        public const char EscapeChar = '\\';
        public const string Escape = "\\";

        public const string NullKeyword = "null";
        public const char NullKeywordFirstChar = 'n';

        public const char SubstitutionFirstChar = '$';
        public const string SubstitutionOpenBrace = "${";
        public const int SubstitutionOpenBraceLength = 2;
        public const string OptionalSubstitutionOpenBrace = "${?";
        public const int OptionalSubstitutionOpenBraceLength = 3;
        // hardcoded to 1 char only
        public const string SubstitutionCloseBrace = "}";

        public const string IncludeKeyword = "include";
        public const int IncludeKeywordLength = 7;
        public const char IncludeKeywordFirstChar = 'i';
        public const string IncludeOptionalKeyword = IncludeKeyword + "?";
        public const int IncludeOptionalKeywordLength = IncludeKeywordLength + 1;
        public const char IncludeOptionalKeywordFirstChar = IncludeKeywordFirstChar;

        // open and close brackets hardcoded to 1 char only
        public const char OpenBracketChar = '(';
        public const string OpenBracket = "(";
        public const char CloseBracketChar = ')';
        public const string CloseBracket = ")";

        public const string Comment = "//";
        public const char CommentFirstChar = '/';
        public const string AltComment = "#";
        public const char AltCommentFirstChar = '#';

        public const string StartOfObject = "{";
        public const char StartOfObjectChar = '{';
        public const string EndOfObject = "}";
        public const char EndOfObjectChar = '}';

        public const string StartOfArray = "[";
        public const char StartOfArrayChar = '[';
        public const string EndOfArray = "]";
        public const char EndOfArrayChar = ']';
        public const string ArraySeparator = ",";
        public const char ArraySeparatorChar = ',';

        public const char AssignmentOperatorChar = ':';
        public const char AltAssignmentOperatorChar = '=';

        public const string SelfAssignmentOperator = "+=";
        public const char SelfAssignmentOperatorFirstChar = '+';
        public const int SelfAssignmentOperatorLength = 2;

        // Do not change these without looking at `JPlusTokenizer.PullLiteral(TokenizeResult)`
        public const string NanKeyword = "NaN";
        public const char NanKeywordFirstChar = 'N';
        public const int NanKeywordLength = 3;
        public const string InfinityKeyword = "infinity";
        public const char InfinityKeywordFirstChar = 'i';
        public const string InfinityPositiveKeyword = "+infinity";
        public const string InfinityNegativeKeyword = "-infinity";

        public const string InfiniteTimeKeyword = "infinite";

        public const string TrueKeyword = "true";
        public const char TrueKeywordFirstChar = 't';
        public const int TrueKeywordLength = 4;
        public const string FalseKeyword = "false";
        public const char FalseKeywordFirstChar = 'f';
        public const int FalseKeywordLength = 5;
        public const string AltTrueKeyword = "yes";
        public const char AltTrueKeywordFirstChar = 'y';
        public const int AltTrueKeywordLength = 3;
        public const string AltFalseKeyword = "no";
        public const char AltFalseKeywordFirstChar = 'n';
        public const int AltFalseKeywordLength = 2;
    }
}
