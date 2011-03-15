using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// An object described by a change to another object of a common base.
    /// </summary>
    public interface IDelta<TBase>
    {
        /// <summary>
        /// Gets the original object.
        /// </summary>
        TBase Original { get; }

        /// <summary>
        /// Gets the final object, which is the original with the delta applied.
        /// </summary>
        TBase Final { get; }

        /// <summary>
        /// Gets the final object destructively by appling the delta directly to the original object. This method
        /// will have the same effect as "Final" if the original object can not be safely modified.
        /// </summary>
        TBase FinalUnsafe { get; }
    }
}