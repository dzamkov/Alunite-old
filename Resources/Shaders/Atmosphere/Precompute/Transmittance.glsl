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
	r = Rg + r * (Rt - Rg);
	mu = -1.0 + mu * 2.0;
}

float opticalDepth(float H, float r, float mu) {
    float result = 0.0;
    float dx = limit(r, mu) / float(TRANSMITTANCE_INTEGRAL_SAMPLES);
	
	float acrossdelta = sqrt(1 - mu * mu);
	float updelta = mu;
	
    for (int i = 0; i < TRANSMITTANCE_INTEGRAL_SAMPLES; ++i) {
        float xj = float(i) * dx;
		float across = acrossdelta * xj;
		float up = updelta * xj;
		float ir = sqrt(across * across + (up + r) * (up + r));
        float yj = exp(-(ir - Rg) / H);
		result += yj * dx;
    }
    return result;
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
	float ur = (r - Rg) / (Rt - Rg);
	float umu = (mu + 1.0) / 2.0;
	return vec2(umu, ur);
}

vec3 transmittance(float r, float mu) {
	vec2 uv = getTransmittanceUV(r, mu);
	return texture2D(Transmittance, uv).rgb;
}
#endif