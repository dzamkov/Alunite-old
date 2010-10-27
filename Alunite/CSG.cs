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
            var aintsb = _Intersect(Geometry, B, A, asegs);
            var bintsa = _Intersect(Geometry, A, B, bsegs);

            foreach (var a in aintsb)
            {

            }

            return res;
        }

        /// <summary>
        /// Represents a face segment intersection.
        /// </summary>
        private struct _Intersection
        {
            /// <summary>
            /// Vertex created as a result of the intersection.
            /// </summary>
            public int NewVertex;

            /// <summary>
            /// The face that was hit.
            /// </summary>
            public int TargetFace;

            /// <summary>
            /// The edge that acted as the segment in the intersection.
            /// </summary>
            public FaceEdge<int, int> Edge;
        }

        /// <summary>
        /// Gets the intersections of the specified segments onto the target polyhedron.
        /// </summary>
        private static IEnumerable<_Intersection> _Intersect(
            VectorGeometry Geometry, VectorPolyhedron Target, VectorPolyhedron Source, 
            IEnumerable<UnorderedSegment<int>> Segments)
        {
            foreach (int face in Target.Faces)
            {
                PolyhedronFace<Triangle<int>, VectorPoint> facedata = Target.Lookup(face);
                IEnumerable<Segment<Point>> polygon = _PolygonConvert(facedata.Segments);
                Triangle<Vector> plane = Geometry.Dereference(facedata.Plane);
                foreach (UnorderedSegment<int> seg in Segments)
                {
                    double len;
                    Point uv;
                    Vector pos;
                    Segment<int> hitseg = Triangle.Intersect(plane, Geometry.Dereference(seg.Source), out len, out pos, out uv) ? seg.Source : seg.Source.Flip;
                    if (Polygon.PointTest(uv, polygon).Relation == AreaRelation.Inside)
                    {
                        FaceEdge<int, int> edge = Source.SegmentFace(hitseg).Value;
                        yield return new _Intersection()
                        {
                            NewVertex = Geometry.AddVertex(pos),
                            Edge = edge,
                            TargetFace = face
                        };
                    }
                }
            }
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
    }
}