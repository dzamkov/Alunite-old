using System;
using System.Collections.Generic;

using OpenTK;

namespace Alunite
{
    /// <summary>
    /// Represents a rotation in three-dimensional space.
    /// </summary>
    public struct Quaternion
    {
        public Quaternion(double A, double B, double C, double D)
        {
            this.A = A;
            this.B = B;
            this.C = C;
            this.D = D;
        }

        public Quaternion(double Real, Vector Imag)
        {
            this.A = Real;
            this.B = Imag.X;
            this.C = Imag.Y;
            this.D = Imag.Z;
        }

        public Quaternion(Vector Vector, double Angle)
        {
            double hang = Angle * 0.5;
            double sina = Math.Sin(hang);
            double cosa = Math.Cos(hang);
            this.A = cosa;
            this.B = Vector.X * sina;
            this.C = Vector.Y * sina;
            this.D = Vector.Z * sina;
        }

        /// <summary>
        /// Gets the real (scalar) part of the quaternion.
        /// </summary>
        public double Real
        {
            get
            {
                return this.A;
            }
        }

        /// <summary>
        /// Gets the imaginary part of the quaternion.
        /// </summary>
        public Vector Imag
        {
            get
            {
                return new Vector(this.B, this.C, this.D);
            }
        }

        /// <summary>
        /// Gets the conjugate of this quaternion.
        /// </summary>
        public Quaternion Conjugate
        {
            get
            {
                return new Quaternion(this.A, -this.B, -this.C, -this.D);
            }
        }

        /// <summary>
        /// Rotates a point using this vector.
        /// </summary>
        public Vector Rotate(Vector Point)
        {
            double ta = - this.B * Point.X - this.C * Point.Y - this.D * Point.Z;
            double tb = + this.A * Point.X + this.C * Point.Z - this.D * Point.Y;
            double tc = + this.A * Point.Y - this.B * Point.Z + this.D * Point.X;
            double td = + this.A * Point.Z + this.B * Point.Y - this.C * Point.X;

            double nb = - ta * this.B + tb * this.A - tc * this.D + td * this.C;
            double nc = - ta * this.C + tb * this.D + tc * this.A - td * this.B;
            double nd = - ta * this.D - tb * this.C + tc * this.B + td * this.A;

            return new Vector(nb, nc, nd);
        }

        public static Quaternion operator *(Quaternion A, Quaternion B)
        {
            return new Quaternion(
                A.A * B.A - A.B * B.B - A.C * B.C - A.D * B.D,
                A.A * B.B + A.B * B.A + A.C * B.D - A.D * B.C,
                A.A * B.C - A.B * B.D + A.C * B.A + A.D * B.B,
                A.A * B.D + A.B * B.C - A.C * B.B + A.D * B.A);
        }

        public double A;
        public double B;
        public double C;
        public double D;
    }
}