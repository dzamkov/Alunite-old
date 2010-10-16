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
            this._Tetras = new Dictionary<Tetrahedron<int>, bool>();
            this._ContentBoundary = new HashSet<Triangle<int>>();
            this._WorldBoundary = new HashSet<Triangle<int>>();
            this._Faces = new Dictionary<Triangle<int>, Tetrahedron<int>>();

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
        /// Gets the triangles in the boundary of the filled area to the unfilled area.
        /// </summary>
        public ISet<Triangle<int>> Boundary
        {
            get
            {
                return Set.Create(this._ContentBoundary);
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
            foreach (Triangle<int> tri in this._ContentBoundary)
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
        /// Adds a tetrahedron to the world and updates the boundaries.
        /// </summary>
        private void _AddTetrahedron(Tetrahedron<int> Tetrahedron, bool Filled)
        {
            this._Tetras.Add(Tetrahedron, Filled);

            // Update faces and boundaries
            foreach (Triangle<int> face in Tetrahedron.Faces)
            {
                this._Faces.Add(face, Tetrahedron);
                Triangle<int> flipped = face.Flip;
                if (this._WorldBoundary.Contains(flipped))
                {
                    this._WorldBoundary.Remove(flipped);
                    Tetrahedron<int> border = this._Faces[flipped];
                    bool borderfilled = this._Tetras[border];
                    if (Filled && !borderfilled)
                    {
                        this._ContentBoundary.Add(face);
                    }
                    if (!Filled && borderfilled)
                    {
                        this._ContentBoundary.Add(flipped);
                    }
                }
                else
                {
                    this._WorldBoundary.Add(face);
                }
            }
        }

        private List<Vector> _Vertices;
        private Dictionary<Tetrahedron<int>, bool> _Tetras;
        private HashSet<Triangle<int>> _ContentBoundary;
        private HashSet<Triangle<int>> _WorldBoundary;
        private Dictionary<Triangle<int>, Tetrahedron<int>> _Faces;
    }
}