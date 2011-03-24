using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A collection of two geometric items interpreted as a geometric simplex.
    /// </summary>
    public struct Segment<T>
    {
        public Segment(T A, T B)
        {
            this.A = A;
            this.B = B;
        }

        public T A;
        public T B;
    }

    /// <summary>
    /// Segment-related functions.
    /// </summary>
    public static class Segment
    {
        /// <summary>
        /// Gets the direction vector of the given directed segment.
        /// </summary>
        public static Vector Direction(Segment<Vector> Segment)
        {
            return Vector.Normalize(Segment.B - Segment.A);
        }
    }
}