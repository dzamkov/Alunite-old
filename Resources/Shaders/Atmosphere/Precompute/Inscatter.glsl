#ifdef _VERTEX_
void main()
{
	gl_Position = gl_Vertex;
}
#endif

#ifdef _FRAGMENT_
#define _TRANSMITTANCE_USE_
#include "Common.glsl"
#include "Transmittance.glsl"

void main() {
    gl_FragColor = vec4(1.0, 0.0, 0.0, 0.0);
}
#endif