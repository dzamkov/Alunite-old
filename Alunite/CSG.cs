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
        /// Gets the union (combination of meshes to form one continous surface) of polyhedra. More intresting
        /// affects (such as difference) can be created by inverting one of the polyhedra. The first specified polyhedra
        /// will be changed to reflect the union (less changes are required if A is the most complex of the polyhedra).
        /// </summary>
        public static VectorPolyhedron Union(VectorGeometry Geometry, VectorPolyhedron A, VectorPolyhedron B)
        {
            VectorPolyhedron res = new VectorPolyhedron();
            
            // Segments
            var asegs = A.Segments;
            var bsegs = B.Segments;

            // Intersections
            Dictionary<_FacePair, List<_Intersection>> facepairs = new Dictionary<_FacePair, List<_Intersection>>();
            Dictionary<int, _FaceIntersection> ainfo = new Dictionary<int, _FaceIntersection>();
            Dictionary<int, _FaceIntersection> binfo = new Dictionary<int, _FaceIntersection>();
            _Intersect(Geometry, A, B, true, asegs, facepairs, ainfo, binfo);
            _Intersect(Geometry, A, B, false, bsegs, facepairs, binfo, ainfo);
            

            // Loops
            foreach (List<_Intersection> ilist in facepairs.Values)
            {
                _SortPlanarIntersections(Geometry, ilist);
            }

            Dictionary<int, int> nextinloop = new Dictionary<int, int>();
            Dictionary<int, int> previnloop = new Dictionary<int, int>();
            _CalculateLoops(facepairs, nextinloop, previnloop);

            // Process intersection data
            HashSet<Segment<int>> aexclude = new HashSet<Segment<int>>(); // Segments known not to be in the final result (from A)
            HashSet<Segment<int>> ainclude = new HashSet<Segment<int>>(); // Segments known to be in the final result (from A)
            HashSet<Segment<int>> bexclude = new HashSet<Segment<int>>();
            HashSet<Segment<int>> binclude = new HashSet<Segment<int>>();
            _ProcessFaces(ainfo, A, nextinloop, previnloop, aexclude, ainclude, res);
            

            foreach (PolyhedronFace<Triangle<int>, Point, int> poly in A.FaceData)
            {
                res.Add(poly.Segments, poly.Points, poly.Plane);
            }
            foreach (PolyhedronFace<Triangle<int>, Point, int> poly in B.FaceData)
            {
                res.Add(poly.Segments, poly.Points, poly.Plane);
            }

            return res;
        }

        /// <summary>
        /// Represents a face segment intersection when both intersecting faces of the intersection are known.
        /// </summary>
        private struct _Intersection
        {
            /// <summary>
            /// Vertex created as a result of the intersection.
            /// </summary>
            public int NewVertex;

            /// <summary>
            /// The edge that acted as the segment in the intersection.
            /// </summary>
            public int Edge;

            /// <summary>
            /// Length along the edge the hit is at.
            /// </summary>
            public double Length;

            /// <summary>
            /// True if the edge hit the face in the front-facing direction, false if the edge hit
            /// the face in the back-facing direction.
            /// </summary>
            public bool Direction;

            /// <summary>
            /// True if A owns the segment that's part of the intersection.
            /// </summary>
            public bool ASegment;
        }

        /// <summary>
        /// A pair of faces that intersect.
        /// </summary>
        private struct _FacePair : IEquatable<_FacePair>
        {
            public _FacePair(int AFace, int BFace)
            {
                this.AFace = AFace;
                this.BFace = BFace;
            }

            public bool Equals(_FacePair other)
            {
                return this.AFace == other.AFace && this.BFace == other.BFace;
            }

            public override int GetHashCode()
            {
                return this.AFace.GetHashCode() ^ this.BFace.GetHashCode();
            }

            public int AFace;

            public int BFace;
        }

        /// <summary>
        /// Gets the intersections of the specified segments onto the target polyhedron and outputs them to the
        /// intersection dictionary and face intersection info.
        /// </summary>
        /// <remarks>If two intersecting faces are convex, they will have exactly two intersections. No face pair will have
        /// less than two intersections.</remarks>
        private static void _Intersect(
            VectorGeometry Geometry, VectorPolyhedron A, VectorPolyhedron B,
            bool SegmentsAreA,
            IEnumerable<UnorderedSegment<int>> Segments,
            Dictionary<_FacePair, List<_Intersection>> Intersections,
            Dictionary<int, _FaceIntersection> SegmentInfo,
            Dictionary<int, _FaceIntersection> FaceInfo)
        {
            var res = new Dictionary<_FacePair, List<_Intersection>>();
            var segsource = SegmentsAreA ? A : B;
            var facesource = SegmentsAreA ? B : A;
            foreach (int face in facesource.Faces)
            {
                PolyhedronFace<Triangle<int>, Point, int> facedata = facesource.Lookup(face);
                IEnumerable<Segment<Point>> poly = _PolygonConvert(facedata);
                Triangle<Vector> plane = Geometry.Dereference(facedata.Plane);
                foreach (var seg in Segments)
                {
                    Segment<int> iseg = seg.Source;
                    Segment<Vector> hitseg = Geometry.Dereference(iseg);
                    double len;
                    Point uv;
                    Vector pos;
                    if (!Triangle.Intersect(plane, hitseg, out len, out pos, out uv))
                    {
                        len = 1.0 - len;
                        iseg = iseg.Flip;
                    }
                    if (len > 0.0 && len < 1.0 && Polygon.PointTest(uv, poly).Relation == AreaRelation.Inside)
                    {
                        int nvert = Geometry.AddVertex(pos);
                        FaceEdge<int, int> fer = segsource.SegmentFace(iseg).Value;
                        FaceEdge<int, int> few = segsource.SegmentFace(iseg.Flip).Value;
                        _FacePair ferpair = SegmentsAreA ? new _FacePair(fer.Face, face) : new _FacePair(face, fer.Face);
                        _FacePair fewpair = SegmentsAreA ? new _FacePair(few.Face, face) : new _FacePair(face, few.Face);
                        _Append(Intersections, ferpair, new _Intersection()
                        {
                            Direction = true,
                            Length = len,
                            ASegment = SegmentsAreA,
                            Edge = fer.Edge,
                            NewVertex = nvert
                        });
                        _Append(Intersections, fewpair, new _Intersection()
                        {
                            Direction = false,
                            Length = 1.0 - len,
                            ASegment = SegmentsAreA,
                            Edge = few.Edge,
                            NewVertex = nvert
                        });
                        _AddFaceIntersectionPoint(FaceInfo, face, nvert, uv);
                        _AddFaceIntersectionEdge(SegmentInfo, fer.Face, nvert, fer.Edge, len, false);
                        _AddFaceIntersectionEdge(SegmentInfo, few.Face, nvert, few.Edge, 1.0 - len, true);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a value to a list dictionary. Creates a new entry and list if needed.
        /// </summary>
        private static void _Append<K, V>(Dictionary<K, List<V>> Dict, K Key, V Value)
        {
            List<V> vlist;
            if (!Dict.TryGetValue(Key, out vlist))
            {
                Dict[Key] = vlist = new List<V>();
            }
            vlist.Add(Value);
        }

        /// <summary>
        /// Converts a polygon of vector points to points.
        /// </summary>
        private static IEnumerable<Segment<Point>> _PolygonConvert(PolyhedronFace<Triangle<int>, Point, int> Face)
        {
            foreach (Segment<int> seg in Face.Segments)
            {
                yield return new Segment<Point>(Face.Points[seg.A].A, Face.Points[seg.B].A);
            }
        }

        /// <summary>
        /// Given the first and last (in any order) intersections between two faces, returns true if the loops
        /// of the intersection go in a consistent direction with the segment formed from the first and last intersection.
        /// </summary>
        /// <remarks>Reversing the order of all face pair intersections will invert the result of this function.</remarks>
        private static bool _LoopDirection(_Intersection First, _Intersection Last)
        {
            if (First.ASegment == Last.ASegment)
            {
                return !(First.Direction ^ First.ASegment);
            }
            else
            {
                if (First.ASegment)
                {
                    return First.Direction;
                }
                else
                {
                    return !(Last.Direction);
                }
            }
        }

        /// <summary>
        /// Sorts the provided list of intersections (all lying on the intersection of two planes) to be consistent with loop
        /// direction. The intersections are sorted to ascend left when looking into both front planes on their front face.
        /// </summary>
        private static void _SortPlanarIntersections(VectorGeometry Geometry, List<_Intersection> Intersections)
        {
            _Intersection curfirst = Intersections[0];
            _Intersection cursecond = Intersections[1];
            if (Intersections.Count > 2)
            {
                // Compute distances
                Vector va = Geometry.Lookup(curfirst.NewVertex);
                Vector vb = Geometry.Lookup(cursecond.NewVertex);
                Vector d = vb - va;
                var dists = new _IntersectionDistance[Intersections.Count];
                dists[0] = new _IntersectionDistance(0, 0.0);
                dists[1] = new _IntersectionDistance(1, 1.0);
                for (int i = 2; i < Intersections.Count; i++)
                {
                    dists[i] = new _IntersectionDistance(i, Vector.Dot(Geometry.Lookup(Intersections[i].NewVertex) - va, d));
                }

                // Sort
                Sort.InPlace<StandardArray<_IntersectionDistance>, _IntersectionDistance>(new StandardArray<_IntersectionDistance>(dists), x => (x.A.Dist > x.B.Dist));
                _Intersection[] temp = new _Intersection[Intersections.Count];
                Intersections.CopyTo(temp);

                // Determine direction
                bool reverse = !_LoopDirection(temp[dists[0].Ref], temp[dists[dists.Length - 1].Ref]);
                
                // Put back in list in the correct order.
                if (reverse)
                {
                    int declen = dists.Length - 1;
                    for (int i = 0; i < dists.Length; i++)
                    {
                        Intersections[i] = temp[dists[declen - i].Ref];
                    }
                }
                else
                {
                    for (int i = 0; i < dists.Length; i++)
                    {
                        Intersections[i] = temp[dists[i].Ref];
                    }
                }
            }
            else
            {
                // Swap if needed
                if (!_LoopDirection(curfirst, cursecond))
                {
                    Intersections[0] = cursecond;
                    Intersections[1] = curfirst;
                }
            }
        }

        /// <summary>
        /// For use by sort.
        /// </summary>
        private struct _IntersectionDistance : IEquatable<_IntersectionDistance>
        {
            public _IntersectionDistance(int Ref, double Dist)
            {
                this.Ref = Ref;
                this.Dist = Dist;
            }

            public bool Equals(_IntersectionDistance other)
            {
                return this.Ref == other.Ref;
            }

            public int Ref;
            public double Dist;
        }

        /// <summary>
        /// Information about a single intersecting face during a CSG operation.
        /// </summary>
        private struct _FaceIntersection
        {
            /// <summary>
            /// Gets information about the new vertices created on the edges.
            /// </summary>
            public Dictionary<int, _EdgeIntersection> IntersectingEdges;

            /// <summary>
            /// Gets the UV coordinates of new vertices created on the surface of the Face.
            /// </summary>
            public Dictionary<int, Point> UV;
        }

        /// <summary>
        /// Represents an intersection of a polygon's edge.
        /// </summary>
        private struct _EdgeIntersection
        {
            /// <summary>
            /// The edge that was involved in the intersection.
            /// </summary>
            public int Edge;

            /// <summary>
            /// The length along the edge the intersection is at.
            /// </summary>
            public double Length;

            /// <summary>
            /// Did the intersection go in the reverse direction of the edge?
            /// </summary>
            public bool Reverse;
        }

        private static void _AddFaceIntersectionPoint(Dictionary<int, _FaceIntersection> Intersections, int Face, int Vertex, Point UV)
        {
            _FaceIntersection fi;
            if (!Intersections.TryGetValue(Face, out fi))
            {
                Intersections[Face] = fi = new _FaceIntersection()
                {
                    IntersectingEdges = new Dictionary<int, _EdgeIntersection>(),
                    UV = new Dictionary<int, Point>()
                };
            }
            fi.UV.Add(Vertex, UV);
        }

        private static void _AddFaceIntersectionEdge(Dictionary<int, _FaceIntersection> Intersections, int Face, int Vertex, int Edge, double Length, bool Reverse)
        {
            _FaceIntersection fi;
            if (!Intersections.TryGetValue(Face, out fi))
            {
                Intersections[Face] = fi = new _FaceIntersection()
                {
                    IntersectingEdges = new Dictionary<int, _EdgeIntersection>(),
                    UV = new Dictionary<int, Point>()
                };
            }
            fi.IntersectingEdges.Add(Vertex, new _EdgeIntersection()
            {
                Edge = Edge,
                Length = Length,
                Reverse = Reverse
            });
        }

        /// <summary>
        /// Calculates the "loops" as described by the csg algorithim, given the sorted intersections of
        /// the face pairs.
        /// </summary>
        private static void _CalculateLoops(
            Dictionary<_FacePair, List<_Intersection>> FacePairs,
            Dictionary<int, int> Next,
            Dictionary<int, int> Prev)
        {
            foreach (var faceints in FacePairs.Values)
            {
                bool on = false;
                int prev = 0;
                foreach (var i in faceints)
                {
                    int ne = i.NewVertex;
                    if (on)
                    {
                        Prev.Add(ne, prev);
                        Next.Add(prev, ne);
                    }
                    prev = ne;
                    on = !on;
                }
            }
        }

        /// <summary>
        /// Processes face intersections, gets included and excluded segments that can be inferred from them  and
        /// outputs faces for the specified polyhedron to the output.
        /// </summary>
        private static void _ProcessFaces(
            Dictionary<int, _FaceIntersection> FaceIntersections,
            VectorPolyhedron Input,
            Dictionary<int, int> NextInLoop,
            Dictionary<int, int> PrevInLoop,
            HashSet<Segment<int>> Excluded,
            HashSet<Segment<int>> Included,
            VectorPolyhedron Output)
        {
            foreach (var kvp in FaceIntersections)
            {
                int face = kvp.Key;
                _FaceIntersection faceint = kvp.Value;
                var facedata = Input.Lookup(face);


            }
        }
    }
}