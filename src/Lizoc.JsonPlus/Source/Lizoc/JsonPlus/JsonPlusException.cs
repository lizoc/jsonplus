using System;

namespace Lizoc.JsonPlus
{
    /// <summary>
    /// Represents a generic error in Json+.
    /// </summary>
    public class JsonPlusException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPlusException"/> class with the specified error message.
        /// </summary>
        /// <param name="message">The message associated with the exception.</param>
        public JsonPlusException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPlusException"/> class with the specified error message,  
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message associated with the exception.</param>
        /// <param name="innerException">The inner exception associated with this exception.</param>
        public JsonPlusException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
