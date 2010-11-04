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

            // Loops
            var nextinloop = new Dictionary<int, int>();
            var previnloop = new Dictionary<int, int>();
            MakeLoops<_VectorMakeLoopsInput, _Intersection, int>(new _VectorMakeLoopsInput(
                Geometry, A, B, aints, bints, nextinloop, previnloop));

            /*// Intersections
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
            _ProcessFaces(ainfo, A, nextinloop, aexclude, ainclude, res);
            _ProcessFaces(binfo, B, previnloop, bexclude, binclude, res);*/

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
                    if (!dir)
                    {
                        len = 1.0 - len;
                    }
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
                        consistent = !lastd;
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
                Dictionary<Segment<int>, List<VectorIntersection>> AInts,
                Dictionary<Segment<int>, List<VectorIntersection>> BInts,
                Dictionary<int, int> NextInLoop,
                Dictionary<int, int> PrevInLoop)
            {
                this.NextInLoop = NextInLoop;
                this.PrevInLoop = PrevInLoop;
                this.Geometry = Geometry;

                List<_Intersection> aints = new List<_Intersection>();
                List<_Intersection> bints = new List<_Intersection>();
                foreach (var aint in AInts)
                {
                    int front = A.SegmentFace(aint.Key).Value.Face;
                    int back = A.SegmentFace(aint.Key.Flip).Value.Face;
                    foreach (var i in aint.Value)
                    {
                        bool dir = i.Intersection.Intersection.Direction;
                        aints.Add(new _Intersection()
                        {
                            ASegment = true,
                            HitFace = i.Intersection.Face,
                            BackFace = dir ? back : front,
                            FrontFace = dir ? front : back,
                            NewVertex = i.Intersection.Intersection.Vertex,
                            Length = i.Length
                        });
                    }
                }
                foreach (var bint in BInts)
                {
                    int front = B.SegmentFace(bint.Key).Value.Face;
                    int back = B.SegmentFace(bint.Key.Flip).Value.Face;
                    foreach (var i in bint.Value)
                    {
                        bool dir = i.Intersection.Intersection.Direction;
                        bints.Add(new _Intersection()
                        {
                            ASegment = false,
                            HitFace = i.Intersection.Face,
                            BackFace = dir ? back : front,
                            FrontFace = dir ? front : back,
                            NewVertex = i.Intersection.Intersection.Vertex,
                            Length = i.Length
                        });
                    }
                }

                this.AIntersections = aints;
                this.BIntersections = bints;
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
                List<Segment<int>> finalsegs = new List<Segment<int>>();
                Dictionary<int, int> finalpointmap = new Dictionary<int, int>();
                Tuple<Point, int>[] finalpoints = new Tuple<Point,int>[this.FinalFace.Count];

                int i = 0;
                foreach (Segment<int> seg in this.FinalFace)
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
                    finalsegs.Add(finalseg);
                }

                return new PolyhedronFace<Triangle<int>, Point, int>(Plane, finalpoints, finalsegs);
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