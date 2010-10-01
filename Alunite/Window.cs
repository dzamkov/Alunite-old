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
            this.VSync = VSyncMode.Off;

            GL.Enable(EnableCap.ColorMaterial);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.Diffuse);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            Random r = new Random();

            
            Grid gr = new Grid(new Lattice(new Vector(-0.5, -0.5, -0.5), new Vector(0.1, 0.1, 0.1)), new IVector(11, 11, 11));
            this._Data = new StandardArray<Vector>(gr);
            this._Data = this._Data.Map<Vector>(delegate(Vector In)
            {
                Vector rvec = new Vector(r.NextDouble(), r.NextDouble(), r.NextDouble());
                rvec -= new Vector(0.5, 0.5, 0.5);
                rvec *= 0.05;
                return In + rvec;
            }); // Point fiddlin'
            StandardArray<Tetrahedron<int>> tetras = new StandardArray<Tetrahedron<int>>(gr.Volume);
            StandardArray<Tetrahedron<int>> tetraborders = Tetrahedron.Borders(tetras);
            StandardArray<Content> tetracontents = Seed(tetraborders, r);
            StandardArray<int> tris = Tesselate(tetras, tetraborders, tetracontents);

            // Make a vbo
            this._VBO = new CNVVBO(ColorNormalVertex.Model.Singleton, this._Data.Map<ColorNormalVertex>(delegate(Vector v)
                {
                    return new ColorNormalVertex(Color.RGB(v.X + 0.5, v.Y + 0.5, v.Z + 0.5), v, new Vector());   
                }), tris);
        }

        /// <summary>
        /// Creates the (content) states for a tetrahedron node graph and makes a randomish structure.
        /// </summary>
        public static StandardArray<Content> Seed(StandardArray<Tetrahedron<int>> Borders, Random Random)
        {
            // Create an array of contents for each tetrahedron.
            Content[] contents = new Content[Borders.Size];

            // The open set contains the fringe of the ever expanding blue form.
            HashSet<int> openset = new HashSet<int>();

            // Stuff some random seeds in there.
            for (int t = 0; t < 5; t++)
            {
                openset.Add(Random.Next(contents.Length));
            }
            
            // Begin growth
            int iteration = 0;
            HashSet<int> newopenset = new HashSet<int>();
            while (iteration < 20)
            {
                foreach (int tetra in openset)
                {
                    contents[tetra] = Content.Full;
                    
                    // SPREAD!!
                    foreach (int bord in Borders.Lookup(tetra).Points)
                    {
                        if (bord >= 0 && contents[bord] == Content.Empty && Random.NextDouble() > 0.4)
                        {
                            newopenset.Add(bord);
                        }
                    }
                }

                HashSet<int> temp = openset;
                openset.Clear();
                openset = newopenset;
                newopenset = temp;
                iteration++;
            }

            return new StandardArray<Content>(contents);
        }

        /// <summary>
        /// Creates triangles for rendering purposes for the specified node graph.
        /// </summary>
        public static StandardArray<int> Tesselate(StandardArray<Tetrahedron<int>> Tetrahedrons, StandardArray<Tetrahedron<int>> Borders, StandardArray<Content> Contents)
        {
            List<int> tris = new List<int>();
            foreach (KeyValuePair<int, Tetrahedron<int>> kvp in Tetrahedrons.Items)
            {
                if (Contents.Lookup(kvp.Key) == Content.Full)
                {
                    Triangle<int>[] faces = kvp.Value.Faces;
                    int[] borders = Borders.Lookup(kvp.Key).Points;
                    for (int t = 0; t < borders.Length; t++)
                    {
                        if (borders[t] < 0 || Contents.Lookup(borders[t]) == Content.Empty)
                        {
                            Triangle<int> face = faces[t];
                            tris.Add(face.A);
                            tris.Add(face.B);
                            tris.Add(face.C);
                        }
                    }
                }
            }
            return new StandardArray<int>(tris, tris.Count);
        }

        /// <summary>
        /// The contents of a tetrahedron node graph.
        /// </summary>
        public enum Content
        {
            Empty,
            Full
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