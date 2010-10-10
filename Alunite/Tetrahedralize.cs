using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Describes a valid collection of interconnected tetrahedra that encompass a volume. This also
    /// contains information about the boundary and interior triangles.
    /// </summary>
    /// <typeparam name="T">Type that represents a point.</typeparam>
    public class TetrahedralMesh<T>
        where T : IEquatable<T>
    {
        public TetrahedralMesh()
        {
            this._Tetrahedra = new HashSet<Tetrahedron<T>>();
            this._Boundaries = new Dictionary<Triangle<T>, Tetrahedron<T>>();
            this._Interiors = new Dictionary<Triangle<T>, Tetrahedron<T>>();
        }

        /// <summary>
        /// Adds a tetrahedron to the tetrahedral mesh, ensuring it does not conflict with any
        /// other tetrahedra.
        /// </summary>
        public void Add(Tetrahedron<T> Tetrahedron)
        {
            foreach (Triangle<T> face in Tetrahedron.Faces)
            {
                Tetrahedron<T> bound;
                if (this._Boundaries.TryGetValue(face.Flip, out bound))
                {
                    this._Interiors.Add(face.Flip, bound);
                    this._Interiors.Add(face, Tetrahedron);
                    this._Boundaries.Remove(face.Flip);
                }
                else
                {
                    this._Boundaries.Add(face, Tetrahedron);
                }
            }
            this._Tetrahedra.Add(Tetrahedron);
        }

        /// <summary>
        /// Removes a tetrahedron from the tetrahedral mesh.
        /// </summary>
        public bool Remove(Tetrahedron<T> Tetrahedron)
        {
            if (this._Tetrahedra.Remove(Tetrahedron))
            {
                foreach (Triangle<T> face in Tetrahedron.Faces)
                {
                    if (this._Interiors.ContainsKey(face))
                    {
                        this._Boundaries.Add(face.Flip, this._Interiors[face.Flip]);
                        this._Interiors.Remove(face);
                        this._Interiors.Remove(face.Flip);
                    }
                    else
                    {
                        this._Boundaries.Remove(face);
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the tetrahedron that has the specified interior face.
        /// </summary>
        public Tetrahedron<T>? GetInterior(Triangle<T> Face)
        {
            Tetrahedron<T> tetra;
            if (this._Interiors.TryGetValue(Face, out tetra))
            {
                return tetra;
            }
            return null;
        }

        /// <summary>
        /// Gets if the mesh contains the specified tetrahedron.
        /// </summary>
        public bool Contains(Tetrahedron<T> Tetrahedron)
        {
            return this._Tetrahedra.Contains(Tetrahedron);
        }

        /// <summary>
        /// Gets all the tetrahedra in the mesh.
        /// </summary>
        public IEnumerable<Tetrahedron<T>> Tetrahedra
        {
            get
            {
                return this._Tetrahedra;
            }
        }

        /// <summary>
        /// Gets all the boundaries of the mesh, along with the tetrahedra the boundaries are on.
        /// </summary>
        public IEnumerable<KeyValuePair<Triangle<T>, Tetrahedron<T>>> Boundaries
        {
            get
            {
                return this._Boundaries;
            }
        }

        /// <summary>
        /// Gets the amount of tetrahedra in the mesh.
        /// </summary>
        public int Size
        {
            get
            {
                return this._Tetrahedra.Count;
            }
        }

        private HashSet<Tetrahedron<T>> _Tetrahedra;
        private Dictionary<Triangle<T>, Tetrahedron<T>> _Boundaries;
        private Dictionary<Triangle<T>, Tetrahedron<T>> _Interiors;
    }

    /// <summary>
    /// Contains methods for creating a tetrahedral mesh from a set of points or a PLC.
    /// </summary>
    public static class Tetrahedralize
    {
        /// <summary>
        /// Creates a delaunay tetrahedralization of the specified input vertices.
        /// </summary>
        public static ISequentialArray<Tetrahedron<int>> Delaunay<A>(A Input)
            where A : ISequentialArray<Vector>
        {
            StandardArray<int> mapping = new StandardArray<int>(new IntRange(0, Input.Count));
            Sort.InPlace<StandardArray<int>, int>(mapping, x => Vector.Compare(Input.Lookup(x.A), Input.Lookup(x.B)));
            ISequentialArray<Tetrahedron<int>> mappedtetras = DelaunayOrdered(new MapSequentialArray<int, Vector>(mapping, x => Input.Lookup(x)));
            StandardArray<Tetrahedron<int>> tetras = new StandardArray<Tetrahedron<int>>(mappedtetras);
            tetras.Map(x => new Tetrahedron<int>(mapping.Lookup(x.A), mapping.Lookup(x.B), mapping.Lookup(x.C), mapping.Lookup(x.D)));
            return tetras;
        }

        /// <summary>
        /// Creates a delaunay tetrahedralization for a set of vertices that are guaranteed to be ordered. The
        /// algorithim used is described in the article "3-D TRIANGULATIONS FROM LOCAL TRANSFORMATIONS".
        /// </summary>
        public static ISequentialArray<Tetrahedron<int>> DelaunayOrdered<A>(A Input)
            where A : ISequentialArray<Vector>
        {
            TetrahedralMesh<int> mesh = new TetrahedralMesh<int>();
            if (Input.Count > 4)
            {
                // Form initial tetrahedron.
                Tetrahedron<int> first = new Tetrahedron<int>(0, 1, 2, 3);
                Tetrahedron<Vector> firstactual = Tetrahedron.Dereference<A, int, Vector>(first, Input);
                if (!Tetrahedron.Order(firstactual))
                {
                    first = first.Flip;
                }
                Vector centroid = Tetrahedron.Midpoint(firstactual);
                mesh.Add(first);

                // Begin incremental addition
                Stack<Triangle<int>> newinteriors = new Stack<Triangle<int>>();
                for (int i = 5; i < Input.Count; i++)
                {
                    Vector v = Input.Lookup(i);

                    // Add tetrahedrons to exterior of convex hull
                    List<Tetrahedron<int>> newtetras = new List<Tetrahedron<int>>();
                    foreach (KeyValuePair<Triangle<int>, Tetrahedron<int>> kvp in mesh.Boundaries)
                    {
                        Triangle<int> bound = kvp.Key;
                        if (Triangle.Front(v, new Triangle<Vector>(Input.Lookup(bound.A), Input.Lookup(bound.B), Input.Lookup(bound.C))))
                        {
                            Tetrahedron<int> tetra = new Tetrahedron<int>(i, bound.A, bound.B, bound.C);
                            newtetras.Add(tetra);
                            newinteriors.Push(bound);
                        }
                    }

                    // Apply to tetrahedron set.
                    foreach (Tetrahedron<int> newtetra in newtetras)
                    {
                        mesh.Add(newtetra);
                    }

                    // Refine tetrahedras to have the delaunay property.
                    while (newinteriors.Count > 0)
                    {
                        Triangle<int> bound = newinteriors.Pop();
                        Tetrahedron<int>? interiortetra = mesh.GetInterior(bound);
                        if (interiortetra != null)
                        {
                            Tetrahedron<int> hulla = Tetrahedron.Align(interiortetra.Value, bound).GetValueOrDefault();
                            Tetrahedron<int> hullb = Tetrahedron.Align(mesh.GetInterior(bound.Flip).Value, bound.Flip).GetValueOrDefault();
                            Vector hullaver = Input.Lookup(hulla.Vertex);
                            Vector hullbver = Input.Lookup(hullb.Vertex);
                            Triangle<Vector> boundver = new Triangle<Vector>(Input.Lookup(bound.A), Input.Lookup(bound.B), Input.Lookup(bound.C));
                            Vector hullacircumcenter = Tetrahedron.Circumcenter(new Tetrahedron<Vector>(hullaver, boundver));
                            double hullacircumradius = (hullaver - hullacircumcenter).Length;
                            if ((hullbver - hullacircumcenter).Length < hullacircumradius)
                            {
                                bool transform = true;

                                // Delaunay property needs fixin
                                // Start by determining if the two tetrahedra's form a convex shape
                                double doubledummy;
                                Vector vectordummy;
                                if (Triangle.Intersect(boundver, hullbver, hullaver, out doubledummy, out vectordummy))
                                {
                                    mesh.Remove(hulla);
                                    mesh.Remove(hullb);
                                    mesh.Add(new Tetrahedron<int>(bound.A, bound.B, hullb.Vertex, hulla.Vertex));
                                    mesh.Add(new Tetrahedron<int>(bound.B, bound.C, hullb.Vertex, hulla.Vertex));
                                    mesh.Add(new Tetrahedron<int>(bound.C, bound.A, hullb.Vertex, hulla.Vertex));
                                }
                                else
                                {
                                    transform = false;

                                    // The triangle pair is concave, find the missing piece
                                    foreach (KeyValuePair<int, Edge<int>> edge in new KeyValuePair<int, Edge<int>>[]
                                        {
                                            new KeyValuePair<int, Edge<int>>(bound.C, new Edge<int>(bound.A, bound.B)),
                                            new KeyValuePair<int, Edge<int>>(bound.A, new Edge<int>(bound.B, bound.C)),
                                            new KeyValuePair<int, Edge<int>>(bound.B, new Edge<int>(bound.C, bound.A))
                                        })
                                    {
                                        Tetrahedron<int> other = new Tetrahedron<int>(hulla.Vertex, hullb.Vertex, edge.Value.A, edge.Value.B);
                                        if (mesh.Contains(other))
                                        {
                                            // Merge all three pieces into two
                                            mesh.Remove(hulla);
                                            mesh.Remove(hullb);
                                            mesh.Remove(other);

                                            // Relabel to make this process easier
                                            bound = new Triangle<int>(hulla.Vertex, hullb.Vertex, edge.Key);
                                            hulla = new Tetrahedron<int>(edge.Value.B, bound);
                                            hullb = new Tetrahedron<int>(edge.Value.A, bound.Flip);

                                            mesh.Add(hulla);
                                            mesh.Add(hullb);

                                            transform = true;
                                            break;
                                        }
                                    }
                                }

                                // Extend the stack if there was a transformation.
                                if (transform)
                                {
                                    foreach (Triangle<int> possiblebound in new Triangle<int>[]
                                        {
                                            new Triangle<int>(hullb.Vertex, bound.A, bound.B),
                                            new Triangle<int>(hullb.Vertex, bound.B, bound.C),
                                            new Triangle<int>(hullb.Vertex, bound.C, bound.A),
                                            new Triangle<int>(hulla.Vertex, bound.B, bound.A),
                                            new Triangle<int>(hulla.Vertex, bound.C, bound.B),
                                            new Triangle<int>(hulla.Vertex, bound.A, bound.C)
                                        })
                                    {
                                        if (!newinteriors.Contains(possiblebound) && mesh.GetInterior(possiblebound) != null)
                                        {
                                            newinteriors.Push(possiblebound);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                }
            }
            return new StandardArray<Tetrahedron<int>>(mesh.Tetrahedra, mesh.Size);
        }

        /// <summary>
        /// Gets the boundary triangles (triangles which are one the face of exactly one tetrahedron) of the specified 
        /// tetrahedral mesh. 
        /// </summary>
        public static ISequentialArray<Triangle<int>> Boundary(ISequentialArray<Tetrahedron<int>> Mesh)
        {
            // Since lookups and modifications on hashsets are in constant time, this function runs in linear time.
            HashSet<Triangle<int>> cur = new HashSet<Triangle<int>>();
            foreach (Tetrahedron<int> tetra in Mesh.Values) 
            {
                foreach (Triangle<int> tri in tetra.Faces)
                {
                    if (!cur.Remove(tri.Flip))
                    {
                        cur.Add(tri);
                    }
                }
            }
            return new StandardArray<Triangle<int>>(cur, cur.Count);
        }
    }
}