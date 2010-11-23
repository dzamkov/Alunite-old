#ifdef _VERTEX_
void main()
{
	gl_Position = gl_Vertex;
}
#endif

#ifdef _FRAGMENT_
#undef _FRAGMENT_
#define _TRANSMITTANCE_USE_
#define _COMMON_ATMOSPHERE_TEXTURE_READ_
#include "Common.glsl"
#include "Transmittance.glsl"

#define IRRADIANCE_SPHERICAL_INTEGRAL_SAMPLES 16

void getIrradianceRMu(out float r, out float mu) {
    r = gl_FragCoord.y / float(IRRADIANCE_RES_R);
	mu = gl_FragCoord.x / float(IRRADIANCE_RES_MU);
#ifdef IRRADIANCE_SIMPLE
	r = Rg + r * (Rt - Rg);
	mu = -1.0 + mu * 2.0;
#else
	r = Rg + r * r * (Rt - Rg);
	mu = -1.0 + mu * 2.0;
#endif
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
#ifdef DELTA
uniform sampler3D InscatterDelta;
void main() {
	const float dtheta = Pi / float(IRRADIANCE_SPHERICAL_INTEGRAL_SAMPLES);
	const float dphi = dtheta;
	float r, mus;
	getIrradianceRMu(r, mus);
	vec3 s = vec3(sqrt(1.0 - mus * mus), 0.0, mus);
	vec3 result = vec3(0.0);
	
	for (int iphi = 0; iphi < 2 * IRRADIANCE_SPHERICAL_INTEGRAL_SAMPLES; ++iphi) {
        float phi = (float(iphi) + 0.5) * dphi;
        for (int itheta = 0; itheta < IRRADIANCE_SPHERICAL_INTEGRAL_SAMPLES / 2; ++itheta) {
            float theta = (float(itheta) + 0.5) * dtheta;
            float dw = dtheta * dphi * sin(theta);
            vec3 w = vec3(cos(phi) * sin(theta), sin(phi) * sin(theta), cos(theta));
            float nu = dot(s, w);
            result += lookupAtmosphereTexture(InscatterDelta, w.z, nu, r, mus).rgb * w.z * dw;
        }
    }
	
	gl_FragColor = vec4(result, 0.0);
}
#else
uniform sampler2D IrradianceDelta;
void main() {
	vec2 uv = gl_FragCoord.xy / vec2(IRRADIANCE_RES_MU, IRRADIANCE_RES_R);
    gl_FragColor = texture2D(IrradianceDelta, uv);
}
#endif
#endif
#endif

#ifdef _IRRADIANCE_UV_
vec2 getIrradianceUV(float r, float mu) {
#ifdef IRRADIANCE_SIMPLE
	return vec2((mu + 1.0) / 2.0, (r - Rg) / (Rt - Rg));
#else
	return vec2((mu + 1.0) / 2.0, sqrt((r - Rg) / (Rt - Rg)));
#endif
}
#endif

#ifdef _IRRADIANCE_USE_
uniform sampler2D Irradiance;
vec3 irradiance(float r, float mu) {
	return texture2D(Irradiance, getIrradianceUV(r, mu));
}
#endif