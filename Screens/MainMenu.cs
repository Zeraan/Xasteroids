using System.Collections.Generic;
using System.Drawing;
using GorgonLibrary.InputDevices;

namespace Xasteroids.Screens
{
	public class MainMenu : ScreenInterface
	{
		private GameMain _gameMain;
		private BBSprite _title;

		private BBButton _singlePlayerButton;
		private BBButton _multiPlayerButton;
		private BBButton _exitButton;

		private BBButton _hostOrConnectButton;
		private BBSingleLineTextBox _ipAddressTextBox;
		private BBSingleLineTextBox _playerNameTextBox;
		private BBButton _cancelButton;

		private bool _showingShipSelection;
		private bool _showingMultiplayerOptions;

		public bool Initialize(GameMain gameMain, out string reason)
		{
			_gameMain = gameMain;
			_title = SpriteManager.GetSprite("Title", _gameMain.Random);
			if (_title == null)
			{
				reason = "Title Sprite not found";
				return false;
			}

			_singlePlayerButton = new BBButton();
			_multiPlayerButton = new BBButton();
			_exitButton = new BBButton();

			_hostOrConnectButton = new BBButton();
			_cancelButton = new BBButton();
			_ipAddressTextBox = new BBSingleLineTextBox();
			_playerNameTextBox = new BBSingleLineTextBox();

			_showingMultiplayerOptions = false;
			_showingShipSelection = false;

			int x = _gameMain.ScreenSize.X / 2 - 130;
			int y = _gameMain.ScreenSize.Y / 2 + 50;

			if (!_singlePlayerButton.Initialize("MainButtonBG", "MainButtonFG", "Single Player", "LargeComputerFont", ButtonTextAlignment.CENTER, x, y, 260, 40, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_multiPlayerButton.Initialize("MainButtonBG", "MainButtonFG", "MultiPlayer", "LargeComputerFont", ButtonTextAlignment.CENTER, x, y + 100, 260, 40, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_exitButton.Initialize("MainButtonBG", "MainButtonFG", "Exit", "LargeComputerFont", ButtonTextAlignment.CENTER, x, y + 200, 260, 40, _gameMain.Random, out reason))
			{
				return false;
			}


			if (!_playerNameTextBox.Initialize("Player Name", x, y, 260, 40, false, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_ipAddressTextBox.Initialize(string.Empty, x - 150, y + 100, 260, 40, false, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_hostOrConnectButton.Initialize("MainButtonBG", "MainButtonFG", "Host", "LargeComputerFont", ButtonTextAlignment.CENTER, x + 150, y + 100, 260, 40, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_cancelButton.Initialize("MainButtonBG", "MainButtonFG", "Back", "LargeComputerFont", ButtonTextAlignment.CENTER, x, y + 200, 260, 40, _gameMain.Random, out reason))
			{
				return false;
			}
			_singlePlayerButton.SetTextColor(Color.Gold, Color.Black);
			_multiPlayerButton.SetTextColor(Color.Gold, Color.Black);
			_exitButton.SetTextColor(Color.Gold, Color.Black);
			_hostOrConnectButton.SetTextColor(Color.Gold, Color.Black);
			_cancelButton.SetTextColor(Color.Gold, Color.Black);

			AsteroidType[] typesToInclude = new [] {
													//AsteroidType.GENERIC, 
													//AsteroidType.EXPLOSIVE, 
													//AsteroidType.DENSE, 
													//AsteroidType.CLUMPY, 
													AsteroidType.BLACK, 
													//AsteroidType.GOLD, 
													//AsteroidType.GRAVITIC, 
													//AsteroidType.MAGNETIC, 
													//AsteroidType.PHASING, 
													//AsteroidType.REPULSER, 
													//AsteroidType.ZIPPY
												};
			_gameMain.LevelNumber = 100;
			_gameMain.SetupLevel();

			reason = null;
			return true;
		}

		public void DrawScreen()
		{
			_gameMain.DrawObjects();
			_title.Draw(_gameMain.ScreenSize.X / 2 - 400, (_gameMain.ScreenSize.Y / 2) - 300);
			if (!_showingMultiplayerOptions)
			{
				_singlePlayerButton.Draw();
				_multiPlayerButton.Draw();
				_exitButton.Draw();
			}
			else
			{
				_playerNameTextBox.Draw();
				_ipAddressTextBox.Draw();
				_hostOrConnectButton.Draw();
				_cancelButton.Draw();
			}
			if (_showingShipSelection)
			{
				_gameMain.ShipSelectionWindow.Draw();
			}
		}

		public void Update(int x, int y, float frameDeltaTime)
		{
			_gameMain.AsteroidManager.UpdatePhysics(null, frameDeltaTime, _gameMain.Random);
			_gameMain.AsteroidManager.Update(frameDeltaTime);

			if (_showingShipSelection)
			{
				_gameMain.ShipSelectionWindow.MouseHover(x, y, frameDeltaTime);
				return;
			}
			if (!_showingMultiplayerOptions)
			{
				_singlePlayerButton.MouseHover(x, y, frameDeltaTime);
				_multiPlayerButton.MouseHover(x, y, frameDeltaTime);
				_exitButton.MouseHover(x, y, frameDeltaTime);
			}
			else
			{
				_playerNameTextBox.Update(frameDeltaTime);
				_ipAddressTextBox.Update(frameDeltaTime);
				_hostOrConnectButton.MouseHover(x, y, frameDeltaTime);
				_cancelButton.MouseHover(x, y, frameDeltaTime);
			}
		}

		public void MouseDown(int x, int y)
		{
			if (_showingShipSelection)
			{
				_gameMain.ShipSelectionWindow.MouseDown(x, y);
				return;
			}
			if (!_showingMultiplayerOptions)
			{
				_singlePlayerButton.MouseDown(x, y);
				_multiPlayerButton.MouseDown(x, y);
				_exitButton.MouseDown(x, y);
			}
			else
			{
				_playerNameTextBox.MouseDown(x, y);
				_ipAddressTextBox.MouseDown(x, y);
				_hostOrConnectButton.MouseDown(x, y);
				_cancelButton.MouseDown(x, y);
			}
		}

		public void MouseUp(int x, int y)
		{
			if (_showingShipSelection)
			{
				if (!_gameMain.ShipSelectionWindow.MouseUp(x, y))
				{
					_showingShipSelection = false;
					_gameMain.ShipSelectionWindow.OnSelectShip = null;
				}
				return;
			}
			if (!_showingMultiplayerOptions)
			{
				if (_singlePlayerButton.MouseUp(x, y))
				{
					_gameMain.PlayerManager.ClearMainPlayer(_gameMain.Random);
					var player = _gameMain.PlayerManager.MainPlayer;
					_gameMain.ShipSelectionWindow.LoadShip(player.ShipSize, player.ShipStyle, player.ShipColor, player.Bank);
					_gameMain.ShipSelectionWindow.OnSelectShip = OnSelectShip;
					_showingShipSelection = true;
				}
				if (_multiPlayerButton.MouseUp(x, y))
				{
					_showingMultiplayerOptions = true;
					return;
				}
				if (_exitButton.MouseUp(x, y))
				{
					_gameMain.ExitGame();
				}
			}
			else
			{
				_playerNameTextBox.MouseUp(x, y);
				_ipAddressTextBox.MouseUp(x, y);
				if (_hostOrConnectButton.MouseUp(x, y))
				{
					//If client, initialize connection at this point, then change screen.  Otherwise, set up listen on port
					_gameMain.ChangeToScreen(_ipAddressTextBox.Text.Length == 0 ? Screen.MultiplayerPreGameServer : Screen.MultiplayerPreGameClient);
				}
				if (_cancelButton.MouseUp(x, y))
				{
					_showingMultiplayerOptions = false;
				}
			}
		}

		public void MouseScroll(int direction, int x, int y)
		{
			
		}

		public void KeyDown(KeyboardInputEventArgs e)
		{
			if (_playerNameTextBox.KeyDown(e))
			{
				return;
			}
			if (_ipAddressTextBox.KeyDown(e))
			{
				if (_ipAddressTextBox.Text.Length == 0)
				{
					_hostOrConnectButton.SetText("Host");
					_hostOrConnectButton.Active = true;
				}
				else
				{
					_hostOrConnectButton.SetText("Connect");
					_hostOrConnectButton.Active = IsValidIpAddress();
				}
			}
		}

		private bool IsValidIpAddress()
		{
			string[] components = _ipAddressTextBox.Text.Split(new[] {'.'});
			if (components.Length != 4)
			{
				return false;
			}
			foreach (var component in components)
			{
				int value;
				if (!int.TryParse(component, out value))
				{
					return false;
				}
				if (value < 0 || value > 255)
				{
					return false;
				}
			}
			return true;
		}

		private void OnSelectShip(int size, int style, Color color, int shipCost)
		{
			var player = _gameMain.PlayerManager.MainPlayer;
			player.ShipSize = size;
			player.ShipStyle = style;
			player.ShipColor = color;
			player.Bank -= shipCost;
			player.ShipSprite = SpriteManager.GetShipSprite(size, style, _gameMain.Random);

			_showingShipSelection = false;
			_gameMain.ShipSelectionWindow.OnSelectShip = null;

			_gameMain.ResetGame();
			_gameMain.SetupLevel();

			//Start the game!
			_gameMain.ChangeToScreen(Screen.InGame);
		}
	}
}
