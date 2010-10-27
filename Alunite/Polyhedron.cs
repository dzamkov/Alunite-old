using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Defines operations that can be performed on a geometry that supports polyhedra.
    /// </summary>
    /// <typeparam name="Plane">A directional plane within the geometry. An instance of this does not have to
    /// represent a unique plane in space. The only requirement is that points within a plane are consistent.</typeparam>
    /// <typeparam name="Point">A location within a plane.</typeparam>
    /// <typeparam name="Vertex">A location in space.</typeparam>
    public interface IPolyhedronGeomerty<Plane, Point, Vertex>
        where Point : IEquatable<Point>
        where Vertex : IEquatable<Vertex>
    {
        /// <summary>
        /// Gets the actual position of a point on a plane, in the form of a vertex.
        /// </summary>
        Vertex Position(Point Point, Plane Plane);

    }

    /// <summary>
    /// Represents a polyhedron, a three-dimensional surface created from polygons.
    /// </summary>
    /// <typeparam name="Face">A face (polygon) of the polyhedron.</typeparam>
    /// <typeparam name="Edge">An edge on a polygon.</typeparam>
    public interface IPolyhedron<Face, Edge, Plane, Point, Vertex>
        where Point : IEquatable<Point>
        where Vertex : IEquatable<Vertex>
    {
        /// <summary>
        /// Gets the faces that make up the polyhedron.
        /// </summary>
        IEnumerable<Face> Faces { get; }

        /// <summary>
        /// Gets the information about the specified face.
        /// </summary>
        PolyhedronFace<Plane, Point> Lookup(Face Face);

        /// <summary>
        /// Gets the face that includes the specified segment as an edge, if any exists.
        /// </summary>
        FaceEdge<Face, Edge>? SegmentFace(Segment<Vertex> Segment);
    }

    /// <summary>
    /// A polyhedron that can accept new polygons.
    /// </summary>
    public interface IMutablePolyhedron<Face, Edge, Plane, Point, Vertex> : IPolyhedron<Face, Edge, Plane, Point, Vertex>
        where Point : IEquatable<Point>
        where Vertex : IEquatable<Vertex>
    {
        /// <summary>
        /// Adds a new face to the polyhedron, and returns a reference to it.
        /// </summary>
        Face Add(IEnumerable<Segment<Point>> Segments, Plane Plane);

        /// <summary>
        /// Removes a face from the polyhedron.
        /// </summary>
        void Remove(Face Face);
    }

    /// <summary>
    /// Represents an edge on a face.
    /// </summary>
    public struct FaceEdge<TFace, TEdge>
    {
        public FaceEdge(TFace Face, TEdge Edge)
        {
            this.Face = Face;
            this.Edge = Edge;
        }

        public TFace Face;
        public TEdge Edge;
    }

    /// <summary>
    /// A point used by vector geometry.
    /// </summary>
    public struct VectorPoint : IEquatable<VectorPoint>
    {
        public VectorPoint(Point UV, int Vertex)
        {
            this.UV = UV;
            this.Vertex = Vertex;
        }

        public bool Equals(VectorPoint other)
        {
            return this.Vertex == other.Vertex;
        }

        public override int  GetHashCode()
        {
 	        return this.Vertex.GetHashCode();
        }

        /// <summary>
        /// The uv coordinate of the point within its plane.
        /// </summary>
        public Point UV;

        /// <summary>
        /// The vertex the point is at.
        /// </summary>
        public int Vertex;
    }

    /// <summary>
    /// A geometry based on maintaining the set of vectors used in a list.
    /// </summary>
    public class VectorGeometry : IPolyhedronGeomerty<Triangle<int>, VectorPoint, int>
    {
        public VectorGeometry()
        {
            this._Vertices = new List<Vector>();
        }

        /// <summary>
        /// Adds a vertex to the geometry.
        /// </summary>
        public int AddVertex(Vector Position)
        {
            int ind = this._Vertices.Count;
            this._Vertices.Add(Position);
            return ind;
        }

        public int Position(VectorPoint Point, Triangle<int> Plane)
        {
            return Point.Vertex;
        }

        /// <summary>
        /// Gets the actual position of the specified vertex.
        /// </summary>
        public Vector Lookup(int Vertex)
        {
            return this._Vertices[Vertex];
        }

        /// <summary>
        /// Dereferences the vertices in a triangle to vectors.
        /// </summary>
        public Triangle<Vector> Dereference(Triangle<int> Triangle)
        {
            return new Triangle<Vector>(
                this._Vertices[Triangle.A],
                this._Vertices[Triangle.B],
                this._Vertices[Triangle.C]);
        }

        /// <summary>
        /// Dereferences the vertices in a segment to vectors.
        /// </summary>
        public Segment<Vector> Dereference(Segment<int> Segment)
        {
            return new Segment<Vector>(
                this._Vertices[Segment.A],
                this._Vertices[Segment.B]);
        }

        private List<Vector> _Vertices;
    }

    /// <summary>
    /// A face for a polyhedron.
    /// </summary>
    public struct PolyhedronFace<TPlane, Point>
        where Point : IEquatable<Point>
    {
        public PolyhedronFace(TPlane Plane, List<Segment<Point>> Segments)
        {
            this.Plane = Plane;
            this.Segments = Segments;
        }

        public TPlane Plane;
        public List<Segment<Point>> Segments;
    }

    /// <summary>
    /// A general purpose polyhedron for use with a vector geometry.
    /// </summary>
    public class VectorPolyhedron : IMutablePolyhedron<int, int, Triangle<int>, VectorPoint, int>
    {
        public VectorPolyhedron()
        {
            this._FreePolygon = 0;
            this._Faces = new Dictionary<int, PolyhedronFace<Triangle<int>, VectorPoint>>();
            this._Segments = new Dictionary<Segment<int>, FaceEdge<int, int>>();
        }

        public int Add(IEnumerable<Segment<VectorPoint>> Segments, Triangle<int> Plane)
        {
            int poly = this._FreePolygon++;
            List<Segment<VectorPoint>> segs = new List<Segment<VectorPoint>>();
            int e = 0;
            foreach (Segment<VectorPoint> seg in Segments)
            {
                this._Segments.Add(new Segment<int>(seg.A.Vertex, seg.B.Vertex),
                    new FaceEdge<int, int>(poly, e));
                segs.Add(seg);
                e++;
            }
            this._Faces.Add(poly, new PolyhedronFace<Triangle<int>, VectorPoint>(Plane, segs));
            return poly;
        }

        public void Remove(int Face)
        {
            var poly = this._Faces[Face];
            foreach (Segment<VectorPoint> seg in poly.Segments)
            {
                this._Segments.Remove(new Segment<int>(seg.A.Vertex, seg.B.Vertex));
            }
            this._Faces.Remove(Face);
        }

        public IEnumerable<int> Faces
        {
            get 
            {
                return this._Faces.Keys;
            }
        }

        /// <summary>
        /// Gets the data for the faces in the polyhedron.
        /// </summary>
        public IEnumerable<PolyhedronFace<Triangle<int>, VectorPoint>> FaceData
        {
            get
            {
                return this._Faces.Values;
            }
        }

        /// <summary>
        /// Gets all the segments in the polyhedron.
        /// </summary>
        public IEnumerable<UnorderedSegment<int>> Segments
        {
            get
            {
                HashSet<UnorderedSegment<int>> segs = new HashSet<UnorderedSegment<int>>();
                foreach (Segment<int> seg in this._Segments.Keys)
                {
                    segs.Add(Segment.Unorder(seg));
                }
                return segs;
            }
        }

        public PolyhedronFace<Triangle<int>, VectorPoint> Lookup(int Face)
        {
            return this._Faces[Face];
        }

        public FaceEdge<int, int>? SegmentFace(Segment<int> Segment)
        {
            FaceEdge<int, int> edge;
            if (this._Segments.TryGetValue(Segment, out edge))
            {
                return edge;
            }
            return null;
        }

        /// <summary>
        /// Creates a cubiod polyhedron in the specified vector geometry.
        /// </summary>
        public static VectorPolyhedron Cuboid(VectorGeometry Geometry, Vector Size, Vector Center)
        {
            var poly = new VectorPolyhedron();
            int[] points = new int[8];
            int i = 0;
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        points[i] = Geometry.AddVertex(Center + Vector.Scale(new Vector((double)x - 0.5, (double)y - 0.5, (double)z - 0.5), Size));
                        i++;
                    }
                }
            }

            int[] inds = 
            {
                0, 1, 3, 2,
                6, 7, 5, 4,
                0, 4, 5, 1,
                6, 2, 3, 7,
                0, 2, 6, 4,
                5, 7, 3, 1
            };

            for (int t = 0; t < inds.Length; t += 4)
            {
                int a = points[inds[t + 0]];
                int b = points[inds[t + 1]];
                int c = points[inds[t + 2]];
                int d = points[inds[t + 3]];
                VectorPoint[] vps = new VectorPoint[]
                {
                    new VectorPoint(new Point(0.0, 0.0), a),
                    new VectorPoint(new Point(1.0, 0.0), b),
                    new VectorPoint(new Point(1.0, 1.0), c),
                    new VectorPoint(new Point(0.0, 1.0), d)
                };
                poly.Add(Polygon.Segments(vps), new Triangle<int>(a, b, d));
            }

            return poly;
        }

        /// <summary>
        /// Triangulates the polyhedron's faces.
        /// </summary>
        public IEnumerable<Triangle<int>> Triangulate(VectorGeometry Geometry)
        {
            foreach (PolyhedronFace<Triangle<int>, VectorPoint> face in this._Faces.Values)
            {
                var poly = new PointPolygon<VectorPoint>(x => x.UV, face.Segments);
                foreach (Triangle<VectorPoint> tri in Polygon.Triangulate(poly))
                {
                    yield return new Triangle<int>(tri.A.Vertex, tri.B.Vertex, tri.C.Vertex);
                }
            }
        }

        private Dictionary<int, PolyhedronFace<Triangle<int>, VectorPoint>> _Faces;
        private Dictionary<Segment<int>, FaceEdge<int, int>> _Segments;
        private int _FreePolygon;
    }
}