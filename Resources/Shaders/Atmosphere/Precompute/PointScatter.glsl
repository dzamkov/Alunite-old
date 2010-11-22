#ifdef _VERTEX_
void main()
{
	gl_Position = gl_Vertex;
}
#endif

#ifdef _FRAGMENT_
#undef _FRAGMENT_
uniform int Layer;
uniform sampler2D IrradianceDelta;
uniform sampler2D InscatterDelta;

#define _TRANSMITTANCE_USE_
#define _IRRADIANCE_UV_
#define _COMMON_ATMOSPHERE_TEXTURE_WRITE_
#define _COMMON_ATMOSPHERE_TEXTURE_READ_
#include "Common.glsl"
#include "Transmittance.glsl"
#include "Irradiance.glsl"

void main()
{
	float mu, nu, r, mus;
	getAtmosphereTextureMuNuRMus(mu, nu, r, mus);
	gl_FragColor = texture2D(IrradianceDelta, getIrradianceUV(r, mus));
}

#endif