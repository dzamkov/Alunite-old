using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A group of two homogenous items arranged to form a line segment.
    /// </summary>
    public struct Edge<T> : IEquatable<Edge<T>>
        where T : IEquatable<T>
    {
        public Edge(T A, T B)
        {
            this.A = A;
            this.B = B;
        }

        public bool Equals(Edge<T> Edge)
        {
            return this == Edge;
        }

        public override bool Equals(object obj)
        {
            Edge<T>? edge = obj as Edge<T>?;
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

        public static bool operator ==(Edge<T> A, Edge<T> B)
        {
            return A.A.Equals(B.A) && A.B.Equals(B.B);
        }

        public static bool operator !=(Edge<T> A, Edge<T> B)
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
        public Edge<T> Flip
        {
            get
            {
                return new Edge<T>(this.B, this.A);
            }
        }

        public T A;
        public T B;
    }

    /// <summary>
    /// Edge (2-simplex) related functions.
    /// </summary>
    public static class Edge
    {

    }
}