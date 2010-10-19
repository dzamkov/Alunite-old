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
            // Initialize some things
            this._UnfilledMesh = new TetrahedralMesh<int>();
            this._FilledMesh = new TetrahedralMesh<int>();

            // Create a rectangular slab with an area of air above it.
            this._Vertices = new List<Vector>(Grid.Points(new Vector(-4, -4, -4), new Vector(1.0, 1.0, 1.0), new LVector(9, 9, 9)).Items);

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    for (int z = 0; z < 8; z++)
                    {
                        bool filled = z < 1;
                        foreach (Tetrahedron<int> tetra in Grid.Tesselate(new LVector(9, 9, 9), new LVector(x, y, z), new LVector(1, 1, 1)).Items)
                        {
                            this._AddTetrahedron(tetra, filled);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the vertices used by the world.
        /// </summary>
        public IArray<Vector> Vertices
        {
            get
            {
                return new ListArray<Vector>(this._Vertices);
            }
        }

        /// <summary>
        /// Gets the triangles in the boundary of the filled area.
        /// </summary>
        public ISet<Triangle<int>> Boundary
        {
            get
            {
                return this._FilledMesh.Boundary;
            }
        }

        /// <summary>
        /// Dereferences the vertices in a tetrahedron to vectors.
        /// </summary>
        public Tetrahedron<Vector> Dereference(Tetrahedron<int> Tetrahedron)
        {
            return new Tetrahedron<Vector>(
                this._Vertices[Tetrahedron.A],
                this._Vertices[Tetrahedron.B],
                this._Vertices[Tetrahedron.C],
                this._Vertices[Tetrahedron.D]);
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

        /// <summary>
        /// Calculates the circumsphere for the specified tetrahedron.
        /// </summary>
        public static Sphere Circumsphere(Tetrahedron<Vector> Tetrahedron)
        {
            Vector circumcenter = Alunite.Tetrahedron.Circumcenter(Tetrahedron);
            double radius = (circumcenter - Tetrahedron.A).Length;
            return new Sphere(radius, circumcenter);
        }

        /// <summary>
        /// Calculates the circumsphere for the specified triangle.
        /// </summary>
        public static Sphere Circumsphere(Triangle<Vector> Triangle)
        {
            Vector circumcenter = Alunite.Triangle.Circumcenter(Triangle);
            double radius = (circumcenter - Triangle.A).Length;
            return new Sphere(radius, circumcenter);
        }

        /// <summary>
        /// Calculates the circumsphere for the specified segment.
        /// </summary>
        public static Sphere Circumsphere(Segment<Vector> Segment)
        {
            Vector circumcenter = Alunite.Segment.Midpoint(Segment);
            double radius = (circumcenter - Segment.A).Length;
            return new Sphere(radius, circumcenter);
        }

        /// <summary>
        /// Calculates the circumsphere for the specified tetrahedron in the world.
        /// </summary>
        public Sphere Circumsphere(Tetrahedron<int> Tetrahedron)
        {
            return Circumsphere(this.Dereference(Tetrahedron));
        }

        /// <summary>
        /// Calculates the circumsphere for the specified triangle in the world.
        /// </summary>
        public Sphere Circumsphere(Triangle<int> Triangle)
        {
            return Circumsphere(this.Dereference(Triangle));
        }

        /// <summary>
        /// Calculates the circumsphere for the specified segment in the world.
        /// </summary>
        public Sphere Circumsphere(Segment<int> Segment)
        {
            return Circumsphere(this.Dereference(Segment));
        }

        /// <summary>
        /// Creates a vertex buffer to represent this world.
        /// </summary>
        public VBO<NormalVertex, NormalVertex.Model> CreateVBO()
        {
            ListArray<Vector> verts = new ListArray<Vector>(this._Vertices);
            StandardArray<Vector> norms = Model.ComputeNormals(verts, this.Boundary.Items, true);
            return new VBO<NormalVertex, NormalVertex.Model>(NormalVertex.Model.Singleton,
                 Data.Map<Tuple<Vector, Vector>, NormalVertex>(
                     Data.Zip(verts, norms),
                     delegate(Tuple<Vector, Vector> PosNorm)
                     {
                         return new NormalVertex(PosNorm.A, PosNorm.B);
                     }), this.Boundary);
        }

        /// <summary>
        /// Determines where the specified segment intersects world geometry.
        /// </summary>
        public bool Trace(Segment<Vector> Segment, out double HitLength, out Vector HitPos, out Vector HitNormal)
        {
            // Very slow
            foreach (Triangle<int> tri in this.Boundary.Items)
            {
                Triangle<Vector> acttri = this.Dereference(tri);
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
            this._FillSphere(Location, true, 1.0);
        }

        /// <summary>
        /// Digs at the specified area.
        /// </summary>
        public void Dig(Vector Location)
        {
            this._FillSphere(Location, false, 1.0);
        }

        /// <summary>
        /// Fills the sphere at the specified location.
        /// </summary>
        private void _FillSphere(Vector Location, bool Filled, double Radius)
        {
            LinkedList<Tetrahedron<int>> tochange = new LinkedList<Tetrahedron<int>>();

            // Figure out which tetrahedron need to be changed
            foreach (Tetrahedron<int> tetra in Set.Join(this._FilledMesh.Tetrahedra.Items, this._UnfilledMesh.Tetrahedra.Items))
            {
                Tetrahedron<Vector> atetra = this.Dereference(tetra);
                if ((Tetrahedron.Midpoint(atetra) - Location).Length < Radius)
                {
                    tochange.AddLast(tetra);
                }
            }

            // Do changes
            foreach (Tetrahedron<int> tetra in tochange)
            {
                this._ChangeTetrahedron(tetra, Filled);
            }
        }

        /// <summary>
        /// Adds a point to either the filled or unfilled portions of the mesh.
        /// </summary>
        private void _AddPoint(bool Filled, Vector Point)
        {
            int ind = this._Vertices.Count;
            this._Vertices.Add(Point);
            
            // Keep track of new bounds and tetrahedra removed as a result
            List<Tetrahedron<int>> removed = new List<Tetrahedron<int>>();
            HashSet<Triangle<int>> delbounds = new HashSet<Triangle<int>>();
            TetrahedralMesh<int> mesh = Filled ? this._FilledMesh : this._UnfilledMesh;
            foreach (Tetrahedron<int> tetra in mesh.Tetrahedra.Items)
            {
                if (this.Circumsphere(tetra).In(Point))
                {
                    removed.Add(tetra);
                    foreach (Triangle<int> face in tetra.Faces)
                    {
                        if (!delbounds.Remove(face))
                        {
                            delbounds.Add(face.Flip);
                        }
                    }
                }
            }

            // Remove old tetrahedra and add new ones
            foreach (Tetrahedron<int> remove in removed)
            {
                mesh.Remove(remove);
            }
            foreach (Triangle<int> bound in delbounds)
            {
                mesh.AddUnchecked(new Tetrahedron<int>(ind, bound.Flip));
            }
        }

        /// <summary>
        /// Adds a tetrahedron to the world and updates the boundaries.
        /// </summary>
        private void _AddTetrahedron(Tetrahedron<int> Tetrahedron, bool Filled)
        {
            if (Filled)
            {
                this._FilledMesh.Add(Tetrahedron);
            }
            else
            {
                this._UnfilledMesh.Add(Tetrahedron);
            }
        }

        /// <summary>
        /// Changes a preexisting tetrahedron's state.
        /// </summary>
        private void _ChangeTetrahedron(Tetrahedron<int> Tetrahedron, bool Filled)
        {
            if (Filled)
            {
                if (this._UnfilledMesh.Remove(Tetrahedron))
                {
                    this._FilledMesh.Add(Tetrahedron);
                }
            }
            else
            {
                if (this._FilledMesh.Remove(Tetrahedron))
                {
                    this._UnfilledMesh.Add(Tetrahedron);
                }
            }
        }

        private List<Vector> _Vertices;
        private TetrahedralMesh<int> _FilledMesh;
        private TetrahedralMesh<int> _UnfilledMesh;
    }
}