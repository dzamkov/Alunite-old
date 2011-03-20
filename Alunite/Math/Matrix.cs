using System;
using System.Collections.Generic;

using OpenTK;

namespace Alunite
{
    /// <summary>
    /// Represents an affline or projection transform in three-dimensional space using a matrix.
    /// </summary>
    public struct Matrix
    {
        public Matrix(
            double M11, double M21, double M31, double M41,
            double M12, double M22, double M32, double M42,
            double M13, double M23, double M33, double M43,
            double M14, double M24, double M34, double M44)
        {
            this.M11 = M11;
            this.M12 = M12;
            this.M13 = M13;
            this.M14 = M14;

            this.M21 = M21;
            this.M22 = M22;
            this.M23 = M23;
            this.M24 = M24;

            this.M31 = M31;
            this.M32 = M32;
            this.M33 = M33;
            this.M34 = M34;

            this.M41 = M41;
            this.M42 = M42;
            this.M43 = M43;
            this.M44 = M44;
        }

        /// <summary>
        /// Applies the projection transform represented by this matrix to a vector.
        /// </summary>
        public Vector Apply(Vector Vector)
        {
            double nx = this.M11 * Vector.X + this.M21 * Vector.Y + this.M31 * Vector.Z + this.M41;
            double ny = this.M12 * Vector.X + this.M22 * Vector.Y + this.M32 * Vector.Z + this.M42;
            double nz = this.M13 * Vector.X + this.M23 * Vector.Y + this.M33 * Vector.Z + this.M43;
            double nw = this.M14 * Vector.X + this.M24 * Vector.Y + this.M34 * Vector.Z + this.M44;
            double iw = 1.0 / nw;
            nx *= iw;
            ny *= iw;
            nz *= iw;
            return new Vector(nx, ny, nz);
        }

        /// <summary>
        /// Applies the affline transform represented by this matrix to a vector.
        /// </summary>
        public Vector ApplyAffline(Vector Vector)
        {
            double nx = this.M11 * Vector.X + this.M21 * Vector.Y + this.M31 * Vector.Z + this.M41;
            double ny = this.M12 * Vector.X + this.M22 * Vector.Y + this.M32 * Vector.Z + this.M42;
            double nz = this.M13 * Vector.X + this.M23 * Vector.Y + this.M33 * Vector.Z + this.M43;
            return new Vector(nx, ny, nz);
        }

        public static implicit operator Matrix4d(Matrix Matrix)
        {
            return new Matrix4d(
                Matrix.M11, Matrix.M12, Matrix.M13, Matrix.M14,
                Matrix.M21, Matrix.M22, Matrix.M23, Matrix.M24,
                Matrix.M31, Matrix.M32, Matrix.M33, Matrix.M34,
                Matrix.M41, Matrix.M42, Matrix.M43, Matrix.M44);
        }

        public double M11; public double M21; public double M31; public double M41;
        public double M12; public double M22; public double M32; public double M42;
        public double M13; public double M23; public double M33; public double M43;
        public double M14; public double M24; public double M34; public double M44;
    }
}