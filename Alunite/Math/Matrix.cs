using System;
using System.Collections.Generic;

using OpenTK;

namespace Alunite
{
    /// <summary>
    /// A matrix representation of a three-dimensional orthogonal transform.
    /// </summary>
    public struct OrthogonalMatrix
    {
        public OrthogonalMatrix(
            double M11, double M21, double M31,
            double M12, double M22, double M32,
            double M13, double M23, double M33)
        {
            this.M11 = M11;
            this.M12 = M12;
            this.M13 = M13;

            this.M21 = M21;
            this.M22 = M22;
            this.M23 = M23;

            this.M31 = M31;
            this.M32 = M32;
            this.M33 = M33;
        }

        public OrthogonalMatrix(Vector X, Vector Y, Vector Z)
            : this(
            X.X, Y.X, Z.X,
            X.Y, Y.Y, Z.Y,
            X.Z, Y.Z, Z.Z)
        {
        }

        /// <summary>
        /// Creates a lookat matrix using foward and up unit vectors.
        /// </summary>
        public static OrthogonalMatrix Lookat(Vector Foward, Vector Up)
        {
            Vector y = Vector.Cross(Up, Foward);
            y.Normalize();
            Vector z = Vector.Cross(Foward, y);
            z.Normalize();
            return new OrthogonalMatrix(Foward, y, z);
        }

        /// <summary>
        /// Applies the transform represented by this matrix to a vector.
        /// </summary>
        public Vector Apply(Vector Vector)
        {
            double nx = this.M11 * Vector.X + this.M21 * Vector.Y + this.M31 * Vector.Z;
            double ny = this.M12 * Vector.X + this.M22 * Vector.Y + this.M32 * Vector.Z;
            double nz = this.M13 * Vector.X + this.M23 * Vector.Y + this.M33 * Vector.Z;
            return new Vector(nx, ny, nz);
        }

        public static implicit operator AfflineMatrix(OrthogonalMatrix Matrix)
        {
            return new AfflineMatrix(Matrix, 0.0, 0.0, 0.0);
        }

        public static implicit operator ProjectionMatrix(OrthogonalMatrix Matrix)
        {
            return new ProjectionMatrix(Matrix, 0.0, 0.0, 0.0, 1.0);
        }

        public static implicit operator Matrix4d(OrthogonalMatrix Matrix)
        {
            return new Matrix4d(
                Matrix.M11, Matrix.M12, Matrix.M13, 0.0,
                Matrix.M21, Matrix.M22, Matrix.M23, 0.0,
                Matrix.M31, Matrix.M32, Matrix.M33, 0.0,
                0.0, 0.0, 0.0, 1.0);
        }

        public double M11; public double M21; public double M31;
        public double M12; public double M22; public double M32;
        public double M13; public double M23; public double M33;
    }

    /// <summary>
    /// A matrix representation of a three-dimensional affline transform.
    /// </summary>
    public struct AfflineMatrix
    {
        public AfflineMatrix(OrthogonalMatrix Orthogonal, double M41, double M42, double M43)
        {
            this.Orthogonal = Orthogonal;
            this.M41 = M41;
            this.M42 = M42;
            this.M43 = M43;
        }

        public AfflineMatrix(
            double M11, double M21, double M31, double M41,
            double M12, double M22, double M32, double M42,
            double M13, double M23, double M33, double M43)
            : this(
            new OrthogonalMatrix(
                M11, M21, M31,
                M12, M22, M32,
                M13, M23, M33), M41, M42, M43)
        {

        }

        public AfflineMatrix(OrthogonalMatrix Orthogonal, Vector Translate)
            : this(Orthogonal, Translate.X, Translate.Y, Translate.Z)
        {

        }

        /// <summary>
        /// Applies the transform represented by this matrix to a vector.
        /// </summary>
        public Vector Apply(Vector Vector)
        {
            return this.Orthogonal.Apply(Vector) + new Vector(this.M41, this.M42, this.M43);
        }

