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
#define _COMMON_ATMOSPHERE_TEXTURE_READ_
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
uniform sampler3D Inscatter;
void main() {
	float mu, nu, r, mus;
	getAtmosphereTextureMuNuRMus(mu, nu, r, mus);
	float pr = phaseFunctionR(nu);
	float pm = phaseFunctionM(nu);
	vec4 raymies = lookupAtmosphereTexture(Inscatter, mu, nu, r, mus);
	gl_FragColor = vec4(raymies.rgb * pr + vec3(raymies.w * pm), 0.0);
}
#else
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
#endif
#else
#ifdef DELTA
uniform sampler3D PointScatter;

void main() {
	float mu, nu, r, mus;
	getAtmosphereTextureMuNuRMus(mu, nu, r, mus);
	
	vec3 raymie = vec3(0.0);
    float dx = limit(r, mu) / float(INSCATTER_INTEGRAL_SAMPLES);
    for (int i = 1; i <= INSCATTER_INTEGRAL_SAMPLES; ++i) {
        float t = float(i) * dx;
		
		float ri = sqrt(r * r + t * t + 2.0 * r * mu * t);
		float mui = (r * mu + t) / ri;
		float musi = (nu * t + mus * r) / ri;
        vec3 raymiei = lookupAtmosphereTexture(PointScatter, mui, nu, ri, musi).rgb * transmittance(r, mu, t);
		
        raymie += raymiei * dx;
    }
	
	gl_FragColor = mu < -sqrt(1.0 - (Rg / r) * (Rg / r)) ? vec4(0.0) : vec4(raymie, 0.0);
}
#else
uniform sampler3D InscatterDelta;
void main() {
	float mu, nu, r, mus;
	getAtmosphereTextureMuNuRMus(mu, nu, r, mus);
	float pr = phaseFunctionR(nu);
	vec3 ray = lookupAtmosphereTexture(InscatterDelta, mu, nu, r, mus).rgb;
	gl_FragColor = vec4(ray / pr, 0.0);
}
#endif
#endif
#endif


#ifdef _INSCATTER_USE_
uniform sampler3D Inscatter;

vec4 inscatter(float mu, float nu, float r, float mus) {
	return lookupAtmosphereTexture(Inscatter, mu, nu, r, mus);
}
#endif