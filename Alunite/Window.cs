using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;


namespace Alunite
{
    using CNVVBO = VBO<ColorNormalVertex, ColorNormalVertex.Model>;
    using NVVBO = VBO<NormalVertex, NormalVertex.Model>;
    using VVBO = VBO<Vertex, Vertex.Model>;

    /// <summary>
    /// Main program window
    /// </summary>
    public class Window : GameWindow
    {
        public Window()
            : base(640, 480, GraphicsMode.Default, "Alunite")
        {
            GL.Enable(EnableCap.ColorMaterial);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);

            GL.Light(LightName.Light0, LightParameter.Ambient, Color.RGB(0.2, 0.2, 0.2));
            GL.Light(LightName.Light0, LightParameter.Diffuse, Color.RGB(0.6, 0.6, 0.6));
            GL.Light(LightName.Light0, LightParameter.Specular, Color.RGB(1.0, 0.6, 0.6));
            GL.Light(LightName.Light0, LightParameter.Position, new Vector4(2.0f, 5.0f, -7.8f, 0.0f));

            GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.Diffuse);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            Random r = new Random();

            // Resources
            Path resources = Path.ApplicationStartup.Parent.Parent.Parent["Resources"];
            Path shaders = resources["Shaders"];
            Path textures = resources["Textures"];
            Path models = resources["Models"];

            // Model
            ISequentialArray<Vector> sverts;
            ISequentialArray<Triangle<int>> tris;
            Model.LoadObj(models["Test.obj"], out sverts, out tris);
            StandardArray<Vector> verts = new StandardArray<Vector>(sverts);
            verts.Map(x => x + new Vector(r.NextDouble() / 100.0, r.NextDouble() / 100.0, r.NextDouble() / 100.0));

            // Make a tetrahedralization
            ISequentialArray<Tetrahedron<int>> tetras = Tetrahedralize.Delaunay(verts);

            // Select only some tetrahedra (to make a fractured shape).
            ListArray<Tetrahedron<int>> passedtetras = new ListArray<Tetrahedron<int>>();
            foreach (Tetrahedron<int> tetra in tetras.Values)
            {
                Tetrahedron<Vector> actualtetra = new Tetrahedron<Vector>(
                    verts.Lookup(tetra.A), 
                    verts.Lookup(tetra.B), 
                    verts.Lookup(tetra.C), 
                    verts.Lookup(tetra.D));
                if (Tetrahedron.Midpoint(actualtetra).X < 0.0)
                {
                    passedtetras.Add(tetra);
                }
            }

            ISequentialArray<Triangle<int>> tetratris = Tetrahedralize.Boundary(passedtetras);
            ISequentialArray<Vector> norms = Model.ComputeNormals(verts, tetratris, true);

            // Make a vbo
            this._VBO = new NVVBO(NormalVertex.Model.Singleton,
                new MapSequentialArray<Tuple<Vector, Vector>, NormalVertex>(
                    new ZipSequentialArray<Vector, Vector>(verts, norms),
                    delegate(Tuple<Vector, Vector> PosNorm)
                    {
                        return new NormalVertex(PosNorm.A, PosNorm.B);
                    }), tetratris);

            // Shader test
            int vshade = GL.CreateShader(ShaderType.VertexShader);
            int fshade = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(vshade, Path.ReadText(shaders["TestVS.glsl"]));
            GL.ShaderSource(fshade, Path.ReadText(shaders["TestFS.glsl"]));
            GL.CompileShader(vshade);
            GL.CompileShader(fshade);
            int prog = GL.CreateProgram();
            GL.AttachShader(prog, vshade);
            GL.AttachShader(prog, fshade);
            GL.LinkProgram(prog);
            GL.UseProgram(prog);

            // Textures
            Texture test = Texture.Load(textures["Test.png"]);
            GL.ActiveTexture(TextureUnit.Texture0);
            test.Bind();
            GL.Uniform1(GL.GetUniformLocation(prog, "MaterialDiffuse"), 0);
        }

        /// <summary>
        /// Creates the (content) states for a tetrahedron node graph and makes a randomish structure.
        /// </summary>
        public static StandardArray<Content> Seed(StandardArray<Vector> Vertices, StandardArray<Tetrahedron<int>> Indices,
            StandardArray<Tetrahedron<int>> Borders, Random Random)
        {
            // Create an array of contents for each tetrahedron.
            Content[] contents = new Content[Borders.Count];

            foreach (KeyValuePair<int, Tetrahedron<int>> kvp in Indices.Items)
            {
                Tetrahedron<int> tetra = kvp.Value;
                Tetrahedron<Vector> vectetra = new Tetrahedron<Vector>(
                    Vertices.Lookup(tetra.A),
                    Vertices.Lookup(tetra.B),
                    Vertices.Lookup(tetra.C),
                    Vertices.Lookup(tetra.D));
                Vector midpoint = Tetrahedron.Midpoint(vectetra);
                if (midpoint.Z < 0.0)
                {
                    contents[kvp.Key] = Content.Full;
                }
                else
                {
                    contents[kvp.Key] = Content.Empty;
                }
            }


            return new StandardArray<Content>(contents);
        }

        /// <summary>
        /// Creates triangles for rendering purposes for the specified node graph.
        /// </summary>
        public static void Tesselate(
            StandardArray<Vector> VertexPositions,
            StandardArray<Tetrahedron<int>> Tetrahedrons, StandardArray<Tetrahedron<int>> Borders, StandardArray<Content> Contents,
            out ISequentialArray<ColorNormalVertex> Vertices, out ISequentialArray<int> Indices)
        {
            Dictionary<int, ColorNormalVertex> verts = new Dictionary<int, ColorNormalVertex>();
            ListArray<int> tris = new ListArray<int>();
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
                            Vector facenorm = Triangle.Normal(new Triangle<Vector>(
                                VertexPositions.Lookup(face.A),
                                VertexPositions.Lookup(face.B),
                                VertexPositions.Lookup(face.C)));
                            tris.Add(face.A);
                            tris.Add(face.B);
                            tris.Add(face.C);
                            foreach (int point in face.Points)
                            {
                                ColorNormalVertex vert;
                                if (!verts.TryGetValue(point, out vert))
                                {
                                    vert = new ColorNormalVertex(Color.RGB(0.6, 0.6, 0.6), VertexPositions.Lookup(point), new Vector());
                                }
                                verts[point] = new ColorNormalVertex(vert.Color, vert.Position, vert.Normal + facenorm);
                            }
                        }
                    }
                }
            }

            // Remap vertex data with those that are actually used.
            Dictionary<int, int> vertmapping = new Dictionary<int, int>();
            ListArray<ColorNormalVertex> vertices = new ListArray<ColorNormalVertex>();
            foreach (KeyValuePair<int, ColorNormalVertex> vert in verts)
            {
                vertmapping.Add(vert.Key, vertices.Count);
                vertices.Add(vert.Value);
            }
            tris.Map(x => vertmapping[x]);

            Vertices = vertices;
            Indices = tris;
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
                new Vector3d(Math.Sin(this._Rot), Math.Cos(this._Rot), 1.1),
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(0.0, 0.0, 1.0));
            GL.MultMatrix(ref view);

            this._VBO.Render(BeginMode.Triangles);

            this.SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            this._Rot += e.Time / 5;
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
        }

        private double _Rot;
        private NVVBO _VBO;
    }
}