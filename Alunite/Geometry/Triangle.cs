using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A collection of three geometric items interpreted as a geometric simplex.
    /// </summary>
    public struct Triangle<T>
    {
        public Triangle(T A, T B, T C)
        {
            this.A = A;
            this.B = B;
            this.C = C;
        }

        /// <summary>
        /// Gets the vertices in this triangle.
        /// </summary>
        public IEnumerable<T> Vertices
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
        /// Gets all the segments of this triangle.
        /// </summary>
        public IEnumerable<Segment<T>> Segments
        {
            get
            {
                return new Segment<T>[]
                {
                    new Segment<T>(this.A, this.B),
                    new Segment<T>(this.B, this.C),
                    new Segment<T>(this.C, this.A)
                };
            }
        }

        public T A;
        public T B;
        public T C;
    }

    /// <summary>
    /// Triangle-related functions.
    /// </summary>
    public static class Triangle
    {
        /// <summary>
        /// Gets the position on a triangle defined by its UV coordinates.
        /// </summary>
        public static Vector GetPosition(Triangle<Vector> Triangle, Point UV)
        {
            return Triangle.A + (Triangle.B - Triangle.A) * UV.X + (Triangle.C - Triangle.A) * UV.Y;
        }

        /// <summary>
        /// Gets the unit normal for a vector triangle.
        /// </summary>
        public static Vector Normal(Triangle<Vector> Triangle)
        {
            return Vector.Normalize(Vector.Cross(Triangle.B - Triangle.A, Triangle.C - Triangle.A));
        }

        /// <summary>
        /// Finds the intersection between a directed segment and a triangle. Returns true if the segment intersects the front face and
        /// if the intersection is within or on the triangle's boundaries.
        /// </summary>
        public static bool Intersect(Triangle<Vector> Triangle, Segment<Vector> Segment, out double Length, out Vector Position, out Point UV)
        {
            if (IntersectPlane(Triangle, Segment, out Length, out Position, out UV))
            {
                if (UV.X >= 0.0 && UV.Y >= 0.0 && UV.X + UV.Y <= 1.0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds where the segment intersects the plane and ouputs the point where the intersection
        /// is made, the length along the segment the intersection is at, and the uv coordinates relative to the triangle the intersection is at. Returns
        /// true if the segment hit the triangle on its front face.
        /// </summary>
        public static bool IntersectPlane(Triangle<Vector> Triangle, Segment<Vector> Segment, out double Length, out Vector Position, out Point UV)
        {
            Vector u = Triangle.B - Triangle.A;
            Vector v = Triangle.C - Triangle.A;
            Vector n = Vector.Cross(u, v);

            // Test intersection of segment and triangle plane.
            Vector raydir = Segment.B - Segment.A;
            Vector rayw = Segment.A - Triangle.A;
            double a = -Vector.Dot(n, rayw);
            double b = Vector.Dot(n, raydir);
            double r = a / b;

            Length = r;
            Position = Segment.A + (raydir * r);

            // Check if point is in triangle.
            Vector w = Position - Triangle.A;
            double uu = Vector.Dot(u, u);
            double uv = Vector.Dot(u, v);
            double vv = Vector.Dot(v, v);
            double wu = Vector.Dot(w, u);
            double wv = Vector.Dot(w, v);
            double d = (uv * uv) - (uu * vv);
            UV = new Point(((uv * wv) - (vv * wu)) / d, ((uv * wu) - (uu * wv)) / d);

            return b < 0.0;
        }
    }
}