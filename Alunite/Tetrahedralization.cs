using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Contains methods for creating a tetrahedral mesh from a set of points or a PLC.
    /// </summary>
    public static class Tetrahedralization
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
            HashSet<Tetrahedron<int>> tetras = new HashSet<Tetrahedron<int>>();
            var interiors = new Dictionary<Triangle<int>, Tetrahedron<int>>();
            var bounds = new Dictionary<Triangle<int>, Tetrahedron<int>>();
            if (Input.Count > 4)
            {
                // Form initial tetrahedron.
                Tetrahedron<int> first = new Tetrahedron<int>(0, 1, 2, 3);
                Tetrahedron<Vector> firstactual = new Tetrahedron<Vector>(
                    Input.Lookup(first.A),
                    Input.Lookup(first.B),
                    Input.Lookup(first.C),
                    Input.Lookup(first.D));
                if (!Tetrahedron.Order(firstactual))
                {
                    first = first.Flip;
                }
                Vector centroid = Tetrahedron.Midpoint(firstactual);
                tetras.Add(first);
                foreach(Triangle<int> face in first.Faces)
                {
                    bounds.Add(face, first);
                }

                // Begin incremental addition
                Stack<Triangle<int>> newinteriors = new Stack<Triangle<int>>();
                for (int i = 5; i < Input.Count; i++)
                {
                    Vector v = Input.Lookup(i);

                    // Add tetrahedrons to exterior of convex hull
                    List<Tetrahedron<int>> newtetras = new List<Tetrahedron<int>>();
                    foreach (KeyValuePair<Triangle<int>, Tetrahedron<int>> kvp in bounds)
                    {
                        Triangle<int> bound = kvp.Key;
                        if (Triangle.Front(v, new Triangle<Vector>(Input.Lookup(bound.A), Input.Lookup(bound.B), Input.Lookup(bound.C))))
                        {
                            Tetrahedron<int> tetra = new Tetrahedron<int>(i, bound.A, bound.B, bound.C);
                            newtetras.Add(tetra);
                            newinteriors.Push(bound);
                        }
                    }

                    // Apply changes to current bound and tetrahedron set.
                    foreach (Tetrahedron<int> newtetra in newtetras)
                    {
                        tetras.Add(newtetra);
                        foreach (Triangle<int> face in newtetra.Faces)
                        {
                            Triangle<int> flipped = face.Flip;
                            Tetrahedron<int> otherbound;
                            if (bounds.TryGetValue(flipped, out otherbound))
                            {
                                interiors.Add(flipped, otherbound);
                                interiors.Add(face, newtetra);
                                bounds.Remove(flipped);
                            }
                            else
                            {
                                if (face == new Triangle<int>(14, 16, 13))
                                {

                                }
                                bounds.Add(face, newtetra);
                            }
                        }
                    }

                    if (i == 16)
                    {

                    }

                    // Refine tetrahedras to have the delaunay property.
                    while (newinteriors.Count > 0)
                    {
                        Triangle<int> bound = newinteriors.Pop();
                        Tetrahedron<int> interiortetra;
                        if (interiors.TryGetValue(bound, out interiortetra))
                        {
                            Tetrahedron<int> hulla = Tetrahedron.Align(interiortetra, bound).GetValueOrDefault();
                            Tetrahedron<int> hullb = Tetrahedron.Align(interiors[bound.Flip], bound.Flip).GetValueOrDefault();
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
                                    // Split the convex "hexahedron" into 3 tetrahedra.
                                    tetras.Remove(hulla);
                                    tetras.Remove(hullb);
                                    Tetrahedron<int> newa = new Tetrahedron<int>(bound.A, bound.B, hullb.Vertex, hulla.Vertex);
                                    Tetrahedron<int> newb = new Tetrahedron<int>(bound.B, bound.C, hullb.Vertex, hulla.Vertex);
                                    Tetrahedron<int> newc = new Tetrahedron<int>(bound.C, bound.A, hullb.Vertex, hulla.Vertex);
                                    tetras.Add(newa);
                                    tetras.Add(newb);
                                    tetras.Add(newc);
                                    interiors.Remove(bound);
                                    interiors.Remove(bound.Flip);
                                    interiors.Add(new Triangle<int>(bound.A, hullb.Vertex, hulla.Vertex), newa);
                                    interiors.Add(new Triangle<int>(bound.B, hullb.Vertex, hulla.Vertex), newb);
                                    interiors.Add(new Triangle<int>(bound.C, hullb.Vertex, hulla.Vertex), newc);
                                    interiors.Add(new Triangle<int>(bound.A, hulla.Vertex, hullb.Vertex), newc);
                                    interiors.Add(new Triangle<int>(bound.B, hulla.Vertex, hullb.Vertex), newa);
                                    interiors.Add(new Triangle<int>(bound.C, hulla.Vertex, hullb.Vertex), newb);

                                    // Update bounds and interiors.
                                    foreach (KeyValuePair<Triangle<int>, Tetrahedron<int>> kvp in new KeyValuePair<Triangle<int>, Tetrahedron<int>>[]
                                        {
                                            new KeyValuePair<Triangle<int>, Tetrahedron<int>>(new Triangle<int>(hullb.Vertex, bound.A, bound.B), newa),
                                            new KeyValuePair<Triangle<int>, Tetrahedron<int>>(new Triangle<int>(hullb.Vertex, bound.B, bound.C), newb),
                                            new KeyValuePair<Triangle<int>, Tetrahedron<int>>(new Triangle<int>(hullb.Vertex, bound.C, bound.A), newc),
                                            new KeyValuePair<Triangle<int>, Tetrahedron<int>>(new Triangle<int>(hulla.Vertex, bound.B, bound.A), newa),
                                            new KeyValuePair<Triangle<int>, Tetrahedron<int>>(new Triangle<int>(hulla.Vertex, bound.C, bound.B), newb),
                                            new KeyValuePair<Triangle<int>, Tetrahedron<int>>(new Triangle<int>(hulla.Vertex, bound.A, bound.C), newc),
                                        })
                                    {
                                        if (bounds.ContainsKey(kvp.Key))
                                        {
                                            bounds[kvp.Key] = kvp.Value;
                                        }
                                        else
                                        {
                                            interiors[kvp.Key] = kvp.Value;
                                        }
                                    }
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
                                        if (tetras.Contains(other))
                                        {
                                            // Merge all three pieces into two
                                            tetras.Remove(hulla);
                                            tetras.Remove(hullb);
                                            tetras.Remove(other);

                                            // Relabel to make this process easier
                                            bound = new Triangle<int>(hulla.Vertex, hullb.Vertex, edge.Key);
                                            hulla = new Tetrahedron<int>(edge.Value.B, bound);
                                            hullb = new Tetrahedron<int>(edge.Value.A, bound.Flip);

                                            tetras.Add(hulla);
                                            tetras.Add(hullb);
                                            interiors.Add(bound, hulla);
                                            interiors.Add(bound.Flip, hullb);
                                            interiors.Remove(new Triangle<int>(bound.A, hullb.Vertex, hulla.Vertex));
                                            interiors.Remove(new Triangle<int>(bound.B, hullb.Vertex, hulla.Vertex));
                                            interiors.Remove(new Triangle<int>(bound.C, hullb.Vertex, hulla.Vertex));
                                            interiors.Remove(new Triangle<int>(bound.A, hulla.Vertex, hullb.Vertex));
                                            interiors.Remove(new Triangle<int>(bound.B, hulla.Vertex, hullb.Vertex));
                                            interiors.Remove(new Triangle<int>(bound.C, hulla.Vertex, hullb.Vertex));

                                            // Update bounds and interiors.
                                            foreach (KeyValuePair<Triangle<int>, Tetrahedron<int>> kvp in new KeyValuePair<Triangle<int>, Tetrahedron<int>>[]
                                                {
                                                    new KeyValuePair<Triangle<int>, Tetrahedron<int>>(new Triangle<int>(hullb.Vertex, bound.A, bound.B), hullb),
                                                    new KeyValuePair<Triangle<int>, Tetrahedron<int>>(new Triangle<int>(hullb.Vertex, bound.B, bound.C), hullb),
                                                    new KeyValuePair<Triangle<int>, Tetrahedron<int>>(new Triangle<int>(hullb.Vertex, bound.C, bound.A), hullb),
                                                    new KeyValuePair<Triangle<int>, Tetrahedron<int>>(new Triangle<int>(hulla.Vertex, bound.B, bound.A), hulla),
                                                    new KeyValuePair<Triangle<int>, Tetrahedron<int>>(new Triangle<int>(hulla.Vertex, bound.C, bound.B), hulla),
                                                    new KeyValuePair<Triangle<int>, Tetrahedron<int>>(new Triangle<int>(hulla.Vertex, bound.A, bound.C), hulla),
                                                })
                                            {
                                                if (bounds.ContainsKey(kvp.Key))
                                                {
                                                    bounds[kvp.Key] = kvp.Value;
                                                }
                                                else
                                                {
                                                    interiors[kvp.Key] = kvp.Value;
                                                }
                                            }

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
                                        if (!newinteriors.Contains(possiblebound) && interiors.ContainsKey(possiblebound))
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
            return new StandardArray<Tetrahedron<int>>(tetras, tetras.Count);
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