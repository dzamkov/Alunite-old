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
            var aints = Intersections(Geometry, asegs, B);
            var bints = Intersections(Geometry, bsegs, A);

            // Process
            List<_Intersection> arawints = new List<_Intersection>();
            List<_Intersection> brawints = new List<_Intersection>();
            Dictionary<int, _FaceIntersection> afaceinfo = new Dictionary<int,_FaceIntersection>();
            Dictionary<int, _FaceIntersection> bfaceinfo = new Dictionary<int,_FaceIntersection>();
            _ProcessSegmentIntersections(A, B, aints, true, arawints, afaceinfo, bfaceinfo);
            _ProcessSegmentIntersections(B, A, bints, false, brawints, bfaceinfo, afaceinfo);

            // Loops
            var nextinloop = new Dictionary<int, int>();
            var previnloop = new Dictionary<int, int>();
            MakeLoops<_VectorMakeLoopsInput, _Intersection, int>(new _VectorMakeLoopsInput(
                Geometry, A, B, arawints, brawints, nextinloop, previnloop));

            // Boundary faces
            HashSet<Segment<int>> aincluded = new HashSet<Segment<int>>();
            HashSet<Segment<int>> aexcluded = new HashSet<Segment<int>>();
            HashSet<Segment<int>> bincluded = new HashSet<Segment<int>>();
            HashSet<Segment<int>> bexcluded = new HashSet<Segment<int>>();
            _ProcessFaces(nextinloop, afaceinfo, A, res, aincluded, aexcluded);
            _ProcessFaces(previnloop, bfaceinfo, B, res, bincluded, bexcluded);

            // Fill
            _Propagate(aincluded, A, res);
            _Propagate(bincluded, B, res);

            return res;
        }

        /// <summary>
        /// An intersection between a segment and a face.
        /// </summary>
        public struct Intersection<TVertex, TPoint>
        {
            public Intersection(TVertex Vertex, TPoint Point, bool Direction)
            {
                this.Vertex = Vertex;
                this.Point = Point;
                this.Direction = Direction;
            }

            /// <summary>
            /// The vertex representing the point of intersection.
            /// </summary>
            public TVertex Vertex;

            /// <summary>
            /// The point on the face the hit is at.
            /// </summary>
            public TPoint Point;

            /// <summary>
            /// True if the segment hit the face on its front face, false otherwise.
            /// </summary>
            public bool Direction;
        }

        /// <summary>
        /// An intersection of a segment onto a face.
        /// </summary>
        public struct FaceIntersection<TFace, TVertex, TPoint>
        {
            public FaceIntersection(TFace Face, Intersection<TVertex, TPoint> Intersection)
            {
                this.Face = Face;
                this.Intersection = Intersection;
            }

            public FaceIntersection(TFace Face, TVertex Vertex, TPoint Point, bool Direction)
            {
                this.Face = Face;
                this.Intersection = new Intersection<TVertex, TPoint>(Vertex, Point, Direction);
            }

            /// <summary>
            /// The face that was hit.
            /// </summary>
            public TFace Face;

            /// <summary>
            /// The intersection of the face and segment.
            /// </summary>
            public Intersection<TVertex, TPoint> Intersection;
        }

        /// <summary>
        /// An intersection in a vector geometry.
        /// </summary>
        public struct VectorIntersection
        {
            /// <summary>
            /// Length along the segment the intersection is at.
            /// </summary>
            public double Length;

            /// <summary>
            /// The intersection of the segment onto the face.
            /// </summary>
            public FaceIntersection<int, int, Point> Intersection;
        }

        /// <summary>
        /// Gets the intersections between a collection of segments and a polyhedron of faces. This function
        /// may add new vertices to the geometry to indicate intersections.
        /// </summary>
        public static Dictionary<Segment<int>, List<VectorIntersection>> Intersections(
            VectorGeometry Geometry,
            IEnumerable<UnorderedSegment<int>> Segments,
            VectorPolyhedron Faces)
        {
            var res = new Dictionary<Segment<int>, List<VectorIntersection>>();

            // Collect intersections brute forcily
            foreach (int face in Faces.Faces)
            {
                PolyhedronFace<Triangle<int>, Point, int> facedata = Faces.Lookup(face);
                IEnumerable<Segment<Point>> poly = _PolygonConvert(facedata);
                Triangle<Vector> plane = Geometry.Dereference(facedata.Plane);
                foreach (var seg in Segments)
                {
                    Segment<int> iseg = seg.Source;
                    Segment<Vector> hitseg = Geometry.Dereference(iseg);
                    double len;
                    Point uv;
                    Vector pos;
                    bool dir = Triangle.Intersect(plane, hitseg, out len, out pos, out uv);
                    if (len > 0.0 && len < 1.0 && Polygon.PointTest(uv, poly).Relation == AreaRelation.Inside)
                    {
                        int nvert = Geometry.AddVertex(pos);

                        List<VectorIntersection> vlist;
                        if (!res.TryGetValue(iseg, out vlist))
                        {
                            res[iseg] = vlist = new List<VectorIntersection>();
                        }
                        vlist.Add(new VectorIntersection()
                        {
                            Length = len,
                            Intersection = new FaceIntersection<int,int,Point>(face, nvert, uv, dir)
                        });
                    }
                }
            }

            // Sort intersections within segments
            foreach (var vlist in res.Values)
            {
                if (vlist.Count > 1)
                {
                    Sort.InPlace<VectorIntersection>((a, b) => a.Length > b.Length, vlist);
                }
            }

            return res;
        }

        /// <summary>
        /// Input to the make loops function.
        /// </summary>
        /// <typeparam name="TIntersection">An intersection between a segment and a face that produces a vertex.</typeparam>
        /// <typeparam name="TFace">A face in one of the polyhedra.</typeparam>
        public interface IMakeLoopsInput<TIntersection, TFace>
            where TFace : IEquatable<TFace>
        {
            /// <summary>
            /// Gets all the intersections produced by segments of A.
            /// </summary>
            IEnumerable<TIntersection> AIntersections { get; }

            /// <summary>
            /// Gets all the intersections produced by segments of B.
            /// </summary>
            IEnumerable<TIntersection> BIntersections { get; }

            /// <summary>
            /// Gets the face that is involved in one of the intersections.
            /// </summary>
            TFace HitFace(TIntersection Intersection);

            /// <summary>
            /// Gets the face that produces the segment that is involved in the specified intersection such that the segment
            /// hits the target face on its front.
            /// </summary>
            TFace FrontFace(TIntersection Intersection);

            /// <summary>
            /// Gets the face that produces the segment that is involved in the specified intersection such that the
            /// segment hits the target face on its back.
            /// </summary>
            TFace BackFace(TIntersection Intersection);

            /// <summary>
            /// Gets if segment involved in the specified intersection is from polyhedron A.
            /// </summary>
            bool IsSegmentFromA(TIntersection Intersection);

            /// <summary>
            /// Sorts the intersections and associated data (known to have colinear intersection points) in the list so that they
            /// appear in order along the line they are all colinear to (in either direction).
            /// </summary>
            void Sort<TDatum>(List<Tuple<TIntersection, TDatum>> Intersections);

            /// <summary>
            /// Indicates that segment containing the vertices for the specified intersections is in the loop. A creates 
            /// the vertex before B in the loop and B creates the vertex after A in the loop.
            /// </summary>
            void MarkLoop(TIntersection A, TIntersection B);
        }

        /// <summary>
        /// Marks the "loops" formed by the intersection of two polyhedra. Loops are directed, closed, circuits that indicate
        /// a common area of the two polyhedra. Loops are directed as to be consistent with the faces of polyhedra A.
        /// </summary>
        public static void MakeLoops<TInput, TIntersection, TFace>(TInput Input)
            where TInput : IMakeLoopsInput<TIntersection, TFace>
            where TFace : IEquatable<TFace>
        {
            var intersections = new Dictionary<_FacePair<TFace>, List<Tuple<TIntersection, bool>>>();
            
            // Get face pair intersections.
            foreach (TIntersection inta in Input.AIntersections)
            {
                _Append(intersections, new _FacePair<TFace>(Input.FrontFace(inta), Input.HitFace(inta)), Tuple.Create(inta, true));
                _Append(intersections, new _FacePair<TFace>(Input.BackFace(inta), Input.HitFace(inta)), Tuple.Create(inta, false));
            }
            foreach (TIntersection intb in Input.BIntersections)
            {
                _Append(intersections, new _FacePair<TFace>(Input.HitFace(intb), Input.FrontFace(intb)), Tuple.Create(intb, true));
                _Append(intersections, new _FacePair<TFace>(Input.HitFace(intb), Input.BackFace(intb)), Tuple.Create(intb, false));
            }

            // Sort n stuff
            foreach (var li in intersections.Values)
            {
                Input.Sort(li);
                

                // Check if the order of intersections is consistent with polyhedron A.
                var firsti = li[0];
                var lasti = li[li.Count - 1];
                bool firsta = Input.IsSegmentFromA(firsti.A);
                bool lasta = Input.IsSegmentFromA(lasti.A);
                bool firstd = firsti.B;
                bool lastd = lasti.B;
                bool consistent;
                if (firsta && lasta)
                {
                    consistent = !(firstd ^ firsta);
                }
                else
                {
                    if (firsta)
                    {
                        consistent = firstd;
                    }
                    else
                    {
                        consistent = lastd;
                    }
                }

                // Output segments in list
                // Note: either all lists have an even number of elements, or the input is bad
                for (int t = 0; t < li.Count / 2; t++)
                {
                    TIntersection a = li[t + 0].A;
                    TIntersection b = li[t + 1].A;
                    if (consistent)
                    {
                        Input.MarkLoop(a, b);
                    }
                    else
                    {
                        Input.MarkLoop(b, a);
                    }
                }
            }
        }

        /// <summary>
        /// Make loops input for vector polyhedron.
        /// </summary>
        private struct _VectorMakeLoopsInput : IMakeLoopsInput<_Intersection, int>
        {
            public _VectorMakeLoopsInput(
                VectorGeometry Geometry,
                VectorPolyhedron A,
                VectorPolyhedron B,
                IEnumerable<_Intersection> AInts,
                IEnumerable<_Intersection> BInts,
                Dictionary<int, int> NextInLoop,
                Dictionary<int, int> PrevInLoop)
            {
                this.NextInLoop = NextInLoop;
                this.PrevInLoop = PrevInLoop;
                this.Geometry = Geometry;
                this.AIntersections = AInts;
                this.BIntersections = BInts;
            }

            IEnumerable<_Intersection> IMakeLoopsInput<_Intersection, int>.AIntersections
            {
                get 
                {
                    return this.AIntersections;
                }
            }

            IEnumerable<_Intersection> IMakeLoopsInput<_Intersection, int>.BIntersections
            {
                get 
                {
                    return this.BIntersections;
                }
            }

            public int HitFace(_Intersection Intersection)
            {
                return Intersection.HitFace;
            }

            public int FrontFace(_Intersection Intersection)
            {
                return Intersection.FrontFace;
            }

            public int BackFace(_Intersection Intersection)
            {
                return Intersection.BackFace;
            }

            public bool IsSegmentFromA(_Intersection Intersection)
            {
                return Intersection.ASegment;
            }

            public void Sort<TDatum>(List<Tuple<_Intersection, TDatum>> Intersections)
            {
                if (Intersections.Count > 2)
                {
                    VectorGeometry geo = this.Geometry;
                    Alunite.Sort.InPlace((a, b) => Vector.Compare(geo.Lookup(a.A.NewVertex), geo.Lookup(b.A.NewVertex)), Intersections);
                }
            }

            public void MarkLoop(_Intersection A, _Intersection B)
            {
                this.NextInLoop.Add(A.NewVertex, B.NewVertex);
                this.PrevInLoop.Add(B.NewVertex, A.NewVertex);
            }

            public VectorGeometry Geometry;
            public IEnumerable<_Intersection> AIntersections;
            public IEnumerable<_Intersection> BIntersections;
            public Dictionary<int, int> NextInLoop;
            public Dictionary<int, int> PrevInLoop;
        }

        /// <summary>
        /// An intersection for _VectorMakeLoopsInput.
        /// </summary>
        private struct _Intersection
        {
            /// <summary>
            /// Vertex created as a result of the intersection.
            /// </summary>
            public int NewVertex;

            /// <summary>
            /// Gets the face that was hit in the intersection.
            /// </summary>
            public int HitFace;

            /// <summary>
            /// Gets the face that produces the segment that hits the face on its front.
            /// </summary>
            public int FrontFace;

            /// <summary>
            /// Gets the face that produces the segment that hits the face on its back.
            /// </summary>
            public int BackFace;

            /// <summary>
            /// Length along the edge the hit is at.
            /// </summary>
            public double Length;

            /// <summary>
            /// True if A owns the segment that's part of the intersection.
            /// </summary>
            public bool ASegment;
        }

        /// <summary>
        /// A pair of faces that intersect.
        /// </summary>
        private struct _FacePair<TFace> : IEquatable<_FacePair<TFace>>
            where TFace : IEquatable<TFace>
        {
            public _FacePair(TFace AFace, TFace BFace)
            {
                this.AFace = AFace;
                this.BFace = BFace;
            }

            public bool Equals(_FacePair<TFace> other)
            {
                return this.AFace.Equals(other.AFace) && this.BFace.Equals(other.BFace);
            }

            public override int GetHashCode()
            {
                return this.AFace.GetHashCode() ^ this.BFace.GetHashCode();
            }

            public TFace AFace;

            public TFace BFace;
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
        /// Processes the set of intersections between polyhedra.
        /// </summary>
        private static void _ProcessSegmentIntersections(
            VectorPolyhedron SegmentPolyhedron,
            VectorPolyhedron FacePolyhedron,
            Dictionary<Segment<int>, List<VectorIntersection>> SegmentIntersections,
            bool ASegment,
            List<_Intersection> Intersections,
            Dictionary<int, _FaceIntersection> SegmentPolyhedronIntersections,
            Dictionary<int, _FaceIntersection> FacePolyhedronIntersections)
        {
            foreach (var segint in SegmentIntersections)
            {
                List<VectorIntersection> vis = segint.Value;

                // Add segment to face intersection data.
                FaceEdge<int, int> frontedge = SegmentPolyhedron.SegmentFace(segint.Key).Value;
                FaceEdge<int, int> backedge = SegmentPolyhedron.SegmentFace(segint.Key.Flip).Value;
                _FaceIntersection fif = _FaceIntersection.Get(frontedge.Face, SegmentPolyhedronIntersections);
                _FaceIntersection fib = _FaceIntersection.Get(backedge.Face, SegmentPolyhedronIntersections);

                _EdgeIntersection[] eif = new _EdgeIntersection[vis.Count];
                for (int t = 0; t < eif.Length; t++)
                {
                    var vi = vis[t];
                    Intersection<int, Point> ii = vi.Intersection.Intersection;
                    eif[t] = new _EdgeIntersection()
                    {
                        Length = vi.Length,
                        NewVertex = ii.Vertex,
                        Direction = ii.Direction
                    };
                }
                fif.IntersectingEdges.Add(frontedge.Edge, eif);

                _EdgeIntersection[] bif = new _EdgeIntersection[vis.Count];
                for (int t = 0; t < bif.Length; t++)
                {
                    var vi = vis[bif.Length - t - 1];
                    Intersection<int, Point> ii = vi.Intersection.Intersection;
                    bif[t] = new _EdgeIntersection()
                    {
                        Length = vi.Length,
                        NewVertex = ii.Vertex,
                        Direction = !ii.Direction
                    };
                }
                fib.IntersectingEdges.Add(backedge.Edge, bif);


                // Go through individual intersections
                foreach (VectorIntersection vi in vis)
                {
                    bool dir = vi.Intersection.Intersection.Direction;
                    int hitface = vi.Intersection.Face;
                    _FaceIntersection.Get(hitface, FacePolyhedronIntersections).UV.Add(vi.Intersection.Intersection.Vertex, vi.Intersection.Intersection.Point);
                    Intersections.Add(new _Intersection()
                    {
                        ASegment = ASegment,
                        FrontFace = dir ? frontedge.Face : backedge.Face,
                        BackFace = dir ? backedge.Face : frontedge.Face,
                        HitFace = hitface,
                        Length = vi.Length,
                        NewVertex = vi.Intersection.Intersection.Vertex
                    });
                }
            }
        }

        /// <summary>
        /// Information about a single intersecting face during a CSG operation.
        /// </summary>
        private struct _FaceIntersection
        {
            /// <summary>
            /// Gets information about the new vertices created on the edges.
            /// </summary>
            public Dictionary<int, _EdgeIntersection[]> IntersectingEdges;

            /// <summary>
            /// Gets the UV coordinates of new vertices created on the surface of the Face.
            /// </summary>
            public Dictionary<int, Point> UV;

            /// <summary>
            /// Gets the face intersection information from a face in a dictionary, or creates an entry if none exists.
            /// </summary>
            public static _FaceIntersection Get(int Face, Dictionary<int, _FaceIntersection> Dictionary)
            {
                _FaceIntersection f;
                if (!Dictionary.TryGetValue(Face, out f))
                {
                    Dictionary[Face] = f = new _FaceIntersection()
                    {
                        IntersectingEdges = new Dictionary<int, _EdgeIntersection[]>(),
                        UV = new Dictionary<int, Point>()
                    };
                }
                return f;
            }
        }

        /// <summary>
        /// Represents an intersection of a polygon's edge.
        /// </summary>
        private struct _EdgeIntersection
        {
            /// <summary>
            /// The new vertex produced at the intersection point.
            /// </summary>
            public int NewVertex;

            /// <summary>
            /// Length along the edge the intersection is at.
            /// </summary>
            public double Length;

            /// <summary>
            /// Did the intersection go in the same direction of as the edge?
            /// </summary>
            public bool Direction;
        }

        /// <summary>
        /// An endpoint (loop-boundary) intersection on the edge of a face.
        /// </summary>
        /// <typeparam name="TPoint">A point on the face.</typeparam>
        public struct EdgeEndpoint<TPoint>
        {
            /// <summary>
            /// Gets the point the intersection is at.
            /// </summary>
            public TPoint Point;

            /// <summary>
            /// True if the point was created from the edge intersecting an opposing face on
            /// its front side. False to indicate the back side.
            /// </summary>
            public bool Direction;
        }

        /// <summary>
        /// Input operations and data to ProcessFace
        /// </summary>
        /// <typeparam name="TPoint">A point on the face.</typeparam>
        /// <typeparam name="TEdge">An edge on the original face.</typeparam>
        public interface IProcessFaceInput<TPoint, TEdge>
            where TPoint : IEquatable<TPoint>
            where TEdge : IEquatable<TEdge>
        {
            /// <summary>
            /// Gets the point after the one specified, in the "loop". If the loop ends in that
            /// direction, null is returned.
            /// </summary>
            bool LoopNext(TPoint Point, out TPoint Next);

            /// <summary>
            /// Gets the edge after the specified edge on the face. If (X, Y) is the given edge,
            /// an edge (Y, Z) on the original face should be returned.
            /// </summary>
            TEdge EdgeNext(TEdge Edge);

            /// <summary>
            /// Gets the segment for the specified edge.
            /// </summary>
            Segment<TPoint> EdgeSegment(TEdge Edge);

            /// <summary>
            /// Gets the points on the intersecting loop that are on the face, including endpoints.
            /// </summary>
            IEnumerable<TPoint> LoopPoints { get; }

            /// <summary>
            /// Removes and returns an edge that contains endpoints (along with the endpoints, in order, on it), if any remain.
            /// </summary>
            bool PopEndpointEdge(out TEdge Edge, out IEnumerable<EdgeEndpoint<TPoint>> Points);

            /// <summary>
            /// If an edge contains endpoints, returns true, removes it, and outputs the endpoints (in order)
            /// along it.
            /// </summary>
            bool RemoveEndpointEdge(TEdge Edge, out IEnumerable<EdgeEndpoint<TPoint>> Points);

            /// <summary>
            /// Adds an edge (present on the loop) to the final face.
            /// </summary>
            void AddLoopEdge(Segment<TPoint> Segment);

            /// <summary>
            /// Adds an edge (to be included in the final face) that was part of an endpoint edge.
            /// </summary>
            void AddIncludedEndpointEdge(TEdge EndpointEdge, Segment<TPoint> Segment);

            /// <summary>
            /// Signals that an edge that was part of an endpoint edge is excluded from the final face.
            /// </summary>
            void AddExcludedEndpointEdge(TEdge EndpointEdge, Segment<TPoint> Segment);

            /// <summary>
            /// Marks the specified edge (present in the original face) as excluded (from final face), 
            /// meaning it is inside the area defined by the loop.
            /// </summary>
            void ExcludeEdge(TEdge Edge);

            /// <summary>
            /// Marks the specified edge as included (to final face), outside the loop.
            /// </summary>
            void IncludeEdge(TEdge Edge);
        }

        /// <summary>
        /// Given a face in the intermediate stages of CSG with intersections and loops known, calculates
        /// the final edges of the face, and determines which original segments are included (present in the
        /// final result) or excluded (not present in the final result).
        /// </summary>
        public static void ProcessFace<TInput, TPoint, TEdge>(TInput Input)
            where TPoint : IEquatable<TPoint>
            where TEdge : IEquatable<TEdge>
            where TInput : IProcessFaceInput<TPoint, TEdge>
        {
            // Cycle through the edges starting at endpoints, determine wether these edges are included
            // or excluded.
            TEdge firstedge;
            IEnumerable<EdgeEndpoint<TPoint>> points;      
            while (Input.PopEndpointEdge(out firstedge, out points))
            {
                TEdge edge = firstedge;
                bool exit = false;
                do
                {
                    // Fill in information for the endpoint edge.
                    Segment<TPoint> seg = Input.EdgeSegment(edge);
                    TPoint last = seg.A;
                    bool including = false;
                    foreach (EdgeEndpoint<TPoint> endpoint in points)
                    {
                        including = !endpoint.Direction;
                        if (endpoint.Direction)
                        {
                            Input.AddIncludedEndpointEdge(edge, new Segment<TPoint>(last, endpoint.Point));
                        }
                        else
                        {
                            Input.AddExcludedEndpointEdge(edge, new Segment<TPoint>(last, endpoint.Point));
                        }
                        last = endpoint.Point;
                    }
                    if (including)
                    {
                        Input.AddIncludedEndpointEdge(edge, new Segment<TPoint>(last, seg.B));
                    }
                    else
                    {
                        Input.AddExcludedEndpointEdge(edge, new Segment<TPoint>(last, seg.B));
                    }

                    // Exclude/include subsequent edges
                    while (true)
                    {
                        edge = Input.EdgeNext(edge);
                        if (edge.Equals(firstedge))
                        {
                            exit = true;
                            break;
                        }
                        if (Input.RemoveEndpointEdge(edge, out points))
                        {
                            break;
                        }
                        else
                        {
                            if (including)
                            {
                                Input.IncludeEdge(edge);
                            }
                            else
                            {
                                Input.ExcludeEdge(edge);
                            }
                        }
                    }
                } while (!exit);
            }

            // Loop edges
            foreach (TPoint looppoint in Input.LoopPoints)
            {
                TPoint nextpoint;
                if (Input.LoopNext(looppoint, out nextpoint))
                {
                    Input.AddLoopEdge(new Segment<TPoint>(looppoint, nextpoint));
                }
            }
        }

        /// <summary>
        /// Process face input for vector polyhedra.
        /// </summary>
        private struct _VectorProcessFaceInput : IProcessFaceInput<int, int>
        {
            public _VectorProcessFaceInput(
                PolyhedronFace<Triangle<int>, Point, int> Face,
                _FaceIntersection Intersection, 
                Dictionary<int, int> NextInLoop,
                HashSet<Segment<int>> Included,
                HashSet<Segment<int>> Excluded)
            {
                this.NextInLoop = NextInLoop;
                this.Included = Included;
                this.Excluded = Excluded;

                // Add to points
                this.Points = new Dictionary<int, Point>();
                this.LoopPoints = new HashSet<int>();
                this.EndpointEdges = new Dictionary<int, List<EdgeEndpoint<int>>>();
                foreach (var facepoint in Face.Points)
                {
                    this.Points.Add(facepoint.B, facepoint.A);
                }
                foreach (var intedge in Intersection.IntersectingEdges)
                {
                    Segment<int> faceedge = Face.Segments[intedge.Key];
                    Segment<Point> uvedge = new Segment<Point>(Face.Points[faceedge.A].A, Face.Points[faceedge.B].A);
                    List<EdgeEndpoint<int>> endpoints = new List<EdgeEndpoint<int>>();
                    foreach (var edgint in intedge.Value)
                    {
                        this.Points.Add(edgint.NewVertex, Segment.Along(uvedge, edgint.Direction ? edgint.Length : 1.0 - edgint.Length));
                        this.LoopPoints.Add(edgint.NewVertex);
                        endpoints.Add(new EdgeEndpoint<int>() { Direction = edgint.Direction, Point = edgint.NewVertex });
                    }
                    this.EndpointEdges.Add(intedge.Key, endpoints);
                }
                foreach (var polyint in Intersection.UV)
                {
                    this.Points.Add(polyint.Key, polyint.Value);
                    this.LoopPoints.Add(polyint.Key);
                }

                // Edges
                this.Segments = new List<Segment<int>>();
                this.EdgeContainingAsA = new Dictionary<int, int>();
                int i = 0;
                foreach (var edge in Face.Segments)
                {
                    int a = Face.Points[edge.A].B;
                    this.Segments.Add(new Segment<int>(a, Face.Points[edge.B].B));
                    this.EdgeContainingAsA.Add(a, i);
                    i++;
                }

                // Initialize
                this.FinalSegments = new List<Segment<int>>();
            }

            public bool LoopNext(int Point, out int Next)
            {
                Next = this.NextInLoop[Point];
                return this.LoopPoints.Contains(Next);
            }

            public int EdgeNext(int Edge)
            {
                return this.EdgeContainingAsA[this.Segments[Edge].B];
            }

            public Segment<int> EdgeSegment(int Edge)
            {
                return this.Segments[Edge];
            }

            IEnumerable<int> IProcessFaceInput<int, int>.LoopPoints
            {
                get 
                {
                    return this.LoopPoints;
                }
            }

            public bool PopEndpointEdge(out int Edge, out IEnumerable<EdgeEndpoint<int>> Points)
            {
                IEnumerator<KeyValuePair<int, List<EdgeEndpoint<int>>>> e = this.EndpointEdges.GetEnumerator();
                if (e.MoveNext())
                {
                    var cur = e.Current;
                    Edge = cur.Key;
                    Points = cur.Value;
                    this.EndpointEdges.Remove(Edge);
                    return true;
                }
                else
                {
                    Edge = 0;
                    Points = null;
                    return false;
                }
            }

            public bool RemoveEndpointEdge(int Edge, out IEnumerable<EdgeEndpoint<int>> Points)
            {
                List<EdgeEndpoint<int>> points;
                if (this.EndpointEdges.TryGetValue(Edge, out points))
                {
                    Points = points;
                    this.EndpointEdges.Remove(Edge);
                    return true;
                }
                else
                {
                    Points = null;
                    return false;
                }
            }

            public void AddLoopEdge(Segment<int> Segment)
            {
                this.FinalSegments.Add(Segment);
            }

            public void AddIncludedEndpointEdge(int EndpointEdge, Segment<int> Segment)
            {
                this.FinalSegments.Add(Segment);
            }

            public void AddExcludedEndpointEdge(int EndpointEdge, Segment<int> Segment)
            {

            }

            public void ExcludeEdge(int Edge)
            {
                this.Excluded.Add(this.Segments[Edge].Flip);
            }

            public void IncludeEdge(int Edge)
            {
                Segment<int> seg = this.Segments[Edge];
                this.FinalSegments.Add(seg);
                this.Included.Add(seg.Flip);
            }

            /// <summary>
            /// Creates the final face for the input.
            /// </summary>
            public PolyhedronFace<Triangle<int>, Point, int> CreateFace(Triangle<int> Plane)
            {
                List<Segment<int>> finalfacesegments = new List<Segment<int>>();
                Dictionary<int, int> finalpointmap = new Dictionary<int, int>();
                Tuple<Point, int>[] finalpoints = new Tuple<Point, int>[this.FinalSegments.Count];

                int i = 0;
                foreach (Segment<int> seg in this.FinalSegments)
                {
                    Segment<int> finalseg = new Segment<int>();
                    if (!finalpointmap.TryGetValue(seg.A, out finalseg.A))
                    {
                        finalseg.A = i;
                        finalpointmap[seg.A] = i;
                        finalpoints[i] = Tuple.Create(this.Points[seg.A], seg.A);
                        i++;
                    }
                    if (!finalpointmap.TryGetValue(seg.B, out finalseg.B))
                    {
                        finalseg.B = i;
                        finalpointmap[seg.B] = i;
                        finalpoints[i] = Tuple.Create(this.Points[seg.B], seg.B);
                        i++;
                    }
                    finalfacesegments.Add(finalseg);
                }

                return new PolyhedronFace<Triangle<int>, Point, int>(Plane, finalpoints, finalfacesegments);
            }

            public HashSet<int> LoopPoints;
            public List<Segment<int>> Segments;
            public Dictionary<int, int> EdgeContainingAsA;
            public Dictionary<int, int> NextInLoop;
            public Dictionary<int, List<EdgeEndpoint<int>>> EndpointEdges;
            public Dictionary<int, Point> Points;
            public List<Segment<int>> FinalSegments;
            public HashSet<Segment<int>> Included;
            public HashSet<Segment<int>> Excluded;
        }

        /// <summary>
        /// Updates the faces in a polyhedron based on intersection data.
        /// </summary>
        private static void _ProcessFaces(
            Dictionary<int, int> NextInLoop,
            Dictionary<int, _FaceIntersection> FaceIntersections,
            VectorPolyhedron Polyhedron,
            VectorPolyhedron OutputPolyhedron,
            HashSet<Segment<int>> Included,
            HashSet<Segment<int>> Excluded)
        {
            foreach(var facei in FaceIntersections)
            {
                var facedata = Polyhedron.Lookup(facei.Key);
                _VectorProcessFaceInput vpfi = new _VectorProcessFaceInput(facedata, facei.Value, NextInLoop, Included, Excluded);
                ProcessFace<_VectorProcessFaceInput, int, int>(vpfi);
                OutputPolyhedron.Add(vpfi.CreateFace(facedata.Plane));
            }
        }


        /// <summary>
        /// Determines which faces in the source polyhedron directly or indirectly produce the specified segments.
        /// The segments are removed in the process.
        /// </summary>
        private static void _Propagate(
            HashSet<Segment<int>> Segments,
            VectorPolyhedron Source,
            VectorPolyhedron Output)
        {
            IEnumerator<Segment<int>> e = Segments.GetEnumerator();
            while (e.MoveNext())
            {
                Segment<int> seg = e.Current;
                int face = Source.SegmentFace(seg).Value.Face;
                var facedata = Source.Lookup(face);

                // Add to output
                Output.Add(facedata);

                // Update segments
                foreach (var faceseg in facedata.Segments)
                {
                    Segment<int> rfaceseg = new Segment<int>(
                        facedata.Points[faceseg.A].B,
                        facedata.Points[faceseg.B].B);

                    if (!Segments.Remove(rfaceseg))
                    {
                        Segments.Add(rfaceseg.Flip);
                    }
                }

                // Next
                e = Segments.GetEnumerator();
            }
        }
    }
}