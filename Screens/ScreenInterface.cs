using GorgonLibrary.InputDevices;

namespace Xasteroids.Screens
{
	interface ScreenInterface
	{
		bool Initialize(GameMain gameMain, out string reason);

		void DrawScreen();

		void Update(int x, int y, float frameDeltaTime);

		void MouseDown(int x, int y);

		void MouseUp(int x, int y);

		void MouseScroll(int direction, int x, int y);

		void KeyDown(KeyboardInputEventArgs e);
	}
}
