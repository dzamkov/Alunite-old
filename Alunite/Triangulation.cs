using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Alunite
{
    /// <summary>
    /// Functions for finding triangular or tetrahedronal connections in a set of
    /// vertices.
    /// </summary>
    public static class Triangulation
    {
        /// <summary>
        /// Creates a triangulation for the specified points.
        /// </summary>
        public static void Triangulate<A, I>(A Input, out HashSet<Triple<I>> Surface, out HashSet<Quadruple<I>> Volume)
            where A : IArray<Vector, I>
            where I : IEquatable<I>
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Draws a set of points.
        /// </summary>
        public static void DebugDraw(IEnumerable<Vector> Points)
        {
            GL.Begin(BeginMode.Points);
            GL.Color3(1.0, 1.0, 1.0);

            foreach (Vector p in Points)
            {
                GL.Vertex3(p);
            }

            GL.End();
        }
    }
}