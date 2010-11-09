using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Represents a sphere with position and size information.
    /// </summary>
    public struct Sphere
    {
        public Sphere(double Radius, Vector Center)
        {
            this.Radius = Radius;
            this.Center = Center;
        }

        /// <summary>
        /// Gets if the specified point is in the sphere.
        /// </summary>
        public bool In(Vector Point)
        {
            return (Point - this.Center).Length < this.Radius;
        }

        public double Radius;
        public Vector Center;
    }

    /// <summary>
    /// A simple structure for storing and manipulating
    /// tetrahedra and their "filled" or "empty" state.
    /// </summary>
    public class World
    {
        public World()
        {
            this._Geometry = new VectorGeometry();
            this._Polyhedron = VectorPolyhedron.Cuboid(this._Geometry, new Vector(100.0, 100.0, 100.0), new Vector(0.0, 0.0, -50.0));
            this._Triangulate();
        }

        /// <summary>
        /// Creates a vertex buffer to represent this world.
        /// </summary>
        public VBO<NormalVertex, NormalVertex.Model> CreateVBO()
        {
            List<NormalVertex> vertices = new List<NormalVertex>();
            foreach (Triangle<int> tri in this._Triangles)
            {
                Triangle<Vector> vectri = this._Geometry.Dereference(tri);
                Vector norm = Triangle.Normal(vectri);
                vertices.Add(new NormalVertex(vectri.A, norm));
                vertices.Add(new NormalVertex(vectri.B, norm));
                vertices.Add(new NormalVertex(vectri.C, norm));
            }
            return new VBO<NormalVertex, NormalVertex.Model>(NormalVertex.Model.Singleton, new ListArray<NormalVertex>(vertices));
        }

        /// <summary>
        /// Triangulates the polyhedron that represents the boundary of the world.
        /// </summary>
        private void _Triangulate()
        {
            this._Triangles = new List<Triangle<int>>(this._Polyhedron.Triangulate(this._Geometry));
        }

        /// <summary>
        /// Determines where the specified segment intersects world geometry.
        /// </summary>
        public bool Trace(Segment<Vector> Segment, out double HitLength, out Vector HitPos, out Vector HitNormal)
        {
            // Very slow
            foreach (Triangle<int> tri in this._Triangles)
            {
                Triangle<Vector> acttri = this._Geometry.Dereference(tri);
                if (Triangle.Intersect(acttri, Segment, out HitLength, out HitPos))
                {
                    HitNormal = Triangle.Normal(acttri);
                    return true;
                }
            }
            HitLength = 0;
            HitPos = new Vector();
            HitNormal = new Vector();
            return false;
        }

        /// <summary>
        /// Builds at the specified area.
        /// </summary>
        public void Build(Vector Location)
        {
            this.Change(Location, true);
        }

        /// <summary>
        /// Digs at the specified area.
        /// </summary>
        public void Dig(Vector Location)
        {
            this.Change(Location, false);
        }

        public void Change(Vector Location, bool Add)
        {
            Random r = new Random();
            Vector nsize = new Vector(1.0, 1.0, 1.0);
            VectorPolyhedron npoly = VectorPolyhedron.Cuboid(this._Geometry, nsize, Location);
            this._Polyhedron = CSG.Union(this._Geometry, this._Polyhedron, npoly);
            this._Triangulate();
        }

        private VectorGeometry _Geometry;
        private VectorPolyhedron _Polyhedron;
        private List<Triangle<int>> _Triangles;
    }
}