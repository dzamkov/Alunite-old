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
        public static void Triangulate<A, I>(A Input, out HashSet<Triangle<I>> Surface, out HashSet<Tetrahedron<I>> Volume)
            where A : IArray<Vector, I>
            where I : IEquatable<I>
        {
            Surface = new HashSet<Triangle<I>>();
            Volume = new HashSet<Tetrahedron<I>>();

            // Add points one by one
            List<KeyValuePair<I, Vector>> points = new List<KeyValuePair<I, Vector>>();
            foreach (KeyValuePair<I, Vector> point in Input.Items)
            {
                // First four items become a tetrahedron.
                points.Add(point);
                if (points.Count == 4)
                {
                    Tetrahedron<Vector> vecs = new Tetrahedron<Vector>(
                        points[0].Value, points[1].Value, points[2].Value, points[3].Value);
                    if (Tetrahedron.Determinant(vecs) < 0.0)
                    {
                        KeyValuePair<I, Vector> temp = points[0];
                        points[0] = points[1];
                        points[1] = temp;
                    }

                    Tetrahedron<I> inds = new Tetrahedron<I>(points[0].Key, points[1].Key, points[2].Key, points[3].Key);
                    Volume.Add(inds);
                    foreach (Triangle<I> tri in inds.Faces)
                    {
                        Surface.Add(tri);
                    }
                }

                // Add next points incrementally.
                if (points.Count > 4)
                {

                }
            }
        }

        /// <summary>
        /// Gets the actual triangles from an indexed source with the specified array.
        /// </summary>
        public static IEnumerable<Triangle<Vector>> EnumerateTriangles(IEnumerable<Triangle<int>> Source, IArray<Vector, int> Array)
        {
            foreach (Triangle<int> tri in Source)
            {
                yield return new Triangle<Vector>(Array.Item(tri.A), Array.Item(tri.B), Array.Item(tri.C));
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
        public static void DebugDraw(IEnumerable<Triangle<Vector>> Tris)
        {
            GL.Begin(BeginMode.Triangles);
            GL.Color3(1.0, 0.0, 0.0);

            foreach (Triangle<Vector> tri in Tris)
            {
                foreach (Vector v in tri.Points)
                {
                    GL.Vertex3(v);
                }
            }

            GL.End();
        }
    }
}