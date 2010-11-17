#ifdef COMMON_CONSTANTS

const float Rt = 6420.0;
const float Rg = 6360.0;

// Rayleigh
const float HR = 8.0;
const vec3 betaR = vec3(5.8e-3, 1.35e-2, 3.31e-2);

// Mie
// DEFAULT
const float HM = 1.2;
const vec3 betaMSca = vec3(4e-3);
const vec3 betaMEx = betaMSca / 0.9;
const float mieG = 0.8;

#endif


#ifdef COMMON_TRANSMITTANCE

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





#ifdef COMMON_LIMIT

// Gets the length of the ray before either the atmospheric boundary, or ground, is hit.
float limit(float r, float mu) {
    float atmosdis = -r * mu + sqrt(r * r * (mu * mu - 1.0) + Rt * Rt);
	float groundp = r * r * (mu * mu - 1.0) + Rg * Rg;
	float res = atmosdis;
	if(groundp >= 0.0)
	{
		float grounddis = -r * mu - sqrt(groundp);
		if(grounddis >= 0.0)
		{
			res = grounddis;
		}
	}
    return res;
}

#endif



