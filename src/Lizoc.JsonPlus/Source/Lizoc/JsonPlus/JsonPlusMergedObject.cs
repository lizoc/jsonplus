// -----------------------------------------------------------------------
// <copyright file="JsonPlusMergedObject.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code in this file is subject to the MIT license.
//     See the LICENSE file in the repository root directory for more information.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Lizoc.JsonPlus
{
    /// <summary>
    /// A variation of <see cref="JsonPlusObject"/> that allows individual merged <see cref="JsonPlusObject"/> items to be 
    /// accessed.
    /// </summary>
    public sealed class JsonPlusMergedObject : JsonPlusObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPlusMergedObject"/> class.
        /// </summary>
        /// <param name="parent">The parent container.</param>
        /// <param name="objects">All <see cref="JsonPlusObject"/> items that are merged.</param>
        public JsonPlusMergedObject(IJsonPlusNode parent, List<JsonPlusObject> objects) 
            : base(parent)
        {
            Objects = objects;
            foreach (JsonPlusObject obj in objects)
            {
                base.Merge(obj);
            }
        }

        /// <summary>
        /// Gets all the <see cref="JsonPlusObject"/> items that are merged.
        /// </summary>
        public List<JsonPlusObject> Objects { get; }

        /// <see cref="JsonPlusObject.Merge(JsonPlusObject)"/>
        public override void Merge(JsonPlusObject other)
        {
            ((JsonPlusObjectMember)Parent).Value.Add(other.Clone(((JsonPlusObjectMember)Parent).Value));
            base.Merge(other);
        }
    }
}
