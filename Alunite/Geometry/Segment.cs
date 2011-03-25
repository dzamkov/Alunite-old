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

        /// <summary>
        /// Flips the direction of this segment.
        /// </summary>
        public Segment<T> Flip
        {
            get
            {
                return new Segment<T>(this.B, this.A);
            }
        }

        /// <summary>
        /// Gets an equality comparer that determines if directed segments are equal.
        /// </summary>
        public static IEqualityComparer<Segment<T>> GetDirectedComparer(IEqualityComparer<T> ItemComparer)
        {
            return new _DirectedComparer(ItemComparer);
        }

        private class _DirectedComparer: IEqualityComparer<Segment<T>>
        {
            public _DirectedComparer(IEqualityComparer<T> ItemComparer)
            {
                this._ItemComparer = ItemComparer;
            }

            public bool Equals(Segment<T> x, Segment<T> y)
            {
                return this._ItemComparer.Equals(x.A, y.A) && this._ItemComparer.Equals(x.B, y.B);
            }

            public int GetHashCode(Segment<T> obj)
            {
                int a = this._ItemComparer.GetHashCode(obj.A);
                int b = this._ItemComparer.GetHashCode(obj.B);
                if (a > b)
                    return a ^ b;
                else
                    return ~(a ^ b);
            }

            private IEqualityComparer<T> _ItemComparer;
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

        /// <summary>
        /// Gets the centroid of the given segment.
        /// </summary>
        public static Vector Centroid(Segment<Vector> Segment)
        {
            return (Segment.A + Segment.B) * 0.5;
        }
    }
}