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

            // Random splits
            Random r = new Random(100);
            for (int t = 0; t < 36; t++)
            {
                
                foreach (Triangle<int> tri in this._Triangles)
                {
                    if (r.NextDouble() < 0.1)
                    {
                        this._SplitTriangle(tri);
                        break;
                    }
                }
            }
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
        /// Splits the triangle while maintaining the delaunay property.
        /// </summary>
        private void _SplitTriangle(Triangle<int> Triangle)
        {
            Triangle<Vector> vectri = this.Dereference(Triangle);
            Vector circumcenter = Alunite.Triangle.Normal(vectri);
            int npoint = this._Vertices.Count;
            this._Vertices.Add(circumcenter);

            this._Triangles.Remove(Triangle);

            foreach (Segment<int> seg in Triangle.Segments)
            {
                Triangle<int> othertri = this._SegmentTriangles[seg.Flip];
                int otherpoint = Alunite.Triangle.Align(othertri, seg.Flip).Value.Vertex;
                Triangle<Vector> othervectri = this.Dereference(othertri);
                Vector othercircumcenter = Alunite.Triangle.Normal(othervectri);
                double othercircumangle = Vector.Dot(othercircumcenter, othervectri.A);

                // Check if triangle encroachs the new point
                double npointangle = Vector.Dot(othercircumcenter, circumcenter);
                if (npointangle > othercircumangle)
                {
                    this._Triangles.Remove(othertri);
                    this._AddTriangle(new Triangle<int>(seg.A, otherpoint, npoint));
                    this._AddTriangle(new Triangle<int>(seg.B, npoint, otherpoint));
                }
                else
                {
                    this._AddTriangle(new Triangle<int>(npoint, seg));
                }
            }
        }

        private List<Vector> _Vertices;
        private HashSet<Triangle<int>> _Triangles;
        private Dictionary<Segment<int>, Triangle<int>> _SegmentTriangles;
    }
}