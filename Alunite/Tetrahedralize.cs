using System;
using System.Collections.Generic;

namespace Alunite
{

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
                    foreach (KeyValuePair<Triangle<int>, Tetrahedron<int>> kvp in mesh.TetrahedraBoundary)
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
                                if (Triangle.Intersect(boundver, new Segment<Vector>(hullbver, hullaver), out doubledummy, out vectordummy))
                                {
                                    transform = mesh.SplitPentahedron(hulla, hullb);
                                }
                                else
                                {
                                    transform = false;

                                    // The triangle pair is concave, find the missing piece
                                    foreach (KeyValuePair<int, Segment<int>> edge in new KeyValuePair<int, Segment<int>>[]
                                        {
                                            new KeyValuePair<int, Segment<int>>(bound.C, new Segment<int>(bound.A, bound.B)),
                                            new KeyValuePair<int, Segment<int>>(bound.A, new Segment<int>(bound.B, bound.C)),
                                            new KeyValuePair<int, Segment<int>>(bound.B, new Segment<int>(bound.C, bound.A))
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