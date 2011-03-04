using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTKGUI;

namespace Alunite
{
    /// <summary>
    /// A visualizer for matter.
    /// </summary>
    public class Visualizer : Render3DControl
    {
        public Visualizer(IEnumerable<Vector> PointSetA)
        {
            this._PointSetA = PointSetA;
        }

        public override void SetupProjection(Point Viewsize)
        {
            Vector3d up = new Vector3d(0.0, 0.0, 1.0);
            Matrix4d proj = Matrix4d.CreatePerspectiveFieldOfView(Math.Sin(Math.PI / 8.0), this.Size.AspectRatio, 0.1, 100.0);
            Vector lookpos = new Vector(0.5, 0.5, 0.5);
            Matrix4d view = Matrix4d.LookAt(new Vector(3.0 * Math.Sin(this._Time), 3.0 * Math.Cos(this._Time), 3.0) + lookpos, lookpos, up);
            GL.MultMatrix(ref proj);
            GL.MultMatrix(ref view);
        }

        public override void RenderScene()
        {
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.PointSize(2.0f);
            GL.Begin(BeginMode.Points);
            GL.Color4(Color.RGB(0.0, 0.5, 1.0));
            foreach (Vector v in this._PointSetA)
            {
                GL.Vertex3(v);
            }
            GL.End();
            GL.Disable(EnableCap.DepthTest);
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            this._Time += Time * 0.2;
        }

        private IEnumerable<Vector> _PointSetA;
        private double _Time;
    }
}