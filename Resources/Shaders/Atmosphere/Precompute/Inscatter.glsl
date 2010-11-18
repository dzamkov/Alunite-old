#ifdef _VERTEX_
void main()
{
	gl_Position = gl_Vertex;
}
#endif

#ifdef _FRAGMENT_
#undef _FRAGMENT_
#define _TRANSMITTANCE_USE_
#include "Common.glsl"
#include "Transmittance.glsl"

void getInscatterMuNuRMus(out float mu, out float nu, out float r, out float mus) {
	float x = gl_FragCoord.x;
	float y = gl_FragCoord.y;
	
	mus = floor(y / float(INSCATTER_RES_R));
	r = y - mus * float(INSCATTER_RES_R);
	mus = mus / float(INSCATTER_RES_MU_S);
	r = r / float(INSCATTER_RES_R);
	
	nu = floor(x / float(INSCATTER_RES_MU));
	mu = x - nu * float(INSCATTER_RES_MU);
	nu = nu / float(INSCATTER_RES_NU);
	mu = mu / float(INSCATTER_RES_MU);
}

void main() {
	float mu, nu, r, mus;
	getInscatterMuNuRMus(mu, nu, r, mus);
    gl_FragColor = vec4(mu, nu, r, 1.0);
}
#endif