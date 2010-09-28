using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// A group of four homogenous items arranged to form a tetrahedron. Tetrahedrons are equal to
    /// all other triangles with the same values that are arranged in the same direction.
    /// </summary>
    public struct Tetrahedron<T> : IEquatable<Tetrahedron<T>>
        where T : IEquatable<T>
    {
        public Tetrahedron(T A, T B, T C, T D)
        {
            this.A = A;
            this.B = B;
            this.C = C;
            this.D = D;
        }

        public Tetrahedron(T Vertex, Triangle<T> Base)
        {
            this.A = Vertex;
            this.B = Base.C;
            this.C = Base.B;
            this.D = Base.A;
        }

        public bool Equals(Tetrahedron<T> Tetrahedron)
        {
            return this == Tetrahedron;
        }

        public static bool operator ==(Tetrahedron<T> A, Tetrahedron<T> B)
        {
            T avertex = A.A;
            Triangle<T> abase = new Triangle<T>(A.B, A.C, A.D);
            if (avertex.Equals(B.A))
            {
                return abase == new Triangle<T>(B.B, B.C, B.D);
            }
            if (avertex.Equals(B.B))
            {
                return abase == new Triangle<T>(B.C, B.D, B.A);
            }
            if (avertex.Equals(B.C))
            {
                return abase == new Triangle<T>(B.D, B.A, B.B);
            }
            if (avertex.Equals(B.D))
            {
                return abase == new Triangle<T>(B.A, B.B, B.C);
            }
            return false;
        }

        public static bool operator !=(Tetrahedron<T> A, Tetrahedron<T> B)
        {
            return !(A == B);
        }

        public override bool Equals(object obj)
        {
            Tetrahedron<T>? tetra = obj as Tetrahedron<T>?;
            if (tetra != null)
            {
                return this == tetra;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int a = this.A.GetHashCode();
            int b = this.B.GetHashCode();
            int c = this.C.GetHashCode();
            int d = this.D.GetHashCode();

            // This is probably wrong
            if ((a > b) ^ (b > c) ^ (c > a) ^
                (b > a) ^ (a > d) ^ (d > b) ^
                (c > d) ^ (d > a) ^ (a > c) ^
                (d > c) ^ (c > b) ^ (b > d))
            {
                return a ^ b ^ c ^ d;
            }
            else
            {
                return ~(a ^ b ^ c ^ d);
            }
        }

        /// <summary>
        /// Gets the faces of this tetrahedron.
        /// </summary>
        public Triangle<T>[] Faces
        {
            get
            {
                return new Triangle<T>[]
                {
                    new Triangle<T>(this.A, this.B, this.C),
                    new Triangle<T>(this.B, this.A, this.D),
                    new Triangle<T>(this.C, this.D, this.A),
                    new Triangle<T>(this.D, this.C, this.B)
                };
            }
        }

        /// <summary>
        /// Gets the faces of this tetrahedron that contain the vertex (point A).
        /// </summary>
        public Triangle<T>[] VertexFaces
        {
            get
            {
                return new Triangle<T>[]
                {
                    new Triangle<T>(this.A, this.B, this.C),
                    new Triangle<T>(this.B, this.A, this.D),
                    new Triangle<T>(this.C, this.D, this.A)
                };
            }
        }

        /// <summary>
        /// Gets the points (hopefully 4) in the tetrahedron.
        /// </summary>
        public T[] Points
        {
            get
            {
                return new T[]
                {
                    this.A,
                    this.B,
                    this.C,
                    this.D
                };
            }
        }

        /// <summary>
        /// Splits the tetrahedron into four others that have the same order, share three of the vertices
        /// of the original tetrahedron and contain the specified mid value.
        /// </summary>
        public Tetrahedron<T>[] Split(T MidValue)
        {
            Triangle<T>[] faces = this.Faces;
            Tetrahedron<T>[] splits = new Tetrahedron<T>[faces.Length];
            for (int t = 0; t < splits.Length; t++)
            {
                splits[t] = new Tetrahedron<T>(MidValue, faces[t]);
            }
            return splits;
        }

        public T A;
        public T B;
        public T C;
        public T D;
    }

    /// <summary>
    /// Tetrahedron (4-simplex) related maths.
    /// </summary>
    public static class Tetrahedron
    {
        /// <summary>
        /// Calculates the determinant of a matrix in the form 
        /// [[A.X, A.Y, A.Z, 1.0], [B.X, B.Y, B.Z, 1.0], [C.X, C.Y, C.Z, 1.0], [D.X, D.Y, D.Z, 1.0]]
        /// </summary>
        public static double Determinant(Tetrahedron<Vector> Tetrahedron)
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
        /// Gets the order of the specified tetrahedron. Swapping any two points in the tetrahedron negates the order. The tetrahedron
        /// has the same order as all of its triangles.
        /// </summary>
        public static bool Order(Tetrahedron<Vector> Tetrahedron)
        {
            return Determinant(Tetrahedron) > 0;
        }

        /// <summary>
        /// Gets if the specified point is in the given tetrahedron.
        /// </summary>
        public static bool In(Vector Point, Tetrahedron<Vector> Tetrahedron)
        {
            bool order = Order(Tetrahedron);
            return
                Order(new Tetrahedron<Vector>(Point, Tetrahedron.B, Tetrahedron.C, Tetrahedron.D)) == order &&
                Order(new Tetrahedron<Vector>(Tetrahedron.A, Point, Tetrahedron.C, Tetrahedron.D)) == order &&
                Order(new Tetrahedron<Vector>(Tetrahedron.A, Tetrahedron.B, Point, Tetrahedron.D)) == order &&
                Order(new Tetrahedron<Vector>(Tetrahedron.A, Tetrahedron.B, Tetrahedron.C, Point)) == order;
        }

        /// <summary>
        /// Gets the midpoint of the specified tetrahedron.
        /// </summary>
        public static Vector Midpoint(Tetrahedron<Vector> Tetrahedron)
        {
            return (Tetrahedron.A + Tetrahedron.B + Tetrahedron.C + Tetrahedron.D) * (1.0 / 4.0);
        }

        /*/// <summary>
        /// Gets if the two tetrahedrons overlap.
        /// </summary>
        /// <remarks>http://jgt.akpeters.com/papers/GanovelliPonchioRocchini02/</remarks>
        public static bool Overlap(Tetrahedron<Vector> A, Tetrahedron<Vector> B)
        {
            Triangle<Vector>[] tris = A.Faces;
            double[,] dots = new double[4, 4];
            uint[] masks = new uint[4];

            // Check which side all the points on B are on, compared to each triangle of A.
            for (int t = 0; t < tris.Length; t++)
            {
                Triangle<Vector> tri = tris[t];
                Vector norm = Triangle.Normal(tri);
                double dota = Vector.Dot(norm, B.A - tri.A); dots[t, 0] = dota;
                double dotb = Vector.Dot(norm, B.B - tri.A); dots[t, 1] = dotb;
                double dotc = Vector.Dot(norm, B.C - tri.A); dots[t, 2] = dotc;
                double dotd = Vector.Dot(norm, B.D - tri.A); dots[t, 3] = dotd;
                byte curmask = 0x00;
                
            }


            return false;
        }

        // For use only by overlap.
        private static bool _FaceA(ref int Index, ref Triangle<Vector> Face, ref Tetrahedron<Vector> Other, ref double[,] Dots, ref uint[] Masks)
        {
            Vector norm = Triangle.Normal(Face);
            double dota = Vector.Dot(norm, Other.A - Face.A); Dots[Index, 0] = dota;
            double dotb = Vector.Dot(norm, Other.B - Face.A); Dots[Index, 1] = dotb;
            double dotc = Vector.Dot(norm, Other.C - Face.A); Dots[Index, 2] = dotc;
            double dotd = Vector.Dot(norm, Other.D - Face.A); Dots[Index, 3] = dotd;
            uint mask = 0x00000000;
            if (dota > 0.0)
            {
                mask |= 1;
            }
            if (dotb > 0.0)
            {
                mask |= 2;
            }
            if (dotc > 0.0)
            {
                mask |= 4;
            }
            if (dotd > 0.0)
            {
                mask |= 8;
            }
            Masks[Index] = mask;
            return mask == 0;
        }*/ // Implement later
    }
}