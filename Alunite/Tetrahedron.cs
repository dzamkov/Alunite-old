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
        /// Gets the ordered points for the faces of this tetrahedron.
        /// </summary>
        public T[] FacePoints
        {
            get
            {
                return new T[]
                {
                    this.A, this.B, this.C,
                    this.B, this.A, this.D,
                    this.C, this.D, this.A,
                    this.D, this.C, this.B
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
        /// Gets a flipped form of the tetrahedron (same points, different order).
        /// </summary>
        public Tetrahedron<T> Flip
        {
            get
            {
                return new Tetrahedron<T>(this.A, this.B, this.D, this.C);
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
    /// Tetrahedron (3-simplex) related maths.
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
        /// Finds the borders in an array of tetrahedra. Looking up a value in a resulting tetrahedron will get the index for
        /// the tetrahedron that borders it at the face opposite the point that was used or -1 if there is no border on that face.
        /// </summary>
        public static StandardArray<Tetrahedron<int>> Borders<V>(StandardArray<Tetrahedron<V>> Tetrahedrons)
            where V : IEquatable<V>
        {
            Dictionary<Triangle<V>, KeyValuePair<int, int>> opentriangles = new Dictionary<Triangle<V>, KeyValuePair<int, int>>();
            int[,] borders = new int[Tetrahedrons.Count, 4]; // 0 means no border, every value after is offset by one.

            // Determine connections for the tetrahedrons.
            foreach (KeyValuePair<int, Tetrahedron<V>> kvp in Tetrahedrons.Items)
            {
                Triangle<V>[] tris = kvp.Value.Faces;
                for(int t = 0; t < tris.Length; t++)
                {
                    Triangle<V> tri = tris[t];
                    KeyValuePair<int, int> val;
                    if (opentriangles.TryGetValue(tri, out val))
                    {
                        borders[kvp.Key, t] = val.Key + 1;
                        borders[val.Key, val.Value] = kvp.Key + 1;
                    }
                    else
                    {
                        opentriangles.Add(tri.Flip, new KeyValuePair<int, int>(kvp.Key, t));
                    }
                }
            }

            // Stuff border info into the tetrahedron array.
            Tetrahedron<int>[] nodes = new Tetrahedron<int>[Tetrahedrons.Count];
            for (int t = 0; t < nodes.Length; t++)
            {
                nodes[t] = new Tetrahedron<int>(
                    borders[t, 0] - 1,
                    borders[t, 1] - 1,
                    borders[t, 2] - 1,
                    borders[t, 3] - 1);
            }

            return new StandardArray<Tetrahedron<int>>(nodes);
        }

        

        /// <summary>
        /// Gets the points on the faces of the specified tetrahedron.
        /// </summary>
        public static T[] FacePoints<T>(Tetrahedron<T> Tetrahedron)
            where T : IEquatable<T>
        {
            return Tetrahedron.FacePoints;
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
        /// Gets the midpoint (centroid) of the specified tetrahedron.
        /// </summary>
        public static Vector Midpoint(Tetrahedron<Vector> Tetrahedron)
        {
            return (Tetrahedron.A + Tetrahedron.B + Tetrahedron.C + Tetrahedron.D) * (1.0 / 4.0);
        }

        /// <summary>
        /// Creates tetrahedra (5) to occupy the entire volume of a cube with the specified points. The points in
        /// the array are ordered (0, 0, 0), (1, 0, 0), (0, 1, 0), (1, 1, 0), (0, 0, 1), (1, 0, 1), (0, 1, 1), (1, 1, 1). An additional
        /// parameter can be used to reverse the the axies the tetrahedra are created on (the order will still remain possible).
        /// </summary>
        public static Tetrahedron<T>[] Tesselate<T>(T[] Cube, bool Reverse)
            where T : IEquatable<T>
        {
            if (Reverse)
            {
                return new Tetrahedron<T>[]
                {
                    new Tetrahedron<T>(Cube[1], Cube[5], Cube[0], Cube[3]),
                    new Tetrahedron<T>(Cube[2], Cube[6], Cube[3], Cube[0]),
                    new Tetrahedron<T>(Cube[7], Cube[3], Cube[6], Cube[5]),
                    new Tetrahedron<T>(Cube[4], Cube[0], Cube[5], Cube[6]),
                    new Tetrahedron<T>(Cube[5], Cube[0], Cube[3], Cube[6]),
                };
            }
            else
            {
                return new Tetrahedron<T>[]
                {
                    new Tetrahedron<T>(Cube[0], Cube[4], Cube[2], Cube[1]),
                    new Tetrahedron<T>(Cube[3], Cube[7], Cube[1], Cube[2]),
                    new Tetrahedron<T>(Cube[5], Cube[1], Cube[7], Cube[4]),
                    new Tetrahedron<T>(Cube[6], Cube[2], Cube[4], Cube[7]),
                    new Tetrahedron<T>(Cube[4], Cube[2], Cube[1], Cube[7]),
                };
            }
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