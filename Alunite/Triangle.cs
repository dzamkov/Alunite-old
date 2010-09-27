using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A group of three homogenous items arranged to form a triangle. Triangles are equal to
    /// all other triangles with the same values that are arranged in the same direction.
    /// </summary>
    public struct Triangle<T> : IEquatable<Triangle<T>>
        where T : IEquatable<T>
    {
        public Triangle(T A, T B, T C)
        {
            this.A = A;
            this.B = B;
            this.C = C;
        }

        public bool Equals(Triangle<T> Triangle)
        {
            return this == Triangle;   
        }

        public override int GetHashCode()
        {
            int a = this.A.GetHashCode();
            int b = this.B.GetHashCode();
            int c = this.C.GetHashCode();
            if ((a > b) ^ (b > c) ^ (c > a))
            {
                return a ^ b ^ c;
            }
            else
            {
                return ~(a ^ b ^ c);
            }
        }

        public static bool operator ==(Triangle<T> A, Triangle<T> B)
        {
            // Allow rotational symmetric equality.
            if (A.A.Equals(B.A))
            {
                return A.B.Equals(B.B) && A.C.Equals(B.C);
            }
            if (A.A.Equals(B.B))
            {
                return A.B.Equals(B.C) && A.C.Equals(B.A);
            }
            if (A.A.Equals(B.C))
            {
                return A.B.Equals(B.A) && A.C.Equals(B.B);
            }
            return false;
        }

        public static bool operator !=(Triangle<T> A, Triangle<T> B)
        {
            return !(A == B);
        }

        public override bool Equals(object obj)
        {
            Triangle<T>? tri = obj as Triangle<T>?;
            if (tri.HasValue)
            {
                return this == tri.Value;
            }
            return false;
        }

        /// <summary>
        /// Gets the points (hopefully 3) in the triangle.
        /// </summary>
        public T[] Points
        {
            get
            {
                return new T[]
                {
                    this.A,
                    this.B,
                    this.C
                };
            }
        }

        /// <summary>
        /// Creates a flipped form of the triangle (same points, different order).
        /// </summary>
        public Triangle<T> Flip()
        {
            return new Triangle<T>(this.A, this.C, this.B);
        }

        public T A;
        public T B;
        public T C;
    }

    /// <summary>
    /// Triangle (3-simplex) related functions.
    /// </summary>
    public static class Triangle
    {
        /// <summary>
        /// Gets the normal of the specified triangle.
        /// </summary>
        public static Vector Normal(Triangle<Vector> Triangle)
        {
            return Vector.Normalize(Vector.Cross(Triangle.C - Triangle.A, Triangle.B - Triangle.A));
        }
    }
}