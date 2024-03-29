﻿using System;
using System.Collections.Generic;

using OpenTK;

namespace Alunite
{
    /// <summary>
    /// Represents a point or offset in three-dimensional space.
    /// </summary>
    public struct Vector : IAdditive<Vector, Vector>, IMultiplicative<Vector, Scalar>
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

        public Vector Add(Vector Operand)
        {
            return new Vector(this.X + Operand.X, this.Y + Operand.Y, this.Z + Operand.Z);
        }

        public Vector Subtract(Vector Operand)
        {
            return new Vector(this.X - Operand.X, this.Y - Operand.Y, this.Z - Operand.Z);
        }

        public Vector Multiply(Scalar Operand)
        {
            return new Vector(this.X * Operand, this.Y * Operand, this.Z * Operand);
        }

        public Vector Divide(Scalar Operand)
        {
            return new Vector(this.X / Operand, this.Y / Operand, this.Z / Operand);
        }

        public static Vector operator +(Vector A, Vector B)
        {
            return A.Add(B);
        }

        public static Vector operator -(Vector A, Vector B)
        {
            return A.Subtract(B);
        }

        public static Vector operator *(Vector A, double Magnitude)
        {
            return A.Multiply(Magnitude);
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
        /// Gets if this vector is inside the specified sphere.
        /// </summary>
        public bool InSphere(Vector Center, double Radius)
        {
            return (this - Center).SquareLength < Radius * Radius;
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