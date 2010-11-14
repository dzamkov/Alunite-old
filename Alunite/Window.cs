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
        public Window(Planet Planet)
            : base(640, 480, GraphicsMode.Default, "Alunite")
        {
            this.WindowState = WindowState.Maximized;
            this.VSync = VSyncMode.On;
            this.TargetRenderFrequency = 100.0;
            this.TargetUpdateFrequency = 500.0;

            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.ColorMaterial);
            GL.Disable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthTest);

            GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.Diffuse);

            GL.Enable(EnableCap.Light0);
            GL.Light(LightName.Light0, LightParameter.Position, new Vector4(1.0f, 1.0f, 1.0f, 0.0f));
            GL.Light(LightName.Light0, LightParameter.Diffuse, Color.RGB(0.6, 0.6, 0.6));
            GL.Light(LightName.Light0, LightParameter.Ambient, Color.RGB(0.1, 0.1, 0.1));

            Path resources = Path.ApplicationStartup.Parent.Parent.Parent["Resources"];
            Path shaders = resources["Shaders"];

            this._PlanetShader = Shader.Load(shaders["Planet.glsl"]);
            this._VBO = Planet.CreateVBO();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            double cosx = Math.Cos(this._XRot);
            Vector eyepos = new Vector(Math.Sin(this._ZRot) * cosx, Math.Cos(this._ZRot) * cosx, Math.Sin(this._XRot)) * 1.7;
            GL.MatrixMode(MatrixMode.Projection);
            Matrix4d proj = Matrix4d.CreatePerspectiveFieldOfView(1.2, (double)this.Width / (double)this.Height, 0.01, 400.0);
            GL.LoadMatrix(ref proj);
            Matrix4d view = Matrix4d.LookAt(
                eyepos,
                new Vector3d(),
                new Vector3d(0.0, 0.0, 1.0));
            GL.MultMatrix(ref view);

            this._PlanetShader.SetUniform("EyePosition", eyepos);
            this._PlanetShader.SetUniform("SunDirection", new Vector(1.0, 1.0, 1.0));
            this._PlanetShader.Call();
            this._VBO.Render(BeginMode.Triangles);

            this.SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            double updatetime = e.Time;
            if (this.Keyboard[Key.W]) this._XRot += updatetime * 5;
            if (this.Keyboard[Key.A]) this._ZRot += updatetime * 5;
            if (this.Keyboard[Key.S]) this._XRot -= updatetime * 5;
            if (this.Keyboard[Key.D]) this._ZRot -= updatetime * 5;
            if (this.Keyboard[Key.Escape]) this.Close();
            this._XRot = Math.Min(Math.PI / 2.02, Math.Max(Math.PI / -2.02, this._XRot));
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
        }

        private double _XRot;
        private double _ZRot;
        private Shader _PlanetShader;
        private VBO<NormalVertex, NormalVertex.Model> _VBO;
    }
}