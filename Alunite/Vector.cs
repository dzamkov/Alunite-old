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

        /// <summary>
        /// Calculates the determinant of a matrix in the form 
        /// [[A.X, A.Y, A.Z, 1.0], [B.X, B.Y, B.Z, 1.0], [C.X, C.Y, C.Z, 1.0], [D.X, D.Y, D.Z, 1.0]]
        /// </summary>
        public static double TetrahedronDeterminant(Vector A, Vector B, Vector C, Vector D)
        {
            return 
                B.Z * C.Y * D.X - A.Z * C.Y * D.X -
                B.Y * C.Z * D.X + A.Y * C.Z * D.X +
                A.Z * B.Y * D.X - A.Y * B.Z * D.X -
                B.Z * C.X * D.Y + A.Z * C.X * D.Y +
                B.X * C.Z * D.Y - A.X * C.Z * D.Y -
                A.Z * B.X * D.Y + A.X * B.Z * D.Y +
                B.Y * C.X * D.Z - A.Y * C.X * D.Z -
                B.X * C.Y * D.Z + A.X * C.Y * D.Z +
                A.Y * B.X * D.Z - A.X * B.Y * D.Z -
                A.Z * B.Y * C.X + A.Y * B.Z * C.X +
                A.Z * B.X * C.Y - A.X * B.Z * C.Y -
                A.Y * B.X * C.Z + A.X * B.Y * C.Z;
        }

        public static implicit operator Vector3d(Vector Vector)
        {
            return new Vector3d(Vector.X, Vector.Y, Vector.Z);
        }

        public double X;
        public double Y;
        public double Z;
    }

}