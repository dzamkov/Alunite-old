using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Alunite
{
    /// <summary>
    /// Functions for finding triangular or tetrahedronal connections in a set of
    /// vertices.
    /// </summary>
    public static class Triangulation
    {
        /// <summary>
        /// Creates a triangulation for the specified points.
        /// </summary>
        public static void Triangulate<A, I>(A Input, out HashSet<Triple<I>> Surface, out HashSet<Quadruple<I>> Volume)
            where A : IArray<Vector, I>
            where I : IEquatable<I>
        {
            Surface = new HashSet<Triple<I>>();
            Volume = new HashSet<Quadruple<I>>();

            // Add points one by one
            List<KeyValuePair<I, Vector>> first = new List<KeyValuePair<I, Vector>>(4);
            foreach (KeyValuePair<I, Vector> point in Input.Items)
            {
                // First four items become a tetrahedron.
                if (first != null)
                {
                    first.Add(point);
                    if (first.Count == 4)
                    {
                        Quadruple<Vector> vecs = new Quadruple<Vector>(
                            first[0].Value, first[1].Value, first[2].Value, first[3].Value);
                        if (Tetrahedron.Determinant(vecs) < 0.0)
                        {
                            KeyValuePair<I, Vector> temp = first[0];
                            first[0] = first[1];
                            first[1] = temp;
                        }

                        Quadruple<I> inds = new Quadruple<I>(first[0].Key, first[1].Key, first[2].Key, first[3].Key);
                        Quadruple<Triple<I>> tris = Tetrahedron.Faces(inds);
                        Volume.Add(inds);
                        foreach (Triple<I> tri in tris.Items)
                        {
                            Surface.Add(tri);
                        }

                        first = null;
                    }
                }
                else
                {

                }
            }
        }

        /// <summary>
        /// Gets the actual triangles from an indexed source with the specified array.
        /// </summary>
        public static IEnumerable<Triple<Vector>> EnumerateTriangles(IEnumerable<Triple<int>> Source, IArray<Vector, int> Array)
        {
            foreach (Triple<int> tri in Source)
            {
                yield return new Triple<Vector>(Array.Item(tri.A), Array.Item(tri.B), Array.Item(tri.C));
            }
        }

        /// <summary>
        /// Draws a set of points.
        /// </summary>
        public static void DebugDraw(IEnumerable<Vector> Points)
        {
            GL.Begin(BeginMode.Points);
            GL.Color3(1.0, 1.0, 1.0);

            foreach (Vector p in Points)
            {
                GL.Vertex3(p);
            }

            GL.End();
        }

        /// <summary>
        /// Draws a set of triangles.
        /// </summary>
        public static void DebugDraw(IEnumerable<Triple<Vector>> Tris)
        {
            GL.Begin(BeginMode.Triangles);
            GL.Color3(1.0, 0.0, 0.0);

            foreach (Triple<Vector> tri in Tris)
            {
                foreach (Vector v in tri.Items)
                {
                    GL.Vertex3(v);
                }
            }

            GL.End();
        }
    }
}