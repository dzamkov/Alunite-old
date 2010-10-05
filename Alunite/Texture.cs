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
    /// Represents a two-dimensional image loaded into graphics memory.
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
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter,
                (float)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter,
                (float)TextureMagFilter.Linear);

            GL.TexImage2D(TextureTarget.Texture2D,
                0, PixelInternalFormat.Rgba,
                bd.Width, bd.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bd.Scan0);

            Source.UnlockBits(bd);
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
        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, this._TextureID);
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