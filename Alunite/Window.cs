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
            this._Planet = Planet;
            this._Planet.Load(shaders);

            this._Height = 20000.0;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            double cosx = Math.Cos(this._XRot);
            Vector eyepos = new Vector(Math.Sin(this._ZRot) * cosx, Math.Cos(this._ZRot) * cosx, Math.Sin(this._XRot)) * this._Height;
            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(2.2f, (float)this.Width / (float)this.Height, 0.1f, 10000.0f);
            Matrix4 view = Matrix4.LookAt(
                new Vector3(),
                -(Vector3)eyepos,
                new Vector3(0.0f, 0.0f, 1.0f));
            if (this._SunsetMode)
            {
                double h = 6360 + this._Height / 20000.0;
                view = Matrix4.LookAt(
                    new Vector3(0.0f, (float)h, 0.0f),
                    new Vector3(3.0f, (float)h + 0.5f, 0.0f),
                    new Vector3(0.0f, 1.0f, 0.0f));
                eyepos = new Vector(0.0, h, 0.0);
            }

            this._Planet.Render(proj, view, eyepos, Vector.Normalize(new Vector(Math.Sin(this._SunAngle), Math.Cos(this._SunAngle), 0.0)));

            this.SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            double updatetime = e.Time;
            double zoomfactor = Math.Pow(0.8, updatetime);
            if (this.Keyboard[Key.W]) this._XRot += updatetime;
            if (this.Keyboard[Key.A]) this._ZRot += updatetime;
            if (this.Keyboard[Key.S]) this._XRot -= updatetime;
            if (this.Keyboard[Key.D]) this._ZRot -= updatetime;
            if (this.Keyboard[Key.Q]) this._Height *= zoomfactor;
            if (this.Keyboard[Key.E]) this._Height /= zoomfactor;
            if (this.Keyboard[Key.Z]) this._SunAngle += updatetime * 0.5;
            if (this.Keyboard[Key.X]) this._SunAngle -= updatetime * 0.5;
            if (this.Keyboard[Key.C]) this._SunsetMode = true; else this._SunsetMode = false;
            if (this.Keyboard[Key.Escape]) this.Close();
            this._XRot = Math.Min(Math.PI / 2.02, Math.Max(Math.PI / -2.02, this._XRot));
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
        }

        private bool _SunsetMode;
        private double _Height;
        private double _XRot;
        private double _ZRot;
        private double _SunAngle;
        private Planet _Planet;
    }
}