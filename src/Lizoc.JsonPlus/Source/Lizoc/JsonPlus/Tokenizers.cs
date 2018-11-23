using System;
using System.Collections.Generic;
using System.Text;

namespace Lizoc.JsonPlus
{
    /// <summary>
    /// This class contains methods used to tokenize a string.
    /// </summary>
    internal abstract class Tokenizer : ISourceLocation
    {
        private readonly Stack<int> _indexStack = new Stack<int>();

        private readonly string _text;
        private int _index;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tokenizer"/> class.
        /// </summary>
        /// <param name="source">The string that contains the text to tokenize.</param>
        protected Tokenizer(string source)
        {
            _text = source;
        }

        public int Length
        {
            get { return _text.Length; }
        }

        public int Index
        {
            get
            {
                return _index;
            }
            private set
            {
                _index = value;
                if (_index > _text.Length)
                    _index = _text.Length;
            }
        }

        public int Line { get; private set; } = 1;

        public int Column { get; private set; } = 1;

        /// <summary>
        /// A value indicating whether the tokenizer has reached the end of the string.
        /// </summary>
        protected bool EndOfFile
        {
            get { return Index >= _text.Length; }
        }

        /// <summary>
        /// Retrieves the next character in the tokenizer without advancing its position.
        /// </summary>
        /// <returns>The character at the tokenizer's current position.</returns>
        protected char Peek
        {
            get { return EndOfFile ? (char)0 : _text[Index]; }
        }

        protected void PushIndex()
        {
            _indexStack.Push(Index);
        }

        protected void ResetIndex()
        {
            Index = _indexStack.Pop();
        }

        protected void PopIndex()
        {
            _indexStack.Pop();
        }

