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

        private List<Vector> _Vertices;
    }

    /// <summary>
    /// A face for a polyhedron.
    /// </summary>
    public struct PolyhedronFace<TPlane, Point>
        where Point : IEquatable<Point>
    {
        public PolyhedronFace(TPlane Plane, IEnumerable<Segment<Point>> Segments)
        {
            this.Plane = Plane;
            this.Segments = Segments;
        }

        public TPlane Plane;
        public IEnumerable<Segment<Point>> Segments;
    }

    /// <summary>
    /// A general purpose polyhedron.
    /// </summary>
    public class Polyhedron<Plane, Point, Vertex> : IMutablePolyhedron<int, int, Plane, Point, Vertex>
        where Point : IEquatable<Point>
        where Vertex : IEquatable<Vertex>
    {
        public Polyhedron(IPolyhedronGeomerty<Plane, Point, Vertex> Geometry)
        {
            this._FreePolygon = 0;
            this._Geometry = Geometry;
            this._Faces = new Dictionary<int, PolyhedronFace<Plane, Point>>();
            this._Segments = new Dictionary<Segment<Vertex>, FaceEdge<int, int>>();
        }

        public int Add(IEnumerable<Segment<Point>> Segments, Plane Plane)
        {
            int poly = this._FreePolygon++;
            List<Segment<Point>> segs = new List<Segment<Point>>();
            int e = 0;
            foreach (Segment<Point> seg in Segments)
            {
                this._Segments.Add(
                    new Segment<Vertex>(
                        this._Geometry.Position(seg.A, Plane),
                        this._Geometry.Position(seg.B, Plane)),
                    new FaceEdge<int, int>(poly, e));
                segs.Add(seg);
                e++;
            }
            this._Faces.Add(poly, new PolyhedronFace<Plane, Point>(Plane, segs));
            return poly;
        }

        public void Remove(int Face)
        {
            var poly = this._Faces[Face];
            foreach (Segment<Point> seg in poly.Segments)
            {
                this._Segments.Remove(
                    new Segment<Vertex>(
                        this._Geometry.Position(seg.A, poly.Plane),
                        this._Geometry.Position(seg.B, poly.Plane)));
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

        public PolyhedronFace<Plane, Point> Lookup(int Face)
        {
            return this._Faces[Face];
        }

        public FaceEdge<int, int>? SegmentFace(Segment<Vertex> Segment)
        {
            FaceEdge<int, int> edge;
            if (this._Segments.TryGetValue(Segment, out edge))
            {
                return edge;
            }
            return null;
        }

        private Dictionary<int, PolyhedronFace<Plane, Point>> _Faces;
        private Dictionary<Segment<Vertex>, FaceEdge<int, int>> _Segments;
        private IPolyhedronGeomerty<Plane, Point, Vertex> _Geometry;
        private int _FreePolygon;
    }

    /// <summary>
    /// Polyhedron related functions.
    /// </summary>
    public static class Polyhedron
    {
        /// <summary>
        /// Creates a cubiod polyhedron in the specified vector geometry.
        /// </summary>
        public static Polyhedron<Triangle<int>, VectorPoint, int> Cuboid(VectorGeometry Geometry, Vector Size, Vector Center)
        {
            var poly = new Polyhedron<Triangle<int>, VectorPoint, int>(Geometry);
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
        /// Gets the triangles that make up a polyhedron.
        /// </summary>
        public static IEnumerable<Triangle<int>> Triangluate(Polyhedron<Triangle<int>, VectorPoint, int> Polyhedron)
        {
            foreach (int poly in Polyhedron.Faces)
            {
                var polyinfo = Polyhedron.Lookup(poly);
                PointPolygon<VectorPoint> pp = new PointPolygon<VectorPoint>(x => x.UV, polyinfo.Segments);
                foreach (Triangle<VectorPoint> tri in Polygon.Triangulate(pp))
                {
                    yield return new Triangle<int>(tri.A.Vertex, tri.B.Vertex, tri.C.Vertex);
                }
            }
        }
    }
}