#version 120

varying vec3 Position;
varying vec3 Normal;

void main()
{
    gl_FrontColor = gl_Color;
    gl_Position = ftransform();
	 Position = vec3(gl_ModelViewMatrix * gl_Vertex);
	 Normal = gl_NormalMatrix * gl_Normal;
}