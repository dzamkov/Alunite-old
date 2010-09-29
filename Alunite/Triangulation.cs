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
            where A : IFiniteArray<Vector, I>
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
                    if (!Tetrahedron.Order(vecs))
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
                    // Check if point is already in volume in O(WTF) time
                    Tetrahedron<I>? inside = null;
                    foreach (Tetrahedron<I> tet in Volume)
                    {
                        if (Tetrahedron.In(point.Value,
                            new Tetrahedron<Vector>(
                                Input.Lookup(tet.A),
                                Input.Lookup(tet.B),
                                Input.Lookup(tet.C),
                                Input.Lookup(tet.D))))
                        {
                            inside = tet;
                            break;
                        }
                    }


                    if (inside.HasValue)
                    {
                        // Split tetrahedron
                        Tetrahedron<I> tet = inside.Value;
                        Volume.Remove(tet);
                        Volume.UnionWith(tet.Split(point.Key));
                    }
                    else
                    {
                        // Add point to surface
                        Vector vec = point.Value;
                        List<Triangle<I>> toremove = new List<Triangle<I>>();
                        List<Triangle<I>> toadd = new List<Triangle<I>>();
                        foreach (Triangle<I> tri in Surface)
                        {
                            Triangle<Vector> vectri = new Triangle<Vector>(Input.Lookup(tri.A), Input.Lookup(tri.B), Input.Lookup(tri.C));
                            if (Tetrahedron.Order(new Tetrahedron<Vector>(vec, vectri.Flip)))
                            {
                                Volume.Add(new Tetrahedron<I>(point.Key, tri.Flip));
                                toremove.Add(tri);
                                foreach (Triangle<I> vtri in new Tetrahedron<I>(point.Key, tri.Flip).VertexFaces)
                                {
                                    toadd.Add(vtri);
                                    toremove.Add(vtri.Flip);
                                }
                            }
                        }
                        Surface.UnionWith(toadd);
                        Surface.ExceptWith(toremove);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the edges connecting the specified tetrahedrons.
        /// </summary>
        /// <typeparam name="IT">Index type for tetrahedrons</typeparam>
        /// <typeparam name="V">Type of vertice stored by tetrahedrons</typeparam>
        public static void Edges<AT, IT, V>(AT Tetrahedrons, out HashSet<Edge<IT>> Edges)
            where AT : IFiniteArray<Tetrahedron<V>, IT>
            where IT : IEquatable<IT>
            where V : IEquatable<V>
        {
            Edges = new HashSet<Edge<IT>>();
            Dictionary<Triangle<V>, IT> openfaces = new Dictionary<Triangle<V>, IT>();
            foreach (KeyValuePair<IT, Tetrahedron<V>> item in Tetrahedrons.Items)
            {
                // Try pairing together faces.
                Tetrahedron<V> tetra = item.Value;
                foreach (Triangle<V> face in tetra.Faces)
                {
                    IT connectedtetra;
                    if (openfaces.TryGetValue(face, out connectedtetra))
                    {
                        Edges.Add(new Edge<IT>(item.Key, connectedtetra));
                        openfaces.Remove(face);
                    }
                    else
                    {
                        openfaces.Add(face.Flip, item.Key);
                    }
                }
            }
        }

        /// <summary>
        /// Dereferences a collection of tetrahedrons through an array.
        /// </summary>
        public static IEnumerable<Tetrahedron<V>> Dereference<A, I, V>(A Vertices, IEnumerable<Tetrahedron<I>> Tetrahedrons)
            where A : IArray<V, I>
            where I : IEquatable<I>
            where V : IEquatable<V>
        {
            foreach (Tetrahedron<I> tetra in Tetrahedrons)
            {
                yield return new Tetrahedron<V>(
                    Vertices.Lookup(tetra.A),
                    Vertices.Lookup(tetra.B),
                    Vertices.Lookup(tetra.C),
                    Vertices.Lookup(tetra.D));
            }
        }

        /// <summary>
        /// Dereferences a collection of edges through an array.
        /// </summary>
        public static IEnumerable<Edge<V>> Dereference<A, I, V>(A Vertices, IEnumerable<Edge<I>> Edges)
            where A : IArray<V, I>
            where I : IEquatable<I>
            where V : IEquatable<V>
        {
            foreach (Edge<I> edge in Edges)
            {
                yield return new Edge<V>(
                    Vertices.Lookup(edge.A),
                    Vertices.Lookup(edge.B));
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

        /// <summary>
        /// Draws a set of tetrahedrons
        /// </summary>
        public static void DebugDraw(IEnumerable<Tetrahedron<Vector>> Tetras)
        {
            GL.Begin(BeginMode.Triangles);
            GL.Color3(1.0, 1.0, 0.0);

            foreach (Tetrahedron<Vector> tetra in Tetras)
            {
                foreach (Triangle<Vector> tri in tetra.Faces)
                {
                    foreach (Vector v in tri.Points)
                    {
                        GL.Vertex3(v);
                    }
                }
            }

            GL.End();
        }

        /// <summary>
        /// Draws a set of edges.
        /// </summary>
        public static void DebugDraw(IEnumerable<Edge<Vector>> Edges)
        {
            GL.Begin(BeginMode.Lines);
            GL.Color3(0.0, 1.0, 1.0);

            foreach (Edge<Vector> edge in Edges)
            {
                GL.Vertex3(edge.A);
                GL.Vertex3(edge.B);
            }

            GL.End();
        }
    }
}