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

void getIrradianceRMu(out float r, out float mu) {
    r = gl_FragCoord.y / float(IRRADIANCE_RES_R);
	mu = gl_FragCoord.x / float(IRRADIANCE_RES_MU);
	r = Rg + r * (Rt - Rg);
	mu = -1.0 + mu * 2.0;
}

#ifdef INITIAL
#ifdef DELTA
void main() {
	float r, mu;
	getIrradianceRMu(r, mu);
    gl_FragColor = vec4(transmittance(r, mu) * max(mu, 0.0), 0.0);
}
#else
void main() {
	gl_FragColor = vec4(0.0);
}
#endif
#else


#endif
#endif