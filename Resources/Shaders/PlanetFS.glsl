#version 150

uniform vec3 EyePosition;
uniform vec3 SunDirection;

varying vec3 Position;
varying vec3 Normal;

const vec3 Color = vec3(0.1, 0.1, 0.5);

void main()
{
	vec3 eyedir = normalize(Position - EyePosition);
	
	vec3 sunref = reflect(SunDirection, Normal);
	
	float specdot = max(dot(eyedir, sunref), 0.0);
	float sundot = dot(Normal, SunDirection);
	float light = smoothstep(0.0, 1.0, sundot) * 0.8 + (specdot * specdot) * 0.5 + 0.1;
	
	gl_FragColor = vec4(Color * light, 1.0);
}