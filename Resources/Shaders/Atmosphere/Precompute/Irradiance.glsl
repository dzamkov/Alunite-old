#ifdef _VERTEX_

void main()
{
	gl_Position = gl_Vertex;
}

#else

#define COMMON_CONSTANTS
#include "Common.glsl"

void main() {
    gl_FragColor = vec4(0.0, 0.0, 1.0, 0.0);
}

#endif