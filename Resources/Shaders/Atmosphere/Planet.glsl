uniform vec3 EyePosition;
uniform vec3 SunDirection;
uniform mat4 ProjInverse;
uniform mat4 ViewInverse;

const vec3 SunColor = vec3(100.0);
const vec3 SeaColor = vec3(0.0, 0.0, 0.2);
const vec3 AtmoColor = vec3(0.7, 0.7, 0.9);
const float Radius = 1.0;

varying vec2 Coords;
varying vec3 Ray;
varying vec3 Position;

#ifdef _VERTEX_
void main()
{
	Coords = gl_Vertex.xy * 0.5 + 0.5;
	Ray = (ViewInverse * vec4((ProjInverse * gl_Vertex).xyz, 1.0)).xyz;
	gl_Position = gl_Vertex;
}
#endif


#ifdef _FRAGMENT_
#undef _FRAGMENT_
#define _TRANSMITTANCE_USE_
#define _INSCATTER_USE_
#include "Precompute/Common.glsl"
#include "Precompute/Transmittance.glsl"
#include "Precompute/Inscatter.glsl"

float phaseFunctionR(float mu) {
    return (3.0 / (16.0 * Pi)) * (1.0 + mu * mu);
}

float phaseFunctionM(float mu) {
	return 1.5 * 1.0 / (4.0 * Pi) * (1.0 - mieG * mieG) * pow(1.0 + (mieG * mieG) - 2.0 * mieG * mu, -3.0 / 2.0) * (1.0 + mu * mu) / (2.0 + mieG * mieG);
}

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
		
        vec4 is = inscatter(mu, nu, r, mus);
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
	// Direct sunlight
	float mu = dot(n, sol);
	vec3 direct = transmittance(Rg, mu) * max(mu, 0.0) * SunColor / Pi;
	return direct * SeaColor;
}

vec3 HDR(vec3 L) {
    L = L * 0.4;
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
		groundcolor = groundColor(hitnorm, sol);
	}
	else
	{
		suncolor = sunColor(v, sol);
	}
	gl_FragColor = vec4(HDR(groundcolor + suncolor + atmocolor), 1.0);
	//gl_FragColor = texture2D(Transmittance, Coords);
}
#endif