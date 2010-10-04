using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Alunite
{
    /// <summary>
    /// An interface for a single vertex.
    /// </summary>
    public interface IVertex
    {

    }

    /// <summary>
    /// Describes how to write and use vertices of a certain type. Vertices must be stored as individual units if they
    /// are used by a vertex model.
    /// </summary>
    public interface IVertexModel<V>
        where V : struct, IVertex
    {
        /// <summary>
        /// Writes the specified vertex to the buffer.
        /// </summary>
        unsafe void Write(V Vertex, void* Buffer);

        /// <summary>
        /// Gets the size of each vertex in bytes.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Kindly informs opengl how to use the vertices before any draw calls.
        /// </summary>
        void Initialize();
    }

    /// <summary>
    /// A vertex with a color, position and normal.
    /// </summary>
    public struct ColorNormalVertex : IVertex
    {
        public ColorNormalVertex(Color Color, Vector Position, Vector Normal)
        {
            this.Color = Color;
            this.Position = Position;
            this.Normal = Normal;
        }

        public Color Color;
        public Vector Position;
        public Vector Normal;

        /// <summary>
        /// Vertex model for ColorNormalVertex
        /// </summary>
        public class Model : IVertexModel<ColorNormalVertex>
        {
            private Model()
            {

            }

            public unsafe void Write(ColorNormalVertex Vertex, void* Buffer)
            {
                float* m = (float*)Buffer;
                m[0] = (float)Vertex.Color.R;
                m[1] = (float)Vertex.Color.G;
                m[2] = (float)Vertex.Color.B;
                m[3] = (float)Vertex.Color.A;
                m[4] = (float)Vertex.Normal.X;
                m[5] = (float)Vertex.Normal.Y;
                m[6] = (float)Vertex.Normal.Z;
                m[7] = (float)Vertex.Position.X;
                m[8] = (float)Vertex.Position.Y;
                m[9] = (float)Vertex.Position.Z;
            }

            public int Size
            {
                get 
                {
                    return sizeof(float) * 10;
                }
            }

            public void Initialize()
            {
                GL.InterleavedArrays(InterleavedArrayFormat.C4fN3fV3f, 0, IntPtr.Zero);
            }

            public static readonly Model Singleton = new Model();
        }
    }

    /// <summary>
    /// Represents an array/element vertex buffer stored on the graphics device. Vertices must be
    /// completely self-contained to be used in this VBO.
    /// </summary>
    public class VBO<V, M>
        where V : struct, IVertex
        where M : IVertexModel<V>
    {
        public VBO(M Model, ISequentialArray<V> Source)
        {
            this._Model = Model;
            this._Count = Source.Count;
            this._ArrayBuffer = _WriteArrayBuffer(Model, Source);
        }

        public VBO(M Model, ISequentialArray<V> VerticeSource, ISequentialArray<int> IndiceSource)
        {
            this._Model = Model;
            this._Count = IndiceSource.Count;
            this._ArrayBuffer = _WriteArrayBuffer(Model, VerticeSource);
            this._ElementArrayBuffer = _WriteElementArrayBuffer(IndiceSource);
        }

        private static unsafe uint _WriteArrayBuffer(M Model, ISequentialArray<V> Source)
        {
            uint ab;
            int vertsize = Model.Size;
            GL.GenBuffers(1, out ab);
            GL.BindBuffer(BufferTarget.ArrayBuffer, ab);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Source.Count * vertsize), IntPtr.Zero, BufferUsageHint.StaticDraw);
            byte* buffer = (byte*)GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.WriteOnly).ToPointer();
            foreach (KeyValuePair<int, V> kvp in Source.Items)
            {
                Model.Write(kvp.Value, (void*)(buffer + (kvp.Key * vertsize)));
            }
            GL.UnmapBuffer(BufferTarget.ArrayBuffer);
            return ab;
        }

        private static unsafe uint _WriteElementArrayBuffer(ISequentialArray<int> Source)
        {
            uint eab;
            GL.GenBuffers(1, out eab);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eab);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(Source.Count * sizeof(uint)), IntPtr.Zero, BufferUsageHint.StaticDraw);
            uint* buffer = (uint*)GL.MapBuffer(BufferTarget.ElementArrayBuffer, BufferAccess.WriteOnly).ToPointer();
            foreach (KeyValuePair<int, int> kvp in Source.Items)
            {
                buffer[kvp.Key] = (uint)kvp.Value;
            }
            GL.UnmapBuffer(BufferTarget.ElementArrayBuffer);
            return eab;
        }

        /// <summary>
        /// Renders the contents of the VBO with the specified render mode.
        /// </summary>
        public void Render(BeginMode Mode)
        {
            this._Model.Initialize();
            if (this._ElementArrayBuffer > 0)
            {
                GL.DrawElements(Mode, this._Count, DrawElementsType.UnsignedInt, 0);
            }
            else
            {
                GL.DrawArrays(Mode, 0, this._Count);
            }
        }

        private M _Model;
        private int _Count;
        private uint _ArrayBuffer;
        private uint _ElementArrayBuffer;
    }
}