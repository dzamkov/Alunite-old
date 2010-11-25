using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Alunite
{
    /// <summary>
    /// Arguments defining a spherical atmosphere.
    /// </summary>
    public struct AtmosphereOptions
    {
        /// <summary>
        /// Distance from the center of the planet to ground level.
        /// </summary>
        public float RadiusGround;

        /// <summary>
        /// Distance from the center of the planet to the highest point where atmosphere is computed.
        /// </summary>
        public float RadiusBound;

        /// <summary>
        /// A proportion between 0.0 and 1.0 indicating how much light is reflected from the ground.
        /// </summary>
        public float AverageGroundReflectance;

        /// <summary>
        /// Average height above ground level of all rayleigh particles (fine, distort color).
        /// </summary>
        public float RayleighAverageHeight;

        /// <summary>
        /// Average height above ground level of all mie particles (coarse, white, absorb light).
        /// </summary>
        public float MieAverageHeight;

        /// <summary>
        /// Atmosphere options for an Earthlike planet.
        /// </summary>
        public static readonly AtmosphereOptions DefaultEarth = new AtmosphereOptions()
            {
                AverageGroundReflectance = 0.1f,
                RadiusGround = 6360.0f,
                RadiusBound = 6420.0f,
                RayleighAverageHeight = 8.0f,
                MieAverageHeight = 1.2f,
            };
    }

    /// <summary>
    /// Arguments specifing the quality an atmosphere should be generated with.
    /// </summary>
    public struct AtmosphereQualityOptions
    {
        /// <summary>
        /// Amount of scattering orders to compute. This increases time to generate textures and atmosphere quality without
        /// requiring more runtime resources.
        /// </summary>
        public int MultipleScatteringOrder;

        /// <summary>
        /// Resolution in height of textures involving the atmosphere.
        /// </summary>
        public int AtmosphereResR;

        /// <summary>
        /// Resolution in view angle of textures involving the atmosphere.
        /// </summary>
        public int AtmosphereResMu;

        /// <summary>
        /// Resolution in sun angle of textures involving the atmosphere.
        /// </summary>
        public int AtmosphereResMuS;

        /// <summary>
        /// Resolution in sun/view angle offset of textures involving the atmosphere.
        /// </summary>
        public int AtmosphereResNu;

        /// <summary>
        /// Resolution of sun angle in the irradiance texture.
        /// </summary>
        public int IrradianceResMu;

        /// <summary>
        /// Resolution of height in the irradiance texture.
        /// </summary>
        public int IrradianceResR;

        /// <summary>
        /// Resolution of view angle in the transmittance texture.
        /// </summary>
        public int TransmittanceResMu;

        /// <summary>
        /// Resolution of height in the transmittance texture.
        /// </summary>
        public int TransmittanceResR;

        /// <summary>
        /// Some okay options that generate reasonable results.
        /// </summary>
        public static readonly AtmosphereQualityOptions Default = new AtmosphereQualityOptions()
            {
                MultipleScatteringOrder = 2,
                AtmosphereResMu = 128,
                AtmosphereResR = 32,
                AtmosphereResMuS = 32,
                AtmosphereResNu = 8,
                IrradianceResMu = 64,
                IrradianceResR = 16,
                TransmittanceResR = 64,
                TransmittanceResMu = 256,
            };
    }

    /// <summary>
    /// Information about the precomputed parts of the affects of light on a spherical atmosphere.
    /// </summary>
    public struct PrecomputedAtmosphere
    {
        /// <summary>
        /// 3D (simulating a 4D) texture describing the amount of light given off by the atmosphere.
        /// </summary>
        public Texture Inscatter;

        /// <summary>
        /// 2D texture describing the amount of light given to a point on ground, by the atmosphere.
        /// </summary>
        public Texture Irradiance;

        /// <summary>
        /// 2D texture describing how light is filtered while traveling through the atmosphere.
        /// </summary>
        public Texture Transmittance;

        /// <summary>
        /// Sets up the textures for a currently active shader that involves the precomputed atmosphere.
        /// </summary>
        public void Setup(Shader Shader)
        {
            this.Inscatter.SetUnit(TextureTarget.Texture3D, TextureUnit.Texture0);
            this.Irradiance.SetUnit(TextureTarget.Texture2D, TextureUnit.Texture1);
            this.Transmittance.SetUnit(TextureTarget.Texture2D, TextureUnit.Texture2);
            Shader.SetUniform("Inscatter", TextureUnit.Texture0);
            Shader.SetUniform("Irradiance", TextureUnit.Texture1);
            Shader.SetUniform("Transmittance", TextureUnit.Texture2);
        }
    }

    /// <summary>
    /// Contains functions for manipulating and displaying spherical atmospheres.
    /// </summary>
    public static class Atmosphere
    {
        /// <summary>
        /// Defines precompiler constants for atmosphere shaders based on options.
        /// </summary>
        public static void DefineConstants(
            AtmosphereOptions Options,
            AtmosphereQualityOptions QualityOptions, 
            Shader.PrecompilerInput PrecompilerInput)
        {
            PrecompilerInput.Define("ATMOSPHERE_RES_R", QualityOptions.AtmosphereResR.ToString());
            PrecompilerInput.Define("ATMOSPHERE_RES_MU", QualityOptions.AtmosphereResMu.ToString());
            PrecompilerInput.Define("ATMOSPHERE_RES_MU_S", QualityOptions.AtmosphereResMuS.ToString());
            PrecompilerInput.Define("ATMOSPHERE_RES_NU", QualityOptions.AtmosphereResNu.ToString());
            PrecompilerInput.Define("IRRADIANCE_RES_R", QualityOptions.IrradianceResR.ToString());
            PrecompilerInput.Define("IRRADIANCE_RES_MU", QualityOptions.IrradianceResMu.ToString());
            PrecompilerInput.Define("TRANSMITTANCE_RES_R", QualityOptions.TransmittanceResR.ToString());
            PrecompilerInput.Define("TRANSMITTANCE_RES_MU", QualityOptions.TransmittanceResMu.ToString());

            PrecompilerInput.Define("RADIUS_GROUND", Options.RadiusGround.ToString());
            PrecompilerInput.Define("RADIUS_BOUND", Options.RadiusBound.ToString());
            PrecompilerInput.Define("AVERAGE_GROUND_REFLECTANCE", Options.AverageGroundReflectance.ToString());
            PrecompilerInput.Define("RAYLEIGH_AVERAGE_HEIGHT", Options.RayleighAverageHeight.ToString());
            PrecompilerInput.Define("MIE_AVERAGE_HEIGHT", Options.MieAverageHeight.ToString());
        }

        /// <summary>
        /// Creates a precomputed atmosphere using shaders and the graphics card.
        /// </summary>
        public static PrecomputedAtmosphere Generate(
            AtmosphereOptions Options, 
            AtmosphereQualityOptions QualityOptions, 
            Shader.PrecompilerInput PrecompilerInitial, 
            Path ShaderPath)
        {
            PrecomputedAtmosphere pa = new PrecomputedAtmosphere();
            Shader.PrecompilerInput pci = PrecompilerInitial.Copy();
            DefineConstants(Options, QualityOptions, pci);

            Path atmosphere = ShaderPath["Atmosphere"];
            Path precompute = atmosphere["Precompute"];

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

            Shader.PrecompilerInput irradiancedeltapci = pci.Copy();
            irradiancedeltapci.Define("DELTA");
            Shader irradiancedelta = Shader.Load(precompute["Irradiance.glsl"], irradiancedeltapci);

            Shader.PrecompilerInput irradiancepci = pci.Copy();
            Shader irradiance = Shader.Load(precompute["Irradiance.glsl"], irradiancepci);

            Shader.PrecompilerInput inscatterinitialdeltapci = pci.Copy();
            inscatterinitialdeltapci.Define("INITIAL");
            inscatterinitialdeltapci.Define("DELTA");
            Shader inscatterinitialdelta = Shader.Load(precompute["Inscatter.glsl"], inscatterinitialdeltapci);

            Shader.PrecompilerInput inscatterinitialpci = pci.Copy();
            inscatterinitialpci.Define("INITIAL");
            Shader inscatterinitial = Shader.Load(precompute["Inscatter.glsl"], inscatterinitialpci);

            Shader.PrecompilerInput inscatterdeltapci = pci.Copy();
            inscatterdeltapci.Define("DELTA");
            Shader inscatterdelta = Shader.Load(precompute["Inscatter.glsl"], inscatterdeltapci);

            Shader.PrecompilerInput inscatterpci = pci.Copy();
            Shader inscatter = Shader.Load(precompute["Inscatter.glsl"], inscatterpci);

            Shader.PrecompilerInput pointscatterpci = pci.Copy();
            Shader pointscatter = Shader.Load(precompute["PointScatter.glsl"], pointscatterpci);

            // Initialize textures
            int transwidth = QualityOptions.TransmittanceResMu;
            int transheight = QualityOptions.TransmittanceResR;
            int irrwidth = QualityOptions.IrradianceResMu;
            int irrheight = QualityOptions.IrradianceResR;
            int atwidth = QualityOptions.AtmosphereResMuS * QualityOptions.AtmosphereResNu;
            int atheight = QualityOptions.AtmosphereResMu;
            int atdepth = QualityOptions.AtmosphereResR;
            pa.Transmittance = Texture.Initialize2D(transwidth, transheight, Texture.RGB16Float);
            pa.Irradiance = Texture.Initialize2D(irrwidth, irrheight, Texture.RGB16Float);
            pa.Inscatter = Texture.Initialize3D(atwidth, atheight, atdepth, Texture.RGBA16Float);
            Texture irrdelta = Texture.Initialize2D(irrwidth, irrheight, Texture.RGB16Float);
            Texture insdelta = Texture.Initialize3D(atwidth, atheight, atdepth, Texture.RGB16Float);
            Texture ptsdelta = Texture.Initialize3D(atwidth, atheight, atdepth, Texture.RGB16Float);

            pa.Transmittance.SetUnit(TextureTarget.Texture2D, TextureUnit.Texture1);
            pa.Inscatter.SetUnit(TextureTarget.Texture3D, TextureUnit.Texture2);
            irrdelta.SetUnit(TextureTarget.Texture2D, TextureUnit.Texture3);
            insdelta.SetUnit(TextureTarget.Texture3D, TextureUnit.Texture4);
            ptsdelta.SetUnit(TextureTarget.Texture3D, TextureUnit.Texture5);


            uint fbo;
            GL.GenFramebuffers(1, out fbo);
            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, fbo);
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            // Create transmittance texture (information about how light is filtered through the atmosphere).
            transmittance.Call();
            transmittance.Draw2DFrame(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext,
                pa.Transmittance.ID, transwidth, transheight);

            // Create delta irradiance texture (ground lighting cause by sun).
            irradianceinitialdelta.Call();
            irradianceinitialdelta.SetUniform("Transmittance", TextureUnit.Texture1);
            irradianceinitialdelta.Draw2DFrame(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext,
                irrdelta.ID, irrwidth, irrheight);

            // Create initial inscatter texture (light from atmosphere from sun, rayleigh and mie parts seperated for precision).
            inscatterinitial.Call();
            inscatterinitial.SetUniform("Transmittance", TextureUnit.Texture1);
            inscatterinitial.Draw3DFrame(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext,
                pa.Inscatter.ID, atwidth, atheight, atdepth);

            // Initialize irradiance to zero (ground lighting caused by atmosphere).
            irradianceinitial.Call();
            irradianceinitial.Draw2DFrame(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext,
                pa.Irradiance.ID, irrwidth, irrheight);

            // Copy inscatter to delta inscatter, combining rayleigh and mie parts.
            inscatterinitialdelta.Call();
            inscatterinitialdelta.SetUniform("Inscatter", TextureUnit.Texture2);
            inscatterinitialdelta.Draw3DFrame(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext,
                insdelta.ID, atwidth, atheight, atdepth);

            for (int t = 2; t <= QualityOptions.MultipleScatteringOrder; t++)
            {
                // Generate point scattering information
                // Note that this texture will likely be very dark because it contains data for a single point, as opposed to a long line.
                pointscatter.Call();
                pointscatter.SetUniform("IrradianceDelta", TextureUnit.Texture3);
                pointscatter.SetUniform("InscatterDelta", TextureUnit.Texture4);
                pointscatter.Draw3DFrame(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext,
                    ptsdelta.ID, atwidth, atheight, atdepth);

                // Compute new irradiance delta using current inscatter delta.
                irradiancedelta.Call();
                irradiancedelta.SetUniform("InscatterDelta", TextureUnit.Texture4);
                irradiancedelta.Draw2DFrame(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext,
                    irrdelta.ID, irrwidth, irrheight);

                // Compute new inscatter delta using pointscatter data.
                inscatterdelta.Call();
                inscatterdelta.SetUniform("Transmittance", TextureUnit.Texture1);
                inscatterdelta.SetUniform("PointScatter", TextureUnit.Texture5);
                inscatterdelta.Draw3DFrame(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext,
                    insdelta.ID, atwidth, atheight, atdepth);

                GL.Enable(EnableCap.Blend);
                GL.BlendEquation(BlendEquationMode.FuncAdd);
                GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);

                // Add irradiance delta to irradiance.
                irradiance.Call();
                irradiance.SetUniform("IrradianceDelta", TextureUnit.Texture3);
                irradiance.Draw2DFrame(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext,
                    pa.Irradiance.ID, irrwidth, irrheight);

                // Add inscatter delta to inscatter.
                inscatter.Call();
                inscatter.SetUniform("InscatterDelta", TextureUnit.Texture4);
                inscatter.Draw3DFrame(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext,
                    pa.Inscatter.ID, atwidth, atheight, atdepth);

                GL.Disable(EnableCap.Blend);
            }

            insdelta.Delete();
            irrdelta.Delete();
            ptsdelta.Delete();

            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
            GL.Finish();
            GL.DeleteFramebuffers(1, ref fbo);

            return pa;
        }
    }
}