﻿using System;
using System.Collections.Generic;


using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Alunite
{
    /// <summary>
    /// Contains cached data and settings for rendering.
    /// </summary>
    public class Visual
    {
        /// <summary>
        /// Creates a new visual context.
        /// </summary>
        public static Visual Create()
        {
            return new Visual();
        }

        /// <summary>
        /// Gets a signal of views that shows what is seen by an untransformed camera in an environment.
        /// </summary>
        public static Signal<View> GetViewFeed(Span Environment)
        {
            return new ViewSignal(Environment);
        }

        /// <summary>
        /// Renders an entity to the current graphics context.
        /// </summary>
        public void Render(Entity Entity)
        {
            TransformedEntity te = Entity as TransformedEntity;
            if (te != null)
            {
                Matrix4d tr = te.Transform.OffsetMatrix;
                GL.PushMatrix();
                GL.MultMatrix(ref tr);
                Render(te.Source);
                GL.PopMatrix();
                return;
            }

            BinaryEntity be = Entity as BinaryEntity;
            if (be != null)
            {
                Render(be.Primary);
                Render(be.Secondary);
                return;
            }

            Brush br = Entity as Brush;
            if (br != null)
            {
                Shape<Substance> shape = br.Shape;
                MappedShape<bool, Substance> ms = shape as MappedShape<bool, Substance>;
                if (ms != null)
                {
                    Mask mk = ms.Source as Mask;
                    if (mk != null)
                    {
                        Substance inner = ms.Map(true);
                        Substance outer = ms.Map(false);
                        if (outer == Substance.Vacuum)
                        {
                            Surface<Void> surf = mk.Surface;
                            Mesh<Void> mesh = surf.ApproximateMesh(30);
                            mesh.Resolve(new _MeshRenderResolver());
                        }
                    }
                }
            }
        }

        private class _MeshRenderResolver : Mesh<Void>.IMeshResolver
        {
            public void Resolve<TTriangle, TVertex>(Mesh<Void, TTriangle, TVertex> Mesh)
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                GL.Begin(BeginMode.Triangles);
                GL.Color3(1.0, 0.5, 0.0);
                foreach (TTriangle tri in Mesh.Triangles)
                {
                    Triangle<TVertex> dtri = Mesh.LookupTriangle(tri);
                    Triangle<Vector> vtri = new Triangle<Vector>(
                        Mesh.LookupVertex(dtri.A),
                        Mesh.LookupVertex(dtri.B),
                        Mesh.LookupVertex(dtri.C));
                    GL.Vertex3(vtri.A);
                    GL.Vertex3(vtri.B);
                    GL.Vertex3(vtri.C);
                }
                GL.End();
            }
        }
    }

    /// <summary>
    /// A signal of views generated by cameras.
    /// </summary>
    public class ViewSignal : Signal<View>
    {
        public ViewSignal(Span Environment)
        {
            this._Environment = Environment;
        }

        public override View this[double Time]
        {
            get
            {
                return new View(this._Environment[Time]);
            }
        }

        public override Signal<View> Simplify
        {
            get
            {
                this._Environment = this._Environment.Simplify;
                return this;
            }
        }

        private Span _Environment;
    }

    /// <summary>
    /// A view of a simulation that can be rendered to a graphics context.
    /// </summary>
    public class View
    {
        public View(Entity Entity)
        {
            this._Entity = Entity;
        }

        /// <summary>
        /// Gets the single entity that represents what is seen. The view is looking at the entity from (0.0, 0.0, 0.0) towards (1.0, 0.0, 0.0) with
        /// (0.0, 0.0, 1.0) as the up vector.
        /// </summary>
        public Entity Entity
        {
            get
            {
                return this._Entity;
            }
        }

        /// <summary>
        /// Renders this view to the current graphics context using the given visual data. The projection for the view should be set up before
        /// this call.
        /// </summary>
        public void Render(Visual Visual)
        {
            Visual.Render(this._Entity);
        }

        private Entity _Entity;
    }
}