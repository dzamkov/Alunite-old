#ifdef _VERTEX_
void main()
{
	gl_Position = gl_Vertex;
}
#endif

#ifdef _FRAGMENT_
#undef _FRAGMENT_
#define _TRANSMITTANCE_USE_
#include "Common.glsl"
#include "Transmittance.glsl"

#define INSCATTER_INTEGRAL_SAMPLES 50

uniform int Layer;

void getInscatterMuNuRMus(out float mu, out float nu, out float r, out float mus) {
	float x = gl_FragCoord.x;
	float y = gl_FragCoord.y;
	float z = float(Layer);
	
	mus = mod(x, float(INSCATTER_RES_MU_S)) / (float(INSCATTER_RES_MU_S) - 1.0);
	nu = floor(x / float(INSCATTER_RES_MU_S)) / (float(INSCATTER_RES_NU) - 1.0);
	mu = y / float(INSCATTER_RES_MU);
	r = z / float(INSCATTER_RES_R);
	
#ifdef INSCATTER_SIMPLE
	mu = -1.0 + mu * 2.0;
	nu = -1.0 + nu * 2.0;
	mus = -1.0 + mus * 2.0;
	r = Rg + r * (Rt - Rg);
#else
	mu = tan(((mu * 2.0) - 1.0) * 1.2) / tan(1.2);
	nu = -1.0 + nu * 2.0;
	mus = tan((2.0 * mus - 1.0 + 0.26) * 1.1) / tan(1.26 * 1.1);
	r = Rg + (r * r) * (Rt - Rg);
#endif
}

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

void main() {
	float mu, nu, r, mus;
	getInscatterMuNuRMus(mu, nu, r, mus);
	
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
}
#endif


#ifdef _INSCATTER_USE_
uniform sampler3D Inscatter;

vec4 inscatter(float mu, float nu, float r, float mus) {
#ifdef INSCATTER_SIMPLE
	float umu = (mu + 1.0) / 2.0;
	float unu = (nu + 1.0) / 2.0;
	float umus = (mus + 1.0) / 2.0;
	float ur = (r - Rg) / (Rt - Rg);
#else
	float umu = (atan(mu * tan(1.2)) / 1.2 + 1.0) * 0.5;
	float unu = (nu + 1.0) / 2.0;
	float umus = (atan(mus * tan(1.26 * 1.1)) / 1.1 + (1.0 - 0.26)) * 0.5;
	float ur = sqrt((r - Rg) / (Rt - Rg));
#endif
	
	umus = max(1.0 / float(INSCATTER_RES_MU_S), umus);
	umus = min(1.0 - 1.0 / float(INSCATTER_RES_MU_S), umus);
	
	float lerp = unu * (float(INSCATTER_RES_NU) - 1.0);
    unu = floor(lerp);
    lerp = lerp - unu;
    return texture3D(Inscatter, vec3((unu + umus) / float(INSCATTER_RES_NU), umu, ur)) * (1.0 - lerp) +
           texture3D(Inscatter, vec3((unu + umus + 1.0) / float(INSCATTER_RES_NU), umu, ur)) * lerp;
}
#endif