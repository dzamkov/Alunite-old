using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Alunite
{
    /// <summary>
    /// A possibly interactive representation of a set of geometry, mainly for debugging purposes.
    /// </summary>
    public class Diagram
    {
        public Diagram()
        {
            this._Triangles = new Dictionary<Triangle<int>, Color>();
            this._Segments = new Dictionary<UnorderedSegment<int>, _Style>();
            this._SegmentsByThickness = new Dictionary<double, List<UnorderedSegment<int>>>();
            this._Vertices = new List<Vector>();
        }

        public Diagram(List<Vector> Vertices)
        {
            this._Triangles = new Dictionary<Triangle<int>, Color>();
            this._Segments = new Dictionary<UnorderedSegment<int>, _Style>();
            this._SegmentsByThickness = new Dictionary<double, List<UnorderedSegment<int>>>();
            this._Vertices = Vertices;
        }

        /// <summary>
        /// Adds a vertex to the diagram, and gets its position.
        /// </summary>
        public int AddVertex(Vector Position)
        {
            int ind = this._Vertices.Count;
            this._Vertices.Add(Position);
            return ind;
        }

        /// <summary>
        /// Performs steps to insure the diagram renders correctly.
        /// </summary>
        public void RenderSetup()
        {
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.ColorMaterial);
            GL.Disable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthTest);

            GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.Diffuse);

            GL.UseProgram(0);
           
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Enable(EnableCap.Light0);
            GL.Light(LightName.Light0, LightParameter.Position, new Vector4(1.0f, 1.0f, 1.0f, 0.0f));
            GL.Light(LightName.Light0, LightParameter.Diffuse, Color.RGB(0.6, 0.6, 0.6));
            GL.Light(LightName.Light0, LightParameter.Ambient, Color.RGB(0.1, 0.1, 0.1));
        }

        /// <summary>
        /// Renders the contents of the diagram.
        /// </summary>
        public void Render()
        {
            // Opaque triangles.
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.Lighting); 
            this._OutputTriangles(false);

            // Opaque segments.
            GL.Disable(EnableCap.Lighting);
            this._OutputSegments(false);

            // Translucent triangles.
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Blend);
            this._OutputTriangles(true);

            // Translucent segments.
            GL.Disable(EnableCap.Lighting);
            this._OutputTriangles(false);
        }

        /// <summary>
        /// Sets the styles for a triangle.
        /// </summary>
        public void SetTriangle(Triangle<int> Triangle, Color Color)
        {
            this._Triangles[Triangle] = Color;
        }

        /// <summary>
        /// Sets the styles for a bordered triangle.
        /// </summary>
        public void SetBorderedTriangle(Triangle<int> Triangle, Color InteriorColor, Color BorderColor, double BorderThickness)
        {
            this.SetTriangle(Triangle, InteriorColor);
            foreach (Segment<int> s in Triangle.Segments)
            {
                this.SetSegment(s, BorderColor, BorderThickness);
            }
        }

        /// <summary>
        /// Sets the styles for a segment.
        /// </summary>
        public void SetSegment(Segment<int> Segment, Color Color, double Thickness)
        {
            UnorderedSegment<int> unseg = new UnorderedSegment<int>(Segment);
            this._Segments[unseg] = new _Style() { Color = Color, Thickness = Thickness };
            List<UnorderedSegment<int>> seglist;
            if (!this._SegmentsByThickness.TryGetValue(Thickness, out seglist))
            {
                this._SegmentsByThickness[Thickness] = seglist = new List<UnorderedSegment<int>>();
            }
            seglist.Add(unseg);
        }

        /// <summary>
        /// Displays the diagram in its own window.
        /// </summary>
        public void Display()
        {
            new _DiagramWindow(this).Run();
        }

        private class _DiagramWindow : GameWindow
        {
            public _DiagramWindow(Diagram Diagram) : base(640, 480, GraphicsMode.Default, "Diagram")
            {
                this.VSync = VSyncMode.On;
                this._Diagram = Diagram;
                this._Diagram.RenderSetup();
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

                this._Diagram.Render();

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
            private Diagram _Diagram;
        }

        /// <summary>
        /// Gets the actual position of a triangle in the diagram.
        /// </summary>
        private Triangle<Vector> _Dereference(Triangle<int> Triangle)
        {
            return new Triangle<Vector>(
                this._Vertices[Triangle.A],
                this._Vertices[Triangle.B],
                this._Vertices[Triangle.C]);
        }

        /// <summary>
        /// Gets the actual position of a segment in the diagram.
        /// </summary>
        private Segment<Vector> _Dereference(Segment<int> Segment)
        {
            return new Segment<Vector>(
                this._Vertices[Segment.A],
                this._Vertices[Segment.B]);
        }

        /// <summary>
        /// Outputs a triangle to the current graphics context.
        /// </summary>
        private void _OutputTriangle(Triangle<int> Triangle, Color Color)
        {
            Triangle<Vector> tri = this._Dereference(Triangle);
            Vector norm = Alunite.Triangle.Normal(tri);
            GL.Color4(Color);
            GL.Normal3(norm);
            GL.Vertex3(tri.A);
            GL.Vertex3(tri.B);
            GL.Vertex3(tri.C);
        }

        /// <summary>
        /// Outputs a segment to the current graphics context.
        /// </summary>
        private void _OutputSegment(Segment<int> Segment, Color Color, double Thickness)
        {
            Segment<Vector> seg = this._Dereference(Segment);
            GL.Color4(Color);
            GL.Vertex3(seg.A);
            GL.Vertex3(seg.B);
        }

        /// <summary>
        /// Outputs all segments that meet the criteria.
        /// </summary>
        private void _OutputSegments(bool Translucent)
        {
            foreach (var thickness in this._SegmentsByThickness)
            {
                GL.LineWidth((float)thickness.Key);
                GL.Begin(BeginMode.Lines);
                foreach (var segi in thickness.Value)
                {
                    _Style style = this._Segments[segi];
                    this._OutputSegment(segi.Source, style.Color, style.Thickness);
                }
                GL.End();
            }
        }

        /// <summary>
        /// Outputs all triangles that meet the criteria.
        /// </summary>
        private void _OutputTriangles(bool Translucent)
        {
            GL.Begin(BeginMode.Triangles);
            foreach (var tri in this._Triangles)
            {
                if (Translucent ^ tri.Value.A == 1.0)
                {
                    this._OutputTriangle(tri.Key, tri.Value);
                }
            }
            GL.End();
        }

        /// <summary>
        /// Rendering style for a segment or point.
        /// </summary>
        private struct _Style
        {
            public Color Color;
            public double Thickness;
        }

        private List<Vector> _Vertices;
        private Dictionary<Triangle<int>, Color> _Triangles;
        private Dictionary<UnorderedSegment<int>, _Style> _Segments;
        private Dictionary<double, List<UnorderedSegment<int>>> _SegmentsByThickness;
    }
}