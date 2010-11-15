varying vec2 Coords;

#ifdef _VERTEX_

void main()
{
	Coords = gl_Vertex.xy * 0.5 + 0.5;
	gl_Position = gl_Vertex;
}

#else

#define TRANSMITTANCE_INTEGRAL_SAMPLES 500
#define Rt 6420.0
#define Rg 6360.0

const vec3 RayleighFilter = vec3(5.8e-3, 1.35e-2, 3.31e-2);
const vec3 MieFilter = vec3(4e-3);


// Rayleigh
const float HR = 8.0;
const vec3 betaR = vec3(5.8e-3, 1.35e-2, 3.31e-2);

// Mie
// DEFAULT
const float HM = 1.2;
const vec3 betaMSca = vec3(4e-3);
const vec3 betaMEx = betaMSca / 0.9;
const float mieG = 0.8;

void getTransmittanceRMu(out float r, out float mu) {
    r = gl_FragCoord.y / float(TRANSMITTANCE_H);
	mu = gl_FragCoord.x / float(TRANSMITTANCE_W);
	r = Rg + r * (Rt - Rg);
	mu = -1.0 + mu * 2.0;
}

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