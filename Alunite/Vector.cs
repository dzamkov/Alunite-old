using System;
using System.Collections.Generic;
using OpenTK;

namespace Alunite
{
    /// <summary>
    /// Represents a point or offset in three-dimensional space.
    /// </summary>
    public struct Vector : IEquatable<Vector>
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

        public static bool operator ==(Vector A, Vector B)
        {
            return A.X == B.X && A.Y == B.Y && A.Z == B.Z;
        }

        public static bool operator !=(Vector A, Vector B)
        {
            return A.X != B.X || A.Y != B.Y || A.Z != B.Z;
        }

        public bool Equals(Vector other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            Vector? v = obj as Vector?;
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
            int z = this.Z.GetHashCode();
            h += (x << 3) + (y << 7) + (z << 13)
                + (z >> 3) + (y >> 7) + (x >> 13);
            h = h ^ x ^ y ^ z;
            return h;
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
        /// Gets the dot product between two vectors.
        /// </summary>
        public static double Dot(Vector A, Vector B)
        {
            return A.X * B.X + A.Y * B.Y + A.Z * B.Z;
        }

        public double X;
        public double Y;
        public double Z;
    }
}