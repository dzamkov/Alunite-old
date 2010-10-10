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
        /// Gets the edges of the triangle.
        /// </summary>
        public Edge<T>[] Edges
        {
            get
            {
                return new Edge<T>[]
                {
                    new Edge<T>(this.A, this.B),
                    new Edge<T>(this.B, this.C),
                    new Edge<T>(this.C, this.A)
                };
            }
        }

        /// <summary>
        /// Gets a flipped form of the triangle (same points, different order).
        /// </summary>
        public Triangle<T> Flip
        {
            get
            {
                return new Triangle<T>(this.A, this.C, this.B);
            }
        }

        public override string ToString()
        {
            return this.A.ToString() + ", " + this.B.ToString() + ", " + this.C.ToString();
        }

        public T A;
        public T B;
        public T C;
    }

    /// <summary>
    /// Triangle (2-simplex) related functions.
    /// </summary>
    public static class Triangle
    {
        /// <summary>
        /// Gets the normal of the specified triangle.
        /// </summary>
        public static Vector Normal(Triangle<Vector> Triangle)
        {
            return Vector.Normalize(Vector.Cross(Triangle.B - Triangle.A, Triangle.C - Triangle.A));
        }

        /// <summary>
        /// Gets if the two vectors are on opposite sides of the plane defined by the triangle.
        /// </summary>
        public static bool Oppose(Vector A, Vector B, Triangle<Vector> Triangle)
        {
            Vector norm = Vector.Cross(Triangle.B - Triangle.A, Triangle.C - Triangle.A);
            double dota = Vector.Dot(A - Triangle.A, norm);
            double dotb = Vector.Dot(B - Triangle.A, norm);
            return (dota > 0.0) ^ (dotb > 0.0);
        }

        /// <summary>
        /// Gets if the specified vector is on the front side of the plane defined by the triangle.
        /// </summary>
        public static bool Front(Vector A, Triangle<Vector> Triangle)
        {
            Vector norm = Vector.Cross(Triangle.B - Triangle.A, Triangle.C - Triangle.A);
            double dota = Vector.Dot(A - Triangle.A, norm);
            return (dota > 0.0);
        }

        /// <summary>
        /// Determines if a segment intersects the triangle and if so, outputs the relative distance along
        /// the segment the hit is at and the actual position of the hit. This will only check for a hit on the front side of
        /// the triangle.
        /// </summary>
        public static bool Intersect(Triangle<Vector> Triangle, Vector Start, Vector End, out double Length, out Vector Position)
        {
            Vector u = Triangle.B - Triangle.A;
            Vector v = Triangle.C - Triangle.A;
            Vector n = Vector.Cross(u, v);

            // Test intersection of segment and triangle plane.
            Vector raydir = End - Start;
            Vector rayw = Start - Triangle.A;
            double a = -Vector.Dot(n, rayw);
            double b = Vector.Dot(n, raydir);
            double r = a / b;

            if (r >= 0.0 && r <= 1.0 && b < 0.0)
            {
                Length = r;
                Position = Start + (raydir * r);

                // Check if point is in triangle.
                Vector w = Position - Triangle.A;
                double uu = Vector.Dot(u, u);
                double uv = Vector.Dot(u, v);
                double vv = Vector.Dot(v, v);
                double wu = Vector.Dot(w, u);
                double wv = Vector.Dot(w, v);
                double d = (uv * uv) - (uu * vv);
                double s = ((uv * wv) - (vv * wu)) / d;
                if (s >= 0.0 && s <= 1.0)
                {
                    double t = ((uv * wu) - (uu * wv)) / d;
                    if (t >= 0.0 && (s + t) <= 1.0)
                    {
                        // HIT!
                        return true;
                    }
                }
                return false;
            }
            Length = 0;
            Position = new Vector(0.0, 0.0, 0.0);
            return false;
        }
    }
}