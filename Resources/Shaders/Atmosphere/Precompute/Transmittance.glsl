varying vec2 Coords;

#ifdef _VERTEX_

void main()
{
	Coords = gl_Vertex.xy * 0.5 + 0.5;
	gl_Position = gl_Vertex;
}

#else

void main()
{
	gl_FragColor = vec4(Coords.x, Coords.y, 1.0, 1.0);
}

#endif