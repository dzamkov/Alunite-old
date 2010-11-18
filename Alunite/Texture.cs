using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Path = Alunite.Path;

namespace Alunite
{

    /// <summary>
    /// Represents a textured(of any dimension) loaded into graphics memory.
    /// </summary>
    public class Texture
    {
        public Texture(Bitmap Source)
        {
            GL.GenBuffers(1, out this._TextureID);
            GL.BindTexture(TextureTarget.Texture2D, this._TextureID);

            BitmapData bd = Source.LockBits(
                new Rectangle(0, 0, Source.Width, Source.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexEnv(TextureEnvTarget.TextureEnv,
                TextureEnvParameter.TextureEnvMode,
                (float)TextureEnvMode.Modulate);

            GL.TexImage2D(TextureTarget.Texture2D,
                0, PixelInternalFormat.Rgba,
                bd.Width, bd.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bd.Scan0);

            this.SetInterpolation2D(TextureMinFilter.Linear, TextureMagFilter.Linear);
            this.SetWrap2D(TextureWrapMode.Repeat, TextureWrapMode.Repeat);

            Source.UnlockBits(bd);
        }

        public Texture(int TextureID)
        {
            this._TextureID = (uint)TextureID;
        }

        /// <summary>
        /// Gets the OpenGL id for the texture.
        /// </summary>
        public uint ID
        {
            get
            {
                return this._TextureID;
            }
        }

        /// <summary>
        /// Sets this as the current texture.
        /// </summary>
        public void Bind(TextureTarget TextureTarget)
        {
            GL.BindTexture(TextureTarget, this._TextureID);
        }

        /// <summary>
        /// Sets this as the current 2d texture.
        /// </summary>
        public void Bind2D()
        {
            this.Bind(TextureTarget.Texture2D);
        }

        /// <summary>
        /// Describes the format of a texture.
        /// </summary>
        public struct Format
        {
            public Format(
                PixelInternalFormat PixelInternalFormat, 
                OpenTK.Graphics.OpenGL.PixelFormat PixelFormat, 
                PixelType PixelType)
            {
                this.PixelInternalFormat = PixelInternalFormat;
                this.PixelFormat = PixelFormat;
                this.PixelType = PixelType;
            }

            public PixelInternalFormat PixelInternalFormat;
            public OpenTK.Graphics.OpenGL.PixelFormat PixelFormat;
            public PixelType PixelType;
        }

        public static readonly Format RGB16Float = new Format(
            PixelInternalFormat.Rgb16f, 
            OpenTK.Graphics.OpenGL.PixelFormat.Rgb, 
            PixelType.Float);

        public static readonly Format RGBA16Float = new Format(
            PixelInternalFormat.Rgba16f,
            OpenTK.Graphics.OpenGL.PixelFormat.Rgb,
            PixelType.Float);

        /// <summary>
        /// Creates a generic 2d texture with no data.
        /// </summary>
        public static Texture Initialize2D(int Width, int Height, Format Format)
        {
            int tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexImage2D(TextureTarget.Texture2D, 0, Format.PixelInternalFormat, Width, Height, 0, Format.PixelFormat, Format.PixelType, IntPtr.Zero);
            return new Texture(tex);
        }

        /// <summary>
        /// Creates a generic 3d texture with no data.
        /// </summary>
        public static Texture Initialize3D(int Width, int Height, int Depth, Format Format)
        {
            int tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture3D, tex);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
            GL.TexImage3D(TextureTarget.Texture3D, 0, Format.PixelInternalFormat, Width, Height, Depth, 0, Format.PixelFormat, Format.PixelType, IntPtr.Zero);
            return new Texture(tex);
        }

        /// <summary>
        /// Sets the interpolation used by the texture.
        /// </summary>
        public void SetInterpolation2D(TextureMinFilter Min, TextureMagFilter Mag)
        {
            this.Bind2D();
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)Min);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)Mag);
        }

        /// <summary>
        /// Sets this texture to a texture unit for uses such as shaders.
        /// </summary>
        public void SetUnit(TextureTarget Target, TextureUnit Unit)
        {
            GL.ActiveTexture(Unit);
            this.Bind(Target);
        }

        /// <summary>
        /// Sets the type of wrapping used by the texture.
        /// </summary>
        public void SetWrap2D(TextureWrapMode S, TextureWrapMode T)
        {
            this.Bind2D();
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)S);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)T); 
        }

        /// <summary>
        /// Loads a texture from the specified file.
        /// </summary>
        public static Texture Load(Path File)
        {
            using (FileStream fs = System.IO.File.OpenRead(File))
            {
                return Load(fs);
            }
        }

        /// <summary>
        /// Loads a texture from the specified stream.
        /// </summary>
        public static Texture Load(Stream Stream)
        {
            return new Texture(new Bitmap(Stream));
        }

        private uint _TextureID;
    }

}