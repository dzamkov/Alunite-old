using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Really big thing.
    /// </summary>
    public class Planet
    {
        public Planet()
        {
            this._Triangles = new HashSet<Triangle<int>>();
            this._SegmentTriangles = new Dictionary<Segment<int>, Triangle<int>>();

            // Initialize with an icosahedron.
            Primitive icosa = Primitive.Icosahedron;
            this._Vertices = new List<Vector>(icosa.Vertices);
            foreach (Triangle<int> tri in icosa.Triangles)
            {
                this._AddTriangle(tri);   
            }

            this._Subdivide();
            this._Subdivide();
            this._Subdivide();
        }

        /// <summary>
        /// Creates a diagram for the current state of the planet.
        /// </summary>
        public Diagram CreateDiagram()
        {
            Diagram dia = new Diagram(this._Vertices);
            foreach (Triangle<int> tri in this._Triangles)
            {
                dia.SetBorderedTriangle(tri, Color.RGB(0.0, 0.2, 1.0), Color.RGB(0.3, 1.0, 0.3), 4.0);
            }
            return dia;
        }

        /// <summary>
        /// Creates a vertex buffer representation of the current state of the planet.
        /// </summary>
        public VBO<NormalVertex, NormalVertex.Model> CreateVBO()
        {
            NormalVertex[] verts = new NormalVertex[this._Vertices.Count];
            for (int t = 0; t < verts.Length; t++)
            {
                // Lol, position and normal are the same.
                Vector pos = this._Vertices[t];
                verts[t].Position = pos;
                verts[t].Normal = pos;
            }
            return new VBO<NormalVertex, NormalVertex.Model>(
                NormalVertex.Model.Singleton,
                verts, verts.Length,
                this._Triangles, this._Triangles.Count);
        }

        /// <summary>
        /// Adds a triangle to the planet.
        /// </summary>
        private void _AddTriangle(Triangle<int> Triangle)
        {
            this._Triangles.Add(Triangle);
            foreach (Segment<int> seg in Triangle.Segments)
            {
                this._SegmentTriangles[seg] = Triangle;
            }
        }

        /// <summary>
        /// Dereferences a triangle in the primitive.
        /// </summary>
        public Triangle<Vector> Dereference(Triangle<int> Triangle)
        {
            return new Triangle<Vector>(
                this._Vertices[Triangle.A],
                this._Vertices[Triangle.B],
                this._Vertices[Triangle.C]);
        }

        /// <summary>
        /// Splits the triangle at the specified point while maintaining the delaunay property.
        /// </summary>
        private void _SplitTriangle(Triangle<int> Triangle, Vector Point)
        {
            int npoint = this._Vertices.Count;
            this._Vertices.Add(Point);

            this._Triangles.Remove(Triangle);

            // Maintain delaunay property by flipping encroached triangles.
            List<Segment<int>> finalsegs = new List<Segment<int>>();
            Stack<Segment<int>> possiblealtersegs = new Stack<Segment<int>>();
            possiblealtersegs.Push(new Segment<int>(Triangle.A, Triangle.B));
            possiblealtersegs.Push(new Segment<int>(Triangle.B, Triangle.C));
            possiblealtersegs.Push(new Segment<int>(Triangle.C, Triangle.A));

            while (possiblealtersegs.Count > 0)
            {
                Segment<int> seg = possiblealtersegs.Pop();

                Triangle<int> othertri = Alunite.Triangle.Align(this._SegmentTriangles[seg.Flip], seg.Flip).Value;
                int otherpoint = othertri.Vertex;
                Triangle<Vector> othervectri = this.Dereference(othertri);
                Vector othercircumcenter = Alunite.Triangle.Normal(othervectri);
                double othercircumangle = Vector.Dot(othercircumcenter, othervectri.A);

                // Check if triangle encroachs the new point
                double npointangle = Vector.Dot(othercircumcenter, Point);
                if (npointangle > othercircumangle)
                {
                    this._Triangles.Remove(othertri);
                    possiblealtersegs.Push(new Segment<int>(othertri.A, othertri.B));
                    possiblealtersegs.Push(new Segment<int>(othertri.C, othertri.A));
                }
                else
                {
                    finalsegs.Add(seg);
                }
            }

            foreach (Segment<int> seg in finalsegs)
            {
                this._AddTriangle(new Triangle<int>(npoint, seg));
            }
        }

        /// <summary>
        /// Splits a triangle at its circumcenter while maintaining the delaunay property.
        /// </summary>
        private void _SplitTriangle(Triangle<int> Triangle)
        {
            Triangle<Vector> vectri = this.Dereference(Triangle);
            Vector circumcenter = Alunite.Triangle.Normal(vectri);
            this._SplitTriangle(Triangle, circumcenter);
        }

        /// <summary>
        /// Subdivides the entire planet, quadrupling the amount of triangles.
        /// </summary>
        private void _Subdivide()
        {
            var oldtris = this._Triangles;
            var oldsegs = this._SegmentTriangles;
            this._Triangles = new HashSet<Triangle<int>>();
            this._SegmentTriangles = new Dictionary<Segment<int>, Triangle<int>>();

            Dictionary<Segment<int>, int> newsegs = new Dictionary<Segment<int>, int>();

            foreach (Triangle<int> tri in oldtris)
            {
                int[] midpoints = new int[3];
                Segment<int>[] segs = tri.Segments;
                for (int t = 0; t < 3; t++)
                {
                    Segment<int> seg = segs[t];
                    int midpoint;
                    if (!newsegs.TryGetValue(seg, out midpoint))
                    {
                        midpoint = this._Vertices.Count;
                        this._Vertices.Add(
                            Vector.Normalize(
                                Segment.Midpoint(
                                    new Segment<Vector>(
                                        this._Vertices[seg.A],
                                        this._Vertices[seg.B]))));
                        newsegs.Add(seg.Flip, midpoint);
                    }
                    else
                    {
                        newsegs.Remove(seg);
                    }
                    midpoints[t] = midpoint;
                }
                this._AddTriangle(new Triangle<int>(tri.A, midpoints[0], midpoints[2]));
                this._AddTriangle(new Triangle<int>(tri.B, midpoints[1], midpoints[0]));
                this._AddTriangle(new Triangle<int>(tri.C, midpoints[2], midpoints[1]));
                this._AddTriangle(new Triangle<int>(midpoints[0], midpoints[1], midpoints[2]));
            }
        }

        private List<Vector> _Vertices;
        private HashSet<Triangle<int>> _Triangles;
        private Dictionary<Segment<int>, Triangle<int>> _SegmentTriangles;
    }
}