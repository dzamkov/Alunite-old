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
                Sort.InPlace<_IntersectionDistance>((a, b) => (a.Dist > b.Dist), dists);
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
        /// Represents an edge that has endpoints in a face-loop intersection.
        /// </summary>
        /// <typeparam name="TPoint">A point on the face.</typeparam>
        public struct EndpointEdge<TPoint>
        {
            /// <summary>
            /// Point A of the edge.
            /// </summary>
            public TPoint A;

            /// <summary>
            /// Point B of the edge.
            /// </summary>
            public TPoint B;

            /// <summary>
            /// An ordered collection (from A to B) of the endpoints contained on
            /// this edge.
            /// </summary>
            public IEnumerable<TPoint> Endpoints;

            /// <summary>
            /// True if the first endpoint was created by edge A-B intersecting the opposing polygon
            /// on its front face.
            /// </summary>
            public bool Direction;
        }

        /// <summary>
        /// Input operations and data to ProcessFace
        /// </summary>
        /// <typeparam name="TPoint">A point on the face.</typeparam>
        public interface IProcessFaceInput<TPoint>
            where TPoint : struct, IEquatable<TPoint>
        {
            /// <summary>
            /// Gets the point after the one specified, in the "loop". If the loop ends in that
            /// direction, null is returned.
            /// </summary>
            TPoint? LoopNext(TPoint Point);

            /// <summary>
            /// Gets the point after the one specified in the face such that
            /// any point and its next form an edge on the original polygon.
            /// </summary>
            TPoint FaceNext(TPoint Point);

            /// <summary>
            /// Gets and removes an endpoint edge, if one exists.
            /// </summary>
            EndpointEdge<TPoint>? PopEndpointEdge();

            /// <summary>
            /// Gets if the specified edge contains endpoints, and if so, removes and returns it.
            /// </summary>
            EndpointEdge<TPoint>? PopEndpointEdge(TPoint A, TPoint B);

            /// <summary>
            /// Adds an edge (present on the loop) to the final face.
            /// </summary>
            void AddLoopEdge(TPoint A, TPoint B);

            /// <summary>
            /// Adds an edge (to be included in the final face) that was part of an endpoint edge.
            /// </summary>
            void AddIncludedEndpointEdge(TPoint A, TPoint B);

            /// <summary>
            /// Signals that an edge that was part of an endpoint edge is excluded from the final face.
            /// </summary>
            void AddExcludedEndpointEdge(TPoint A, TPoint B);

            /// <summary>
            /// Marks the specified edge (present in the original face) as excluded (from final face), 
            /// meaning it is inside the area defined by the loop.
            /// </summary>
            void ExcludeEdge(TPoint A, TPoint B);

            /// <summary>
            /// Marks the specified edge as included (to final face), outside the loop.
            /// </summary>
            void IncludeEdge(TPoint A, TPoint B);
        }

        /// <summary>
        /// Given a face in the intermediate stages of CSG with intersections and loops known, calculates
        /// the final edges of the face, and determines which original segments are included (present in the
        /// final result) or excluded (not present in the final result).
        /// </summary>
        public static void ProcessFace<TInput, TPoint>(TInput Input)
            where TPoint : struct, IEquatable<TPoint>
            where TInput : IProcessFaceInput<TPoint>
        {
            // Maintain a set of points that act as endpoints of the loop onto the face that continue
            // with LoopNext.
            HashSet<TPoint> fowardendpoints = new HashSet<TPoint>();
            
            // Cycle through the edges starting at endpoints, determine wether these edges are included
            // or excluded, and find which of the endpoints are foward.
            EndpointEdge<TPoint>? eestartq;
            while ((eestartq = Input.PopEndpointEdge()) != null)
            {
                EndpointEdge<TPoint> curedge /* lol */ = eestartq.Value;
                TPoint startpoint = curedge.A;
                bool finishedchain = false;
                while(!finishedchain)
                {
                    TPoint last = curedge.A;
                    bool addnext = curedge.Direction;
                    foreach (TPoint edgepoint in curedge.Endpoints)
                    {
                        if (addnext)
                        {
                            Input.AddIncludedEndpointEdge(last, edgepoint);
                            fowardendpoints.Add(edgepoint);
                            addnext = false;
                        }
                        else
                        {
                            Input.AddExcludedEndpointEdge(last, edgepoint);
                            addnext = true;
                        }
                        last = edgepoint;
                    }
                    if (addnext)
                    {
                        Input.AddIncludedEndpointEdge(last, curedge.B);
                    }
                    else
                    {
                        Input.AddExcludedEndpointEdge(last, curedge.A);
                    }

                    TPoint curpoint = curedge.B;
                    while (true)
                    {
                        if (curpoint.Equals(startpoint))
                        {
                            finishedchain = true;
                            break;
                        }
                        TPoint nextpoint = Input.FaceNext(curpoint);
                        EndpointEdge<TPoint>? possiblee = Input.PopEndpointEdge(curpoint, nextpoint);
                        if (possiblee != null)
                        {
                            curedge = possiblee.Value;
                            break;
                        }
                        if (addnext)
                        {
                            Input.IncludeEdge(curpoint, nextpoint);
                        }
                        else
                        {
                            Input.ExcludeEdge(curpoint, nextpoint);
                        }
                        curpoint = nextpoint;
                    }
                }
            }

            // Add loop parts to face
            foreach (TPoint startpoint in fowardendpoints)
            {
                TPoint curpoint = startpoint;
                TPoint nextpoint = Input.LoopNext(startpoint).Value;
                while (true)
                {
                    Input.AddLoopEdge(curpoint, nextpoint);

                    TPoint? possiblenextpoint = Input.LoopNext(nextpoint);
                    if (possiblenextpoint != null)
                    {
                        curpoint = nextpoint;
                        nextpoint = possiblenextpoint.Value;
                    }
                    else
                    {
                        break;
                    }
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
                var pfi = new _ProcessFaceInput(faceint, NextInLoop, facedata, Excluded, Included);
                ProcessFace<_ProcessFaceInput, int>(pfi);
                Output.Add(pfi.BuildPolygon(facedata.Plane));
            }
        }

        /// <summary>
        /// Default process face input for process faces.
        /// </summary>
        private struct _ProcessFaceInput : IProcessFaceInput<int>
        {
            public _ProcessFaceInput(
                _FaceIntersection FaceIntersections,
                Dictionary<int, int> NextInLoop,
                PolyhedronFace<Triangle<int>, Point, int> Face,
                HashSet<Segment<int>> Excluded,
                HashSet<Segment<int>> Included)
            {
                this.NextInLoop = NextInLoop;
                this.Included = Included;
                this.Excluded = Excluded;

                // Build next in face, construct points.
                this.NextInFace = new Dictionary<int, int>();
                this.Points = new Dictionary<int, Point>();
                foreach (Segment<int> faceseg in Face.Segments)
                {
                    var deref = Face.Points[faceseg.A];
                    this.NextInFace.Add(deref.B, Face.Points[faceseg.B].B);
                    this.Points.Add(deref.B, deref.A);
                }

                // End points
                this.EndpointEdges = new Dictionary<Segment<int>, EndpointEdge<int>>();
                var curedges = new Dictionary<Segment<int>, List<Tuple<int, _EdgeIntersection>>>();
                foreach (var edgeint in FaceIntersections.IntersectingEdges)
                {
                    int edgeind = edgeint.Value.Edge;
                    Segment<int> faceseg = Face.Segments[edgeind];
                    Segment<int> realseg = new Segment<int>(Face.Points[faceseg.A].B, Face.Points[faceseg.B].B);
                    Point uv = Segment.Along(
                        new Segment<Point>(Face.Points[faceseg.A].A, Face.Points[faceseg.B].A), 
                        edgeint.Value.Reverse ? 1.0 - edgeint.Value.Length : edgeint.Value.Length);
                    this.Points.Add(edgeint.Key, uv);

                    List<Tuple<int, _EdgeIntersection>> es;
                    if (!curedges.TryGetValue(realseg, out es))
                    {
                        curedges[realseg] = es = new List<Tuple<int, _EdgeIntersection>>();
                    }
                    es.Add(Tuple.Create(edgeint.Key, edgeint.Value));
                }
                foreach (var edgeints in curedges)
                {
                    // Sort and add as endpoints
                    var intlist = edgeints.Value;
                    Sort.InPlace<Tuple<int, _EdgeIntersection>>((a, b) => (a.B.Reverse ? 1.0 - a.B.Length : a.B.Length) > (b.B.Reverse ? 1.0 - b.B.Length : b.B.Length), intlist);
                    List<int> endpoints = new List<int>();
                    foreach (var endpoint in intlist)
                    {
                        endpoints.Add(endpoint.A);
                    }
                    this.EndpointEdges.Add(edgeints.Key, new EndpointEdge<int>() { A = edgeints.Key.A, B = edgeints.Key.B, Direction = !intlist[0].B.Reverse, Endpoints = endpoints });
                }

                // Loop points
                foreach (var looppoint in FaceIntersections.UV)
                {
                    this.Points.Add(looppoint.Key, looppoint.Value);
                }

                this.FinalFace = new HashSet<Segment<int>>();
            }

            public int? LoopNext(int Point)
            {
                int n = NextInLoop[Point];
                if (Points.ContainsKey(n))
                {
                    return n;
                }
                else
                {
                    return null;
                }
            }

            public int FaceNext(int Point)
            {
                return this.NextInFace[Point];
            }

            public EndpointEdge<int>? PopEndpointEdge()
            {
                var en = this.EndpointEdges.GetEnumerator();
                if (en.MoveNext())
                {
                    var seg = en.Current;
                    this.EndpointEdges.Remove(seg.Key);
                    return seg.Value;
                }
                else
                {
                    return null;
                }
            }

            public EndpointEdge<int>? PopEndpointEdge(int A, int B)
            {
                Segment<int> seg = new Segment<int>(A, B);
                EndpointEdge<int> ee;
                if (this.EndpointEdges.TryGetValue(seg, out ee))
                {
                    this.EndpointEdges.Remove(seg);
                    return ee;
                }
                else
                {
                    return null;
                }
            }

            public void AddLoopEdge(int A, int B)
            {
                FinalFace.Add(new Segment<int>(A, B));
            }

            public void AddIncludedEndpointEdge(int A, int B)
            {
                FinalFace.Add(new Segment<int>(A, B));
            }

            public void AddExcludedEndpointEdge(int A, int B)
            {
              
            }

            public void ExcludeEdge(int A, int B)
            {
                Excluded.Add(new Segment<int>(A, B));
            }

            public void IncludeEdge(int A, int B)
            {
                FinalFace.Add(new Segment<int>(A, B));
                Included.Add(new Segment<int>(A, B));
            }

            /// <summary>
            /// Creates the new polygon for this face.
            /// </summary>
            public PolyhedronFace<Triangle<int>, Point, int> BuildPolygon(Triangle<int> Plane)
            {
                throw new NotImplementedException();
            }

            public Dictionary<int, int> NextInLoop;
            public Dictionary<int, Point> Points;
            public Dictionary<Segment<int>, EndpointEdge<int>> EndpointEdges;
            public Dictionary<int, int> NextInFace;
            public HashSet<Segment<int>> Included;
            public HashSet<Segment<int>> Excluded;
            public HashSet<Segment<int>> FinalFace;
        }
    }
}