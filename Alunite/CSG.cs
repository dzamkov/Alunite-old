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
            Dictionary<_FacePair, List<_Intersection>> intersections = new Dictionary<_FacePair, List<_Intersection>>();
            _Intersect(Geometry, A, B, true, asegs, intersections);
            _Intersect(Geometry, A, B, false, bsegs, intersections);
            foreach (List<_Intersection> ilist in intersections.Values)
            {
                _SortPlanarIntersections(Geometry, ilist);
            }

            foreach (PolyhedronFace<Triangle<int>, VectorPoint> poly in A.FaceData)
            {
                res.Add(poly.Segments, poly.Plane);
            }
            foreach (PolyhedronFace<Triangle<int>, VectorPoint> poly in B.FaceData)
            {
                res.Add(poly.Segments, poly.Plane);
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
        /// intersection dictionary.
        /// </summary>
        private static void _Intersect(
            VectorGeometry Geometry, VectorPolyhedron A, VectorPolyhedron B,
            bool SegmentsAreA,
            IEnumerable<UnorderedSegment<int>> Segments,
            Dictionary<_FacePair, List<_Intersection>> Intersections)
        {
            var res = new Dictionary<_FacePair, List<_Intersection>>();
            var segsource = SegmentsAreA ? A : B;
            var facesource = SegmentsAreA ? B : A;
            foreach (int face in facesource.Faces)
            {
                PolyhedronFace<Triangle<int>, VectorPoint> facedata = facesource.Lookup(face);
                IEnumerable<Segment<Point>> poly = _PolygonConvert(facedata.Segments);
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
        private static IEnumerable<Segment<Point>> _PolygonConvert(IEnumerable<Segment<VectorPoint>> Face)
        {
            foreach (Segment<VectorPoint> seg in Face)
            {
                yield return new Segment<Point>(seg.A.UV, seg.B.UV);
            }
        }

        /// <summary>
        /// Sorts the provided list of intersections (all lying on the intersection of two planes) to be consistent with loop
        /// direction. The intersections are sorted to ascend left when looking into both front planes on their front face.
        /// </summary>
        private static void _SortPlanarIntersections(VectorGeometry Geometry, List<_Intersection> Intersections)
        {
            // Get if the first two intersections are in the right direction
            _Intersection first = Intersections[0];
            _Intersection second = Intersections[1];
            bool swap = false;
            if (first.ASegment == second.ASegment)
            {
                if (first.Direction == second.Direction)
                {
                    // Rare case when the direction can not be identified by these points alone. Luckily, it can be
                    // guarnteed another intersection exists.
                    for (int i = 2; i < Intersections.Count; i++)
                    {
                        _Intersection next = Intersections[i];
                        if (next.ASegment == second.ASegment && next.Direction != first.Direction)
                        {
                            Intersections[1] = next;
                            Intersections[i] = second;
                            second = next;
                            break;
                        }
                    } 
                }
                swap = first.Direction ^ first.ASegment;
            }
            else
            {
                if (first.ASegment)
                {
                    swap = !first.Direction;
                }
                else
                {
                    swap = second.Direction;
                }
            }


            if (Intersections.Count > 2)
            {
                // Compute distances
                Vector va = Geometry.Lookup(first.NewVertex);
                Vector vb = Geometry.Lookup(second.NewVertex);
                Vector d = vb - va;
                var dists = new _IntersectionDistance[Intersections.Count];
                dists[0] = new _IntersectionDistance(0, 0.0);
                dists[1] = new _IntersectionDistance(1, 1.0);
                for (int i = 2; i < Intersections.Count; i++)
                {
                    dists[i] = new _IntersectionDistance(i, Vector.Dot(Geometry.Lookup(Intersections[i].NewVertex) - va, d));
                }

                // Sort
                Sort.InPlace<StandardArray<_IntersectionDistance>, _IntersectionDistance>(new StandardArray<_IntersectionDistance>(dists), x => (x.A.Dist > x.B.Dist ^ swap));
                _Intersection[] temp = new _Intersection[Intersections.Count];
                Intersections.CopyTo(temp);
                for (int i = 0; i < dists.Length; i++)
                {
                    Intersections[i] = temp[dists[i].Ref];
                }
            }
            else
            {
                if (swap)
                {
                    // Quick swap
                    _Intersection temp = Intersections[0];
                    Intersections[0] = Intersections[1];
                    Intersections[1] = temp;
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
    }
}