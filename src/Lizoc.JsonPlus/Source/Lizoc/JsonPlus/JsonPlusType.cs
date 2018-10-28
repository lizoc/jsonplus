namespace Lizoc.JsonPlus
{
    /// <summary>
    /// Primary data types defined under the Json+ specification.
    /// </summary>
    public enum JsonPlusType
    {
        /// <summary>
        /// An empty node that does not contain any data.
        /// </summary>
        Empty,

        /// <summary>
        /// A literal node contains data. Multiple subtypes are defined under <see cref="JsonPlusLiteralType"/>.
        /// </summary>
        Literal,

        /// <summary>
        /// An enumerable collection of nodes. Items in the collection may be one of or a mixture of any valid data types 
        /// defined in the Json+ specification.
        /// </summary>
        Array,

        /// <summary>
        /// A node consisting of multiple key-value pairs. The key is a string, and the value may be 
        /// of any valid data type defined in the Json+ specification. The value can be accessed by providing the 
        /// associated key.
        /// </summary>
        Object,
    }
}
