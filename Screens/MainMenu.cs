using GorgonLibrary.InputDevices;

namespace Xasteroids.Screens
{
	public class MainMenu : ScreenInterface
	{
		private GameMain _gameMain;
		private BBSprite _title;

		public bool Initialize(GameMain gameMain, out string reason)
		{
			_gameMain = gameMain;
			_title = SpriteManager.GetSprite("Title", _gameMain.Random);
			if (_title == null)
			{
				reason = "Title Sprite not found";
				return false;
			}
			reason = null;
			return true;
		}

		public void DrawScreen()
		{
			_title.Draw(_gameMain.ScreenSize.X / 2 - 400, (_gameMain.ScreenSize.Y / 2) - 300);
		}

		public void Update(int x, int y, float frameDeltaTime)
		{
			
		}

		public void MouseDown(int x, int y)
		{
			
		}

		public void MouseUp(int x, int y)
		{
			
		}

		public void MouseScroll(int direction, int x, int y)
		{
			
		}

		public void KeyDown(KeyboardInputEventArgs e)
		{
			
		}
	}
}
