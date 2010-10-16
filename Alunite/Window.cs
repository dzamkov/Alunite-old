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

            GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.Diffuse);

            Random r = new Random();

            // Resources
            Path resources = Path.ApplicationStartup.Parent.Parent.Parent["Resources"];
            Path shaders = resources["Shaders"];
            Path textures = resources["Textures"];
            Path models = resources["Models"];

            // World
            this._World = new World();
            this._VBO = this._World.CreateVBO();

            // Shader test
            int vshade = GL.CreateShader(ShaderType.VertexShader);
            int fshade = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(vshade, Path.ReadText(shaders["TestVS.glsl"]));
            GL.ShaderSource(fshade, Path.ReadText(shaders["TestFS.glsl"]));
            GL.CompileShader(vshade);
            GL.CompileShader(fshade);
            int prog = this._ShaderProgram = GL.CreateProgram();
            GL.AttachShader(prog, vshade);
            GL.AttachShader(prog, fshade);
            GL.LinkProgram(prog);

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
                new Vector3d(Math.Sin(this._Rot) * 2, Math.Cos(this._Rot) * 2, 2.1),
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(0.0, 0.0, 1.0));
            GL.MultMatrix(ref view);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.UseProgram(this._ShaderProgram);
            this._VBO.Render(BeginMode.Triangles);
            GL.UseProgram(0);
            
            // Test tracing
            GL.LineWidth(3.0f);
            Vector tracestart = new Vector(0.9, 0.9, 0.2);
            Vector traceend = new Vector(-0.4, -0.3, -1.5);
            GL.Begin(BeginMode.Lines);
            GL.Color4(Color.RGB(1.0, 0.0, 0.0));
            GL.Vertex3(tracestart);
            GL.Vertex3(traceend);
            GL.End();
            Vector hitpos;
            Vector hitnorm;
            double hitlen;
            if (this._World.Trace(new Segment<Vector>(tracestart, traceend), out hitlen, out hitpos, out hitnorm))
            {
                Vector incoming = (traceend - tracestart) * (1.0 - hitlen);
                Vector outgoing = Vector.Reflect(incoming, hitnorm);
                GL.Begin(BeginMode.Lines);
                GL.Color4(Color.RGB(0.0, 0.0, 1.0));
                GL.Vertex3(hitpos);
                GL.Vertex3(hitpos + hitnorm * 0.2);
                GL.Color4(Color.RGB(0.0, 1.0, 0.0));
                GL.Vertex3(hitpos);
                GL.Vertex3(hitpos + outgoing);
                GL.End();
            }

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
        private int _ShaderProgram;
        private World _World;
        private NVVBO _VBO;
    }
}