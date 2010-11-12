#version 150

uniform vec3 EyePosition;
uniform vec3 SunDirection;

varying vec3 Position;
varying vec3 Normal;

const vec3 SeaColor = vec3(0.1, 0.1, 0.5);
const vec3 AtmoColor = vec3(0.7, 0.7, 0.9);

void main()
{
	vec3 eyedir = normalize(Position - EyePosition);
	
	vec3 sunref = reflect(SunDirection, Normal);
	
	float atmos = pow(max(1.0 - dot(Normal, -eyedir), 0.0), 8.0) + 0.1;
	float specdot = max(dot(eyedir, sunref), 0.0);
	float sundot = dot(Normal, SunDirection);
	float normlight = smoothstep(-0.2, 1.0, sundot);
	float sealight = normlight * 0.8 + pow(specdot, 4.0) * 0.1 + 0.1;
	float atmolight = normlight * 0.4;
	
	gl_FragColor = vec4(SeaColor * sealight * (1.0 - atmos) + AtmoColor * atmolight * atmos, 1.0);
}