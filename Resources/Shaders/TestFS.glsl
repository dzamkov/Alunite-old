#version 120

uniform sampler2D MaterialDiffuse;

varying vec3 Position;
varying vec3 Normal;

void main()
{
	vec3 diffusez = texture2D(MaterialDiffuse, Position.xy).rgb;
	vec3 diffusex = texture2D(MaterialDiffuse, Position.yz).rgb;
	vec3 diffusey = texture2D(MaterialDiffuse, Position.zx).rgb;
	gl_FragColor = vec4(diffusez, 1.0);
}