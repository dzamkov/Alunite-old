﻿using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Alunite
{
    /// <summary>
    /// Main program window
    /// </summary>
    public class Window : GameWindow
    {
        public Window()
            : base(640, 480, GraphicsMode.Default, "Alunite")
        {
            GL.Enable(EnableCap.ColorMaterial);
            GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.Diffuse);
            GL.Enable(EnableCap.CullFace);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            List<Vector> testvecs = new List<Vector>();
            Random r = new Random(DateTime.Now.GetHashCode());
            for (int t = 0; t < 300; t++)
            {
                testvecs.Add(new Vector(r.NextDouble() - 0.5, r.NextDouble() - 0.5, r.NextDouble() - 0.5));
            }

            // Gets the edges between the midpoints of the tetrahedrons in the triangulation of this set of points.
            this._Data = new StandardArray<Vector>(testvecs.ToArray());
            HashSet<Tetrahedron<int>> tetras;
            HashSet<Triangle<int>> tris;
            Triangulation.Triangulate(this._Data, out tris, out tetras);
            StandardArray<Tetrahedron<int>> atetras = new StandardArray<Tetrahedron<int>>(tetras, tetras.Count);
            StandardArray<Vector> tetramidpoints = atetras.Map<Vector>(delegate(Tetrahedron<int> tetra)
            {
                return Tetrahedron.Midpoint(new Tetrahedron<Vector>(
                    this._Data.Item(tetra.A),
                    this._Data.Item(tetra.B),
                    this._Data.Item(tetra.C),
                    this._Data.Item(tetra.D)));
            });
            HashSet<Edge<int>> edges;
            Triangulation.Edges<StandardArray<Tetrahedron<int>>, int, int>(atetras, out edges);
            this._Edges = Triangulation.Dereference<StandardArray<Vector>, int, Vector>(tetramidpoints, edges);
        }

        /// <summary>
        /// Program main entry point.
        /// </summary>
        public unsafe static void Main(string[] Args)
        {
            Window w = new Window();
            w.Run();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Projection);
            Matrix4d proj = Matrix4d.CreatePerspectiveFieldOfView(0.7, (double)this.Width / (double)this.Height, 0.01, 50.0);
            GL.LoadMatrix(ref proj);
            Matrix4d view = Matrix4d.LookAt(
                new Vector3d(1.1, 1.1, 1.1),
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(0.0, 0.0, 1.0));
            GL.MultMatrix(ref view);

            GL.Rotate(this._Rot * 16, new Vector(0.0, 0.0, 1.0));

            Triangulation.DebugDraw(this._Edges);
            Triangulation.DebugDraw(this._Data.Values);

            this.SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            this._Rot += e.Time;
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
        }

        private double _Rot;
        private StandardArray<Vector> _Data;
        private IEnumerable<Edge<Vector>> _Edges;
    }
}