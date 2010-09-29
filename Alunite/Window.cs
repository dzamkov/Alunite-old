using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;


namespace Alunite
{
    using CNVVBO = VBO<ColorNormalVertex, ColorNormalVertex.Model>;

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

            Random r = new Random(1337);

            // Gets the edges between the midpoints of the tetrahedrons in the triangulation of this set of points.
            Grid gr = new Grid(new Lattice(new Vector(-0.5, -0.5, -0.5), new Vector(0.2, 0.2, 0.2)), new IVector(6, 6, 6));
            this._Data = new StandardArray<Vector>(gr);
            this._Data = this._Data.Map<Vector>(delegate(Vector In)
            {
                Vector rvec = new Vector(r.NextDouble(), r.NextDouble(), r.NextDouble());
                rvec -= new Vector(0.5, 0.5, 0.5);
                rvec *= 0.1;
                return In + rvec;
            }); // Point fiddlin'
            HashSet<Tetrahedron<int>> hstetras = new HashSet<Tetrahedron<int>>(gr.Volume);
            StandardArray<Tetrahedron<int>> tetras = new StandardArray<Tetrahedron<int>>(hstetras, hstetras.Count);
            StandardArray<int> tris = tetras.Expand<int>(12, delegate(Tetrahedron<int> tetra)
            {
                int[] items = new int[12];
                int i = 0;
                foreach (Triangle<int> tri in tetra.Faces)
                {
                    foreach (int point in tri.Points)
                    {
                        items[i] = point;
                        i++;
                    }
                }
                return items;
            });

            // Make a vbo
            this._VBO = new CNVVBO(ColorNormalVertex.Model.Singleton, this._Data.Map<ColorNormalVertex>(delegate(Vector v)
                {
                    return new ColorNormalVertex(Color.RGB(v.X + 0.5, v.Y + 0.5, v.Z + 0.5), v, new Vector());   
                }), tris);
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

            this._VBO.Render(BeginMode.Triangles);

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
        private IEnumerable<Tetrahedron<Vector>> _Tetras;
        private CNVVBO _VBO;
    }
}