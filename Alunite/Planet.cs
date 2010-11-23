﻿using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Alunite
{
    /// <summary>
    /// Really big thing.
    /// </summary>
    public class Planet
    {
        public Planet()
        {
            this._Triangles = new HashSet<Triangle<int>>();
            this._SegmentTriangles = new Dictionary<Segment<int>, Triangle<int>>();

            // Initialize with an icosahedron.
            Primitive icosa = Primitive.Icosahedron;
            this._Vertices = new List<Vector>(icosa.Vertices);
            foreach (Triangle<int> tri in icosa.Triangles)
            {
                this._AddTriangle(tri);   
            }

            this._Subdivide();
            this._Subdivide();
            this._Subdivide();

        }

        /// <summary>
        /// Creates a diagram for the current state of the planet.
        /// </summary>
        public Diagram CreateDiagram()
        {
            Diagram dia = new Diagram(this._Vertices);
            foreach (Triangle<int> tri in this._Triangles)
            {
                dia.SetBorderedTriangle(tri, Color.RGB(0.0, 0.2, 1.0), Color.RGB(0.3, 1.0, 0.3), 4.0);
            }
            return dia;
        }

        /// <summary>
        /// Creates a vertex buffer representation of the current state of the planet.
        /// </summary>
        public VBO<NormalVertex, NormalVertex.Model> CreateVBO()
        {
            NormalVertex[] verts = new NormalVertex[this._Vertices.Count];
            for (int t = 0; t < verts.Length; t++)
            {
                // Lol, position and normal are the same.
                Vector pos = this._Vertices[t];
                verts[t].Position = pos;
                verts[t].Normal = pos;
            }
            return new VBO<NormalVertex, NormalVertex.Model>(
                NormalVertex.Model.Singleton,
                verts, verts.Length,
                this._Triangles, this._Triangles.Count);
        }

        public void Load(Alunite.Path ShaderPath)
        {
            Shader.PrecompilerInput pci = _DefineCommon();



            Path atmosphere = ShaderPath["Atmosphere"];
            Path precompute = atmosphere["Precompute"];
            this._PlanetShader = Shader.Load(atmosphere["Planet.glsl"], pci.Copy());

            // Atmospheric scattering precompution
            uint fbo;
            GL.GenFramebuffers(1, out fbo);
            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, fbo);
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            // Load shaders.
            Shader.PrecompilerInput transmittancepci = pci.Copy();
            Shader transmittance = Shader.Load(precompute["Transmittance.glsl"], transmittancepci);

            Shader.PrecompilerInput irradianceinitialdeltapci = pci.Copy();
            irradianceinitialdeltapci.Define("INITIAL");
            irradianceinitialdeltapci.Define("DELTA");
            Shader irradianceinitialdelta = Shader.Load(precompute["Irradiance.glsl"], irradianceinitialdeltapci);

            Shader.PrecompilerInput irradianceinitialpci = pci.Copy();
            irradianceinitialpci.Define("INITIAL");
            Shader irradianceinitial = Shader.Load(precompute["Irradiance.glsl"], irradianceinitialpci);

            Shader.PrecompilerInput inscatterinitialdeltapci = pci.Copy();
            inscatterinitialdeltapci.Define("INITIAL");
            inscatterinitialdeltapci.Define("DELTA");
            Shader inscatterinitialdelta = Shader.Load(precompute["Inscatter.glsl"], inscatterinitialdeltapci);

            Shader.PrecompilerInput inscatterinitialpci = pci.Copy();
            inscatterinitialpci.Define("INITIAL");
            Shader inscatterinitial = Shader.Load(precompute["Inscatter.glsl"], inscatterinitialpci);

            Shader.PrecompilerInput pointscatterpci = pci.Copy();
            Shader pointscatter = Shader.Load(precompute["PointScatter.glsl"], pointscatterpci);

            // Initialize textures
            this._TransmittanceTexture = Texture.Initialize2D(TransmittanceResMu, TransmittanceResR, Texture.RGB16Float);
            this._IrradianceTexture = Texture.Initialize2D(IrradianceResMu, IrradianceResR, Texture.RGB16Float);
            this._InscatterTexture = Texture.Initialize3D(AtmosphereResMuS * AtmosphereResNu, AtmosphereResMu, AtmosphereResR, Texture.RGBA16Float);
            Texture irrdelta = Texture.Initialize2D(IrradianceResMu, IrradianceResR, Texture.RGB16Float);
            Texture insdelta = Texture.Initialize3D(AtmosphereResMuS * AtmosphereResNu, AtmosphereResMu, AtmosphereResR, Texture.RGB16Float);
            Texture ptsdelta = Texture.Initialize3D(AtmosphereResMuS * AtmosphereResNu, AtmosphereResMu, AtmosphereResR, Texture.RGB16Float);

            this._TransmittanceTexture.SetUnit(TextureTarget.Texture2D, TextureUnit.Texture0);
            this._InscatterTexture.SetUnit(TextureTarget.Texture3D, TextureUnit.Texture2);
            irrdelta.SetUnit(TextureTarget.Texture2D, TextureUnit.Texture3);
            insdelta.SetUnit(TextureTarget.Texture3D, TextureUnit.Texture4);
            ptsdelta.SetUnit(TextureTarget.Texture3D, TextureUnit.Texture5);

            // Create transmittance texture (information about how light is filtered through the atmosphere).
            transmittance.Call();
            transmittance.Draw2DFrame(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, 
                this._TransmittanceTexture.ID, TransmittanceResMu, TransmittanceResR);

            // Create delta irradiance texture (ground lighting cause by sun).
            irradianceinitialdelta.Call();
            irradianceinitialdelta.SetUniform("Transmittance", TextureUnit.Texture0);
            irradianceinitialdelta.Draw2DFrame(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext,
                irrdelta.ID, IrradianceResMu, IrradianceResR);
 
            // Create initial inscatter texture (light from atmosphere from sun, rayleigh and mie parts seperated for precision).
            inscatterinitial.Call();
            inscatterinitial.SetUniform("Transmittance", TextureUnit.Texture0);
            inscatterinitial.Draw3DFrame(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext,
                this._InscatterTexture.ID, AtmosphereResMuS * AtmosphereResNu, AtmosphereResMu, AtmosphereResR);

            // Initialize irradiance to zero
            irradianceinitial.Call();
            irradianceinitial.Draw2DFrame(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext,
                this._IrradianceTexture.ID, IrradianceResMu, IrradianceResR);

            // Copy inscatter to delta inscatter, combining rayleigh and mie parts.
            inscatterinitialdelta.Call();
            inscatterinitialdelta.SetUniform("Inscatter", TextureUnit.Texture2);
            inscatterinitialdelta.Draw3DFrame(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext,
                insdelta.ID, AtmosphereResMuS * AtmosphereResNu, AtmosphereResMu, AtmosphereResR);

            //for (int t = 2; t <= MultipleScatterOrder; t++)
            //{
                // Generate point scattering information
                pointscatter.Call();
                pointscatter.SetUniform("IrradianceDelta", TextureUnit.Texture3);
                pointscatter.SetUniform("InscatterDelta", TextureUnit.Texture4);
                pointscatter.Draw3DFrame(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext,
                    ptsdelta.ID, AtmosphereResMuS * AtmosphereResNu, AtmosphereResMu, AtmosphereResR);
            //}

            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
        }

        private const int MultipleScatterOrder = 5;
        private const int AtmosphereResR = 32;
        private const int AtmosphereResMu = 256;
        private const int AtmosphereResMuS = 32;
        private const int AtmosphereResNu = 8;
        private const int IrradianceResR = 16;
        private const int IrradianceResMu = 64;
        private const int TransmittanceResR = 64;
        private const int TransmittanceResMu = 256;

        /// <summary>
        /// Creates a shader precompiler input with common atmosphere shader paramters from the specified source
        /// input.
        /// </summary>
        private static Shader.PrecompilerInput _DefineCommon()
        {
            Shader.PrecompilerInput res = Shader.CreatePrecompilerInput();
            res.Define("ATMOSPHERE_RES_R", AtmosphereResR.ToString());
            res.Define("ATMOSPHERE_RES_MU", AtmosphereResMu.ToString());
            res.Define("ATMOSPHERE_RES_MU_S", AtmosphereResMuS.ToString());
            res.Define("ATMOSPHERE_RES_NU", AtmosphereResNu.ToString());
            res.Define("IRRADIANCE_RES_R", IrradianceResR.ToString());
            res.Define("IRRADIANCE_RES_MU", IrradianceResMu.ToString());
            res.Define("TRANSMITTANCE_RES_R", TransmittanceResR.ToString());
            res.Define("TRANSMITTANCE_RES_MU", TransmittanceResMu.ToString());
            return res;
        }

        public void Render(Matrix4 Proj, Matrix4 View, Vector EyePosition, Vector SunDirection)
        {
            Proj.Invert();
            //View.Invert();
            GL.LoadIdentity();

            this._TransmittanceTexture.SetUnit(TextureTarget.Texture2D, TextureUnit.Texture0);
            this._InscatterTexture.SetUnit(TextureTarget.Texture3D, TextureUnit.Texture1);

            this._PlanetShader.SetUniform("Inscatter", TextureUnit.Texture4);
            this._PlanetShader.SetUniform("Transmittance", TextureUnit.Texture0);
            this._PlanetShader.SetUniform("ProjInverse", ref Proj);
            this._PlanetShader.SetUniform("ViewInverse", ref View);
            this._PlanetShader.SetUniform("EyePosition", EyePosition);
            this._PlanetShader.SetUniform("SunDirection", SunDirection);
            this._PlanetShader.DrawFull();
        }

        /// <summary>
        /// Adds a triangle to the planet.
        /// </summary>
        private void _AddTriangle(Triangle<int> Triangle)
        {
            this._Triangles.Add(Triangle);
            foreach (Segment<int> seg in Triangle.Segments)
            {
                this._SegmentTriangles[seg] = Triangle;
            }
        }

        /// <summary>
        /// Dereferences a triangle in the primitive.
        /// </summary>
        public Triangle<Vector> Dereference(Triangle<int> Triangle)
        {
            return new Triangle<Vector>(
                this._Vertices[Triangle.A],
                this._Vertices[Triangle.B],
                this._Vertices[Triangle.C]);
        }

        /// <summary>
        /// Splits the triangle at the specified point while maintaining the delaunay property.
        /// </summary>
        private void _SplitTriangle(Triangle<int> Triangle, Vector Point)
        {
            int npoint = this._Vertices.Count;
            this._Vertices.Add(Point);

            this._Triangles.Remove(Triangle);

            // Maintain delaunay property by flipping encroached triangles.
            List<Segment<int>> finalsegs = new List<Segment<int>>();
            Stack<Segment<int>> possiblealtersegs = new Stack<Segment<int>>();
            possiblealtersegs.Push(new Segment<int>(Triangle.A, Triangle.B));
            possiblealtersegs.Push(new Segment<int>(Triangle.B, Triangle.C));
            possiblealtersegs.Push(new Segment<int>(Triangle.C, Triangle.A));

            while (possiblealtersegs.Count > 0)
            {
                Segment<int> seg = possiblealtersegs.Pop();

                Triangle<int> othertri = Alunite.Triangle.Align(this._SegmentTriangles[seg.Flip], seg.Flip).Value;
                int otherpoint = othertri.Vertex;
                Triangle<Vector> othervectri = this.Dereference(othertri);
                Vector othercircumcenter = Alunite.Triangle.Normal(othervectri);
                double othercircumangle = Vector.Dot(othercircumcenter, othervectri.A);

                // Check if triangle encroachs the new point
                double npointangle = Vector.Dot(othercircumcenter, Point);
                if (npointangle > othercircumangle)
                {
                    this._Triangles.Remove(othertri);
                    possiblealtersegs.Push(new Segment<int>(othertri.A, othertri.B));
                    possiblealtersegs.Push(new Segment<int>(othertri.C, othertri.A));
                }
                else
                {
                    finalsegs.Add(seg);
                }
            }

            foreach (Segment<int> seg in finalsegs)
            {
                this._AddTriangle(new Triangle<int>(npoint, seg));
            }
        }

        /// <summary>
        /// Splits a triangle at its circumcenter while maintaining the delaunay property.
        /// </summary>
        private void _SplitTriangle(Triangle<int> Triangle)
        {
            Triangle<Vector> vectri = this.Dereference(Triangle);
            Vector circumcenter = Alunite.Triangle.Normal(vectri);
            this._SplitTriangle(Triangle, circumcenter);
        }

        /// <summary>
        /// Subdivides the entire planet, quadrupling the amount of triangles.
        /// </summary>
        private void _Subdivide()
        {
            var oldtris = this._Triangles;
            var oldsegs = this._SegmentTriangles;
            this._Triangles = new HashSet<Triangle<int>>();
            this._SegmentTriangles = new Dictionary<Segment<int>, Triangle<int>>();

            Dictionary<Segment<int>, int> newsegs = new Dictionary<Segment<int>, int>();

            foreach (Triangle<int> tri in oldtris)
            {
                int[] midpoints = new int[3];
                Segment<int>[] segs = tri.Segments;
                for (int t = 0; t < 3; t++)
                {
                    Segment<int> seg = segs[t];
                    int midpoint;
                    if (!newsegs.TryGetValue(seg, out midpoint))
                    {
                        midpoint = this._Vertices.Count;
                        this._Vertices.Add(
                            Vector.Normalize(
                                Segment.Midpoint(
                                    new Segment<Vector>(
                                        this._Vertices[seg.A],
                                        this._Vertices[seg.B]))));
                        newsegs.Add(seg.Flip, midpoint);
                    }
                    else
                    {
                        newsegs.Remove(seg);
                    }
                    midpoints[t] = midpoint;
                }
                this._AddTriangle(new Triangle<int>(tri.A, midpoints[0], midpoints[2]));
                this._AddTriangle(new Triangle<int>(tri.B, midpoints[1], midpoints[0]));
                this._AddTriangle(new Triangle<int>(tri.C, midpoints[2], midpoints[1]));
                this._AddTriangle(new Triangle<int>(midpoints[0], midpoints[1], midpoints[2]));
            }
        }

        private Texture _InscatterTexture;
        private Texture _TransmittanceTexture;
        private Texture _IrradianceTexture;
        private Shader _PlanetShader;

        private List<Vector> _Vertices;
        private HashSet<Triangle<int>> _Triangles;
        private Dictionary<Segment<int>, Triangle<int>> _SegmentTriangles;
    }
}