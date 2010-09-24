using namespace OpenTK;
using namespace OpenTK::Graphics;
using namespace OpenTK::Graphics::OpenGL;
using namespace System;

int main();

namespace Alunite
{
	/// <summary>
	/// Main window for the application.
	/// </summary>
	public ref class Window : GameWindow
	{
	public:
		Window();

	protected:
		virtual void OnRenderFrame(FrameEventArgs^ Args) override;
		virtual void OnUpdateFrame(FrameEventArgs^ Args) override;

	private:
	};
}