using System;
using System.Collections.Generic;

namespace Alunite
{
    /* CSG (Constructive solid geometry) of triangular meshes appears to be a hard problem that i'll need to
     * think a lot about before implementing. To avoid fogetting how the algorithims work, i'll derive them here.
     * 
     * A CSG for two meshes is only valid if the intersecting region between the two triangular surfaces is made up
     * of an amount of closed loops (containing points and segments). If any loops are not closed (have more vertices than segments,
     * or have some vertices used more or less than two times), no CSG mesh can be constructed. The vertices in these loops are
     * the only vertices that need to be used in addition to the original vertices in the final mesh.
     * 
     * Note that this algorithim does not yet work when two triangles between the inputs are coplanar, however this feature can
     * be easily added with little modification to the original algorithim.
     * 
     * Using the above observations, a general outline for the algorithim can be created.
     * 
     * 1. Find the loops that make up the intersecting regions
     * 2. Retriangulate the areas the loops intersect such that the loops become the boundaries of the regions
     * 3. Remove excess triangles as the csg method dictates
     * 
    */

    /// <summary>
    /// Functions relating to constructive solid geometry.
    /// </summary>
    public static class CSG
    {
        /// <summary>
        /// Gets the intersection between two triangular surfaces in three dimensional space. The intersection is likely to involve
        /// a set of loops. This function may add vertices.
        /// </summary>
        public static HashSet<UnorderedSegment<int>> SurfaceIntersection(
            IEnumerable<Triangle<int>> SurfaceA, 
            IEnumerable<Triangle<int>> SurfaceB, 
            List<Vector> Vertices)
        {
            // Compute segments and their triangular owners
            var asegs = Segments(SurfaceA);
            var bsegs = Segments(SurfaceB);

            // Compute intersections
            var aints = SurfaceSegmentIntersection(SurfaceA, bsegs.Keys, Vertices);
            var bints = SurfaceSegmentIntersection(SurfaceB, asegs.Keys, Vertices);

            // Find conflicts
            var aconflicts = Conflicting(aints, bsegs);
            var bconflicts = Conflicting(bints, asegs);

            HashSet<UnorderedSegment<int>> finalsegs = new HashSet<UnorderedSegment<int>>();

            // Resolve conflicts
            foreach (var aconflict in aconflicts)
            {
                foreach (var other in aconflict.Value)
                {
                    finalsegs.Add(TriangleIntersection(aconflict.Key, other, aints, bints));
                }
            }
            foreach (var bconflict in bconflicts)
            {
                foreach (var other in bconflict.Value)
                {
                    finalsegs.Add(TriangleIntersection(bconflict.Key, other, bints, aints));
                }
            }

            return finalsegs;
        }

        /// <summary>
        /// Maps the ordered segments, to the triangles that produce them, in a triangular surface.
        /// </summary>
        public static Dictionary<Segment<T>, Triangle<T>> Segments<T>(IEnumerable<Triangle<T>> Surface)
            where T : IEquatable<T>
        {
            Dictionary<Segment<T>, Triangle<T>> segs = new Dictionary<Segment<T>, Triangle<T>>();
            foreach (Triangle<T> tri in Surface)
            {
                foreach (Segment<T> seg in tri.Segments)
                {
                    segs.Add(seg, tri);
                }
            }
            return segs;
        }

        /// <summary>
        /// Creates a set of unordered segments from a set of ordered segments (any segment that appears 
        /// will be unordered, any excess segments are removed).
        /// </summary>
        public static HashSet<UnorderedSegment<T>> Collapse<T>(IEnumerable<Segment<T>> Segments)
            where T : IEquatable<T>
        {
            HashSet<UnorderedSegment<T>> segs = new HashSet<UnorderedSegment<T>>();
            foreach (Segment<T> segment in Segments)
            {
                segs.Add(new UnorderedSegment<T>(segment));
            }
            return segs;
        }

        /// <summary>
        /// Represents the intersection between a triangle and a segment.
        /// </summary>
        public struct Intersection
        {
            /// <summary>
            /// The new vertex produced as a result of the intersection.
            /// </summary>
            public int NewVertex;
        }

