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
            this._Feed = Feed;
        }

        public override void RenderScene()
        {

        }

        public override void SetupProjection(Point Viewsize)
        {

        }

        private Signal<Maybe<View>> _Feed;
    }
}