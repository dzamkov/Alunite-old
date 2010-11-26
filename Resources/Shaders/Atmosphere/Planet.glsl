uniform vec3 EyePosition;
uniform vec3 SunDirection;
uniform mat4 ProjectionInverse;
uniform float NearDistance;
uniform float FarDistance;
uniform samplerCube CubeMap;

const vec3 SunColor = vec3(20.0);
const float Radius = 1.0;

varying vec2 Coords;
varying vec3 Ray;

#ifdef _VERTEX_
void main()
{
	Coords = gl_Vertex.xy * 0.5 + 0.5;
	vec4 trans = ProjectionInverse * gl_Vertex;
	Ray = (trans / trans.w).xyz - EyePosition;
	gl_Position = gl_Vertex;
}
#endif


#ifdef _FRAGMENT_
#undef _FRAGMENT_
#define _IRRADIANCE_UV_
#define _TRANSMITTANCE_USE_
#define _INSCATTER_USE_
#define _IRRADIANCE_USE_
#define _COMMON_ATMOSPHERE_TEXTURE_READ_
#include "Precompute/Common.glsl"
#include "Precompute/Transmittance.glsl"
#include "Precompute/Irradiance.glsl"
#include "Precompute/Inscatter.glsl"

vec3 getMie(vec4 rayMie) {
	return rayMie.rgb * rayMie.w / max(rayMie.r, 1e-4) * (betaR.r / betaR);
}

vec3 atmoColor(float t, vec3 x, vec3 v, vec3 sol)
{
	vec3 result = vec3(0.0);
	float r = length(x);
    float mu = dot(x, v) / r;
    float d = -r * mu - sqrt(r * r * (mu * mu - 1.0) + Rt * Rt);
    if (d > 0.0) {
        x += d * v;
        t -= d;
        mu = (r * mu + d) / Rt;
        r = Rt;
    }
    if (r <= Rt) { // if ray intersects atmosphere
        float nu = dot(v, sol);
        float mus = dot(x, sol) / r;
		
		float phaseR = phaseFunctionR(nu);
        float phaseM = phaseFunctionM(nu);
		vec4 is = max(inscatter(mu, nu, r, mus), 0.0);
		
		result = max(is.rgb * phaseR + getMie(is) * phaseM, 0.0);
    }
    return result * SunColor;
}

vec3 sunColor(vec3 v, vec3 sol)
{
	return step(cos(Pi / 180.0), dot(v, sol)) *  SunColor;
}

vec3 groundColor(vec3 n, vec3 sol)
{
	float mus = dot(n, sol);
	vec3 direct = transmittance(Rg, mus) * max(mus, 0.0) * SunColor / Pi;
	vec3 irr = irradiance(Rg, mus);
	vec3 full = direct + irr;
	
	vec3 color = textureCube(CubeMap, n);
	
	return full * color;
}

vec3 HDR(vec3 L) {
    L.r = L.r < 1.413 ? pow(L.r * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.r);
    L.g = L.g < 1.413 ? pow(L.g * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.g);
    L.b = L.b < 1.413 ? pow(L.b * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.b);
    return L;
}

void main()
{
	vec3 v = normalize(Ray);
	vec3 sol = SunDirection;
	vec3 x = EyePosition;
	float r = length(x);
	float mu = dot(x, v) / r;
	
	// Find where the ray intersects the planet.
	float t = -r * mu - sqrt(r * r * (mu * mu - 1.0) + Rg * Rg);
	vec3 hit = x + v * t;
	
	vec3 atmocolor = atmoColor(t, x, v, sol);
	vec3 groundcolor = vec3(0.0);
	vec3 suncolor = vec3(0.0);
	if(t > 0.0)
	{
		vec3 hitnorm = normalize(hit);
		groundcolor = groundColor(hitnorm, sol) * transmittance(r, mu, t);
	}
	else
	{
		suncolor = sunColor(v, sol);
		gl_FragDepth = 1.0;
	}
	gl_FragColor = vec4(HDR(groundcolor + suncolor + atmocolor), 1.0);
}
#endif