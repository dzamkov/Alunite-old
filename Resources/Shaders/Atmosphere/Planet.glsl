uniform vec3 EyePosition;
uniform vec3 SunDirection;
uniform mat4 ProjInverse;
uniform mat4 ViewInverse;

const vec3 SunColor = vec3(100.0);
const vec3 SeaColor = vec3(0.1, 0.1, 0.5);
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

#else

#define COMMON_CONSTANTS
#define COMMON_TRANSMITTANCE
#include "Precompute/Common.glsl"

vec3 sunColor(vec3 v, vec3 sol)
{
	return  step(cos(3.1415 / 180.0), dot(v, sol)) *  SunColor;
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
	
	vec3 hitnorm = normalize(hit);
	
	
	vec3 groundcolor = vec3(0.0);
	vec3 suncolor = sunColor(v, sol);
	if(t > 0.0)
	{
		vec3 eyedir = normalize(hit - EyePosition);

		vec3 sunref = reflect(SunDirection, hitnorm);

		float atmos = min(pow(max(1.0 - dot(hitnorm, -eyedir), 0.0), 8.0) + 0.1, 1.0);
		float specdot = max(dot(eyedir, sunref), 0.0);
		float sundot = dot(hitnorm, SunDirection);
		float normlight = smoothstep(-0.2, 1.0, sundot);
		float sealight = normlight * 0.8 + pow(specdot, 4.0) * 0.5 + 0.1;
		float atmolight = normlight * 0.4;

		
		
		groundcolor = SeaColor * sealight * (1.0 - atmos) + AtmoColor * atmolight * atmos;
	}
	
	gl_FragColor = vec4(groundcolor + suncolor, 1.0);
	
	//gl_FragColor = texture2D(Transmittance, Coords);
}

#endif