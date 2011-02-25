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
            this._Particles = new List<_Particle>();

            Random r = new Random();
            for (int t = 0; t < 100; t++)
            {
                this._Particles.Add(new _Particle()
                {
                    Position = new Vector(r.NextDouble(), r.NextDouble(), r.NextDouble()),
                    Velocity = new Vector(0.0, 0.0, 0.0)
                });
            }
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
            GL.Color4(0.0, 0.5, 1.0, 1.0);
            GL.Begin(BeginMode.Points);
            foreach (_Particle pt in this._Particles)
            {
                GL.Vertex3(pt.Position);
            }
            GL.End();
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            List<_Particle> nparts = new List<_Particle>(this._Particles.Count);
            for(int t = 0; t < this._Particles.Count; t++)
            {
                _Particle pt = this._Particles[t];
                Vector pos = pt.Position;
                Vector vel = pt.Velocity;

                // Make force vector
                Vector force = new Vector(0.0, 0.0, -1.0);
                for(int i = 0; i < this._Particles.Count; i++)
                {
                    _Particle opt = this._Particles[i];
                    if (t != i)
                    {
                        Vector away = pos - opt.Position;
                        double len = Math.Max(away.Length, 0.001);
                        Vector awayforce = away * (0.001 / (len * len * len));
                        force += awayforce;
                    }
                }

                vel += force * Time;
                pos += vel * Time;

                double rest = 0.2;
                if (pos.X > 1.0) { pos.X = 1.0; vel.X = -Math.Abs(vel.X) * rest; }
                if (pos.Y > 1.0) { pos.Y = 1.0; vel.Y = -Math.Abs(vel.Y) * rest; }
                if (pos.Z > 1.0) { pos.Z = 1.0; vel.Z = -Math.Abs(vel.Z) * rest; }
                if (pos.X < 0.0) { pos.X = 0.0; vel.X = Math.Abs(vel.X) * rest; }
                if (pos.Y < 0.0) { pos.Y = 0.0; vel.Y = Math.Abs(vel.Y) * rest; }
                if (pos.Z < 0.0) { pos.Z = 0.0; vel.Z = Math.Abs(vel.Z) * rest; }

                nparts.Add(new _Particle()
                {
                    Position = pos,
                    Velocity = vel
                });
            }
            this._Particles = nparts;
        }

        private struct _Particle
        {
            public Vector Velocity;
            public Vector Position;
        }

        private List<_Particle> _Particles;
        private Matter _Matter;
    }
}