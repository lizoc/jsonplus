using System;
using System.Collections.Generic;

namespace Lizoc.JsonPlus
{
    /// <summary>
    /// This interface defines the contract for a Json+ node. All Json+ data types must implement this interface.
    /// </summary>
    public interface IJsonPlusNode : IEquatable<IJsonPlusNode>
    {
        /// <summary>
        /// The parent <see cref="JsonPlusObject"/> or <see cref="JsonPlusArray"/> that contains this value.
        /// </summary>
        IJsonPlusNode Parent { get; }

        /// <summary>
        /// The data type of this value.
        /// </summary>
        JsonPlusType Type { get; }

        /// <summary>
        /// The Json+ source code of this value.
        /// </summary>
        string Source { get; }

        /// <summary>
        /// Returns the value of this node as a <see cref="JsonPlusObject"/>.
        /// </summary>
        /// <returns>The result of the value of this node casted to a <see cref="JsonPlusObject"/>.</returns>
        JsonPlusObject GetObject();

        /// <summary>
        /// Returns the value of this node as a <see cref="string"/>.
        /// </summary>
        /// <returns>The result of the value of this node casted to a <see cref="string"/>.</returns>
        string GetString();

        /// <summary>
        /// Returns the value of this node as a <see cref="JsonPlusArray"/>.
        /// </summary>
        /// <returns>The result of the value of this node casted to a <see cref="JsonPlusArray"/>.</returns>
        List<IJsonPlusNode> GetArray();

        /// <summary>
        /// Returns the value of this node as a <see cref="JsonPlusValue"/>.
        /// </summary>
        /// <returns>The result of the value of this node casted to a <see cref="JsonPlusValue"/>.</returns>
        JsonPlusValue GetValue();

        /// <summary>
        /// Do deep copy of this node.
        /// </summary>
        /// <returns>A deep company of this element.</returns>
        /// <summary>
        /// Performs a deep clone of this node.
        /// </summary>
        /// <param name="newParent">Sets the <see cref="Parent"/> property of the cloned output.</param>
        /// <returns>A deep clone of this node.</returns>
        IJsonPlusNode Clone(IJsonPlusNode newParent);

        /// <summary>
        /// Returns a string representation of this value, indented for improved readibility.
        /// </summary>
        /// <param name="indent">The number of indents for this value.</param>
        /// <param name="indentSize">The number of spaces for each indent level.</param>
        /// <returns>A string representation of this value, indented for improved readibility.</returns>
        string ToString(int indent, int indentSize);
    }
}

