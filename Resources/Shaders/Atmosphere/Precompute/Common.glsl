const float Pi = 3.14159265;

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
