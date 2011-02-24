using System;
using System.Collections.Generic;

using OpenTK;

namespace Alunite
{
    /// <summary>
    /// Represents a point or offset in three-dimensional space.
    /// </summary>
    public struct Vector
    {
        public Vector(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public static implicit operator Vector3d(Vector Vector)
        {
            return new Vector3d(Vector.X, Vector.Y, Vector.Z);
        }

        public static explicit operator Vector3(Vector Vector)
        {
            return new Vector3((float)Vector.X, (float)Vector.Y, (float)Vector.Z);
        }

        public static bool operator ==(Vector A, Vector B)
        {
            return A.X == B.X && A.Y == B.Y && A.Z == B.Z;
        }

        public static bool operator !=(Vector A, Vector B)
        {
            return A.X != B.X || A.Y != B.Y || A.Z != B.Z;
        }

        public static Vector operator +(Vector A, Vector B)
        {
            return new Vector(A.X + B.X, A.Y + B.Y, A.Z + B.Z);
        }

        public static Vector operator -(Vector A, Vector B)
        {
            return new Vector(A.X - B.X, A.Y - B.Y, A.Z - B.Z);
        }

        public static Vector operator *(Vector A, double Magnitude)
        {
            return new Vector(A.X * Magnitude, A.Y * Magnitude, A.Z * Magnitude);
        }

        public static Vector operator -(Vector A)
        {
            return new Vector(-A.X, -A.Y, -A.Z);
        }

        public override string ToString()
        {
            return this.X.ToString() + ", " + this.Y.ToString() + ", " + this.Z.ToString();
        }

        /// <summary>
        /// Gets the cross product of two vectors.
        /// </summary>
        public static Vector Cross(Vector A, Vector B)
        {
            return
                new Vector(
                    (A.Y * B.Z) - (A.Z * B.Y),
                    (A.Z * B.X) - (A.X * B.Z),
                    (A.X * B.Y) - (A.Y * B.X));
        }

        /// <summary>
        /// Compares two vectors lexographically.
        /// </summary>
        public static bool Compare(Vector A, Vector B)
        {
            if (A.X > B.X)
                return true;
            if (A.X < B.X)
                return false;
            if (A.Y > B.Y)
                return true;
            if (A.Y < B.Y)
                return false;
            if (A.Z > B.Z)
                return true;
            return false;
        }

        /// <summary>
        /// Gets the outgoing ray of an object hitting a plane with the specified normal at the
        /// specified incoming ray.
        /// </summary>
        public static Vector Reflect(Vector Incoming, Vector Normal)
        {
            return Incoming - Normal * (2 * Vector.Dot(Incoming, Normal) / Normal.SquareLength);
        }

        /// <summary>
        /// Multiplies each component of the vectors with the other's corresponding component.
        /// </summary>
        public static Vector Scale(Vector A, Vector B)
        {
            return new Vector(A.X * B.X, A.Y * B.Y, A.Z * B.Z);
        }

        /// <summary>
        /// Gets the dot product between two vectors.
        /// </summary>
        public static double Dot(Vector A, Vector B)
        {
            return A.X * B.X + A.Y * B.Y + A.Z * B.Z;
        }

        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        public double Length
        {
            get
            {
                return Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);
            }
        }

        /// <summary>
        /// Gets the square of the length of the vector.
        /// </summary>
        public double SquareLength
        {
            get
            {
                return this.X * this.X + this.Y * this.Y + this.Z * this.Z;
            }
        }

        /// <summary>
        /// Normalizes the vector so its length is one but its direction is unchanged.
        /// </summary>
        public void Normalize()
        {
            double ilen = 1.0 / this.Length;
            this.X *= ilen;
            this.Y *= ilen;
            this.Z *= ilen;
        }

        /// <summary>
        /// Normalizes the specified vector.
        /// </summary>
        public static Vector Normalize(Vector A)
        {
            A.Normalize();
            return A;
        }

        public double X;
        public double Y;
        public double Z;
    }
}