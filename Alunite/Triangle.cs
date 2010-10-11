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

        public Triangle(T Vertex, Edge<T> Base)
        {
            this.A = Vertex;
            this.B = Base.A;
            this.C = Base.B;
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
        /// Gets the primary vertex of the triangle (A).
        /// </summary>
        public T Vertex
        {
            get
            {
                return this.A;
            }
        }

        /// <summary>
        /// Gets the primary base of the triangle (B, C).
        /// </summary>
        public Edge<T> Base
        {
            get
            {
                return new Edge<T>(this.B, this.C);
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
        /// Aligns the base (B, C) of the source triangle so that it equals the base. Returns
        /// null if the triangle does not include the specified base.
        /// </summary>
        public static Triangle<T>? Align<T>(Triangle<T> Source, Edge<T> Base)
            where T : IEquatable<T>
        {
            if (new Edge<T>(Source.A, Source.B) == Base)
            {
                return new Triangle<T>(Source.C, Base);
            }
            if (new Edge<T>(Source.B, Source.C) == Base)
            {
                return new Triangle<T>(Source.A, Base);
            }
            if (new Edge<T>(Source.C, Source.A) == Base)
            {
                return new Triangle<T>(Source.B, Base);
            }
            return null;
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
            double u;
            double v;
            Intersect(Triangle, Start, End, out Length, out Position, out u, out v);
            if(Length >= 0.0 && Length <= 1.0)
            {
                if (u >= 0.0 && u <= 1.0)
                {
                    if (v >= 0.0 && (v + u) <= 1.0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Finds where the segment intersects the plane and ouputs the point where the intersection
        /// is made, the length along the segment the intersection is at, and the uv coordinates relative to the triangle the intersection is at.
        /// </summary>
        public static void Intersect(Triangle<Vector> Triangle, Vector Start, Vector End, out double Length, out Vector Position, out double U, out double V)
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
            U = ((uv * wv) - (vv * wu)) / d;
            V = ((uv * wu) - (uu * wv)) / d;
        }
    }
}