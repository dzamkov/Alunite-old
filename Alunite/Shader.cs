using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
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
        /// Sets a uniform matrix.
        /// </summary>
        public void SetUniform(string Name, ref Matrix4 Matrix)
        {
            GL.UniformMatrix4(GL.GetUniformLocation(this.Program, Name), true, ref Matrix);
        }

        /// <summary>
        /// Sets a uniform texture.
        /// </summary>
        public void SetUniform(string Name, TextureUnit Unit)
        {
            GL.Uniform1(GL.GetUniformLocation(this.Program, Name), (int)Unit);
        }

        /// <summary>
        /// Runs the fragment shader on all pixels on the current viewport.
        /// </summary>
        public void DrawFull()
        {
            this.Call();
            DrawQuad();
        }

        /// <summary>
        /// Sets the rendering mode to default (removing all shaders).
        /// </summary>
        public static void Dismiss()
        {
            GL.UseProgram(0);
        }

        /// <summary>
        /// Draws a shape that includes the entire viewport.
        /// </summary>
        public static void DrawQuad()
        {
            GL.Begin(BeginMode.TriangleStrip);
            GL.Vertex2(-1.0, -1.0);
            GL.Vertex2(+1.0, -1.0);
            GL.Vertex2(-1.0, +1.0);
            GL.Vertex2(+1.0, +1.0);
            GL.End();
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
        /// Loads a shader from a single, defining _VERTEX_ for vertex shaders and _FRAGMENT_ for fragment shaders.
        /// </summary>
        public static Shader Load(Alunite.Path File)
        {
            return Load(new Alunite.Path[] { File }, new Dictionary<string, string>());
        }

        /// <summary>
        /// More advanced shader loading function that will dynamically replace constants in the specified files.
        /// </summary>
        public static Shader Load(IEnumerable<Alunite.Path> Files, Dictionary<string, string> Constants)
        {
            int vshade = GL.CreateShader(ShaderType.VertexShader);
            int fshade = GL.CreateShader(ShaderType.FragmentShader);

            StringBuilder vshadesource = new StringBuilder();
            StringBuilder fshadesource = new StringBuilder();
            List<string> fullsourcelines = new List<string>();
            foreach (Alunite.Path Path in Files)
            {
                string[] lines = File.ReadAllLines(Path.PathString);
                foreach (string line in lines)
                {
                    fullsourcelines.Add(line);
                }
            }
            Dictionary<string, string> vshadeconsts = new Dictionary<string, string>(Constants);
            Dictionary<string, string> fshadeconsts = new Dictionary<string, string>(Constants);
            vshadeconsts.Add("_VERTEX_", "1");
            fshadeconsts.Add("_FRAGMENT_", "1");

            BuildSource(fullsourcelines, vshadeconsts, vshadesource);
            GL.ShaderSource(vshade, vshadesource.ToString());
            GL.CompileShader(vshade);
            vshadesource = null;

            BuildSource(fullsourcelines, fshadeconsts, fshadesource);
            GL.ShaderSource(fshade, fshadesource.ToString());
            GL.CompileShader(fshade);
            fshadesource = null;

            Shader shade = new Shader();
            shade.Program = GL.CreateProgram();
            GL.AttachShader(shade.Program, vshade);
            GL.AttachShader(shade.Program, fshade);
            GL.LinkProgram(shade.Program);
            return shade;
        }


        /// <summary>
        /// Precompiles the source code defined by the lines with the specified constants defined. Outputs the precompiled source
        /// to the given stringbuilder.
        /// </summary>
        public static void BuildSource(IEnumerable<string> Lines, Dictionary<string, string> Constants, StringBuilder Output)
        {
            _ProcessBlock(Lines.GetEnumerator(), Constants, Output, true);
        }

        /// <summary>
        /// Processes an ifdef/else/endif block where Interpret denotes the success of the if statement. Returns true if exited on an endif or false
        /// if exited on an else.
        /// </summary>
        private static bool _ProcessBlock(IEnumerator<string> LineEnumerator, Dictionary<string, string> Constants, StringBuilder Output, bool Interpret)
        {
            while (LineEnumerator.MoveNext())
            {
                string line = LineEnumerator.Current;

                // Does this line contain a directive?
                if (line.Length > 0)
                {
                    if (line[0] == '#')
                    {
                        string[] lineparts = line.Split(' ');
                        if (lineparts[0] == "#ifdef")
                        {
                            if (Interpret)
                            {
                                bool contains = Constants.ContainsKey(lineparts[1]);
                                if (!_ProcessBlock(LineEnumerator, Constants, Output, contains))
                                {
                                    _ProcessBlock(LineEnumerator, Constants, Output, !contains);
                                }
                            }
                            else
                            {
                                if (!_ProcessBlock(LineEnumerator, Constants, Output, false))
                                {
                                    _ProcessBlock(LineEnumerator, Constants, Output, false);
                                }
                            }
                        }
                        if (lineparts[0] == "#else")
                        {
                            return false;
                        }
                        if (lineparts[0] == "#endif")
                        {
                            return true;
                        }
                        if (Interpret)
                        {
                            if (lineparts[0] == "#define")
                            {
                                if (lineparts.Length > 2)
                                {
                                    Constants[lineparts[1]] = lineparts[2];
                                }
                                else
                                {
                                    Constants[lineparts[1]] = "1";
                                }
                            }
                            if (lineparts[0] == "#undef")
                            {
                                Constants.Remove(lineparts[1]);
                            }
                        }
                    }
                    else
                    {
                        if (Interpret)
                        {
                            // Replace constants
                            foreach (KeyValuePair<string, string> constant in Constants)
                            {
                                line = line.Replace(constant.Key, constant.Value);
                            }
                            Output.AppendLine(line);
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Index of the program of the shader.
        /// </summary>
        public int Program;
    }
}