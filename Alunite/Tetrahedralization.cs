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
            var interiors = new Dictionary<Triangle<int>, Tuple<Tetrahedron<int>, Tetrahedron<int>>>();
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
                            interiors.Add(bound, new Tuple<Tetrahedron<int>, Tetrahedron<int>>(tetra, kvp.Value));
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
                            if (bounds.ContainsKey(flipped))
                            {
                                bounds.Remove(flipped);
                            }
                            else
                            {
                                bounds.Add(face, newtetra);
                            }
                        }
                    }

                    // Refine tetrahedras to have the delunay property.
                    while (newinteriors.Count > 0)
                    {
                        Triangle<int> bound = newinteriors.Pop();

                    }
                }
            }
            return new StandardArray<Tetrahedron<int>>(tetras, tetras.Count);
        }
    }
}