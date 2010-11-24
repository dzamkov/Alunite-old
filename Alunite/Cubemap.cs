using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Alunite
{
    /// <summary>
    /// Cubemap related functions.
    /// </summary>
    public static class Cubemap
    {
        /// <summary>
        /// Creates a cubemap from a scene.
        /// </summary>
        public static Texture Generate<Renderable>(Texture.Format Format, int Length, Renderable Scene, double MinDistance, double MaxDistance)
            where Renderable : IRenderable
        {
            Texture cubemap = Texture.InitializeCubemap(Length, Format);
            uint fbo;
            GL.GenFramebuffers(1, out fbo);
            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, fbo);
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            Draw<Renderable>(cubemap, FramebufferTarget.FramebufferExt, Length, Scene, MinDistance, MaxDistance);
            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
            GL.DeleteFramebuffers(1, ref fbo);
            return cubemap;
        }

        /// <summary>
        /// Draws a scene to the cubemap texture using the specified framebuffer.
        /// </summary>
        public static void Draw<Renderable>(Texture Cubemap, FramebufferTarget Framebuffer, int Length, Renderable Scene, double MinDistance, double MaxDistance)
            where Renderable : IRenderable
        {
            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI / 2.0), 1.0f, (float)MinDistance, (float)MaxDistance);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref proj);

            Matrix4 view;

            GL.Viewport(0, 0, Length, Length);

            GL.PushMatrix();
            view = Matrix4.LookAt(
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(-1.0f, 0.0f, 0.0f),
                new Vector3(0.0f, -1.0f, 0.0f));
            GL.MultMatrix(ref view);
            GL.FramebufferTexture2D(Framebuffer, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.TextureCubeMapNegativeX, Cubemap.ID, 0);     
            Scene.Render();
            GL.PopMatrix();

            GL.PushMatrix();
            view = Matrix4.LookAt(
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector3(0.0f, -1.0f, 0.0f));
            GL.MultMatrix(ref view);
            GL.FramebufferTexture2D(Framebuffer, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.TextureCubeMapPositiveX, Cubemap.ID, 0);
            Scene.Render();
            GL.PopMatrix();

            GL.PushMatrix();
            view = Matrix4.LookAt(
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(0.0f, -1.0f, 0.0f),
                new Vector3(0.0f, 0.0f, -1.0f));
            GL.MultMatrix(ref view);
            GL.FramebufferTexture2D(Framebuffer, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.TextureCubeMapNegativeY, Cubemap.ID, 0);
            Scene.Render();
            GL.PopMatrix();

            GL.PushMatrix();
            view = Matrix4.LookAt(
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 0.0f),
                new Vector3(0.0f, 0.0f, 1.0f));
            GL.MultMatrix(ref view);
            GL.FramebufferTexture2D(Framebuffer, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.TextureCubeMapPositiveY, Cubemap.ID, 0);
            Scene.Render();
            GL.PopMatrix();

            GL.PushMatrix();
            view = Matrix4.LookAt(
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(0.0f, 0.0f, -1.0f),
                new Vector3(0.0f, -1.0f, 0.0f));
            GL.MultMatrix(ref view);
            GL.FramebufferTexture2D(Framebuffer, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.TextureCubeMapNegativeZ, Cubemap.ID, 0);
            Scene.Render();
            GL.PopMatrix();

            GL.PushMatrix();
            view = Matrix4.LookAt(
                new Vector3(0.0f, 0.0f, 0.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, -1.0f, 0.0f));
            GL.MultMatrix(ref view);
            GL.FramebufferTexture2D(Framebuffer, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.TextureCubeMapPositiveZ, Cubemap.ID, 0);
            Scene.Render();
            GL.PopMatrix();
        }
    }
}