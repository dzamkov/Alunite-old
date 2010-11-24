const float Pi = 3.1415926535;

varying vec2 Coords;

#ifdef _VERTEX_
void main()
{
	Coords = gl_Vertex.xy;
	gl_Position = gl_Vertex;
}
#endif

#ifdef _FRAGMENT_
uniform samplerCube Cubemap;

void main()
{
	float siny = sqrt(1.0 - Coords.y * Coords.y);
	float cosy = Coords.y;
	vec3 sample = vec3(sin(Coords.x * Pi) * siny, cos(Coords.x * Pi) * siny, cosy);
	gl_FragColor = textureCube(Cubemap, sample);
}
#endif