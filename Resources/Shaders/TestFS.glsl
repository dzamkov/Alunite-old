#version 120

uniform sampler2D MaterialDiffuse;

varying vec3 Position;
varying vec3 Normal;

const float Smoothness = 0.2;

float blendtex(float val, float cmpa, float cmpb, float smooth)
{
	float lower = cmpa - smooth;
	float higher = cmpa + smooth;
	float adiff = 1.0 - smoothstep(lower, higher, val);
	lower = cmpb - smooth;
	higher = cmpb + smooth;
	float bdiff = 1.0 - smoothstep(lower, higher, val);
	return 1.0 - max(adiff, bdiff);
}

void main()
{
	vec3 texcoord = Position * 4;
	
	vec3 texstrength;
	texstrength.x = abs(Normal.x);
	texstrength.y = abs(Normal.y);
	texstrength.z = abs(Normal.z);
	
	vec3 blendstrength;
	blendstrength.x = blendtex(texstrength.x, texstrength.y, texstrength.z, Smoothness);
	blendstrength.y = blendtex(texstrength.y, texstrength.z, texstrength.x, Smoothness);
	blendstrength.z = blendtex(texstrength.z, texstrength.x, texstrength.y, Smoothness);

	vec3 diffuse = vec3(0.0, 0.0, 0.0);
	vec3 diffusez;
	vec3 diffusex;
	vec3 diffusey;
	
	if (blendstrength.x > 0.0)
	{
		diffuse = diffuse + texture2D(MaterialDiffuse, texcoord.yz).rgb * blendstrength.x;
	}
	if (blendstrength.y > 0.0)
	{
		diffuse = diffuse + texture2D(MaterialDiffuse, texcoord.zx).rgb * blendstrength.y;
	}
	if (blendstrength.z > 0.0)
	{
		diffuse = diffuse + texture2D(MaterialDiffuse, texcoord.xy).rgb * blendstrength.z;
	}
	
	float light = max(dot(Normal, vec3(1.0, 1.0, 1.0)), 0.0);
	
	gl_FragColor = vec4(diffuse * (light * 0.8 + 0.2), 1.0);
}