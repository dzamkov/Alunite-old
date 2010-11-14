uniform vec3 EyePosition;
uniform vec3 SunDirection;
uniform mat4 ProjInverse;
uniform mat4 ViewInverse;

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

void main()
{
	vec3 ray = normalize(Ray);
	vec3 cen = -EyePosition;
	
	// Find where the ray intersects the planet.
	float r = length(cen);
    float mu = dot(cen, ray);
    float t = mu - sqrt(mu * mu - r * r + Radius * Radius); // Distance along ray of hit
	vec3 hit = ray * t; // Point on ray where hit

	gl_FragColor = vec4(hit + EyePosition, 1.0);
}

#endif