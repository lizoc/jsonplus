using System;
using System.Globalization;
using System.Text;

namespace Lizoc.JsonPlus
{
    /// <summary>
    /// Represents an error when parsing Json+ text.
    /// </summary>
    public sealed class JsonPlusParserException : JsonPlusException, ISourceLocation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPlusParserException"/> class.
        /// </summary>
        public JsonPlusParserException()
            : base(RS.ParserError)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPlusParserException"/> class
        /// with the specified error message.
        /// </summary>
        /// <see cref="JsonPlusException.JsonPlusException(string)"/>
        public JsonPlusParserException(string message) 
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPlusParserException"/> class
        /// with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <see cref="JsonPlusException.JsonPlusException(string, Exception)"/>
        public JsonPlusParserException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        internal JsonPlusParserException(ISourceLocation info)
            : base(RS.ParserError)
        {
            Line = info.Line;
            Column = info.Column;
        }

        internal JsonPlusParserException(ISourceLocation info, string message) 
            : base(message)
        {
            Line = info.Line;
            Column = info.Column;
        }

        internal JsonPlusParserException(ISourceLocation info, string message, Exception innerException) 
            : base(message, innerException)
        {
            Line = info.Line;
            Column = info.Column;
        }

        /// <see cref="ISourceLocation.Line"/>
        public int Line { get; }

        /// <see cref="ISourceLocation.Column"/>
        public int Column { get; }

        internal static JsonPlusParserException Create(ISourceLocation tokenLocation, JsonPlusPath path, string message)
        {
            return Create(tokenLocation, path, message, null);
        }

        internal static JsonPlusParserException Create(ISourceLocation tokenLocation, JsonPlusPath path, string message, Exception ex)
        {
            message = FormatMessage(tokenLocation, path, message);
            return new JsonPlusParserException(tokenLocation, message, ex);
        }

        private static string FormatMessage(ISourceLocation tokenLocation, JsonPlusPath path, string message)
        {
            StringBuilder sb = new StringBuilder();

            // don't add a fullstop and space when message ends with a new line
            if (!message.EndsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                sb.Append(message.Trim());

                if (!message.EndsWith(RS.FullStop))
                    sb.Append(RS.FullStop);

                sb.Append(RS.Space);
            }

            bool addComma = false;
            if (path != null)
            {
                sb.Append(string.Format(CultureInfo.InvariantCulture, RS.AtPath, path));
                addComma = true;
            }

            if (tokenLocation != null)
            {
                sb.Append(addComma ? RS.Comma : (RS.Space + RS.At));
                sb.Append(string.Format(CultureInfo.InvariantCulture, RS.LineAndColumn, tokenLocation.Line, tokenLocation.Column));
            }

            sb.Append(RS.FullStop);

            return sb.ToString();
        }
    }
}
