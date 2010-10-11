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
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            Random r = new Random();

            // Resources
            Path resources = Path.ApplicationStartup.Parent.Parent.Parent["Resources"];
            Path shaders = resources["Shaders"];
            Path textures = resources["Textures"];
            Path models = resources["Models"];

            // Shape
            IArray<Vector> verts;
            IArray<Triangle<int>> tris;
            Model.LoadObj(models["Test.obj"], out verts, out tris);
            IArray<Vector> norms = Model.ComputeNormals(verts, tris, true);

            // Make a vbo
            this._VBO = new NVVBO(NormalVertex.Model.Singleton,
                Data.Map<Tuple<Vector, Vector>, NormalVertex>(
                    Data.Zip(verts, norms),
                    delegate(Tuple<Vector, Vector> PosNorm)
                    {
                        return new NormalVertex(PosNorm.A, PosNorm.B);
                    }), tris);

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