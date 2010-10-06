using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Tuple helper functions.
    /// </summary>
    public static class Tuple
    {
        public static Tuple<TA, TB> Create<TA, TB>(TA A, TB B)
        {
            return new Tuple<TA, TB>(A, B);
        }
    }

    /// <summary>
    /// A hetrogenous group of two values.
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

}