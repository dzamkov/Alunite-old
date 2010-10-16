using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using System;
using System.Collections.Generic;

using System.Windows.Forms;
using System.Drawing;

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
            this.WindowState = WindowState.Maximized;
            this.VSync = VSyncMode.Off;
            this.TargetRenderFrequency = 100.0;
            this.TargetUpdateFrequency = 500.0;

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
            Texture test = Texture.Load(textures["Dirt.png"]);
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
            Vector eyepos = this._PlayerPos + new Vector(0.0, 0.0, 1.3);
            Matrix4d view = Matrix4d.LookAt(
                eyepos,
                eyepos + this.LookDir,
                new Vector3d(0.0, 0.0, 1.0));
            GL.MultMatrix(ref view);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.UseProgram(this._ShaderProgram);
            this._VBO.Render(BeginMode.Triangles);
            GL.UseProgram(0);

            this.SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (this.Keyboard[Key.M])
            {
                this.WindowState = WindowState.Minimized;
            }
            if (this.Keyboard[Key.N])
            {
                this.WindowState = WindowState.Fullscreen;
            }
            if (this.Keyboard[Key.Escape])
            {
                this.Close();
            }

            // Mouse look
            if (this.Focused)
            {
                Cursor.Hide();

                Point curpoint = Cursor.Position;
                Size screensize = Screen.PrimaryScreen.WorkingArea.Size;
                Point center = new Point(screensize.Width / 2, screensize.Height / 2);
                Cursor.Position = center;

                this._CamZ += (double)(curpoint.X - center.X) / 100.0;
                this._CamX += (double)(curpoint.Y - center.Y) / -100.0;

                this._CamX = Math.Min(this._CamX, Math.PI / 2.1);
                this._CamX = Math.Max(this._CamX, -Math.PI / 2.1);
            }
            else
            {
                Cursor.Show();
            }

            // Gravity affects everything
            this._PlayerVelocity.Z -= 9.8 * e.Time;

            // Determine if the player is on ground
            Vector hitpos;
            Vector hitnorm;
            double hitlen;
            bool onground = this._World.Trace(new Segment<Vector>(
                this._PlayerPos + new Vector(0.0, 0.0, 0.1), 
                this._PlayerPos - new Vector(0.0, 0.0, 0.1)), out hitlen, out hitpos, out hitnorm);

            // If player is on ground, movement is highly constrained
            if (onground)
            {
                double fricmult = Math.Pow(Math.Pow(0.02, 0.1), e.Time * 10);
                this._PlayerVelocity = Vector.Scale(this._PlayerVelocity, new Vector(fricmult, fricmult, 1));

                // But he can't fall
                this._PlayerVelocity.Z = Math.Max(0.0, this._PlayerVelocity.Z);

                // Lucky he can control his movements
                Vector side = Vector.Cross(new Vector(0.0, 0.0, 1.0), this.LookDir);
                if (this.Keyboard[Key.Space])
                {
                    this._PlayerVelocity.Z = 5.0; // Slightly strong jump
                }
                if (this.Keyboard[Key.W])
                {
                    this._PlayerVelocity += this.LookDir * 30.0 * e.Time;
                }
                if (this.Keyboard[Key.S])
                {
                    this._PlayerVelocity -= this.LookDir * 30.0 * e.Time;
                }
                if (this.Keyboard[Key.A])
                {
                    this._PlayerVelocity += side * 30.0 * e.Time;
                }
                if (this.Keyboard[Key.D])
                {
                    this._PlayerVelocity -= side * 30.0 * e.Time;
                }
            }

            // Now apply velocity forces and collision
            Vector movement = this._PlayerVelocity * e.Time;
            bool hit = true;
            while (hit)
            {
                hit = this._World.Trace(new Segment<Vector>(this._PlayerPos, this._PlayerPos + movement), out hitlen, out hitpos, out hitnorm) && hitlen > 0.0 && hitlen < 1.0;
                if (hit)
                {
                    this._PlayerPos = hitpos;
                    movement = Vector.Reflect(movement * (1.0 - hitlen), hitnorm);
                    this._PlayerVelocity = Vector.Reflect(this._PlayerVelocity, hitnorm) * 0.5;
                }
            }
            this._PlayerPos += movement;
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
        }

        /// <summary>
        /// Gets the direction vector for where the player is looking.
        /// </summary>
        public Vector LookDir
        {
            get
            {
                double cosx = Math.Cos(this._CamX);
                return new Vector(Math.Sin(this._CamZ) * cosx, Math.Cos(this._CamZ) * cosx, Math.Sin(this._CamX));
            }
        }

        private double _CamZ;
        private double _CamX;
        private Vector _PlayerPos;
        private Vector _PlayerVelocity;

        private int _ShaderProgram;
        private World _World;
        private NVVBO _VBO;
    }
}