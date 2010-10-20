using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A group of two homogenous items arranged to form a line segment.
    /// </summary>
    public struct Segment<T> : IEquatable<Segment<T>>
        where T : IEquatable<T>
    {
        public Segment(T A, T B)
        {
            this.A = A;
            this.B = B;
        }

        public bool Equals(Segment<T> Segment)
        {
            return this == Segment;
        }

        public override bool Equals(object obj)
        {
            Segment<T>? edge = obj as Segment<T>?;
            if (edge != null)
            {
                return edge.Value == this;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int a = this.A.GetHashCode();
            int b = this.B.GetHashCode();
            if ((a > b))
            {
                return a ^ b;
            }
            else
            {
                return ~(a ^ b);
            }
        }

        public static bool operator ==(Segment<T> A, Segment<T> B)
        {
            return A.A.Equals(B.A) && A.B.Equals(B.B);
        }

        public static bool operator !=(Segment<T> A, Segment<T> B)
        {
            return !(A == B);
        }

        /// <summary>
        /// Gets the points (hopefully 2) in the edge.
        /// </summary>
        public T[] Points
        {
            get
            {
                return new T[]
                {
                    this.A,
                    this.B
                };
            }
        }

        /// <summary>
        /// Creates a flipped form of the triangle (same points, different order).
        /// </summary>
        public Segment<T> Flip
        {
            get
            {
                return new Segment<T>(this.B, this.A);
            }
        }

        public T A;
        public T B;
    }

    /// <summary>
    /// A segment where point order is irrelevant. The only functional difference between this and a normal segment is the implementation
    /// of Equals and GetHashCode.
    /// </summary>
    public struct UnorderedSegment<T> : IEquatable<UnorderedSegment<T>>
        where T : IEquatable<T>
    {
        public UnorderedSegment(Segment<T> Source)
        {
            this.Source = Source;
        }

        public UnorderedSegment(T A, T B)
        {
            this.Source = new Segment<T>(A, B);
        }

        public bool Equals(UnorderedSegment<T> Segment)
        {
            return this == Segment;
        }

        public override bool Equals(object obj)
        {
            UnorderedSegment<T>? segment = obj as UnorderedSegment<T>?;
            if (segment != null)
            {
                return segment.Value == this;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int a = this.Source.A.GetHashCode();
            int b = this.Source.B.GetHashCode();
            return a ^ b;
        }

        public static bool operator ==(UnorderedSegment<T> A, UnorderedSegment<T> B)
        {
            return A.Source == B.Source || A.Source.Flip == B.Source;
        }

        public static bool operator !=(UnorderedSegment<T> A, UnorderedSegment<T> B)
        {
            return !(A == B);
        }

        public Segment<T> Source;
    }

    /// <summary>
    /// Segment (1-simplex) related functions.
    /// </summary>
    public static class Segment
    {
        /// <summary>
        /// Gets the midpoint of the specified segment.
        /// </summary>
        public static Vector Midpoint(Segment<Vector> Segment)
        {
            return (Segment.B + Segment.A) * 0.5;
        }
    }
}