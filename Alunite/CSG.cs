﻿using System;
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
        /// Gets the segments (indicating triangle borders) in a mesh.
        /// </summary>
        public static HashSet<UnorderedSegment<T>> Segments<T>(IEnumerable<Triangle<T>> Mesh)
            where T : IEquatable<T>
        {
            HashSet<UnorderedSegment<T>> segs = new HashSet<UnorderedSegment<T>>();
            foreach (Triangle<T> tri in Mesh)
            {
                foreach (Segment<T> seg in tri.Segments)
                {
                    segs.Add(Segment.Unorder(seg));
                }
            }
            return segs;
        }

        /// <summary>
        /// Gets the ordered segments and the triangles that produce them in a mesh.
        /// </summary>
        public static Dictionary<Segment<T>, Triangle<T>> TriangleSegments<T>(IEnumerable<Triangle<T>> Mesh)
            where T : IEquatable<T>
        {
            Dictionary<Segment<T>, Triangle<T>> segs = new Dictionary<Segment<T>, Triangle<T>>();
            foreach (Triangle<T> tri in Mesh)
            {
                foreach (Segment<T> seg in tri.Segments)
                {
                    segs.Add(seg, tri);
                }
            }
            return segs;
        }

        /// <summary>
        /// A set of intersections between a collection of triangles and segments.
        /// </summary>
        public class IntersectionSet
        {
            public IntersectionSet()
            {
                this._Intersections = new Dictionary<Segment<int>, LinkedList<SegmentIntersection>>();
                this._VertexIntersections = new Dictionary<int, Intersection>();
            }

            /// <summary>
            /// Gets the intersections in this set.
            /// </summary>
            public IEnumerable<Intersection> Intersections
            {
                get
                {
                    return this._VertexIntersections.Values;
                }
            }

            /// <summary>
            /// Gets all the intersections a segment makes, in order of length along the segment.
            /// </summary>
            public IEnumerable<SegmentIntersection> SegmentIntersections(Segment<int> Segment)
            {
                LinkedList<SegmentIntersection> li;
                if (this._Intersections.TryGetValue(Segment, out li))
                {
                    return li;
                }
                else
                {
                    return new SegmentIntersection[0];
                }
            }

            /// <summary>
            /// Adds an intersection to the set.
            /// </summary>
            public void AddIntersection(Segment<int> Segment, SegmentIntersection Info)
            {
                LinkedList<SegmentIntersection> curlist;
                if (!this._Intersections.TryGetValue(Segment, out curlist))
                {
                    this._Intersections[Segment] = curlist = new LinkedList<SegmentIntersection>();
                }

                // Find the correct insertion point (ordered by length).
                LinkedListNode<SegmentIntersection> curnode = curlist.First;
                while (true)
                {
                    if (curnode == null)
                    {
                        curlist.AddLast(Info);
                        break;
                    }
                    if (curnode.Value.TriangleIntersection.Length > Info.TriangleIntersection.Length)
                    {
                        curlist.AddBefore(curnode, Info);
                        break;
                    }
                    curnode = curnode.Next;
                }

                // Add to vertex intersections
                this._VertexIntersections.Add(Info.TriangleIntersection.Position, new Intersection() 
                { 
                    Segment = Segment, 
                    Triangle = Info.Triangle, 
                    SegmentTriangleIntersection = Info.TriangleIntersection 
                });
            }

            /// <summary>
            /// Gets the intersection between a segment and a triangle, if any exists.
            /// </summary>
            public SegmentTriangleIntersection? SegmentTriangleIntersection(Segment<int> Segment, Triangle<int> Triangle)
            {
                LinkedList<SegmentIntersection> segints;
                if (this._Intersections.TryGetValue(Segment, out segints))
                {
                    foreach (SegmentIntersection segint in segints)
                    {
                        if (segint.Triangle == Triangle)
                        {
                            return segint.TriangleIntersection;
                        }
                    }
                }
                return null;
            }

            /// <summary>
            /// Gets an intersection by the vertex it produced.
            /// </summary>
            public Intersection? VertexIntersection(int NewVertex)
            {
                Intersection i;
                if (this._VertexIntersections.TryGetValue(NewVertex, out i))
                {
                    return i;
                }
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// Mapping of segments to the sorted list of intersections they make (by length).
            /// </summary>
            private Dictionary<Segment<int>, LinkedList<SegmentIntersection>> _Intersections;

            private Dictionary<int, Intersection> _VertexIntersections;
        }

        /// <summary>
        /// Represents a segment triangle intersection.
        /// </summary>
        public struct Intersection
        {
            /// <summary>
            /// The triangle that was hit.
            /// </summary>
            public Triangle<int> Triangle;

            /// <summary>
            /// The segment that was hit.
            /// </summary>
            public Segment<int> Segment;

            /// <summary>
            /// More intersection information between the triangle and segment.
            /// </summary>
            public SegmentTriangleIntersection SegmentTriangleIntersection;
        }
        
        /// <summary>
        /// Represents a segment triangle intersection where the segment is known.
        /// </summary>
        public struct SegmentIntersection
        {
            /// <summary>
            /// The triangle that was hit.
            /// </summary>
            public Triangle<int> Triangle;

            /// <summary>
            /// More intersection information with the triangle.
            /// </summary>
            public SegmentTriangleIntersection TriangleIntersection;
        }

        /// <summary>
        /// Represents a segment triangle intersection where both the segment and triangle are known.
        /// </summary>
        public struct SegmentTriangleIntersection
        {
            /// <summary>
            /// Length along the segment the intersection is at.
            /// </summary>
            public double Length;

            /// <summary>
            /// The uv coordinates, relative to the triangle, the hit is at.
            /// </summary>
            public Point UV;

            /// <summary>
            /// The vertex representing the point of intersection.
            /// </summary>
            public int Position;
        }

        /// <summary>
        /// Creates a set of intersections between a collection of triangles and segments. Vertices for each
        /// intersection are added to the vertices list and are their indices are retrievable from the resulting set.
        /// </summary>
        public static IntersectionSet MeshSegmentIntersect(
            IEnumerable<Triangle<int>> Triangles,
            IEnumerable<UnorderedSegment<int>> Segments,
            List<Vector> Vertices)
        {
            IntersectionSet intersections = new IntersectionSet();
            foreach (Triangle<int> tri in Triangles)
            {
                Triangle<Vector> vectri = new Triangle<Vector>(Vertices[tri.A], Vertices[tri.B], Vertices[tri.C]);

                // A more efficent version of this algorithim would save each triangles plane and
                // uv parameters for much faster intersections.
                foreach (UnorderedSegment<int> seg in Segments)
                {
                    Segment<Vector> vecseg = new Segment<Vector>(Vertices[seg.Source.A], Vertices[seg.Source.B]);
        
                    double hitlen;
                    Vector hitpos;
                    Point hituv;

                    bool hitface = Triangle.Intersect(vectri, vecseg, out hitlen, out hitpos, out hituv);
                    if (hitlen > 0.0 && hitlen < 1.0 && hituv.X > 0.0 && hituv.X < 1.0 && hituv.Y > 0.0 && (hituv.X + hituv.Y) < 1.0)
                    {
                        int ind = Vertices.Count;
                        Vertices.Add(hitpos);
                        intersections.AddIntersection(hitface ? seg.Source : seg.Source.Flip, new SegmentIntersection()
                        {
                            Triangle = tri,
                            TriangleIntersection = new SegmentTriangleIntersection()
                            {
                                Length = hitlen,
                                UV = hituv,
                                Position = ind
                            }
                        });
                    }
                }
            }
            return intersections;
        }

        /// <summary>
        /// Given loop, a mapping of the new vertices of intersections and the next intersection
        /// in a loop, this function will add new values based on the triangle triangle intersections
        /// it finds in the specified data sets. An additional reverse argument is supplied to indicate
        /// the direction the loop is made in.
        /// </summary>
        /// <param name="NewVertexOffset">The first vertex index that is included in either set of intersections.</param>
        /// <param name="Loop">An array with an element for each intersection in both sets.</param>
        public static void IntersectionLoop(
            Dictionary<Segment<int>, Triangle<int>> TriangleSegments,
            IntersectionSet IntersectionSet,
            IntersectionSet ReverseIntersectionSet,
            int NewVertexOffset,
            int[] Loop,
            bool Reverse)
        {
            foreach (Intersection i in IntersectionSet.Intersections)
            {
                Segment<int> iseg = Reverse ? i.Segment.Flip : i.Segment;
                Triangle<int> penetratingtri = Triangle.Align(TriangleSegments[iseg], iseg).Value;
                Triangle<int> hittri = i.Triangle;

                // Check for mutal intersection.
                foreach (Segment<int> hitseg in hittri.Segments)
                {
                    SegmentTriangleIntersection? sti = ReverseIntersectionSet.SegmentTriangleIntersection(Reverse ? hitseg.Flip : hitseg, penetratingtri);
                    if (sti != null)
                    {
                        Loop[i.SegmentTriangleIntersection.Position - NewVertexOffset] = sti.Value.Position - NewVertexOffset;
                    }
                }
            }
        }

        /// <summary>
        /// Finds the polygons representing the surface at the boundary of a CSG operation.
        /// </summary>
        /// <param name="NewVertexOffset">The amount values and indices in loop are offset by.</param>
        /// <param name="Loop">A loop of vertices representing the boundary region.</param>
        /// <param name="Reverse">Should the loop be interpreted in reverse?</param>
        /// <param name="ExcludedSegments">A set where segments that belong to boundary triangles are stuffed.</param>
        /// <param name="Polygons">A set where boundary polygons are stuffed.</param>
        /// <param name="IntersectionSet">Intersection set containing the intersections of the boundary triangles onto the other triangles.</param>
        /// <param name="ReverseIntersectionSet">Intersection set containing the intersections of the other triangles onto the boundary triangles.</param>
        public static void TrimBoundary(
            int NewVertexOffset,
            int[] Loop,
            bool Reverse,
            HashSet<Segment<int>> ExcludedSegments,
            List<LinkedList<int>> Polygons,
            Dictionary<Segment<int>, Triangle<int>> TriangleSegments,
            IntersectionSet IntersectionSet,
            IntersectionSet ReverseIntersectionSet)
        {
            // Mark parts of loop already accounted for
            bool[] completedloop = new bool[Loop.Length];

            for (int t = 0; t < Loop.Length; t++)
            {
                if (!completedloop[t])
                {
                    Intersection? firsti = IntersectionSet.VertexIntersection(t + NewVertexOffset);
                    if (firsti != null)
                    {
                        // Begin a polygon
                        LinkedList<int> curpoly = new LinkedList<int>();
                        Triangle<int> tri = TriangleSegments[firsti.Value.Segment];

                        // Circle around the triangle this vertex is in.
                        int d = t;
                        do // Do whiles are nasty things, but make sense in this case
                        {
                            // Add to polygon, mark as checked
                            completedloop[d] = true;
                            curpoly.AddLast(d + NewVertexOffset);

                            Intersection? pi = IntersectionSet.VertexIntersection(d + NewVertexOffset);
                            if (pi != null)
                            {
                                Intersection vpi = pi.Value;
                                Triangle<int>? otri = Triangle.Align(tri, vpi.Segment.Flip);
                                if (otri != null)
                                {
                                    // Entering the triangles segments
                                    throw new NotImplementedException();
                                }
                                else
                                {
                                    // Next
                                    d = Loop[d];
                                }
                            }
                            else
                            {
                                d = Loop[d];
                            }
                        } while (d != t);

                        // Add polygon
                        Polygons.Add(curpoly);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the union of two triangular meshes. More interesting effects can be created by inverting either of the meshes. New vertices may
        /// be added in order to form the final triangles.
        /// </summary>
        public static HashSet<Triangle<int>> MeshUnion(IEnumerable<Triangle<int>> MeshA, IEnumerable<Triangle<int>> MeshB, List<Vector> Vertices)
        {
            // Find segments of both meshes
            var segsa = Segments(MeshA); var trisegsa = TriangleSegments(MeshA);
            var segsb = Segments(MeshB); var trisegsb = TriangleSegments(MeshB);

            // Find intersections
            int oldvertamount = Vertices.Count;
            var intsa = MeshSegmentIntersect(MeshB, segsa, Vertices);
            var intsb = MeshSegmentIntersect(MeshA, segsb, Vertices);
            int intamount = Vertices.Count - oldvertamount;

            // Create intersection loop
            int[] loop = new int[intamount];
            IntersectionLoop(trisegsa, intsa, intsb, oldvertamount, loop, false);
            IntersectionLoop(trisegsb, intsb, intsa, oldvertamount, loop, true);

            // Create boundary region
            List<LinkedList<int>> polygons = new List<LinkedList<int>>();
            HashSet<Segment<int>> excludedsegments = new HashSet<Segment<int>>();
            TrimBoundary(oldvertamount, loop, false, excludedsegments, polygons, trisegsa, intsa, intsb);

            HashSet<Triangle<int>> tris = new HashSet<Triangle<int>>();
            tris.UnionWith(MeshA);
            tris.UnionWith(MeshB);
            return tris;
        }
    }
}