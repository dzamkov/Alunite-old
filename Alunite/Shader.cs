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
            return Load(File, new Dictionary<string, string>());
        }

        /// <summary>
        /// More advanced shader loading function that will dynamically replace constants in the specified files.
        /// </summary>
        public static Shader Load(Alunite.Path File, Dictionary<string, string> Constants)
        {
            return Load(File, new PrecompilerInput()
            {
                Constants = Constants,
                LoadedFiles = new Dictionary<string, string[]>()
            });
        }

        /// <summary>
        /// Loads a shader from the specified file using the specified input.
        /// </summary>
        public static Shader Load(Alunite.Path File, PrecompilerInput Input)
        {
            int vshade = GL.CreateShader(ShaderType.VertexShader);
            int fshade = GL.CreateShader(ShaderType.FragmentShader);

            StringBuilder vshadesource = new StringBuilder();
            StringBuilder fshadesource = new StringBuilder();
            PrecompilerInput vpce = Input.Copy();
            PrecompilerInput fpce = Input.Copy();
            vpce.Define("_VERTEX_", "1");
            fpce.Define("_FRAGMENT_", "1");

            BuildSource(File, vpce, vshadesource);
            GL.ShaderSource(vshade, vshadesource.ToString());
            GL.CompileShader(vshade);
            vshadesource = null;

            BuildSource(File, fpce, fshadesource);
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
        /// Creates a new precompiler input set.
        /// </summary>
        public static PrecompilerInput CreatePrecompilerInput()
        {
            return new PrecompilerInput()
            {
                Constants = new Dictionary<string, string>(),
                LoadedFiles = new Dictionary<string, string[]>()
            };
        }

        /// <summary>
        /// Input to the precompiler.
        /// </summary>
        public struct PrecompilerInput
        {
            public PrecompilerInput(Path File)
            {
                this.Constants = new Dictionary<string, string>();
                this.LoadedFiles = new Dictionary<string, string[]>();
            }

            /// <summary>
            /// Gets the file at the specified path.
            /// </summary>
            public string[] GetFile(Path Path)
            {
                string[] lines;
                if (!this.LoadedFiles.TryGetValue(Path.PathString, out lines))
                {
                    this.LoadedFiles[Path.PathString] = lines = File.ReadAllLines(Path.PathString);
                }
                return lines;
            }

            /// <summary>
            /// Creates a copy of this precompiler input with its own precompiler constants.
            /// </summary>
            public PrecompilerInput Copy()
            {
                return new PrecompilerInput()
                {
                    LoadedFiles = this.LoadedFiles,
                    Constants = new Dictionary<string, string>(this.Constants)
                };
            }

            /// <summary>
            /// Defines a constant.
            /// </summary>
            public void Define(string Constant, string Value)
            {
                this.Constants[Constant] = Value;
            }

            /// <summary>
            /// Undefines a constant.
            /// </summary>
            public void Undefine(string Constant)
            {
                this.Constants.Remove(Constant);
            }

            /// <summary>
            /// Constants defined for the precompiler.
            /// </summary>
            public Dictionary<string, string> Constants;

            /// <summary>
            /// The files loaded for the precompiler.
            /// </summary>
            public Dictionary<string, string[]> LoadedFiles;
        }

        /// <summary>
        /// Precompiles the source code defined by the lines with the specified constants defined. Outputs the precompiled source
        /// to the given stringbuilder.
        /// </summary>
        public static void BuildSource(Path File, PrecompilerInput Input, StringBuilder Output)
        {
            string[] lines = Input.GetFile(File);
            _ProcessBlock(((IEnumerable<string>)lines).GetEnumerator(), File, Input, Output, true);
        }

        /// <summary>
        /// Processes an ifdef/else/endif block where Interpret denotes the success of the if statement. Returns true if exited on an endif or false
        /// if exited on an else.
        /// </summary>
        private static bool _ProcessBlock(IEnumerator<string> LineEnumerator, Path File, PrecompilerInput Input, StringBuilder Output, bool Interpret)
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
                                bool contains = Input.Constants.ContainsKey(lineparts[1]);
                                if (!_ProcessBlock(LineEnumerator, File, Input, Output, contains))
                                {
                                    _ProcessBlock(LineEnumerator, File, Input, Output, !contains);
                                }
                            }
                            else
                            {
                                if (!_ProcessBlock(LineEnumerator, File, Input, Output, false))
                                {
                                    _ProcessBlock(LineEnumerator, File, Input, Output, false);
                                }
                            }
                        }
                        if (lineparts[0] == "#include")
                        {
                            if (Interpret)
                            {
                                string filepath = lineparts[1].Substring(1, lineparts[1].Length - 2);
                                BuildSource(File.Parent.Lookup(filepath), Input, Output);
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
                                    Input.Define(lineparts[1], lineparts[2]);
                                }
                                else
                                {
                                    Input.Define(lineparts[1], "1");
                                }
                            }
                            if (lineparts[0] == "#undef")
                            {
                                Input.Undefine(lineparts[1]);
                            }
                        }
                    }
                    else
                    {
                        if (Interpret)
                        {
                            // Replace constants
                            foreach (KeyValuePair<string, string> constant in Input.Constants)
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