using System;
using System.Collections.Generic;

using OpenTK;

namespace Alunite
{
    /// <summary>
    /// Represents a point or offset in two-dimensional space.
    /// </summary>
    public struct Point
    {
        public Point(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public static implicit operator Vector2d(Point Vector)
        {
            return new Vector2d(Vector.X, Vector.Y);
        }

        public static explicit operator Vector2(Point Vector)
        {
            return new Vector2((float)Vector.X, (float)Vector.Y);
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

        public override string ToString()
        {
            return this.X.ToString() + ", " + this.Y.ToString();
        }

        /// <summary>
        /// Gets the cross product of a point.
        /// </summary>
        public static Point Cross(Point A)
        {
            return new Point(A.Y, -A.X);
        }

        /// <summary>
        /// Multiplies each component of the points with the other's corresponding component.
        /// </summary>
        public static Point Scale(Point A, Point B)
        {
            return new Point(A.X * B.X, A.Y * B.Y);
        }

        /// <summary>
        /// Gets the dot product between two points.
        /// </summary>
        public static double Dot(Point A, Point B)
        {
            return A.X * B.X + A.Y * B.Y;
        }

        /// <summary>
        /// Gets the length of the point.
        /// </summary>
        public double Length
        {
            get
            {
                return Math.Sqrt(this.X * this.X + this.Y * this.Y);
            }
        }

        /// <summary>
        /// Gets the square of the length of the point.
        /// </summary>
        public double SquareLength
        {
            get
            {
                return this.X * this.X + this.Y * this.Y;
            }
        }

        /// <summary>
        /// Gets a point representing the origin of a coordinate system.
        /// </summary>
        public static Point Origin
        {
            get
            {
                return Zero;
            }
        }

        /// <summary>
        /// Gets a point representing a zero offset.
        /// </summary>
        public static Point Zero
        {
            get
            {
                return new Point(0.0, 0.0);
            }
        }

        /// <summary>
        /// Normalizes the point so its length is one but its direction is unchanged.
        /// </summary>
        public void Normalize()
        {
            double ilen = 1.0 / this.Length;
            this.X *= ilen;
            this.Y *= ilen;
        }

        /// <summary>
        /// Normalizes the specified point.
        /// </summary>
        public static Point Normalize(Point A)
        {
            A.Normalize();
            return A;
        }

        public double X;
        public double Y;
    }
}