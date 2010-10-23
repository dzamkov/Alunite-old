using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Represents a point or offset in two-dimensional space.
    /// </summary>
    public struct Point : IEquatable<Point>
    {
        public Point(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public static bool operator ==(Point A, Point B)
        {
            return A.X == B.X && A.Y == B.Y;
        }

        public static bool operator !=(Point A, Point B)
        {
            return A.X != B.X || A.Y != B.Y;
        }

        public static Point operator +(Point A, Point B)
        {
            return new Point(A.X + B.X, A.Y + B.Y);
        }

        public static Point operator -(Point A, Point B)
        {
            return new Point(A.X - B.X, A.Y - B.Y);
        }

        public static Point operator *(Point A, double Magnitude)
        {
            return new Point(A.X * Magnitude, A.Y * Magnitude);
        }

        public static Point operator -(Point A)
        {
            return new Point(-A.X, -A.Y);
        }

        /// <summary>
        /// Gets the dot product between two points.
        /// </summary>
        public static double Dot(Point A, Point B)
        {
            return A.X * B.X + A.Y * B.Y;
        }

        /// <summary>
        /// Gets the cross product of a point (Creates a perpendicular point).
        /// </summary>
        public static Point Cross(Point A)
        {
            return new Point(A.Y, -A.X);
        }

        public bool Equals(Point other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            Point? v = obj as Point?;
            if (v.HasValue)
            {
                return this == v.Value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int h = 0x10807565;
            int x = this.X.GetHashCode();
            int y = this.Y.GetHashCode();
            h += (x << 3) + (y << 7)
                + (y >> 7) + (x >> 13);
            h = h ^ x ^ y;
            return h;
        }

        public override string ToString()
        {
            return this.X.ToString() + ", " + this.Y.ToString();
        }

        public double X;
        public double Y;
    }
}