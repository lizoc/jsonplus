// -----------------------------------------------------------------------
// <copyright file="IncludeSource.cs" repo="Json+">
//     Copyright (C) 2018 Lizoc Inc. <http://www.lizoc.com>
//     The source code documented in this file is subject to the MIT license.
//     See the LICENSE file in the project root for more information.
// </copyright>
// -----------------------------------------------------------------------
 
using System;

namespace Lizoc.JsonPlus
{
    /// <summary>
    /// The source where Json+ source code is loaded from using the `include` directive. This enum is usually used in 
    /// conjunction with the <see cref="IncludeCallbackAsync"/> delegate.
    /// </summary>
    public enum IncludeSource
    {
        /// <summary>
        /// The source is a local file.
        /// </summary>
        File,

        /// <summary>
        /// The source is accessible from a HTTP URL.
        /// </summary>
        Url,

        /// <summary>
        /// The source is embedded in a binary assembly.
        /// </summary>
        Resource,

        /// <summary>
        /// The source is unspecified.
        /// </summary>
        Unspecified
    }
}
