namespace Lizoc.JsonPlus
{
    /// <summary>
    /// Interface class for any class that contains information on its location within a Json+ source code.
    /// </summary>
    internal interface ISourceLocation
    {
        /// <summary>
        /// The line number where the token is present.
        /// </summary>
        int Line { get; }

        /// <summary>
        /// The index position within the line where the the token is present.
        /// </summary>
        int Column { get; }
    }
}
