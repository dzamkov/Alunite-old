using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Alunite
{
    /// <summary>
    /// A triangulation over a unit sphere.
    /// </summary>
    public struct SphericalTriangulation
    {

        /// <summary>
        /// Adds a triangle to the triangulation.
        /// </summary>
        public void AddTriangle(Triangle<int> Triangle)
        {
            this.Triangles.Add(Triangle);
            foreach (Segment<int> seg in Triangle.Segments)
            {
                this.SegmentTriangles[seg] = Triangle;
            }
        }

        /// <summary>
        /// Dereferences a triangle in the primitive.
        /// </summary>
        public Triangle<Vector> Dereference(Triangle<int> Triangle)
        {
            return new Triangle<Vector>(
                this.Vertices[Triangle.A],
                this.Vertices[Triangle.B],
                this.Vertices[Triangle.C]);
        }

        /// <summary>
        /// Subdivides the entire sphere, quadrupling the amount of triangles while maintaining the delaunay property.
        /// </summary>
        public void Subdivide()
        {
            var oldtris = this.Triangles;
            var oldsegs = this.SegmentTriangles;
            this.Triangles = new HashSet<Triangle<int>>();
            this.SegmentTriangles = new Dictionary<Segment<int>, Triangle<int>>();

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
                        midpoint = this.Vertices.Count;
                        this.Vertices.Add(
                            Vector.Normalize(
                                Segment.Midpoint(
                                    new Segment<Vector>(
                                        this.Vertices[seg.A],
                                        this.Vertices[seg.B]))));
                        newsegs.Add(seg.Flip, midpoint);
                    }
                    else
                    {
                        newsegs.Remove(seg);
                    }
                    midpoints[t] = midpoint;
                }
                this.AddTriangle(new Triangle<int>(tri.A, midpoints[0], midpoints[2]));
                this.AddTriangle(new Triangle<int>(tri.B, midpoints[1], midpoints[0]));
                this.AddTriangle(new Triangle<int>(tri.C, midpoints[2], midpoints[1]));
                this.AddTriangle(new Triangle<int>(midpoints[0], midpoints[1], midpoints[2]));
            }
        }

        /// <summary>
        /// Inserts a point in the triangulation and splits triangles to maintain the delaunay property.
        /// </summary>
        public void SplitTriangle(Triangle<int> Triangle, Vector NewPosition, int NewPoint)
        {
            this.Triangles.Remove(Triangle);

            // Maintain delaunay property by flipping encroached triangles.
            List<Segment<int>> finalsegs = new List<Segment<int>>();
            Stack<Segment<int>> possiblealtersegs = new Stack<Segment<int>>();
            possiblealtersegs.Push(new Segment<int>(Triangle.A, Triangle.B));
            possiblealtersegs.Push(new Segment<int>(Triangle.B, Triangle.C));
            possiblealtersegs.Push(new Segment<int>(Triangle.C, Triangle.A));

            while (possiblealtersegs.Count > 0)
            {
                Segment<int> seg = possiblealtersegs.Pop();

                Triangle<int> othertri = Alunite.Triangle.Align(this.SegmentTriangles[seg.Flip], seg.Flip).Value;
                int otherpoint = othertri.Vertex;
                Triangle<Vector> othervectri = this.Dereference(othertri);
                Vector othercircumcenter = Alunite.Triangle.Normal(othervectri);
                double othercircumangle = Vector.Dot(othercircumcenter, othervectri.A);

                // Check if triangle encroachs the new point
                double npointangle = Vector.Dot(othercircumcenter, NewPosition);
                if (npointangle > othercircumangle)
                {
                    this.Triangles.Remove(othertri);
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
                this.AddTriangle(new Triangle<int>(NewPoint, seg));
            }
        }

        /// <summary>
        /// Splits a triangle at its center, maintaining the delaunay property.
        /// </summary>
        public void SplitTriangle(Triangle<int> Triangle)
        {
            Triangle<Vector> vectri = this.Dereference(Triangle);
            Vector npos = Alunite.Triangle.Normal(vectri);
            this.SplitTriangle(Triangle, npos, this.AddVertex(npos));
        }

        /// <summary>
        /// Adds a vertex (with length 1) to the spherical triangulation.
        /// </summary>
        public int AddVertex(Vector Position)
        {
            int ind = this.Vertices.Count;
            this.Vertices.Add(Position);
            return ind;
        }

        /// <summary>
        /// Creates a spherical triangulation based off an icosahedron.
        /// </summary>
        public static SphericalTriangulation CreateIcosahedron()
        {
            SphericalTriangulation st = new SphericalTriangulation();
            Primitive icosa = Primitive.Icosahedron;

            st.Vertices = new List<Vector>(icosa.Vertices.Length);
            st.Vertices.AddRange(icosa.Vertices);
            st.SegmentTriangles = new Dictionary<Segment<int>, Triangle<int>>();
            st.Triangles = new HashSet<Triangle<int>>();

            foreach (Triangle<int> tri in icosa.Triangles)
            {
                st.AddTriangle(tri);
            }

            return st;
        }

        /// <summary>
        /// Triangles that are part of the sphere.
        /// </summary>
        public HashSet<Triangle<int>> Triangles;

        /// <summary>
        /// A mapping of segments to the triangles that produce them.
        /// </summary>
        public Dictionary<Segment<int>, Triangle<int>> SegmentTriangles;

        /// <summary>
        /// An ordered list of vertices that make up the sphere.
        /// </summary>
        public List<Vector> Vertices;
    }
}