using System.Collections.Generic;
using System;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Alunite
{
    /// <summary>
    /// Represents a shader in graphics memory.
    /// </summary>
    public struct Shader
    {

        /// <summary>
        /// Sets the shader to be used for subsequent render operations.
        /// </summary>
        public void Call()
        {
            GL.UseProgram(this.Program);
        }

        /// <summary>
        /// Sets a uniform variable.
        /// </summary>
        public void SetUniform(string Name, Vector Value)
        {
            GL.Uniform3(GL.GetUniformLocation(this.Program, Name), (Vector3)Value);
        }

        /// <summary>
        /// Sets the rendering mode to default (removing all shaders).
        /// </summary>
        public static void Dismiss()
        {
            GL.UseProgram(0);
        }

        /// <summary>
        /// Loads a shader program from a vertex and fragment shader in GLSL format.
        /// </summary>
        public static Shader Load(Alunite.Path Vertex, Alunite.Path Fragment)
        {
            Shader shade = new Shader();
            int vshade = GL.CreateShader(ShaderType.VertexShader);
            int fshade = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(vshade, Path.ReadText(Vertex));
            GL.ShaderSource(fshade, Path.ReadText(Fragment));
            GL.CompileShader(vshade);
            GL.CompileShader(fshade);
            shade.Program = GL.CreateProgram();
            GL.AttachShader(shade.Program, vshade);
            GL.AttachShader(shade.Program, fshade);
            GL.LinkProgram(shade.Program);
            return shade;
        }

        /// <summary>
        /// Index of the program of the shader.
        /// </summary>
        public int Program;
    }
}