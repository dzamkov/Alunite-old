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
        public Visualizer(Matter Matter)
        {
            this._Matter = Matter;
        }

        /// <summary>
        /// Gets the matter to be visualized.
        /// </summary>
        public Matter Matter
        {
            get
            {
                return this._Matter;
            }
        }

        public override void SetupProjection(Point Viewsize)
        {
            Vector3d up = new Vector3d(0.0, 0.0, 1.0);
            Matrix4d proj = Matrix4d.CreatePerspectiveFieldOfView(Math.Sin(Math.PI / 8.0), this.Size.AspectRatio, 0.1, 100.0);
            Matrix4d view = Matrix4d.LookAt(new Vector(3.0, 3.0, 3.0), new Vector(0.0, 0.0, 0.0), up);
            GL.MultMatrix(ref proj);
            GL.MultMatrix(ref view);
        }

        public override void RenderScene()
        {
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.PointSize(5.0f);
            GL.Begin(BeginMode.Points);
            foreach (Particle p in this._Matter.Particles)
            {
                Color col = Color.RGB(1.0, 1.0, 1.0);
                IVisualSubstance vissub = p.Substance as IVisualSubstance;
                if (vissub != null)
                {
                    col = vissub.Color;
                }
                GL.Color4(col);
                GL.Vertex3(p.Position);
            }
            GL.End();
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            this._Matter = this._Matter.Update(Matter.Null, Time);
        }

        private Matter _Matter;
    }
}