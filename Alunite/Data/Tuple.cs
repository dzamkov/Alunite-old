using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A tuple containing two heterogenously-typed items.
    /// </summary>
    public struct Tuple<TA, TB>
    {
        public Tuple(TA A, TB B)
        {
            this.A = A;
            this.B = B;
        }

        public TA A;
        public TB B;
    }

    /// <summary>
    /// Contains tuple-related functions.
    /// </summary>
    public static class Tuple
    {
        /// <summary>
        /// Creates a tuple with two items.
        /// </summary>
        public static Tuple<TA, TB> Create<TA, TB>(TA A, TB B)
        {
            return new Tuple<TA, TB>(A, B);
        }
    }
}