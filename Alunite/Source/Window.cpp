#include "Window.h"
using namespace Alunite;

int main()
{
	Window^ w = gcnew Window();
	w->Run();
}

Window::Window() : GameWindow(640, 480, GraphicsMode::Default, "Alunite")
{

}

void Window::OnUpdateFrame(OpenTK::FrameEventArgs ^Args)
{


}

void Window::OnRenderFrame(OpenTK::FrameEventArgs ^Args)
{
	GL::Clear(ClearBufferMask::DepthBufferBit ^ ClearBufferMask::ColorBufferBit);


	this->SwapBuffers();
}