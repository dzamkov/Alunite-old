#ifdef _VERTEX_
void main()
{
	gl_Position = gl_Vertex;
}
#endif

#ifdef _FRAGMENT_
#undef _FRAGMENT_
uniform int Layer;

#define _TRANSMITTANCE_USE_
#define _COMMON_ATMOSPHERE_TEXTURE_WRITE_
#include "Common.glsl"
#include "Transmittance.glsl"

#define INSCATTER_INTEGRAL_SAMPLES 50

void pointScatter(float r, float mu, float mus, float nu, float t, out vec3 ray, out float mie)
{
	ray = vec3(0.0);
	mie = 0.0;
    float ri = sqrt(r * r + t * t + 2.0 * r * mu * t);
    float musi = (nu * t + mus * r) / ri;
    ri = max(Rg, ri);
    if (musi >= -sqrt(1.0 - Rg * Rg / (ri * ri))) {
        vec3 tra = transmittance(r, mu, t) * transmittanceWithShadow(ri, musi);
        ray = exp(-(ri - Rg) / HR) * tra;
        mie = exp(-(ri - Rg) / HM) * tra;
    }
	ray *= betaR;
    mie *= betaMSca;
}

#ifdef INITIAL
#ifdef DELTA
void main() {
	float mu, nu, r, mus;
	getAtmosphereTextureMuNuRMus(mu, nu, r, mus);
	
	vec3 ray = vec3(0.0);
	float mie = 0.0;
	float dx = limit(r, mu) / float(INSCATTER_INTEGRAL_SAMPLES);
	for (int i = 1; i <= INSCATTER_INTEGRAL_SAMPLES; ++i) {
		vec3 dray;
		float dmie;
		pointScatter(r, mu, mus, nu, dx * float(i), dray, dmie);
		ray += dray * dx;
		mie += dmie * dx;
	}
	
	gl_FragColor = vec4(ray, mie);
	
	if(r < Rg + 0.001 && mu < 0.0)
	{
		// Degeneracy fix
		gl_FragColor = vec4(0.0);
	}
}
#else
uniform sampler3D InscatterDelta;
void main() {
	vec3 uvw = vec3(gl_FragCoord.xy, float(Layer) + 0.5) / vec3(ivec3(ATMOSPHERE_RES_MU_S * ATMOSPHERE_RES_NU, ATMOSPHERE_RES_MU, ATMOSPHERE_RES_R));
	gl_FragColor = texture3D(InscatterDelta, uvw);
}
#endif
#else

#endif
#endif


#ifdef _INSCATTER_USE_
uniform sampler3D Inscatter;

vec4 inscatter(float mu, float nu, float r, float mus) {
	return lookupAtmosphereTexture(Inscatter, mu, nu, r, mus);
}
#endif