using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTKGUI;

namespace Alunite
{
    /// <summary>
    /// A visualizer for a simulation.
    /// </summary>
    public class Visualizer : Render3DControl
    {
        public Visualizer(Signal<Maybe<View>> Feed)
        {
            this._Visual = Visual.Create();
            this._Feed = Feed;
        }

        public override void RenderScene()
        {
            this._Feed[this._Time].Data.Render(this._Visual);
        }

        public override void SetupProjection(Point Viewsize)
        {
            Matrix4d proj = Matrix4d.Perspective(Math.Sin(Math.PI / 8.0), Viewsize.AspectRatio, 0.1, 100.0);
            GL.MultMatrix(ref proj);
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            this._Time += Time;
        }

        private double _Time;
        private Visual _Visual;
        private Signal<Maybe<View>> _Feed;
    }
}