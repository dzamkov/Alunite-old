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

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.ColorMaterial);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthTest);

            GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.Diffuse);

            Path resources = Path.ApplicationStartup.Parent.Parent.Parent["Resources"];
            Path shaders = resources["Shaders"];

            // Initial triangulation
            this._Triangulation = SphericalTriangulation.CreateIcosahedron();
            this._Triangulation.Subdivide();
            this._Triangulation.Subdivide();
            this._Triangulation.Subdivide();
            this._Triangulation.Subdivide();
            this._Triangulation.Subdivide();

            // Assign random colors for testing
            Random r = new Random(101);
            this._VertexColors = new Color[this._Triangulation.Vertices.Count];
            for (int t = 0; t < this._VertexColors.Length; t++)
            {
                if (r.NextDouble() < 0.4)
                {
                    this._VertexColors[t] = Color.RGB(0.0, 0.4, 0.1);
                }
                else
                {
                    this._VertexColors[t] = Color.RGB(0.0, 0.0, 0.2);
                }
            }

            // Create a cubemap
            this._Cubemap = Cubemap.Generate(Texture.RGB16Float, 512, new _CubemapRenderable() { Window = this }, RadiusGround * 0.2f, RadiusGround * 1.2f);

            // Planet
            Shader.PrecompilerInput pci = Shader.CreatePrecompilerInput();
            AtmosphereOptions ao = AtmosphereOptions.DefaultEarth; ao.RadiusGround = (float)RadiusGround;
            AtmosphereQualityOptions aqo = AtmosphereQualityOptions.Default;
            this._Atmosphere = Atmosphere.Generate(ao, aqo, pci, shaders);
            Atmosphere.DefineConstants(ao, aqo, pci);
            this._Planet = Shader.Load(shaders["Atmosphere"]["Planet.glsl"], pci);

            this._Height = RadiusGround * 3;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            double cosx = Math.Cos(this._XRot);
            Vector eyepos = new Vector(Math.Sin(this._ZRot) * cosx, Math.Cos(this._ZRot) * cosx, Math.Sin(this._XRot)) * this._Height;
            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(1.2f, (float)this.Width / (float)this.Height, 300.0f, 20000.0f);
            Matrix4 view = Matrix4.LookAt(
                (Vector3)eyepos,
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(0.0f, 0.0f, 1.0f));
            Matrix4 total = view * proj;
            Matrix4 itotal = Matrix4.Invert(total);

            Vector4 test = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            test = Vector4.Transform(test, itotal);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref total);

            GL.DepthFunc(DepthFunction.Lequal);

            // Planet
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            this._Planet.Call();
            this._Atmosphere.Setup(this._Planet);
            this._Cubemap.SetUnit(TextureTarget.TextureCubeMap, TextureUnit.Texture4);
            this._Planet.SetUniform("EyePosition", eyepos);
            this._Planet.SetUniform("SunDirection", new Vector(Math.Sin(this._SunAngle), Math.Cos(this._SunAngle), 0.0));
            this._Planet.SetUniform("ProjectionInverse", ref itotal);
            this._Planet.SetUniform("NearDistance", 300.0f);
            this._Planet.SetUniform("FarDistance", 20000.0f);
            this._Planet.SetUniform("CubeMap", TextureUnit.Texture4);
            Shader.DrawQuad();

            this.Title = "Alunite (" + this.RenderFrequency.ToString() + ")";

            this.SwapBuffers();
        }

        /// <summary>
        /// Renderable to produce a cubemap for the planet.
        /// </summary>
        private struct _CubemapRenderable : IRenderable
        {
            public void Render()
            {
                GL.Begin(BeginMode.Triangles);
                foreach (Triangle<int> tri in this.Window._Triangulation.Triangles)
                {
                    Triangle<Vector> vectri = this.Window._Triangulation.Dereference(tri);
                    GL.Normal3(Triangle.Normal(vectri));
                    GL.Color4(this.Window._VertexColors[tri.A]);
                    GL.Vertex3(vectri.A * RadiusGround);
                    GL.Color4(this.Window._VertexColors[tri.C]);
                    GL.Vertex3(vectri.C * RadiusGround);
                    GL.Color4(this.Window._VertexColors[tri.B]);
                    GL.Vertex3(vectri.B * RadiusGround);
                }
                GL.End();
            }

            public Window Window;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            double updatetime = e.Time;
            double zoomfactor = Math.Pow(0.8, updatetime);
            if (this.Keyboard[Key.W]) this._XRot += updatetime;
            if (this.Keyboard[Key.A]) this._ZRot += updatetime;
            if (this.Keyboard[Key.S]) this._XRot -= updatetime;
            if (this.Keyboard[Key.D]) this._ZRot -= updatetime;
            if (this.Keyboard[Key.Z]) this._SunAngle += updatetime;
            if (this.Keyboard[Key.X]) this._SunAngle -= updatetime;
            if (this.Keyboard[Key.Q]) this._Height *= zoomfactor;
            if (this.Keyboard[Key.E]) this._Height /= zoomfactor;
            if (this.Keyboard[Key.Escape]) this.Close();
            this._XRot = Math.Min(Math.PI / 2.02, Math.Max(Math.PI / -2.02, this._XRot));
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
        }

        public const double RadiusGround = 6360.0;

        private PrecomputedAtmosphere _Atmosphere;
        private Texture _Cubemap;
        private Shader _CubemapUnroll;
        private Shader _Planet;
        private Color[] _VertexColors;
        private SphericalTriangulation _Triangulation;
        private double _SunAngle;
        private double _Height;
        private double _XRot;
        private double _ZRot;
    }
}