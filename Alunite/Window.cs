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
            Path skyboxes = resources["Skyboxes"];
            Path misc = resources["Misc"];

            // World
            this._World = new World();
            this._VBO = this._World.CreateVBO();

            // Skybox
            Path testskybox = skyboxes["Test"];
            this._SunDir = Vector.Normalize(new Vector(-1.0, 0.0, 0.5));
            this._Skybox = new Texture[6];
            this._Skybox[0] = Texture.Load(testskybox["Front.jpg"]);
            this._Skybox[1] = Texture.Load(testskybox["Left.jpg"]);
            this._Skybox[2] = Texture.Load(testskybox["Back.jpg"]);
            this._Skybox[3] = Texture.Load(testskybox["Right.jpg"]);
            this._Skybox[4] = Texture.Load(testskybox["Top.jpg"]);
            foreach (Texture t in this._Skybox)
            {
                if (t != null)
                {
                    t.SetInterpolation(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
                    t.SetWrap(TextureWrapMode.Clamp, TextureWrapMode.Clamp);
                }
            }

            // Shader
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
            this._Dirt = Texture.Load(textures["Test.png"]);

            this.Mouse.ButtonDown += delegate(object sender, MouseButtonEventArgs e)
            {
                if (this._LookTarget != null)
                {
                    if (e.Button == MouseButton.Left)
                    {
                        this._World.Dig(this._LookTarget.Value);
                        this._VBO.Dispose();
                        this._VBO = this._World.CreateVBO();
                    }
                    if (e.Button == MouseButton.Right)
                    {
                        this._World.Build(this._LookTarget.Value);
                        this._VBO.Dispose();
                        this._VBO = this._World.CreateVBO();
                    }
                }
            };
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

            Vector eyepos = this._PlayerPos + EyeOffset;

            GL.MatrixMode(MatrixMode.Projection);
            Matrix4d proj = Matrix4d.CreatePerspectiveFieldOfView(1.2, (double)this.Width / (double)this.Height, 0.01, 400.0);
            GL.LoadMatrix(ref proj);
            Matrix4d view = Matrix4d.LookAt(
                new Vector3d(0.0, 0.0, 0.0),
                this.LookDir,
                new Vector3d(0.0, 0.0, 1.0));
            GL.MultMatrix(ref view);

            // Skybox
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.Color4(Color.RGB(1.0, 1.0, 1.0));
            Model.RenderSkybox(this._Skybox);

            // World
            GL.Translate(-eyepos);
            GL.Enable(EnableCap.DepthTest);
            GL.UseProgram(this._ShaderProgram);
            GL.ActiveTexture(TextureUnit.Texture0);
            this._Dirt.Bind();
            GL.Uniform1(GL.GetUniformLocation(this._ShaderProgram, "MaterialDiffuse"), 0);
            GL.Uniform3(GL.GetUniformLocation(this._ShaderProgram, "SunDirection"), (Vector3)this._SunDir);

            GL.LineWidth(2.0f);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            this._VBO.Render(BeginMode.Triangles);
            GL.UseProgram(0);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);


            // Cursor
            if (this._LookTarget != null)
            {
                GL.Disable(EnableCap.Texture2D);
                GL.Disable(EnableCap.DepthTest);
                GL.PointSize(6.0f);
                GL.Begin(BeginMode.Points);
                GL.Color4(Color.RGB(1.0, 1.0, 1.0));
                GL.Vertex3(this._LookTarget.Value);
                GL.End();
            }

            // Sun flare
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Texture2D);
            double flare = Vector.Dot(this.LookDir, this._SunDir);
            flare *= flare * flare;
            flare /= 2;
            if (flare > 0.0)
            {
                GL.Begin(BeginMode.Quads);
                GL.Color4(Color.RGBA(1.0, 1.0, 1.0, flare));
                GL.Vertex2(-1.0f, -1.0f);
                GL.Vertex2(1.0f, -1.0f);
                GL.Vertex2(1.0f, 1.0f);
                GL.Vertex2(-1.0f, 1.0f);
                GL.End();
            }
            GL.Disable(EnableCap.Blend);
            GL.PopMatrix();

            System.Threading.Thread.Sleep(1);

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
                if (this.WindowState == WindowState.Fullscreen)
                {
                    this.WindowState = WindowState.Maximized;
                }
                else
                {
                    this.WindowState = WindowState.Fullscreen;
                }
            }
            if (this.Keyboard[Key.Escape])
            {
                this.Close();
            }

            // Mouse look
            if (this.Focused)
            {
                Cursor.Hide();

                var curpoint = Cursor.Position;
                Size screensize = Screen.PrimaryScreen.WorkingArea.Size;
                var center = new System.Drawing.Point(screensize.Width / 2, screensize.Height / 2);
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

            // What is being looked at?
            Vector hitpos;
            Vector hitnorm;
            double hitlen;
            Vector eyepos = this._PlayerPos + EyeOffset;
            if (this._World.Trace(new Segment<Vector>(eyepos, eyepos + this.LookDir * 4), out hitlen, out hitpos, out hitnorm))
            {
                this._LookTarget = hitpos;
            }
            else
            {
                this._LookTarget = null;
            }

            // Gravity affects everything
            this._PlayerVelocity.Z -= 9.8 * e.Time;

            // Determine if the player is on ground
            bool onground = this._World.Trace(new Segment<Vector>(
                this._PlayerPos + new Vector(0.0, 0.0, 0.1), 
                this._PlayerPos - new Vector(0.0, 0.0, 0.1)), out hitlen, out hitpos, out hitnorm);

            // If player is on ground, movement is highly constrained
            if (onground)
            {
                double fricmult = Math.Pow(Math.Pow(0.02, 0.1), e.Time * 10);
                this._PlayerVelocity = Vector.Scale(this._PlayerVelocity, new Vector(fricmult, fricmult, 1));

                // Luckily he can control his movements
                Vector side = Vector.Cross(new Vector(0.0, 0.0, 1.0), this.LookDir);
                Vector move = new Vector();
                if (this.Keyboard[Key.W])
                {
                    move += this.LookDir;
                }
                if (this.Keyboard[Key.S])
                {
                    move -= this.LookDir;
                }
                if (this.Keyboard[Key.A])
                {
                    move += side;
                }
                if (this.Keyboard[Key.D])
                {
                    move -= side;
                }
                double moveamount = move.Length;
                if (moveamount > 0.0)
                {
                    move *= (1.0 / moveamount);
                    move.Z = 0.0;
                    move *= 30.0 * e.Time;
                    this._PlayerVelocity += move;
                }

                if (this.Keyboard[Key.Space])
                {
                    this._PlayerVelocity.Z = 5.0; // Slightly strong jump
                }

                // And he can't fall
                this._PlayerVelocity.Z = Math.Max(0.0, this._PlayerVelocity.Z);
            }

            // Now apply velocity forces and collision
            Vector movement = this._PlayerVelocity * e.Time;
            bool hit = true;
            while (hit && movement.SquareLength > 0)
            {
                hit = this._World.Trace(new Segment<Vector>(this._PlayerPos, this._PlayerPos + movement), out hitlen, out hitpos, out hitnorm);
                if (hit)
                {
                    this._PlayerPos = hitpos;
                    movement = Vector.Reflect(movement * (1.0 - hitlen), hitnorm);
                    this._PlayerVelocity = Vector.Reflect(this._PlayerVelocity, hitnorm) * 0.9;
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

        public static readonly Vector EyeOffset = new Vector(0.0, 0.0, 1.4);

        private Vector _SunDir;

        private double _CamZ;
        private double _CamX;
        private Vector _PlayerPos;
        private Vector _PlayerVelocity;
        private Vector? _LookTarget;

        private Texture[] _Skybox;
        private Texture _Dirt;

        private List<List<Point>> _Polygon;
        private int _ShaderProgram;
        private World _World;
        private NVVBO _VBO;
    }
}