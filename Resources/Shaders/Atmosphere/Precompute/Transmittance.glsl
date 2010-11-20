#ifdef _VERTEX_
void main()
{
	gl_Position = gl_Vertex;
}
#endif


#ifdef _FRAGMENT_
#undef _FRAGMENT_
#include "Common.glsl"

#define TRANSMITTANCE_INTEGRAL_SAMPLES 500

void getTransmittanceRMu(out float r, out float mu) {
    r = gl_FragCoord.y / float(TRANSMITTANCE_RES_R);
	mu = gl_FragCoord.x / float(TRANSMITTANCE_RES_MU);
#ifdef TRANSMITTANCE_SIMPLE
	r = Rg + r * (Rt - Rg);
	mu = -1.0 + mu * 2.0;
#else
	r = Rg + (r * r) * (Rt - Rg);
    mu = -0.15 + tan(1.5 * mu) / tan(1.5) * (1.0 + 0.15);
#endif
}

float opticalDepth(float H, float r, float mu) {
    float result = 0.0;
    float dx = limit(r, mu) / float(TRANSMITTANCE_INTEGRAL_SAMPLES);
	
	float acrossdelta = sqrt(1 - mu * mu);
	float updelta = mu;
	
    for (int i = 1; i <= TRANSMITTANCE_INTEGRAL_SAMPLES; ++i) {
        float xj = float(i) * dx;
		float across = acrossdelta * xj;
		float up = updelta * xj;
		float ir = sqrt(across * across + (up + r) * (up + r));
		result += exp(-(ir - Rg) / H) * dx;
    }
    return mu < -sqrt(1.0 - (Rg / r) * (Rg / r)) ? 1e9 : result;
}

void main() {
    float r, mu;
    getTransmittanceRMu(r, mu);
	vec3 depth = betaR * opticalDepth(HR, r, mu) + betaMEx * opticalDepth(HM, r, mu);
    gl_FragColor = vec4(exp(-depth), 0.0);
}
#endif


#ifdef _TRANSMITTANCE_USE_
uniform sampler2D Transmittance;

vec2 getTransmittanceUV(float r, float mu) {
#ifdef TRANSMITTANCE_SIMPLE
	float ur = (r - Rg) / (Rt - Rg);
	float umu = (mu + 1.0) / 2.0;
#else
	float ur = sqrt((r - Rg) / (Rt - Rg));
	float umu = atan((mu + 0.15) / (1.0 + 0.15) * tan(1.5)) / 1.5;
#endif
	return vec2(umu, ur);
}

// Gets transmittance with the atmosphere (inaccurate when hitting ground).
vec3 transmittance(float r, float mu) {
	vec2 uv = getTransmittanceUV(r, mu);
	return texture2D(Transmittance, uv).rgb;
}

// Gets transmittance up to a certain distance on a ray.
vec3 transmittance(float r, float mu, float t) {
	vec3 result = vec3(0.0);
    float r1 = sqrt(r * r + t * t + 2.0 * r * mu * t);
    float mu1 = (r * mu + t) / r1;
	if (mu > 0.0) {
        result = min(transmittance(r, mu) / transmittance(r1, mu1), 1.0);
    } else {
        result = min(transmittance(r1, -mu1) / transmittance(r, -mu), 1.0);
    }
    return result;
}

// Gets transmittance with atmosphere, or zero if hitting ground.
vec3 transmittanceWithShadow(float r, float mu) {
    return mu < -sqrt(1.0 - (Rg / r) * (Rg / r)) ? vec3(0.0) : transmittance(r, mu);
}
#endif