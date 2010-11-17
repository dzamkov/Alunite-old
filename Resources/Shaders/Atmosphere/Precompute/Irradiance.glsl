#ifdef _VERTEX_

void main()
{
	gl_Position = gl_Vertex;
}

#else

#define COMMON_CONSTANTS
#define COMMON_TRANSMITTANCE
#include "Common.glsl"

void getIrradianceRMu(out float r, out float mu) {
    r = gl_FragCoord.y / float(IRRADIANCE_RES_R);
	mu = gl_FragCoord.x / float(IRRADIANCE_RES_MU);
	r = Rg + r * (Rt - Rg);
	mu = -1.0 + mu * 2.0;
}

void main() {
	float r, mu;
	getIrradianceRMu(r, mu);
    gl_FragColor = vec4(transmittance(r, mu) * max(mu, 0.0), 0.0);
}

#endif