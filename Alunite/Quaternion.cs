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

        public Quaternion(Vector Axis, double Angle)
        {
            double hang = Angle * 0.5;
            double sina = Math.Sin(hang);
            double cosa = Math.Cos(hang);
            this.A = cosa;
            this.B = Axis.X * sina;
            this.C = Axis.Y * sina;
            this.D = Axis.Z * sina;
        }

        /// <summary>
        /// Gets a rotation quaternion for a rotation described in axis angle form.
        /// </summary>
        public static Quaternion AxisAngle(Vector Axis, double Angle)
        {
            return new Quaternion(Axis, Angle);
        }

        /// <summary>
        /// Gets a rotation quaternion for the angle between two distinct normal vectors.
        /// </summary>
        public static Quaternion AngleBetween(Vector A, Vector B)
        {
            double hcosang = Math.Sqrt(0.5 + 0.5 * Vector.Dot(A, B));
            double hsinang = Math.Sqrt(1.0 - hcosang * hcosang);
            double sinang = 2.0 * hsinang * hcosang;
            Vector axis = Vector.Cross(A, B);
            axis *= 1.0 / sinang;
            return new Quaternion(hcosang, axis * hsinang);
        }

        /// <summary>
        /// Gets the identity quaternion.
        /// </summary>
        public static Quaternion Identity
        {
            get
            {
                return new Quaternion(1.0, 0.0, 0.0, 0.0);
            }
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
        /// Applies the rotation represented by this quaternion to another.
        /// </summary>
        public Quaternion ApplyTo(Quaternion Other)
        {
            return this * Other;
        }

        /// <summary>
        /// Applies a rotation to this quaternion.
        /// </summary>
        public Quaternion Apply(Quaternion Other)
        {
            return Other * this;
        }

        /// <summary>
        /// Normalizes the quaternion.
        /// </summary>
        public void Normalize()
        {
            double d = 1.0 / Math.Sqrt(this.A * this.A + this.B * this.B + this.C * this.C + this.D * this.D);
            this.A *= d;
            this.B *= d;
            this.C *= d;
            this.D *= d;
        }

        /// <summary>
        /// Normalizes the specified quaternion.
        /// </summary>
        public static Quaternion Normalize(Quaternion A)
        {
            A.Normalize();
            return A;
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

            return (this * new Quaternion(0.0, Point) * this.Conjugate).Imag;
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

    /// <summary>
    /// An axis angle representation of rotation.
    /// </summary>
    public struct AxisAngle
    {
        public AxisAngle(Vector Axis, double Angle)
        {
            this.Axis = Axis;
            this.Angle = Angle;
        }

        /// <summary>
        /// Gets an axis angle representation of an identity rotation.
        /// </summary>
        public static AxisAngle Identity
        {
            get
            {
                return new AxisAngle(new Vector(1.0, 0.0, 0.0), 0.0);
            }
        }

        public static implicit operator Quaternion(AxisAngle AxisAngle)
        {
            return Quaternion.AxisAngle(AxisAngle.Axis, AxisAngle.Angle);
        }

        public static AxisAngle operator *(AxisAngle AxisAngle, double Factor)
        {
            return new AxisAngle(AxisAngle.Axis, AxisAngle.Angle * Factor);
        }

        /// <summary>
        /// The angle, in radians, of the rotation.
        /// </summary>
        public double Angle;

        /// <summary>
        /// The normalized axis of the rotation.
        /// </summary>
        public Vector Axis;
    }
}