        /// <summary>
        /// Creates the intersections between a collection of triangles and segments.
        /// </summary>
        public static Dictionary<Segment<int>, Dictionary<Triangle<int>, Intersection>> SurfaceSegmentIntersection(
            IEnumerable<Triangle<int>> Triangles, 
            IEnumerable<Segment<int>> Segments, 
            List<Vector> Vertices)
        {
            var intersections = new Dictionary<Segment<int>, Dictionary<Triangle<int>, Intersection>>();
            foreach (Triangle<int> tri in Triangles)
            {
                Triangle<Vector> vectri = new Triangle<Vector>(Vertices[tri.A], Vertices[tri.B], Vertices[tri.C]);
                foreach (Segment<int> seg in Segments)
                {
                    Segment<Vector> vecseg = new Segment<Vector>(Vertices[seg.A], Vertices[seg.B]);
                    double hitlen;
                    Vector hitpos;
                    if (Triangle.Intersect(vectri, vecseg, out hitlen, out hitpos))
                    {
                        int ind = Vertices.Count;
                        Vertices.Add(hitpos);
                        Dictionary<Triangle<int>, Intersection> subdict = new Dictionary<Triangle<int>, Intersection>();
                        if (!intersections.TryGetValue(seg, out subdict))
                        {
                            intersections.Add(seg, subdict = new Dictionary<Triangle<int>, Intersection>());
                        }
                        subdict.Add(tri, new Intersection() { NewVertex = ind });
                    }
                }
            }
            return intersections;
        }

        /// <summary>
        /// Gets the conflicting triangles in a mapped collection of intersections. The penetrating triangles are the
        /// keys of the resulting dictionary.
        /// </summary>
        public static Dictionary<Triangle<int>, HashSet<Triangle<int>>> Conflicting(
            Dictionary<Segment<int>, Dictionary<Triangle<int>, Intersection>> Intersections,
            Dictionary<Segment<int>, Triangle<int>> Segments)
        {
            Dictionary<Triangle<int>, HashSet<Triangle<int>>> res = new Dictionary<Triangle<int>, HashSet<Triangle<int>>>();
            foreach (var segints in Intersections)
            {
                Segment<int> seg = segints.Key;
                foreach (var triints in segints.Value)
                {
                    Triangle<int> tri = triints.Key;
                    Intersection intersection = triints.Value;

                    foreach (Triangle<int> penetrating in new Triangle<int>[] 
                        { 
                            Segments[seg],
                            Segments[seg.Flip] 
                        })
                    {
                        HashSet<Triangle<int>> triset;
                        if (!res.TryGetValue(penetrating, out triset))
                        {
                            res[penetrating] = triset = new HashSet<Triangle<int>>();
                        }
                        triset.Add(tri);
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Finds the common area (as an unordered segment) between two intersecting triangles.
        /// </summary>
        public static UnorderedSegment<int> TriangleIntersection(
            Triangle<int> Penetrating,
            Triangle<int> Other,
            Dictionary<Segment<int>, Dictionary<Triangle<int>, Intersection>> AIntersections,
            Dictionary<Segment<int>, Dictionary<Triangle<int>, Intersection>> BIntersections)
        {
            // Case 1 : Penetrating and Other both intersect each other

            foreach (Segment<int> penseg in Penetrating.Segments)
            {
                Dictionary<Triangle<int>, Intersection> pensegints;
                if (AIntersections.TryGetValue(penseg, out pensegints))
                {
                    Intersection penint;
                    if (pensegints.TryGetValue(Other, out penint))
                    {
                        foreach (Segment<int> oseg in Other.Segments)
                        {
                            Dictionary<Triangle<int>, Intersection> osegints;
                            if (BIntersections.TryGetValue(oseg, out osegints))
                            {
                                Intersection oint;
                                if (osegints.TryGetValue(Penetrating, out oint))
                                {
                                    return new UnorderedSegment<int>(penint.NewVertex, oint.NewVertex);
                                }
                            }
                        }
                    }
                }
            }

            // Case 2 : Penetrating intersects Other twice

            // Not done yet

            return new UnorderedSegment<int>();
        }
    }
}