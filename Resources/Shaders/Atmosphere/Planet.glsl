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
#include "Precompute/Common.glsl"
#include "Precompute/Transmittance.glsl"

vec3 sunColor(vec3 v, vec3 sol)
{
	return  step(cos(Pi / 180.0), dot(v, sol)) *  SunColor;
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
	vec3 cen = -EyePosition;
	float r = length(cen);
	float mu = dot(cen, v);
	
	// Find where the ray intersects the planet.
	float t = mu - sqrt(mu * mu - r * r + Rg * Rg); // Distance along ray of hit
	vec3 hit = v * t + EyePosition; // Point on ray where hit
	
	vec3 groundcolor = vec3(0.0);
	vec3 suncolor = sunColor(v, sol);
	if(t > 0.0)
	{
		vec3 hitnorm = normalize(hit);
		groundcolor = groundColor(hitnorm, sol);
	}
	
	gl_FragColor = vec4(HDR(groundcolor + suncolor), 1.0);
	
	//gl_FragColor = texture2D(Transmittance, Coords);
}
#endif