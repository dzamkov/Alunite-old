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
    /// <typeparam name="Polygon">A polygon in the polyhedron.</typeparam>
    /// <typeparam name="Edge">An edge on a polygon.</typeparam>
    public interface IPolyhedron<Polygon, Edge, Plane, Point, Vertex>
        where Point : IEquatable<Point>
        where Vertex : IEquatable<Vertex>
    {
        /// <summary>
        /// Gets the polygons that make up the polyhedron.
        /// </summary>
        IEnumerable<Polygon> Polygons { get; }

        /// <summary>
        /// Gets the information about the specified polygon.
        /// </summary>
        PolyhedronPolygon<Plane, Point> Lookup(Polygon Polygon);

        /// <summary>
        /// Gets the polygon that includes the specified segment, if any exists.
        /// </summary>
        PolygonEdge<Polygon, Edge>? SegmentPolygon(Segment<Vertex> Segment);
    }

    /// <summary>
    /// A polyhedron that can accept new polygons.
    /// </summary>
    public interface IMutablePolyhedron<Polygon, Edge, Plane, Point, Vertex> : IPolyhedron<Polygon, Edge, Plane, Point, Vertex>
        where Point : IEquatable<Point>
        where Vertex : IEquatable<Vertex>
    {
        /// <summary>
        /// Adds a new polygon to the polyhedron, and returns a reference to it.
        /// </summary>
        Polygon Add(IEnumerable<Segment<Point>> Segments, Plane Plane);

        /// <summary>
        /// Removes a polygon from the polyhedron.
        /// </summary>
        void Remove(Polygon Polygon);
    }

    /// <summary>
    /// Represents an edge on a polygon.
    /// </summary>
    public struct PolygonEdge<TPolygon, TEdge>
    {
        public PolygonEdge(TPolygon Polygon, TEdge Edge)
        {
            this.Polygon = Polygon;
            this.Edge = Edge;
        }

        public TPolygon Polygon;
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
    /// A polygon for a polyhedron.
    /// </summary>
    public struct PolyhedronPolygon<TPlane, Point>
        where Point : IEquatable<Point>
    {
        public PolyhedronPolygon(TPlane Plane, IEnumerable<Segment<Point>> Segments)
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
            this._Polygons = new Dictionary<int, PolyhedronPolygon<Plane, Point>>();
            this._Segments = new Dictionary<Segment<Vertex>, PolygonEdge<int, int>>();
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
                    new PolygonEdge<int, int>(poly, e));
                segs.Add(seg);
                e++;
            }
            this._Polygons.Add(poly, new PolyhedronPolygon<Plane, Point>(Plane, segs));
            return poly;
        }

        public void Remove(int Polygon)
        {
            var poly = this._Polygons[Polygon];
            foreach (Segment<Point> seg in poly.Segments)
            {
                this._Segments.Remove(
                    new Segment<Vertex>(
                        this._Geometry.Position(seg.A, poly.Plane),
                        this._Geometry.Position(seg.B, poly.Plane)));
            }
            this._Polygons.Remove(Polygon);
        }

        public IEnumerable<int> Polygons
        {
            get 
            {
                return this._Polygons.Keys;
            }
        }

        public PolyhedronPolygon<Plane, Point> Lookup(int Polygon)
        {
            return this._Polygons[Polygon];
        }

        public PolygonEdge<int, int>? SegmentPolygon(Segment<Vertex> Segment)
        {
            PolygonEdge<int, int> edge;
            if (this._Segments.TryGetValue(Segment, out edge))
            {
                return edge;
            }
            return null;
        }

        private Dictionary<int, PolyhedronPolygon<Plane, Point>> _Polygons;
        private Dictionary<Segment<Vertex>, PolygonEdge<int, int>> _Segments;
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

            VectorPoint[] vps = new VectorPoint[]
            {
                new VectorPoint(new Point(0.0, 0.0), 1),
                new VectorPoint(new Point(1.0, 0.0), 5),
                new VectorPoint(new Point(1.0, 1.0), 7),
                new VectorPoint(new Point(0.0, 1.0), 3)
            };

            poly.Add(Polygon.Segments(vps), new Triangle<int>(1, 5, 3));

            return poly;
        }

        /// <summary>
        /// Gets the triangles that make up a polyhedron.
        /// </summary>
        public static IEnumerable<Triangle<int>> Triangluate(Polyhedron<Triangle<int>, VectorPoint, int> Polyhedron)
        {
            foreach (int poly in Polyhedron.Polygons)
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