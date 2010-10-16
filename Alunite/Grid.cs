using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Contains methods for creating uniform grids.
    /// </summary>
    public static class Grid
    {
        /// <summary>
        /// Creates an array of vectors in a uniform grid.
        /// </summary>
        public static IArray<Vector> Points(Vector Origin, Vector Unit, LVector Size)
        {
            Vector[] vecs = new Vector[Size.X * Size.Y * Size.Z];
            LVector cur = new LVector();
            for (int t = 0; t < vecs.Length; t++)
            {
                vecs[t] = Origin + Vector.Scale(Unit, cur);
                cur.X++;
                if (cur.X >= Size.X)
                {
                    cur.Y++;
                    cur.X = 0;
                }
                if (cur.Y >= Size.Y)
                {
                    cur.Z++;
                    cur.Y = 0;
                }
            }
            return Data.Create(vecs);
        }

        /// <summary>
        /// Creates a weakly delaunay tesselation of a grid with the specified point size. The start and size
        /// of the tesselated area is also specified.
        /// </summary>
        public static ISet<Tetrahedron<int>> Tesselate(LVector PointSize, LVector Start, LVector Size)
        {
            List<Tetrahedron<int>> tetras = new List<Tetrahedron<int>>();
            int[] inds = new int[8];
            for (int x = 0; x < Size.X; x++)
            {
                int rx = x + Start.X;
                for (int y = 0; y < Size.Y; y++)
                {
                    int ry = y + Start.Y;
                    for (int z = 0; z < Size.Z; z++)
                    {
                        int rz = z + Start.Z;
                        int baseind = rx + ry * PointSize.X + rz * PointSize.X * PointSize.Y;
                        for (int t = 0; t < inds.Length; t++)
                        {
                            inds[t] = baseind;
                        }
                        for (int t = 0; t < 4; t++)
                        {
                            inds[4 + t] += PointSize.X * PointSize.Y;
                            inds[2 + ((t / 2) * 2) + t] += PointSize.X;
                            inds[1 + (t * 2)] += 1;
                        }

                        tetras.AddRange(Tetrahedron.Tesselate(inds, (rx + ry + rz) % 2 == 0));
                    }
                }
            }
            return new SimpleSet<Tetrahedron<int>>(tetras, tetras.Count);
        }
    }
}