        /// <summary>
        /// Determines whether the given pattern matches the value at the current
        /// position of the tokenizer.
        /// </summary>
        /// <param name="pattern">The string that contains the characters to match.</param>
        /// <returns>`true` if the pattern matches, otherwise `false`.</returns>
        protected bool Matches(string pattern)
        {
            if (pattern.Length + Index > _text.Length)
                return false;

            for (int i = 0; i < pattern.Length; ++i)
            {
                if (pattern[i] != _text[Index + i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether any of the given patterns match the value at the current
        /// position of the tokenizer.
        /// </summary>
        /// <param name="patterns">The string array that contains the characters to match.</param>
        /// <returns>`true` if any one of the patterns match, otherwise `false`.</returns>
        protected bool Matches(params string[] patterns)
        {
            foreach (string pattern in patterns)
            {
                if (pattern.Length + Index > _text.Length)
                    continue;

                bool match = true;
                for (int i = 0; i < pattern.Length; ++i)
                {
                    if (pattern[i] == _text[Index + i])
                        continue;
                    match = false;
                    break;
                }
                if (match)
                    return true;
            }
            return false;
        }

        protected bool Matches(char pattern)
        {
            if (EndOfFile)
                return false;

            return _text[Index] == pattern;
        }

        protected bool Matches(params char[] patterns)
        {
            if (EndOfFile)
                return false;

            foreach (char pattern in patterns)
            {
                if (_text[Index] == pattern)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves the next character in the tokenizer.
        /// </summary>
        /// <returns>The character at the tokenizer's current position.</returns>
        protected void Take()
        {
            Index++;
            if (EndOfFile)
                return;
            Column++;
            if (_text[Index] == JPlusConstants.NewLineChar)
            {
                Line++;
                Column = 1;
            }
        }

        /// <summary>
        /// Retrieves a string of the given length from the current position of the tokenizer.
        /// </summary>
        /// <param name="length">The length of the string to return.</param>
        /// <returns>
        /// The string of the given length. If the length exceeds where the
        /// current index is located, then `null` is returned.
        /// </returns>
        protected void Take(int length)
        {
            if (Index + length > _text.Length)
                return;

            for (int i = 0; i < length; ++i)
            {
                Take();
            }
        }

        protected string TakeWithResult(int length)
        {
            if (Index + length > _text.Length)
                return null;

            string s = _text.Substring(Index, length);
            Index += length;
            return s;
        }

        protected char PeekAndTake()
        {
            if (EndOfFile)
                return (char)0;
            Take();
            return _text[Index - 1];
        }

        protected void PullWhitespaces()
        {
            while (!EndOfFile && Peek.IsJsonPlusWhitespaceExceptNewLine())
            {
                Take();
            }
        }
    }

    /// <summary>
    /// This class contains methods used to tokenize Json+ source text.
    /// </summary>
    internal sealed class JPlusTokenizer : Tokenizer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JPlusTokenizer"/> class.
        /// </summary>
        /// <param name="source">The string that contains the source code to tokenize.</param>
        public JPlusTokenizer(string source)
            : base(source)
        {
        }

        public TokenizeResult Tokenize()
        {
            TokenizeResult tokens = Tokenize(TokenType.EndOfFile);
            tokens.Add(new Token(string.Empty, TokenType.EndOfFile, this));
            return tokens;
        }

        private TokenizeResult Tokenize(TokenType closingTokenType)
        {
            TokenizeResult tokens = new TokenizeResult();

            while (!EndOfFile)
            {
                switch (Peek)
                {
                    case JPlusConstants.StartOfObjectChar:
                        Take();
                        tokens.Add(new Token(JPlusConstants.StartOfObject, TokenType.StartOfObject, this));
                        tokens.AddRange(Tokenize(TokenType.EndOfObject));
                        continue;

                    case JPlusConstants.EndOfObjectChar:
                        Take();
                        tokens.Add(new Token(JPlusConstants.EndOfObject, TokenType.EndOfObject, this));
                        if (closingTokenType != tokens[tokens.Count - 1].Type)
                        {
                            throw new JsonPlusTokenizerException(
                                string.Format(RS.UnexpectedToken, closingTokenType, tokens[tokens.Count - 1].Type),
                                tokens[tokens.Count - 1]);
                        }
                        return tokens;

                    case JPlusConstants.StartOfArrayChar:
                        Take();
                        tokens.Add(new Token(JPlusConstants.StartOfArray, TokenType.StartOfArray, this));
                        tokens.AddRange(Tokenize(TokenType.EndOfArray));
                        continue;

                    case JPlusConstants.EndOfArrayChar:
                        Take();
                        tokens.Add(new Token(JPlusConstants.EndOfArray, TokenType.EndOfArray, this));
                        if (closingTokenType != tokens[tokens.Count - 1].Type)
                        {
                            throw new JsonPlusTokenizerException(
                                string.Format(RS.UnexpectedToken, closingTokenType, tokens[tokens.Count - 1].Type),
                                tokens[tokens.Count - 1]);
                        }
                        return tokens;

                    case JPlusConstants.ArraySeparatorChar:
                        Take();
                        tokens.Add(new Token(JPlusConstants.ArraySeparator, TokenType.ArraySeparator, LiteralTokenType.UnquotedLiteralValue, this));
                        continue;

                    case JPlusConstants.AssignmentOperatorChar:
                    case JPlusConstants.AltAssignmentOperatorChar:
                        char c = PeekAndTake();
                        tokens.Add(new Token(c.ToString(), TokenType.Assignment, this));
                        continue;

                    case JPlusConstants.SelfAssignmentOperatorFirstChar:
                        if (PullSelfAssignment(tokens))
                            continue;
                        break;

                    case JPlusConstants.CommentFirstChar:
                    case JPlusConstants.AltCommentFirstChar:
                        if (PullComment(tokens))
                            continue;
                        break;

                    case JPlusConstants.SubstitutionFirstChar:
                        if (PullSubstitution(tokens))
                            continue;
                        break;

                    case JPlusConstants.NewLineChar:
                        Take();
                        tokens.Add(new Token(JPlusConstants.NewLine, TokenType.EndOfLine, this));
                        continue;

                    case JPlusConstants.IncludeKeywordFirstChar:
                        if (PullInclude(tokens))
                            continue;
                        break;
                }

                if (PullNonNewLineWhitespace(tokens))
                    continue;
                if (PullLiteral(tokens))
                    continue;

                throw new JsonPlusTokenizerException(string.Format(RS.InvalidTokenAtIndex, Index), Token.Error(this));
            }

            if (closingTokenType != TokenType.EndOfFile)
            {
                throw new JsonPlusTokenizerException(
                    string.Format(RS.UnexpectedToken, closingTokenType, TokenType.EndOfFile),
                    tokens[tokens.Count - 1]);
            }
            return tokens;
        }

        /// <summary>
        /// Retrieves a <see cref="TokenType.SelfAssignment"/> token from the tokenizer's current position.
        /// </summary>
        /// <returns>A <see cref="TokenType.SelfAssignment"/> token from the tokenizer's current position.</returns>
        private bool PullSelfAssignment(TokenizeResult tokens)
        {
            if (!Matches(JPlusConstants.SelfAssignmentOperator))
                return false;

            Take(JPlusConstants.SelfAssignmentOperatorLength);
            tokens.Add(new Token(JPlusConstants.SelfAssignmentOperator, TokenType.SelfAssignment, this));
            return true;
        }

        /// <summary>
        /// Retrieves a <see cref="TokenType.Comment"/> token from the tokenizer's current position.
        /// </summary>
        /// <returns>A <see cref="TokenType.EndOfLine"/> token from the tokenizer's last position, discarding the comment.</returns>
        private bool PullComment(TokenizeResult tokens)
        {
            if (!Matches(JPlusConstants.Comment, JPlusConstants.AltComment))
                return false;

            string comment = DiscardRestOfLine();

            //tokens.Add(new Token(TokenType.Comment, this, start, Index - start));
            tokens.Add(new Token(comment, TokenType.EndOfLine, this));
            return true;
        }

        private bool PullInclude(TokenizeResult tokens)
        {
            if (!Matches(JPlusConstants.IncludeKeyword))
                return false;

            PushIndex();
            TokenizeResult includeTokens = new TokenizeResult();

            Take(JPlusConstants.IncludeKeywordLength);
            includeTokens.Add(new Token(JPlusConstants.IncludeKeyword, TokenType.Include, this));
            PullWhitespaces();

            int parenCount = 0;
            if (PullRequire(includeTokens))
            {
                if (!PullOpenBracket(includeTokens))
                {
                    ResetIndex();
                    return false;
                }
                parenCount++;
            }

            if (PullFileInclude(includeTokens))
            {
                if (!PullOpenBracket(includeTokens))
                {
                    ResetIndex();
                    return false;
                }
                parenCount++;
            }
            else if (PullUrlInclude(includeTokens))
            {
                if (!PullOpenBracket(includeTokens))
                {
                    ResetIndex();
                    return false;
                }
                parenCount++;
            }
            else if (PullResourceInclude(includeTokens))
            {
                if (!PullOpenBracket(includeTokens))
                {
                    ResetIndex();
                    return false;
                }
                parenCount++;
            }

            if (!PullQuoted(includeTokens, JPlusConstants.QuoteChar) && 
                !PullQuoted(includeTokens, JPlusConstants.AltQuoteChar))
            {
                ResetIndex();
                return false;
            }

            for (; parenCount > 0; --parenCount)
            {
                if (!PullCloseBracket(includeTokens))
                {
                    ResetIndex();
                    return false;
                }
            }

            PopIndex();
            tokens.AddRange(includeTokens);
            return true;
        }

        private bool PullOpenBracket(TokenizeResult tokens)
        {
            if (Peek != JPlusConstants.OpenBracketChar)
                return false;

            Take();
            tokens.Add(new Token(JPlusConstants.OpenBracket, TokenType.OpenBracket, this));
            PullWhitespaces();
            return true;
        }

        private bool PullCloseBracket(TokenizeResult tokens)
        {
            if (Peek != JPlusConstants.CloseBracketChar)
                return false;

            Take();
            tokens.Add(new Token(JPlusConstants.CloseBracket, TokenType.CloseBracket, this));
            PullWhitespaces();
            return true;
        }

        private bool PullRequire(TokenizeResult tokens)
        {
            if (!Matches(JPlusConstants.RequireKeyword))
                return false;

            Take(JPlusConstants.RequireKeywordLength);
            tokens.Add(new Token(JPlusConstants.RequireKeyword, TokenType.Required, this));
            PullWhitespaces();
            return true;
        }

        private bool PullUrlInclude(TokenizeResult tokens)
        {
            if (!Matches(JPlusConstants.IncludeUrlKeyword))
                return false;

            Take(JPlusConstants.IncludeUrlKeywordLength);
            tokens.Add(new Token(JPlusConstants.IncludeUrlKeyword, TokenType.Url, this));
            PullWhitespaces();
            return true;
        }

        private bool PullFileInclude(TokenizeResult tokens)
        {
            if (!Matches(JPlusConstants.IncludeFileKeyword))
                return false;

            Take(JPlusConstants.IncludeFileKeywordLength);
            tokens.Add(new Token(JPlusConstants.IncludeFileKeyword, TokenType.File, this));
            PullWhitespaces();
            return true;
        }

        private bool PullResourceInclude(TokenizeResult tokens)
        {
            if (!Matches(JPlusConstants.IncludeResourceKeyword))
                return false;

            Take(JPlusConstants.IncludeResourceKeywordLength);
            tokens.Add(new Token(JPlusConstants.IncludeResourceKeyword, TokenType.ClassPath, this));
            PullWhitespaces();
            return true;
        }

        private string PullEscapeSequence()
        {
            // escapes are hardcoded

            Take(); //consume "\"
            char escaped = PeekAndTake();
            switch (escaped)
            {
                case '"':
                    return ("\"");
                case '\'':
                    return ("'");
                case '\\':
                    return ("\\");
                case '/':
                    return ("/");
                case 'b':
                    return ("\b");
                case 'f':
                    return ("\f");
                case 'n':
                    return ("\n");
                case 'r':
                    return ("\r");
                case 't':
                    return ("\t");
                case 'u':
                    string hex = $"0x{TakeWithResult(4)}";
                    try
                    {
                        int j = Convert.ToInt32(hex, 16);
                        return ((char)j).ToString();
                    }
                    catch
                    {
                        throw new JsonPlusTokenizerException(string.Format(RS.InvalidUnicodeEscapeCode, escaped), Token.Error(this));
                    }
                default:
                    throw new JsonPlusTokenizerException(string.Format(RS.InvalidEscapeCode, escaped), Token.Error(this));
            }
        }

        /// <summary>
        /// Returns a <see cref="TokenType.Substitution"/> token from the tokenizer's current position.
        /// </summary>
        /// <returns>A <see cref="TokenType.Substitution"/> token from the tokenizer's current position.</returns>
        private bool PullSubstitution(TokenizeResult tokens)
        {
            bool questionMarked = false;
            if (Matches(JPlusConstants.OptionalSubstitutionOpenBrace))
            {
                Take(JPlusConstants.OptionalSubstitutionOpenBraceLength);
                questionMarked = true;
            }
            else if (Matches(JPlusConstants.SubstitutionOpenBrace))
            {
                Take(JPlusConstants.SubstitutionOpenBraceLength);
            }
            else
            {
                return false;
            }

            StringBuilder sb = new StringBuilder();
            while (!EndOfFile && !Matches(JPlusConstants.SubstitutionCloseBrace))
            {
                sb.Append(PeekAndTake());
            }

            if (EndOfFile)
                throw new JsonPlusTokenizerException(RS.UnexpectedTokenEndOfSubstitutionVsEof, Token.Error(this));

            Take();

            tokens.Add(Token.Substitution(sb.ToString().TrimWhitespace(), this, questionMarked));
            return true;
        }

        private bool PullNonNewLineWhitespace(TokenizeResult tokens)
        {
            if (!Peek.IsJsonPlusWhitespaceExceptNewLine())
                return false;

            StringBuilder sb = new StringBuilder();
            while (Peek.IsJsonPlusWhitespaceExceptNewLine())
            {
                sb.Append(PeekAndTake());
            }
            tokens.Add(Token.LiteralValue(sb.ToString(), LiteralTokenType.Whitespace, this));
            return true;
        }

        private bool PullLiteral(TokenizeResult tokens)
        {
            // Do not change this without looking at `JPlusConstants`

            switch (Peek)
            {
                case JPlusConstants.AltQuoteChar:
                    if (PullTripleQuoted(tokens, JPlusConstants.AltTripleQuote) || PullQuoted(tokens, JPlusConstants.AltQuoteChar))
                        return true;
                    throw new JsonPlusTokenizerException(RS.CloseLiteralQuoteMissing, Token.Error(this));

                case JPlusConstants.QuoteChar:
                    if (PullTripleQuoted(tokens, JPlusConstants.TripleQuote) || PullQuoted(tokens, JPlusConstants.QuoteChar))
                        return true;
                    throw new JsonPlusTokenizerException(RS.CloseLiteralQuoteMissing, Token.Error(this));

                case '-':
                case '+':
                    return PullInfinity(tokens) || PullNumbers(tokens) || PullUnquoted(tokens);

                case '0':
                    return PullHexadecimal(tokens) || PullOctet(tokens) || PullNumbers(tokens) || PullUnquoted(tokens);

                case '.':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return PullNumbers(tokens) || PullUnquoted(tokens);

                case JPlusConstants.InfinityKeywordFirstChar:
                    return PullInfinity(tokens) || PullUnquoted(tokens);

                case JPlusConstants.NanKeywordFirstChar:
                    return PullNan(tokens) || PullUnquoted(tokens);

                case JPlusConstants.TrueKeywordFirstChar: // true
                case JPlusConstants.FalseKeywordFirstChar: // false
                case JPlusConstants.AltTrueKeywordFirstChar: // yes
                    return PullBoolean(tokens) || PullUnquoted(tokens);

                case JPlusConstants.NullKeywordFirstChar: // null or no
                    return PullNull(tokens) || PullBoolean(tokens) || PullUnquoted(tokens);

                default:
                    return PullUnquoted(tokens);
            }
        }

        private bool PullInfinity(TokenizeResult tokens)
        {
            string[] infinityKeywords = new string[]
            {
                JPlusConstants.InfinityKeyword,
                JPlusConstants.InfinityPositiveKeyword,
                JPlusConstants.InfinityNegativeKeyword
            };

            foreach (string keyword in infinityKeywords)
            {
                if (Matches(keyword))
                {
                    Take(keyword.Length);
                    tokens.Add(Token.LiteralValue(keyword, LiteralTokenType.Decimal, this));
                    return true;
                }
            }

            return false;
        }

        private bool PullNan(TokenizeResult tokens)
        {
            if (Matches(JPlusConstants.NanKeyword))
            {
                Take(JPlusConstants.NanKeywordLength);
                tokens.Add(Token.LiteralValue(JPlusConstants.NanKeyword, LiteralTokenType.Decimal, this));
                return true;
            }

            return false;
        }

        private bool PullBoolean(TokenizeResult tokens)
        {
            if (Matches(JPlusConstants.TrueKeyword))
            {
                Take(JPlusConstants.TrueKeywordLength);
                tokens.Add(Token.LiteralValue(JPlusConstants.TrueKeyword, LiteralTokenType.Boolean, this));
                return true;
            }
            if (Matches(JPlusConstants.FalseKeyword))
            {
                Take(JPlusConstants.FalseKeywordLength);
                tokens.Add(Token.LiteralValue(JPlusConstants.FalseKeyword, LiteralTokenType.Boolean, this));
                return true;
            }

            if (Matches(JPlusConstants.AltTrueKeyword))
            {
                Take(JPlusConstants.AltTrueKeywordLength);
                tokens.Add(Token.LiteralValue(JPlusConstants.AltTrueKeyword, LiteralTokenType.Boolean, this));
                return true;
            }
            if (Matches(JPlusConstants.AltFalseKeyword))
            {
                Take(JPlusConstants.AltFalseKeywordLength);
                tokens.Add(Token.LiteralValue(JPlusConstants.AltFalseKeyword, LiteralTokenType.Boolean, this));
                return true;
            }

            return false;
        }

        private bool PullHexadecimal(TokenizeResult tokens)
        {
            if (!Matches("0x", "0X", "&h", "&H"))
                return false;

            PushIndex();

            StringBuilder sb = new StringBuilder();

            Take(2);
            sb.Append("0x");

            while (Peek.IsHexadecimal())
            {
                sb.Append(PeekAndTake());
            }

            try
            {
                Convert.ToInt64(sb.ToString(), 16);
            }
            catch
            {
                ResetIndex();
                return false;
            }

            PopIndex();

            tokens.Add(Token.LiteralValue(sb.ToString(), LiteralTokenType.Hexadecimal, this));
            return true;
        }

        private bool PullOctet(TokenizeResult tokens)
        {
            PushIndex();
            StringBuilder sb = new StringBuilder();
            sb.Append(PeekAndTake());
            while (Peek.IsOctet())
            {
                sb.Append(PeekAndTake());
            }

            try
            {
                Convert.ToInt64(sb.ToString(), 8);
            }
            catch
            {
                ResetIndex();
                return false;
            }

            PopIndex();
            tokens.Add(Token.LiteralValue(sb.ToString(), LiteralTokenType.Octet, this));

            return true;
        }

        private bool PullNumbers(TokenizeResult tokens)
        {
            StringBuilder sb = new StringBuilder();
            // Parse numbers
            bool parsing = true;

            Token lastValidToken = null;

            // coefficient, significand, exponent
            string state = "coefficient";

            while (parsing)
            {
                switch (state)
                {
                    case "coefficient":
                        // possible double number without coefficient
                        if (Matches("-.", "+.", "."))
                        {
                            state = "significand";
                            break;
                        }

                        PushIndex(); // long test index

                        if (Matches('+', '-'))
                            sb.Append(PeekAndTake());
                        
                        // numbers could not start with a 0
                        if (!Peek.IsDigit() || Peek == '0')
                        {
                            ResetIndex(); // reset long test index
                            parsing = false;
                            break;
                        }

                        while (Peek.IsDigit())
                        {
                            sb.Append(PeekAndTake());
                        }

                        if (!long.TryParse(sb.ToString(), out _))
                        {
                            ResetIndex(); // reset long test index
                            parsing = false;
                            break;
                        }
                        PopIndex(); // end long test index
                        lastValidToken = Token.LiteralValue(sb.ToString(), LiteralTokenType.Integer, this);
                        state = "significand";
                        break;

                    case "significand":
                        // short logic, no significand, but probably have an exponent
                        if (!Matches("-.", "+.", "."))
                        {
                            state = "exponent";
                            break;
                        }

                        PushIndex(); // validate significand in number test

                        if (Matches('+', '-'))
                            sb.Insert(0, PeekAndTake());

                        sb.Append(PeekAndTake());

                        if (!Peek.IsDigit())
                        {
                            ResetIndex(); // reset validate significand in number test
                            parsing = false;
                            break;
                        }

                        while (Peek.IsDigit())
                        {
                            sb.Append(PeekAndTake());
                        }

                        if (!double.TryParse(sb.ToString(), out _))
                        {
                            ResetIndex(); // reset validate significand in number test
                            parsing = false;
                            break;
                        }

                        PopIndex(); // end validate significand in number test
                        lastValidToken = Token.LiteralValue(sb.ToString(), LiteralTokenType.Decimal, this);
                        state = "exponent";
                        break;

                    case "exponent":
                        // short logic, check if number is a double with exponent
                        if (!Matches('e', 'E'))
                        {
                            parsing = false;
                            break;
                        }

                        PushIndex(); // validate exponent
                        sb.Append(PeekAndTake());

                        // check for signed exponent
                        if (Matches('-', '+'))
                            sb.Append(PeekAndTake());

                        if (!Peek.IsDigit())
                        {
                            ResetIndex(); // reset validate exponent
                            parsing = false;
                            break;
                        }

                        while (Peek.IsDigit())
                        {
                            sb.Append(PeekAndTake());
                        }

                        if (!double.TryParse(sb.ToString(), out _))
                        {
                            ResetIndex(); // reset validate exponent
                            parsing = false;
                            break;
                        }

                        PopIndex(); // end validate exponent
                        lastValidToken = Token.LiteralValue(sb.ToString(), LiteralTokenType.Decimal, this);
                        parsing = false;
                        break;
                }
            }

            if (lastValidToken == null)
                return false;

            tokens.Add(lastValidToken);
            return true;
        }

        private bool PullNull(TokenizeResult tokens)
        {
            if (!Matches(JPlusConstants.NullKeyword))
                return false;

            Take(4);
            tokens.Add(Token.LiteralValue(JPlusConstants.NullKeyword, LiteralTokenType.Null, this));
            return true;
        }

        private bool PullUnquoted(TokenizeResult tokens)
        {
            if (!IsUnquoted())
                return false;

            StringBuilder sb = new StringBuilder();
            while (!EndOfFile && IsUnquoted())
            {
                sb.Append(PeekAndTake());
            }

            tokens.Add(Token.LiteralValue(sb.ToString(), LiteralTokenType.UnquotedLiteralValue, this));
            return true;
        }

        /// <summary>
        /// Returns a quoted <see cref="TokenType.LiteralValue"/> token from the tokenizer's current position.
        /// </summary>
        /// <returns>A <see cref="TokenType.LiteralValue"/> token from the tokenizer's current position.</returns>
        private bool PullQuoted(TokenizeResult tokens, char quoteChar)
        {
            if (!Matches(quoteChar))
                return false;

            StringBuilder sb = new StringBuilder();
            Take();
            while (!EndOfFile && !Matches(quoteChar))
            {
                if (Matches(JPlusConstants.Escape))
                    sb.Append(PullEscapeSequence());
                else
                    sb.Append(PeekAndTake());
            }

            if (EndOfFile)
                throw new JsonPlusTokenizerException(string.Format(RS.UnexpectedTokenExpectQuote, TokenType.EndOfFile), Token.Error(this));

            Take();

            tokens.Add(Token.QuotedLiteralValue(sb.ToString(), this));
            return true;
        }

        /// <summary>
        /// Retrieves a triple quoted <see cref="TokenType.LiteralValue"/> token from the tokenizer's current position.
        /// </summary>
        /// <returns>
        /// A <see cref="TokenType.LiteralValue"/> token from the tokenizer's current position.
        /// </returns>
        private bool PullTripleQuoted(TokenizeResult tokens, string quoteSequence)
        {
            if (!Matches(quoteSequence))
                return false;

            StringBuilder sb = new StringBuilder();
            Take(3);
            while (!EndOfFile && !Matches(quoteSequence))
            {
                if (Matches(JPlusConstants.Escape))
                    sb.Append(PullEscapeSequence());
                else
                    sb.Append(PeekAndTake());
            }

            if (EndOfFile)
                throw new JsonPlusTokenizerException(string.Format(RS.UnexpectedTokenExpectTripleQuote, TokenType.EndOfFile), Token.Error(this));

            Take(3);

            tokens.Add(Token.TripleQuotedLiteralValue(sb.ToString(), this));
            return true;
        }

        /// <summary>
        /// Retrieves the current line from where the current token
        /// is located in the string.
        /// </summary>
        /// <returns>The current line from where the current token is located.</returns>
        private string DiscardRestOfLine()
        {
            StringBuilder sb = new StringBuilder();
            while (!EndOfFile && !Matches(JPlusConstants.NewLineChar))
            {
                sb.Append(PeekAndTake());
            }

            return sb.ToString();
        }

        // #todo alt comment
        private bool IsStartOfComment()
        {
            return Matches(JPlusConstants.Comment, JPlusConstants.AltComment);
        }

        private bool IsUnquoted()
        {
            return !EndOfFile && 
                !Peek.IsJsonPlusWhitespace() && 
                !IsStartOfComment() && 
                !Peek.IsNotInUnquoted();
        }
    }
}
