using System;
using System.Windows.Forms;
using GorgonLibrary.InputDevices;
using Xasteroids.Screens;
using MainMenu = Xasteroids.Screens.MainMenu;
using MouseButtons = System.Windows.Forms.MouseButtons;

namespace Xasteroids
{
	public enum Screen
	{
		MainMenu,
		MultiplayerPreGameClient,
		MultiplayerPreGameServer,
	};

	public class GameMain
	{
		#region Screens
		private ScreenInterface _screenInterface;
		private MainMenu _mainMenu;
		private MultiplayerGameSetup _multiplayerGameSetup;

		private Screen _currentScreen;
		#endregion

		private Form _parentForm;

		private BackgroundStars _backgroundStars;

		public PlayerManager PlayerManager { get; private set; }
		public AsteroidManager AsteroidManager { get; private set; }
		public Random Random { get; private set; }
		public Point MousePos;
		public Point ScreenSize { get; private set; }
		public GorgonLibrary.Graphics.FXShader ShipShader { get; private set; }

		private BBSprite Cursor;

		public ShipSelectionWindow ShipSelectionWindow { get; private set; }

		public bool Initialize(int screenWidth, int screenHeight, Form parentForm, out string reason)
		{
			_parentForm = parentForm;
			Random = new Random();
			MousePos = new Point();
			ScreenSize = new Point(screenWidth, screenHeight);

			ShipShader = GorgonLibrary.Graphics.FXShader.FromFile("ColorShader.fx", GorgonLibrary.Graphics.ShaderCompileOptions.OptimizationLevel3);

			if (!SpriteManager.Initialize(out reason))
			{
				return false;
			}
			if (!FontManager.Initialize(out reason))
			{
				return false;
			}

			_backgroundStars = new BackgroundStars();
			if (!_backgroundStars.Initialize(this, out reason))
			{
				return false;
			}

			Cursor = SpriteManager.GetSprite("Cursor", Random);
			if (Cursor == null)
			{
				reason = "Cursor is not defined in sprites.xml";
				return false;
			}

			PlayerManager = new PlayerManager();
			AsteroidManager = new AsteroidManager();
			ShipSelectionWindow = new ShipSelectionWindow();
			if (!ShipSelectionWindow.Initialize(this, out reason))
			{
				return false;
			}

			_mainMenu = new MainMenu();
			if (!_mainMenu.Initialize(this, out reason))
			{
				return false;
			}

			_screenInterface = _mainMenu;
			_currentScreen = Screen.MainMenu;

			return true;
		}

		public void ExitGame()
		{
			//dispose of any resources in use
			_parentForm.Close();
		}

		//Handle events
		public void ProcessGame(float frameDeltaTime)
		{
			_backgroundStars.Draw();
			AsteroidManager.UpdateAsteroids(frameDeltaTime);
			_screenInterface.Update(MousePos.X, MousePos.Y, frameDeltaTime);
			_screenInterface.DrawScreen();

			Cursor.Draw(MousePos.X, MousePos.Y);
			Cursor.Update(frameDeltaTime, Random);
		}

		public void ChangeToScreen(Screen whichScreen)
		{
			switch (whichScreen)
			{
				case Screen.MainMenu:
				{
					//Main Menu is always initialized before this point
					_screenInterface = _mainMenu;
					break;
				}
				case Screen.MultiplayerPreGameClient:
				{
					if (_multiplayerGameSetup == null)
					{
						string reason;
						_multiplayerGameSetup = new MultiplayerGameSetup();
						if (!_multiplayerGameSetup.Initialize(this, out reason))
						{
							MessageBox.Show("Error in loading Multiplayer PreGame Screen.  Reason: " + reason);
							ExitGame();
						}
					}
					_multiplayerGameSetup.SetHost(false);
					_screenInterface = _multiplayerGameSetup;
					break;
				}
				case Screen.MultiplayerPreGameServer:
				{
					if (_multiplayerGameSetup == null)
					{
						string reason;
						_multiplayerGameSetup = new MultiplayerGameSetup();
						if (!_multiplayerGameSetup.Initialize(this, out reason))
						{
							MessageBox.Show("Error in loading Multiplayer PreGame Screen.  Reason: " + reason);
							ExitGame();
						}
					}
					_multiplayerGameSetup.SetHost(true);
					_screenInterface = _multiplayerGameSetup;
					break;
				}
			}
		}

		public void MouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				_screenInterface.MouseDown(e.X, e.Y);
			}
		}

		public void MouseUp(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				_screenInterface.MouseUp(e.X, e.Y);
			}
		}

		public void MouseScroll(int delta)
		{
			_screenInterface.MouseScroll(delta, MousePos.X, MousePos.Y);
		}

		public void KeyDown(KeyboardInputEventArgs e)
		{
			_screenInterface.KeyDown(e);
		}

		public void DrawObjects()
		{
			//Draws the asteroids, and if a game is in-progress, ships, weapons, and effects
			//First, take the current player's position
			int x = 0, y = 0;
			
			//if (_currentScreen != Screen.InGame)
			{
				//Put in center of level
				x = AsteroidManager.LevelSize.X / 2;
				y = AsteroidManager.LevelSize.Y / 2;
			}

			int screenWidth = ScreenSize.X / 2;
			int screenHeight = ScreenSize.Y / 2;

			int leftBounds = x - screenWidth;
			int rightBounds = x + screenWidth;
			int topBounds = y - screenHeight;
			int bottomBounds = y + screenHeight;

			bool overlapsLeft = (leftBounds - 80) < 0;
			bool overlapsRight = !overlapsLeft && rightBounds + 80 >= AsteroidManager.LevelSize.X;
			bool overlapsTop = (topBounds - 80) < 0;
			bool overlapsBottom = !overlapsTop && bottomBounds + 80 >= AsteroidManager.LevelSize.Y;

			foreach (var asteroid in AsteroidManager.Asteroids)
			{
				int size = 16 * asteroid.Size; //For performance, cache the value
				float modifiedX = asteroid.PositionX;
				float modifiedY = asteroid.PositionY;

				if (overlapsLeft && asteroid.PositionX >= rightBounds + size)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedX -= AsteroidManager.LevelSize.X;
				}
				else if (overlapsRight && asteroid.PositionX < leftBounds - size)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedX += AsteroidManager.LevelSize.X;
				}
				if (overlapsTop && asteroid.PositionY >= bottomBounds + size)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedY -= AsteroidManager.LevelSize.Y;
				}
				else if (overlapsBottom && asteroid.PositionY < topBounds - size)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedY += AsteroidManager.LevelSize.Y;
				}
				
				if (modifiedX >= leftBounds - size && modifiedX < rightBounds + size && modifiedY >= topBounds - size && modifiedY < bottomBounds + size)
				{
					//It is visible
					asteroid.AsteroidSprite.Draw((modifiedX + screenWidth) - x, (modifiedY + screenHeight) - y, 1, 1, asteroid.Color, asteroid.Angle);
				}
			}
		}
	}
}
