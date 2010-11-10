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


        public Vector[] Vertices;
        public Triangle<int>[] Indices;
    }
}