        /// <summary>
        /// Applies the transform represented by this matrix to a four-dimensional vector.
        /// </summary>
        public void Apply(ref Vector Vector, ref double W)
        {
            OrthogonalMatrix om = this.Orthogonal;
            Vector nvec = new Vector();
            nvec.X = om.M11 * Vector.X + om.M21 * Vector.Y + om.M31 * Vector.Z + this.M41 * W;
            nvec.Y = om.M12 * Vector.X + om.M22 * Vector.Y + om.M32 * Vector.Z + this.M42 * W;
            nvec.Z = om.M13 * Vector.X + om.M23 * Vector.Y + om.M33 * Vector.Z + this.M43 * W;
            Vector = nvec;
        }

        public static implicit operator ProjectionMatrix(AfflineMatrix Matrix)
        {
            return new ProjectionMatrix(Matrix, 0.0, 0.0, 0.0, 1.0);
        }

        public static implicit operator Matrix4d(AfflineMatrix Matrix)
        {
            OrthogonalMatrix om = Matrix.Orthogonal;
            return new Matrix4d(
                om.M11, om.M12, om.M13, 0.0,
                om.M21, om.M22, om.M23, 0.0,
                om.M31, om.M32, om.M33, 0.0,
                Matrix.M41, Matrix.M42, Matrix.M43, 1.0);
        }

        /// <summary>
        /// The orthogonal portion of this transformation matrix.
        /// </summary>
        public OrthogonalMatrix Orthogonal;

        public double M41; 
        public double M42; 
        public double M43;
    }

    /// <summary>
    /// A matrix representation of a three-dimensional projection transform.
    /// </summary>
    public struct ProjectionMatrix
    {
        public ProjectionMatrix(AfflineMatrix Affline, double M14, double M24, double M34, double M44)
        {
            this.Affline = Affline;
            this.M14 = M14;
            this.M24 = M24;
            this.M34 = M34;
            this.M44 = M44;
        }

        public ProjectionMatrix(
            double M11, double M21, double M31, double M41,
            double M12, double M22, double M32, double M42,
            double M13, double M23, double M33, double M43,
            double M14, double M24, double M34, double M44)
            : this(
            new AfflineMatrix(
                M11, M21, M31, M41,
                M12, M22, M32, M42,
                M13, M23, M33, M43), 
                M14, M24, M34, M44)
           
        {

        }

        /// <summary>
        /// Applies the transform represented by this matrix to a vector.
        /// </summary>
        public Vector Apply(Vector Vector)
        {
            double w = 1.0;
            this.Apply(ref Vector, ref w);
            double iw = 1.0 / w;
            Vector.X *= iw;
            Vector.Y *= iw;
            Vector.Z *= iw;
            return Vector;
        }

        /// <summary>
        /// Applies the transform represented by this matrix to a four-dimensional vector.
        /// </summary>
        public void Apply(ref Vector Vector, ref double W)
        {
            double nw = this.M14 * Vector.X + this.M24 * Vector.Y + this.M34 * Vector.Z + this.M44 * W;
            this.Affline.Apply(ref Vector, ref W);
            W = nw;
        }

        /// <summary>
        /// Gets or sets the orthogonal portions of this transformation matrix.
        /// </summary>
        public OrthogonalMatrix Orthogonal
        {
            get
            {
                return this.Affline.Orthogonal;
            }
            set
            {
                this.Affline.Orthogonal = value;
            }
        }

        public static implicit operator Matrix4d(ProjectionMatrix Matrix)
        {
            OrthogonalMatrix om = Matrix.Orthogonal;
            AfflineMatrix am = Matrix.Affline;
            return new Matrix4d(
                om.M11, om.M12, om.M13, Matrix.M14,
                om.M21, om.M22, om.M23, Matrix.M24,
                om.M31, om.M32, om.M33, Matrix.M34,
                am.M41, am.M42, am.M43, Matrix.M44);
        }

        /// <summary>
        /// The affline portion of this transformation matrix.
        /// </summary>
        public AfflineMatrix Affline;

        public double M14; public double M24; public double M34; public double M44;
    }
}