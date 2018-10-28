namespace Lizoc.JsonPlus
{
    /// <summary>
    /// This enumeration defines the different types of tokens that the <see cref="JsonPlusParser"/> can support.
    /// </summary>
    internal enum TokenType
    {
        /// <summary>
        /// This token type represents the end of the source code.
        /// </summary>
        EndOfFile,

        /// <summary>
        /// This token type represents the beginning of an object, `{`.
        /// </summary>
        StartOfObject,

        /// <summary>
        /// This token type represents the end of an object, `}`.
        /// </summary>
        EndOfObject,

        /// <summary>
        /// This token type represents the beginning of an array, `[`.
        /// </summary>
        StartOfArray,

        /// <summary>
        /// This token type represents the end of an array, `]`.
        /// </summary>
        EndOfArray,

        /// <summary>
        /// This token type represents the opening parenthesis, `(`.
        /// </summary>
        OpenBracket,

        /// <summary>
        /// This token type represents the closing parenthesis, `)`.
        /// </summary>
        CloseBracket,

        /// <summary>
        /// This token type represents a comment.
        /// </summary>
        Comment,

        /// <summary>
        /// This token type represents the value portion of a key-value pair.
        /// </summary>
        LiteralValue,

        /// <summary>
        /// This token type represents the assignment operator, `+=`.
        /// </summary>
        SelfAssignment,

        /// <summary>
        /// This token type represents the assignment operator, `=` or `:`.
        /// </summary>
        Assignment,

        /// <summary>
        /// This token type represents the separator in an array, `,`.
        /// </summary>
        ArraySeparator,

        /// <summary>
        /// This token type represents the start of a replacement variable, `${`.
        /// </summary>
        Substitution,

        /// <summary>
        /// This token type represents the start of a replacement variable with question mark, `${?`.
        /// </summary>
        OptionalSubstitution,

        /// <summary>
        /// This token type represents a newline character, <c>\n</c> .
        /// </summary>
        EndOfLine,

        /// <summary>
        /// This token type represents the include directive.
        /// </summary>
        Include,

        /// <summary>
        /// This token type represents the required() directive.
        /// </summary>
        Required,

        /// <summary>
        /// This token type represents the url() directive.
        /// </summary>
        Url,

        /// <summary>
        /// This token type represents the file() directive.
        /// </summary>
        File,

        /// <summary>
        /// This token type represents the classpath() directive.
        /// </summary>
        ClassPath,

        /// <summary>
        /// This token type represents a tokenizer error.
        /// </summary>
        Error,
    }

    /// <summary>
    /// All possible literal tokens. This is different from <see cref="JsonPlusLiteralType"/>.
    /// </summary>
    internal enum LiteralTokenType
    {
        None,
        Null,
        Whitespace,
        UnquotedLiteralValue,
        QuotedLiteralValue,
        TripleQuotedLiteralValue,
        Boolean,
        Integer,
        Decimal,
        Hexadecimal,
        Octet
    }

    /// <summary>
    /// This class represents a token within a Json+ string.
    /// </summary>
    internal sealed class Token: ISourceLocation
    {
        // this is used by the ToString() function
        private const string TokenToStringFormat = "Type: {0}, Value: {1}, Ln/Col: {2}/{3}";

        // for serialization
        private Token()
        {
        }

        public Token(string value, TokenType type, ISourceLocation source)
            : this(value, type, LiteralTokenType.None, source)
        {
        }

        public Token(string value, TokenType type, LiteralTokenType literalType, ISourceLocation source)
        {
            Type = type;
            LiteralType = literalType;
            Value = value;

            if (source != null)
            {
                Line = source.Line;
                Column = source.Column - (value?.Length ?? 0);
            }
        }

        /*
        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> class.
        /// </summary>
        /// <param name="type">The type of token to associate with.</param>
        /// <param name="source">The <see cref="ISourceLocation"/> of this <see cref="Token"/>, used for exception generation purposes.</param>
        public Token(TokenType type, IHoconLineInfo source) 
            : this(null, type, TokenLiteralType.None, source)
        {
        }
        */

        public static readonly Token Empty = new Token();

        public int Line { get; }

        public int Column { get; }

        /// <summary>
        /// The value associated with this token. If this token is
        /// a <see cref="TokenType.LiteralValue"/>, then this property
        /// holds the string literal.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// The type that represents this token.
        /// </summary>
        public TokenType Type { get; }

        public LiteralTokenType LiteralType { get; }

        public override string ToString()
        {
            return string.Format(TokenToStringFormat,
                Type,
                Value ?? "null",
                Line,
                Column);
        }

        /// <summary>
        /// Creates a substitution token with a given <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path to associate with this token.</param>
        /// <param name="location">The <see cref="ISourceLocation"/> of this <see cref="Token"/>, used for exception generation purposes.</param>
        /// <param name="isOptional">Designate whether the substitution <see cref="Token"/> was declared as `${?`.</param>
        /// <returns>A substitution token with the given path.</returns>
        public static Token Substitution(string path, ISourceLocation location, bool isOptional)
        {
            return new Token(
                path, 
                isOptional ? TokenType.OptionalSubstitution : TokenType.Substitution, 
                LiteralTokenType.None,
                location);
        }

        /// <summary>
        /// Creates a string literal token with a given <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to associate with this token.</param>
        /// <param name="literalType">The <see cref="LiteralTokenType"/> of this <see cref="Token"/>.</param>
        /// <param name="location">The <see cref="ISourceLocation"/> of this <see cref="Token"/>, used for exception generation purposes.</param>
        /// <returns>A string literal token with the given value.</returns>
        public static Token LiteralValue(string value, LiteralTokenType literalType, ISourceLocation location)
        {
            return new Token(value, TokenType.LiteralValue, literalType, location);
        }

        public static Token QuotedLiteralValue(string value, ISourceLocation location)
        {
            return LiteralValue(value, LiteralTokenType.QuotedLiteralValue, location);
        }

        public static Token TripleQuotedLiteralValue(string value, ISourceLocation location)
        {
            return LiteralValue(value, LiteralTokenType.TripleQuotedLiteralValue, location);
        }

        public static Token Include(string path, ISourceLocation location)
        {
            return new Token(path, TokenType.Include, LiteralTokenType.None, location);
        }

        public static Token Error(ISourceLocation source)
        {
            return new Token(string.Empty, TokenType.Error, LiteralTokenType.None, source);
        }
    }
}
