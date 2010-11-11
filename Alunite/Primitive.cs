using System;
using System.Collections.Generic;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Alunite
{
    /// <summary>
    /// Represents a basic triangular mesh.
    /// </summary>
    public struct Primitive
    {
        public Primitive(Vector[] Vertices, Triangle<int>[] Indices)
        {
            this.Vertices = Vertices;
            this.Indices = Indices;
        }

        /// <summary>
        /// Gets an icosahedron with a radius one centered at the origin.
        /// </summary>
        public static Primitive Icosahedron
        {
            get
            {
                Vector[] verts = new Vector[12];
                Triangle<int>[] tris = new Triangle<int>[20];

                // Set vertices
                double phi = (1.0 + Math.Sqrt(5)) / 2.0; // Golden ratio FTW!
                double ea = 1.0 / Math.Sqrt(phi * phi + 1);
                double eb = phi * ea;
                verts[0] = new Vector(-ea, -eb, 0);
                verts[1] = new Vector(0, -ea, -eb);
                verts[2] = new Vector(-eb, 0, -ea);
                verts[3] = new Vector(-ea, eb, 0);
                verts[4] = new Vector(0, -ea, eb);
                verts[5] = new Vector(eb, 0, -ea);
                verts[6] = new Vector(ea, -eb, 0);
                verts[7] = new Vector(0, ea, -eb);
                verts[8] = new Vector(-eb, 0, ea);
                verts[9] = new Vector(ea, eb, 0);
                verts[10] = new Vector(0, ea, eb);
                verts[11] = new Vector(eb, 0, ea);

                // Set edge pairs
                tris[0] = new Triangle<int>(0, 8, 2);
                tris[1] = new Triangle<int>(3, 2, 8);
                tris[2] = new Triangle<int>(1, 6, 0);
                tris[3] = new Triangle<int>(4, 0, 6);
                tris[4] = new Triangle<int>(2, 7, 1);
                tris[5] = new Triangle<int>(5, 1, 7);
                tris[6] = new Triangle<int>(6, 5, 11);
                tris[7] = new Triangle<int>(9, 11, 5);
                tris[8] = new Triangle<int>(7, 3, 9);
                tris[9] = new Triangle<int>(10, 9, 3);
                tris[10] = new Triangle<int>(8, 4, 10);
                tris[11] = new Triangle<int>(11, 10, 4);

                // Set inner triangles
                tris[12] = new Triangle<int>(0, 4, 8);
                tris[13] = new Triangle<int>(8, 10, 3);
                tris[14] = new Triangle<int>(1, 5, 6);
                tris[15] = new Triangle<int>(2, 1, 0);
                tris[16] = new Triangle<int>(10, 11, 9);
                tris[17] = new Triangle<int>(2, 3, 7);
                tris[18] = new Triangle<int>(5, 7, 9);
                tris[19] = new Triangle<int>(6, 11, 4);

                return new Primitive(verts, tris);
            }
        }

        /// <summary>
        /// Creates a diagram representing this primitive.
        /// </summary>
        public Diagram CreateDiagram()
        {
            Diagram dia = new Diagram();
            int[] verti = new int[this.Vertices.Length];
            for (int t = 0; t < verti.Length; t++)
            {
                verti[t] = dia.AddVertex(this.Vertices[t]);
            }
            foreach (Triangle<int> tri in this.Indices)
            {
                dia.SetBorderedTriangle(
                    new Triangle<int>(verti[tri.A], verti[tri.B], verti[tri.C]), 
                    Color.RGB(1.0, 0.8, 0.3), 
                    Color.RGB(1.0, 0.0, 0.0), 3.0);
            }
            return dia;
        }

        public Vector[] Vertices;
        public Triangle<int>[] Indices;
    }
}