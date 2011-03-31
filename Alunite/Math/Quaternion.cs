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
        /// Gets a rotational quaternion from an orthogonal matrix representing a rotation.
        /// </summary>
        public static Quaternion FromMatrix(OrthogonalMatrix Matrix)
        {
            double r = Math.Sqrt(1 + Matrix.M11 - Matrix.M22 - Matrix.M33);
            double dr = r * 2.0;
            Quaternion q = new Quaternion(
                (Matrix.M23 - Matrix.M32) / dr,
                r / 2.0,
                (Matrix.M12 + Matrix.M21) / dr,
                (Matrix.M31 + Matrix.M13) / dr);
            return q;
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
        /// Gets the absolute value of the quaternion.
        /// </summary>
        public double Abs
        {
            get
            {
                return Math.Sqrt(this.A * this.A + this.B * this.B + this.C * this.C + this.D * this.D);
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
            return Quaternion.Normalize(this * Other);
        }

        /// <summary>
        /// Applies a rotation to this quaternion.
        /// </summary>
        public Quaternion Apply(Quaternion Other)
        {
            return Quaternion.Normalize(Other * this);
        }

        /// <summary>
        /// Normalizes the quaternion.
        /// </summary>
        public void Normalize()
        {
            double d = 1.0 / this.Abs;
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

            return new Vector(nb, nc, nd);
        }

        public static Quaternion operator *(Quaternion A, Quaternion B)
        {
            Quaternion q = new Quaternion(
                A.A * B.A - A.B * B.B - A.C * B.C - A.D * B.D,
                A.A * B.B + A.B * B.A + A.C * B.D - A.D * B.C,
                A.A * B.C - A.B * B.D + A.C * B.A + A.D * B.B,
                A.A * B.D + A.B * B.C - A.C * B.B + A.D * B.A);
            return q;
        }

        public static implicit operator OrthogonalMatrix(Quaternion A)
        {
            double aa = A.A * A.A;
            double dab = A.A * A.B * 2.0;
            double dac = A.A * A.C * 2.0;
            double dad = A.A * A.D * 2.0;
            double bb = A.B * A.B;
            double dbc = A.B * A.C * 2.0;
            double dbd = A.B * A.D * 2.0;
            double cc = A.C * A.C;
            double dcd = A.C * A.D * 2.0;
            double dd = A.D * A.D;
            return new OrthogonalMatrix(
                aa + bb - cc - dd, dbc - dad, dbd + dac,
                dbc + dad, aa - bb + cc - dd, dcd - dab,
                dbd - dac, dcd + dab, aa - bb - cc + dd);
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

        public AxisAngle(Vector Rotation)
        {
            this.Angle = Rotation.Length;
            this.Axis = Rotation * (1.0 / this.Angle);
        }

        /// <summary>
        /// Gets a single vector representing the rotation of this axis angle pair by setting the length of the axis
        /// to the amount of rotation. Rotation vectors are useful for representing angular velocity or torque because adding
        /// the vectors together will add their effects.
        /// </summary>
        public Vector Rotation
        {
            get
            {
                return this.Axis * this.Angle;
            }
        }

        /// <summary>
        /// Gets the axis angle rotation between the two specified normal vectors.
        /// </summary>
        public static AxisAngle Between(Vector A, Vector B)
        {
            return new AxisAngle(Vector.Normalize(Vector.Cross(A, B)), Math.Acos(Vector.Dot(A, B)));
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

        /// <summary>
        /// Applies a rotation created by a quaternion to this axis-angle rotation.
        /// </summary>
        public AxisAngle Apply(Quaternion Rotation)
        {
            return new AxisAngle(Rotation.Rotate(this.Axis), Angle);
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