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

void getInscatterMuNuRMus(out float mu, out float nu, out float r, out float mus) {
	float x = gl_FragCoord.x;
	float y = gl_FragCoord.y;
	
	mus = floor(y / float(INSCATTER_RES_R));
	r = y - mus * float(INSCATTER_RES_R);
	mus = mus / float(INSCATTER_RES_MU_S);
	r = r / float(INSCATTER_RES_R);
	
	nu = floor(x / float(INSCATTER_RES_MU));
	mu = x - nu * float(INSCATTER_RES_MU);
	nu = nu / float(INSCATTER_RES_NU);
	mu = mu / float(INSCATTER_RES_MU);
	
	
	mu = -1.0 + mu * 2.0;
	nu = -1.0 + nu * 2.0;
	mus = -1.0 + mus * 2.0;
	r = Rg + r * (Rt - Rg);
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
uniform sampler2D Inscatter;

vec4 inscatter(float mu, float nu, float r, float mus) {
	mu = (mu + 1.0) / 2.0;
	nu = (nu + 1.0) / 2.0;
	mus = (mus + 1.0) / 2.0;
	r = (r - Rg) / (Rt - Rg);
	float multmu = 1 / float(INSCATTER_RES_NU);
	float multr = 1 / float(INSCATTER_RES_MU_S);
	float offnu = min(floor(nu * float(INSCATTER_RES_NU)), INSCATTER_RES_NU - 1) / float(INSCATTER_RES_NU);
	float offmus = min(floor(mus * float(INSCATTER_RES_MU_S)), INSCATTER_RES_MU_S - 1) / float(INSCATTER_RES_MU_S);
	return texture2D(Inscatter, vec2(mu * multmu + offnu, r * multr + offmus));
}
#endif