using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Describes a valid collection of interconnected tetrahedra that encompass a volume. This also
    /// contains information about the boundary and interior triangles.
    /// </summary>
    /// <typeparam name="T">Type that represents a point.</typeparam>
    public class TetrahedralMesh<T> : ISet<Tetrahedron<T>>
        where T : IEquatable<T>
    {
        public TetrahedralMesh()
        {
            this._Tetrahedra = new HashSet<Tetrahedron<T>>();
            this._Boundaries = new Dictionary<Triangle<T>, Tetrahedron<T>>();
            this._Interiors = new Dictionary<Triangle<T>, Tetrahedron<T>>();
        }

        private TetrahedralMesh(
            HashSet<Tetrahedron<T>> Tetrahedra, 
            Dictionary<Triangle<T>, Tetrahedron<T>> Boundaries, 
            Dictionary<Triangle<T>, Tetrahedron<T>> Interiors)
        {
            this._Tetrahedra = Tetrahedra;
            this._Boundaries = Boundaries;
            this._Interiors = Interiors;
        }

        /// <summary>
        /// Creates a tetrahedral mesh from this mesh based on the specified point mapping with the requirement
        /// that the mapping function is one to one.
        /// </summary>
        public TetrahedralMesh<F> Map<F>(Func<T, F> Mapping)
            where F : IEquatable<F>
        {
            HashSet<Tetrahedron<F>> newtetras = new HashSet<Tetrahedron<F>>();
            Dictionary<Triangle<F>, Tetrahedron<F>> newbounds = new Dictionary<Triangle<F>, Tetrahedron<F>>();
            Dictionary<Triangle<F>, Tetrahedron<F>> newinteriors = new Dictionary<Triangle<F>, Tetrahedron<F>>();
            foreach (Tetrahedron<T> tet in this._Tetrahedra)
            {
                newtetras.Add(new Tetrahedron<F>(
                    Mapping(tet.A),
                    Mapping(tet.B),
                    Mapping(tet.C),
                    Mapping(tet.D)));
            }
            foreach (KeyValuePair<Triangle<T>, Tetrahedron<T>> bound in this._Boundaries)
            {
                newbounds.Add(
                    new Triangle<F>(
                        Mapping(bound.Key.A),
                        Mapping(bound.Key.B),
                        Mapping(bound.Key.C)),
                    new Tetrahedron<F>(
                        Mapping(bound.Value.A),
                        Mapping(bound.Value.B),
                        Mapping(bound.Value.C),
                        Mapping(bound.Value.D)));
            }
            foreach (KeyValuePair<Triangle<T>, Tetrahedron<T>> interior in this._Interiors)
            {
                newinteriors.Add(
                    new Triangle<F>(
                        Mapping(interior.Key.A),
                        Mapping(interior.Key.B),
                        Mapping(interior.Key.C)),
                    new Tetrahedron<F>(
                        Mapping(interior.Value.A),
                        Mapping(interior.Value.B),
                        Mapping(interior.Value.C),
                        Mapping(interior.Value.D)));
            }
            return new TetrahedralMesh<F>(newtetras, newbounds, newinteriors);
        }

        /// <summary>
        /// Tries adding a tetrahedron to the tetrahedral mesh. If this will result in a conflict, the tetrahedron
        /// is not added and false is returned.
        /// </summary>
        public bool Add(Tetrahedron<T> Tetrahedron)
        {
            if (this.CanAdd(Tetrahedron))
            {
                this.AddUnchecked(Tetrahedron);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Forces the specified tetrahedron to be added, causing the validity of the structure to
        /// fail if a conflict with another tetrahedron results.
        /// </summary>
        public void AddUnchecked(Tetrahedron<T> Tetrahedron)
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
        /// Gets if the specified tetrahedron can be added to the mesh without conflict.
        /// </summary>
        public bool CanAdd(Tetrahedron<T> Tetrahedron)
        {
            foreach (Triangle<T> face in Tetrahedron.Faces)
            {
                if (this._Interiors.ContainsKey(face) || this._Boundaries.ContainsKey(face))
                {
                    return false;
                }
            }
            return true;
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
        /// Merges the pentahedron defined by A and B (the area between them should be divided into three tetrahedra in
        /// the mesh, each tetrahedra sharing two points on the base and the vertices of A and B) into two tetrahedra. If merging the pentahedron
        /// will result in a conflict with other tetrahedra, no change is made and false is returned.
        /// </summary>
        public bool MergePentahedron(Tetrahedron<T> A, Tetrahedron<T> B)
        {
            Triangle<T> b = A.Base;
            Tetrahedron<T> olda = new Tetrahedron<T>(B.Vertex, A.Vertex, b.A, b.B);
            Tetrahedron<T> oldb = new Tetrahedron<T>(B.Vertex, A.Vertex, b.B, b.C);
            Tetrahedron<T> oldc = new Tetrahedron<T>(B.Vertex, A.Vertex, b.C, b.A);
            this.Remove(olda);
            this.Remove(oldb);
            this.Remove(oldc);
            if (!this.Add(A))
            {
                this.Add(olda);
                this.Add(oldb);
                this.Add(oldc);
                return false;
            }
            if (!this.Add(B))
            {
                this.Remove(A);
                this.Add(olda);
                this.Add(oldb);
                this.Add(oldc);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Splits the two tetrahedra sharing a common base into three tetrahedra. If splitting the pentahedron
        /// they create will result in a conflict with other tetrahedra, no change is made and false is returned.
        /// </summary>
        public bool SplitPentahedron(Tetrahedron<T> A, Tetrahedron<T> B)
        {
            Triangle<T> b = A.Base;
            Tetrahedron<T> newa = new Tetrahedron<T>(b.A, b.B, B.Vertex, A.Vertex);
            Tetrahedron<T> newb = new Tetrahedron<T>(b.B, b.C, B.Vertex, A.Vertex); // hehe
            Tetrahedron<T> newc = new Tetrahedron<T>(b.C, b.A, B.Vertex, A.Vertex);
            this.Remove(A);
            this.Remove(B);
            if (!this.Add(newa))
            {
                this.Add(A);
                this.Add(B);
                return false;
            }
            if (!this.Add(newb))
            {
                this.Remove(newa);
                this.Add(A);
                this.Add(B);
                return false;
            }
            if (!this.Add(newc))
            {
                this.Remove(newb);
                this.Remove(newa);
                this.Add(A);
                this.Add(B);
                return false;
            }
            return true;
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
        public IEnumerable<KeyValuePair<Triangle<T>, Tetrahedron<T>>> TetrahedraBoundaries
        {
            get
            {
                return this._Boundaries;
            }
        }

        /// <summary>
        /// Gets the triangles that form the boundaries of this tetrahedral mesh.
        /// </summary>
        public ISet<Triangle<T>> Boundaries
        {
            get
            {
                return new SimpleSet<Triangle<T>>(this._Boundaries.Keys, this._Boundaries.Count);
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

        public IEnumerable<Tetrahedron<T>> Items
        {
            get 
            {
                return this._Tetrahedra;
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
        public static TetrahedralMesh<int> Delaunay<A>(A Input)
            where A : IArray<Vector>
        {
            StandardArray<int> mapping = new StandardArray<int>(new IntRange(0, Input.Size));
            Sort.InPlace<StandardArray<int>, int>(mapping, x => Vector.Compare(Input.Lookup(x.A), Input.Lookup(x.B)));
            TetrahedralMesh<int> tetras = DelaunayOrdered(Data.Map(mapping, x => Input.Lookup(x)));
            tetras.Map(x => mapping.Lookup(x));
            return tetras;
        }

        /// <summary>
        /// Creates a delaunay tetrahedralization for a set of vertices that are guaranteed to be ordered. The
        /// algorithim used is described in the article "3-D TRIANGULATIONS FROM LOCAL TRANSFORMATIONS". Note that if any
        /// of the given points are coplanar, the tetrahedra outputted will not be delaunay and might not even have a positive
        /// volume.
        /// </summary>
        public static TetrahedralMesh<int> DelaunayOrdered<A>(A Input)
            where A : IArray<Vector>
        {
            TetrahedralMesh<int> mesh = new TetrahedralMesh<int>();
            if (Input.Size > 4)
            {
                // Form initial tetrahedron.
                Tetrahedron<int> first = new Tetrahedron<int>(0, 1, 2, 3);
                Tetrahedron<Vector> firstactual = Tetrahedron.Dereference<A, Vector>(first, Input);
                if (!Tetrahedron.Order(firstactual))
                {
                    first = first.Flip;
                }
                Vector centroid = Tetrahedron.Midpoint(firstactual);
                mesh.Add(first);

                // Begin incremental addition
                Stack<Triangle<int>> newinteriors = new Stack<Triangle<int>>();
                for (int i = 5; i < Input.Size; i++)
                {
                    Vector v = Input.Lookup(i);

                    // Add tetrahedrons to exterior of convex hull
                    List<Tetrahedron<int>> newtetras = new List<Tetrahedron<int>>();
                    foreach (KeyValuePair<Triangle<int>, Tetrahedron<int>> kvp in mesh.TetrahedraBoundaries)
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
                                // Delaunay property needs fixin
                                // Start by determining if the two tetrahedra's form a convex shape
                                double doubledummy;
                                Vector vectordummy;
                                bool transform = true;

                                // Tetrahedra are convex with no coplanar points
                                if (Triangle.Intersect(boundver, hullbver, hullaver, out doubledummy, out vectordummy))
                                {
                                    transform = mesh.SplitPentahedron(hulla, hullb);
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
                                            // Relabel to make this process easier
                                            bound = new Triangle<int>(hulla.Vertex, hullb.Vertex, edge.Key);
                                            hulla = new Tetrahedron<int>(edge.Value.B, bound);
                                            hullb = new Tetrahedron<int>(edge.Value.A, bound.Flip);

                                            // Merge all three pieces into two
                                            transform = mesh.MergePentahedron(hulla, hullb);
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
            return mesh;
        }

        /// <summary>
        /// Gets the boundary triangles (triangles which are one the face of exactly one tetrahedron) of the specified 
        /// tetrahedral mesh. 
        /// </summary>
        public static ISet<Triangle<int>> Boundary(ISet<Tetrahedron<int>> Mesh)
        {
            // Since lookups and modifications on hashsets are in constant time, this function runs in linear time.
            HashSet<Triangle<int>> cur = new HashSet<Triangle<int>>();
            foreach (Tetrahedron<int> tetra in Mesh.Items) 
            {
                foreach (Triangle<int> tri in tetra.Faces)
                {
                    if (!cur.Remove(tri.Flip))
                    {
                        cur.Add(tri);
                    }
                }
            }
            return new SimpleSet<Triangle<int>>(cur, cur.Count);
        }
    }
}