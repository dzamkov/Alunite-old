using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A dataless object with only one value.
    /// </summary>
    public struct Void
    {
        /// <summary>
        /// Gets the only value of this object.
        /// </summary>
        public static Void Value = new Void();
    }
}