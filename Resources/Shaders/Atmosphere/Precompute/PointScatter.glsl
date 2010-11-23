#ifdef _VERTEX_
void main()
{
	gl_Position = gl_Vertex;
}
#endif

#ifdef _FRAGMENT_
#undef _FRAGMENT_
uniform int Layer;
uniform int Order;
uniform sampler2D IrradianceDelta;
uniform sampler3D InscatterDelta;

#define _TRANSMITTANCE_USE_
#define _IRRADIANCE_UV_
#define _COMMON_ATMOSPHERE_TEXTURE_WRITE_
#define _COMMON_ATMOSPHERE_TEXTURE_READ_
#include "Common.glsl"
#include "Transmittance.glsl"
#include "Irradiance.glsl"

#define POINTSCATTER_SPHERICAL_INTEGRAL_SAMPLES 16

void main()
{
	float mu, nu, r, mus;
	getAtmosphereTextureMuNuRMus(mu, nu, r, mus);
	
	r = clamp(r, Rg, Rt);
    mu = clamp(mu, -1.0, 1.0);
    mus = clamp(mus, -1.0, 1.0);
    float var = sqrt(1.0 - mu * mu) * sqrt(1.0 - mus * mus);
    nu = clamp(nu, mus * mu - var, mus * mu + var);
	
	float cthetaground = -sqrt(1.0 - (Rg / r) * (Rg / r));
	
	vec3 v = vec3(sqrt(1.0 - mu * mu), 0.0, mu);
	float sx = v.x == 0.0 ? 0.0 : (nu - mus * mu) / v.x;
    vec3 s = vec3(sx, sqrt(max(0.0, 1.0 - sx * sx - mus * mus)), mus);
	
	vec3 raymie = vec3(0.0);
	
	const float dtheta = Pi / float(POINTSCATTER_SPHERICAL_INTEGRAL_SAMPLES);
	const float dphi = dtheta;
	
	for (int itheta = 0; itheta < POINTSCATTER_SPHERICAL_INTEGRAL_SAMPLES; ++itheta) {
        float theta = (float(itheta) + 0.5) * dtheta;
        float ctheta = cos(theta);
		
		float greflectance = 0.0;
        float dground = 0.0;
        vec3 gtransp = vec3(0.0);
        if (ctheta < cthetaground) { 
            greflectance = GroundReflectance / Pi;
            dground = -r * ctheta - sqrt(r * r * (ctheta * ctheta - 1.0) + Rg * Rg);
            gtransp = transmittance(Rg, -(r * ctheta + dground) / Rg, dground);
        }
		
		for (int iphi = 0; iphi < 2 * POINTSCATTER_SPHERICAL_INTEGRAL_SAMPLES; ++iphi) {
            float phi = (float(iphi) + 0.5) * dphi;
            float dw = dtheta * dphi * sin(theta);
            vec3 w = vec3(cos(phi) * sin(theta), sin(phi) * sin(theta), ctheta);

            float nu1 = dot(s, w);
            float nu2 = dot(v, w);
            float pr2 = phaseFunctionR(nu2);
            float pm2 = phaseFunctionM(nu2);

            vec3 gnormal = (vec3(0.0, 0.0, r) + dground * w) / Rg;
            vec3 girradiance = texture2D(IrradianceDelta, getIrradianceUV(Rg, dot(gnormal, s)));

            vec3 ix = greflectance * girradiance * gtransp + lookupAtmosphereTexture(InscatterDelta, ctheta, nu1, r, mus).rgb;

            raymie += ix * (betaR * exp(-(r - Rg) / HR) * pr2 + betaMSca * exp(-(r - Rg) / HM) * pm2) * dw;
        }
	}
	
	gl_FragColor = vec4(raymie, 0.0);
}

#endif