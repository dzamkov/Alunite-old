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
        /// Gets the triangles intersecting, or getting intersected by the other intersection set.
        /// </summary>
        public static HashSet<Triangle<int>> PartialTriangles(
            Dictionary<Segment<int>, Triangle<int>> TriangleSegments,
            IntersectionSet IntersectionSet, 
            IntersectionSet ReverseIntersectionSet)
        {
            HashSet<Triangle<int>> res = new HashSet<Triangle<int>>();
            foreach (Intersection i in IntersectionSet.Intersections)
            {
                res.Add(TriangleSegments[i.Segment]);
                res.Add(TriangleSegments[i.Segment.Flip]);
            }
            foreach (Intersection i in ReverseIntersectionSet.Intersections)
            {
                res.Add(i.Triangle);
            }
            return res;
        }

        /// <summary>
        /// Creates a list of polygons to replace the partial triangles that were
        /// intersected during a CSG operation. Segments that are fully removed are
        /// are added to excluded segments.
        /// </summary>
        public static List<LinkedList<TriPoint>> Modify(
            IEnumerable<Triangle<int>> Triangles,
            HashSet<Segment<int>> ExcludedSegments,
            IntersectionSet IntersectionSet,
            IntersectionSet ReverseIntersectionSet,
            int NewVertexOffset,
            int[] Loop,
            bool Reverse)
        {
            var res = new List<LinkedList<TriPoint>>();
            foreach (Triangle<int> tri in Triangles)
            {
                var poly = new LinkedList<TriPoint>();
                Segment<int>[] segs = tri.Segments;
                Point[] seguvs = new Point[] { new Point(1.0, 0.0), new Point(0.0, 1.0), new Point(0.0, 0.0) };
                int iseg = 0;

                while (iseg < segs.Length)
                {
                    Segment<int> endseg = segs[iseg];
                    foreach (var i in IntersectionSet.SegmentIntersections(endseg))
                    {
                        int c = i.TriangleIntersection.Position - NewVertexOffset;
                        poly.AddLast(new TriPoint(c + NewVertexOffset, UV(tri, endseg, i.TriangleIntersection.Length)));
                        Intersection? pint = null;
                        while (pint == null)
                        {
                            c = Loop[c];
                            pint = IntersectionSet.VertexIntersection(c + NewVertexOffset);
                            if (pint == null)
                            {
                                Intersection rint = ReverseIntersectionSet.VertexIntersection(c + NewVertexOffset).Value;
                                poly.AddLast(new TriPoint(c + NewVertexOffset, rint.SegmentTriangleIntersection.UV));
                            }
                        }
                        var pintdata = pint.Value;
                        poly.AddLast(new TriPoint(c + NewVertexOffset, UV(tri, pintdata.Segment.Flip, pintdata.SegmentTriangleIntersection.Length)));
                        endseg = pint.Value.Segment.Flip;
                    }

                    while(segs[iseg] != endseg)
                    {
                        ExcludedSegments.Add(segs[iseg]);
                        iseg++;
                    }

                    poly.AddLast(new TriPoint(segs[iseg].B, seguvs[iseg]));
                    iseg++;
                }

                res.Add(poly);
            }
            return res;
        }

        public struct TriPoint : IEquatable<TriPoint>
        {
            public TriPoint(int Vert, Point UV)
            {
                this.Vert = Vert;
                this.UV = UV;
            }

            public bool Equals(TriPoint other)
            {
                return this.Vert == other.Vert && this.UV == other.UV;
            }

            public int Vert;
            public Point UV;
        }

        /// <summary>
        /// Gets the uv coordinate for a point on a segment defined by the segments length.
        /// </summary>
        public static Point UV<T>(Triangle<T> Triangle, Segment<T> Segment, double SegmentLength)
            where T : IEquatable<T>
        {
            if (Segment == new Segment<T>(Triangle.A, Triangle.B))
            {
                return new Point(SegmentLength, 0.0);
            }
            if (Segment == new Segment<T>(Triangle.B, Triangle.C))
            {
                return new Point(1.0 - SegmentLength, SegmentLength);
            }
            if (Segment == new Segment<T>(Triangle.C, Triangle.A))
            {
                return new Point(0.0, 1.0 - SegmentLength);
            }
            return new Point();
        }

        /// <summary>
        /// Forms the final triangles from the output of Modify.
        /// </summary>
        public static void Form(
            Dictionary<Segment<int>, Triangle<int>> TriangleSegments,
            HashSet<Segment<int>> Excluded,
            List<LinkedList<TriPoint>> Polygons,
            HashSet<Triangle<int>> Output)
        {
            foreach (var poly in Polygons)
            {
                foreach(var tri in Polygon.Triangulate<TriPoint>(poly, delegate(Triangle<TriPoint> tri)
                    {
                        Triangle<Point> uvtri = new Triangle<Point>(tri.A.UV, tri.B.UV, tri.C.UV);
                        if (Triangle.Order(uvtri))
                        {
                            foreach (var opoint in poly)
                            {
                                if (opoint.Vert != tri.A.Vert && opoint.Vert != tri.B.Vert && opoint.Vert != tri.C.Vert)
                                {
                                    if (Triangle.Relation(opoint.UV, uvtri) != AreaRelation.Outside)
                                    {
                                        return false;
                                    }
                                }
                            }
                            return true;
                        }
                        return false;
                    }))
                {
                    Output.Add(new Triangle<int>(tri.A.Vert, tri.B.Vert, tri.C.Vert));
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
            
            // Get partial triangles
            var partiala = PartialTriangles(trisegsa, intsa, intsb);
            var partialb = PartialTriangles(trisegsb, intsb, intsa);

            // Get modified triangles
            HashSet<Triangle<int>> tris = new HashSet<Triangle<int>>();
            HashSet<Segment<int>> excluded = new HashSet<Segment<int>>();
            var modifieda = Modify(partiala, excluded, intsa, intsb, oldvertamount, loop, false);
            Form(trisegsa, excluded, modifieda, tris);

            return tris;
        }
    }
}