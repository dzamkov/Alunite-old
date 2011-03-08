using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// An object that determines which objects of a common type may pass.
    /// </summary>
    /// <typeparam name="T">The type of the filtered objects.</typeparam>
    public abstract class Filter<T>
    {
        /// <summary>
        /// Gets wether the specified item is allowed to pass through the filter.
        /// </summary>
        public abstract bool Allow(T Item);
    }
}