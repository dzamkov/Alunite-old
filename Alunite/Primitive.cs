using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Contains functions for creating a static primitive object.
    /// </summary>
    public struct Primitive
    {
        public Primitive(ISet<Tetrahedron<int>> Tetrahedra, IArray<Vector> Vertices)
        {
            this.Vertices = Vertices;
            this.Tetrahedra = Tetrahedra;
        }

        /// <summary>
        /// Creates a weakly delaunay cube with the specified edge length.
        /// </summary>
        public static Primitive Cube(double EdgeLength)
        {
            Vector[] vertices = new Vector[8];
            double hel = EdgeLength / 2;
            for (int t = 0; t < vertices.Length; t++)
            {
                vertices[t] = new Vector(
                    (t % 8 < 4) ? -hel : hel,
                    (t % 4 < 2) ? -hel : hel,
                    (t % 2 < 1) ? -hel : hel);
            }

            int[] inds = new int[8];
            for (int t = 0; t < inds.Length; t++)
            {
                inds[t] = 7 - t;
            }

            return new Primitive(Set.Create(Tetrahedron.Tesselate(inds, false)), Data.Create(vertices));
        }

        public ISet<Tetrahedron<int>> Tetrahedra;
        public IArray<Vector> Vertices;
    }
}