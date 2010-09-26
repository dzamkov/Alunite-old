using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Tetrahedron (4-simplex) related maths.
    /// </summary>
    public static class Tetrahedron
    {
        /// <summary>
        /// Calculates the determinant of a matrix in the form 
        /// [[A.X, A.Y, A.Z, 1.0], [B.X, B.Y, B.Z, 1.0], [C.X, C.Y, C.Z, 1.0], [D.X, D.Y, D.Z, 1.0]]
        /// </summary>
        public static double Determinant(Quadruple<Vector> Tetrahedron)
        {
            return
                Tetrahedron.B.Z * Tetrahedron.C.Y * Tetrahedron.D.X - Tetrahedron.A.Z * Tetrahedron.C.Y * Tetrahedron.D.X -
                Tetrahedron.B.Y * Tetrahedron.C.Z * Tetrahedron.D.X + Tetrahedron.A.Y * Tetrahedron.C.Z * Tetrahedron.D.X +
                Tetrahedron.A.Z * Tetrahedron.B.Y * Tetrahedron.D.X - Tetrahedron.A.Y * Tetrahedron.B.Z * Tetrahedron.D.X -
                Tetrahedron.B.Z * Tetrahedron.C.X * Tetrahedron.D.Y + Tetrahedron.A.Z * Tetrahedron.C.X * Tetrahedron.D.Y +
                Tetrahedron.B.X * Tetrahedron.C.Z * Tetrahedron.D.Y - Tetrahedron.A.X * Tetrahedron.C.Z * Tetrahedron.D.Y -
                Tetrahedron.A.Z * Tetrahedron.B.X * Tetrahedron.D.Y + Tetrahedron.A.X * Tetrahedron.B.Z * Tetrahedron.D.Y +
                Tetrahedron.B.Y * Tetrahedron.C.X * Tetrahedron.D.Z - Tetrahedron.A.Y * Tetrahedron.C.X * Tetrahedron.D.Z -
                Tetrahedron.B.X * Tetrahedron.C.Y * Tetrahedron.D.Z + Tetrahedron.A.X * Tetrahedron.C.Y * Tetrahedron.D.Z +
                Tetrahedron.A.Y * Tetrahedron.B.X * Tetrahedron.D.Z - Tetrahedron.A.X * Tetrahedron.B.Y * Tetrahedron.D.Z -
                Tetrahedron.A.Z * Tetrahedron.B.Y * Tetrahedron.C.X + Tetrahedron.A.Y * Tetrahedron.B.Z * Tetrahedron.C.X +
                Tetrahedron.A.Z * Tetrahedron.B.X * Tetrahedron.C.Y - Tetrahedron.A.X * Tetrahedron.B.Z * Tetrahedron.C.Y -
                Tetrahedron.A.Y * Tetrahedron.B.X * Tetrahedron.C.Z + Tetrahedron.A.X * Tetrahedron.B.Y * Tetrahedron.C.Z;
        }

        /// <summary>
        /// Gets the ordered faces of the specified tetrahedron.
        /// </summary>
        public static Quadruple<Triple<T>> Faces<T>(Quadruple<T> Tetrahedron)
            where T : IEquatable<T>
        {
            return new Quadruple<Triple<T>>(
                new Triple<T>(Tetrahedron.A, Tetrahedron.D, Tetrahedron.B),
                new Triple<T>(Tetrahedron.B, Tetrahedron.D, Tetrahedron.C),
                new Triple<T>(Tetrahedron.C, Tetrahedron.D, Tetrahedron.A),
                new Triple<T>(Tetrahedron.A, Tetrahedron.B, Tetrahedron.C));
        }
    }
}