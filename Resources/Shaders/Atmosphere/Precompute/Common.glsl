const float Pi = 3.14159265;

const float Rt = float(RADIUS_BOUND);
const float Rg = float(RADIUS_GROUND);

const float GroundReflectance = float(AVERAGE_GROUND_REFLECTANCE);

// Rayleigh
const float HR = float(RAYLEIGH_AVERAGE_HEIGHT);
const vec3 betaR = vec3(5.8e-3, 1.35e-2, 3.31e-2);

// Mie
const float HM = float(MIE_AVERAGE_HEIGHT);
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

float phaseFunctionR(float mu) {
    return (3.0 / (16.0 * Pi)) * (1.0 + mu * mu);
}

float phaseFunctionM(float mu) {
	return 1.5 * 1.0 / (4.0 * Pi) * (1.0 - mieG * mieG) * pow(1.0 + (mieG * mieG) - 2.0 * mieG * mu, -3.0 / 2.0) * (1.0 + mu * mu) / (2.0 + mieG * mieG);
}

#ifdef _COMMON_ATMOSPHERE_TEXTURE_WRITE_
void getAtmosphereTextureMuNuRMus(out float mu, out float nu, out float r, out float mus) {
	float x = gl_FragCoord.x;
	float y = gl_FragCoord.y;
	float z = float(Layer);
	
	mus = mod(x, float(ATMOSPHERE_RES_MU_S)) / (float(ATMOSPHERE_RES_MU_S) - 1.0);
	nu = floor(x / float(ATMOSPHERE_RES_MU_S)) / (float(ATMOSPHERE_RES_NU) - 1.0);
	mu = y / float(ATMOSPHERE_RES_MU);
	r = z / float(ATMOSPHERE_RES_R);
	
#ifdef ATMOSPHERE_TEXTURE_SIMPLE
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
#endif

#ifdef _COMMON_ATMOSPHERE_TEXTURE_READ_
vec4 lookupAtmosphereTexture(sampler3D tex, float mu, float nu, float r, float mus) {
#ifdef ATMOSPHERE_TEXTURE_SIMPLE
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
	
	umus = max(1.0 / float(ATMOSPHERE_RES_MU_S), umus);
	umus = min(1.0 - 1.0 / float(ATMOSPHERE_RES_MU_S), umus);
	
	float lerp = unu * (float(ATMOSPHERE_RES_NU) - 1.0);
    unu = floor(lerp);
    lerp = lerp - unu;
    return texture3D(tex, vec3((unu + umus) / float(ATMOSPHERE_RES_NU), umu, ur)) * (1.0 - lerp) +
           texture3D(tex, vec3((unu + umus + 1.0) / float(ATMOSPHERE_RES_NU), umu, ur)) * lerp;
}
#endif