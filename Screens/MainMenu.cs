using System.Drawing;
using System.Net;
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

		private bool _showingMultiplayerOptions;

		private BBLabel _debugText;

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
			_debugText = new BBLabel();

			_showingMultiplayerOptions = false;

			int x = _gameMain.ScreenSize.X / 2 - 130;
			int y = _gameMain.ScreenSize.Y / 2 + 50;

			if (!_singlePlayerButton.Initialize("MainButtonBG", "MainButtonFG", "Single Player", "LargeComputerFont", ButtonTextAlignment.CENTER, x, y + 50, 260, 40, _gameMain.Random, out reason))
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
			if (!_ipAddressTextBox.Initialize(string.Empty, x - 150, y + 50, 260, 40, false, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_hostOrConnectButton.Initialize("MainButtonBG", "MainButtonFG", "Host", "LargeComputerFont", ButtonTextAlignment.CENTER, x + 150, y + 50, 260, 40, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_cancelButton.Initialize("MainButtonBG", "MainButtonFG", "Back", "LargeComputerFont", ButtonTextAlignment.CENTER, x, y + 100, 260, 40, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_debugText.Initialize(10, _gameMain.ScreenSize.Y - 30, string.Empty, Color.White, out reason))
			{
				return false;
			}
			_singlePlayerButton.SetTextColor(Color.Gold, Color.Black);
			_multiPlayerButton.SetTextColor(Color.Gold, Color.Black);
			_exitButton.SetTextColor(Color.Gold, Color.Black);
			_hostOrConnectButton.SetTextColor(Color.Gold, Color.Black);
			_cancelButton.SetTextColor(Color.Gold, Color.Black);

			_gameMain.LevelNumber = 100;
			_gameMain.SetupLevel();
			_debugText.SetText("Num of Asteroids: " + _gameMain.AsteroidManager.Asteroids.Count);

			reason = null;
			return true;
		}

		public void DrawScreen()
		{
			_gameMain.DrawObjects();
			_title.Draw(_gameMain.ScreenSize.X / 2 - 400, (_gameMain.ScreenSize.Y / 2) - 300);
			_playerNameTextBox.Draw();
			if (!_showingMultiplayerOptions)
			{
				_singlePlayerButton.Draw();
				_multiPlayerButton.Draw();
				_exitButton.Draw();
			}
			else
			{
				_ipAddressTextBox.Draw();
				_hostOrConnectButton.Draw();
				_cancelButton.Draw();
			}
			_debugText.Draw();
		}

		public void Update(int x, int y, float frameDeltaTime)
		{
			_gameMain.AsteroidManager.UpdatePhysics(null, null, null, frameDeltaTime, _gameMain.Random);
			_gameMain.AsteroidManager.Update(frameDeltaTime);

			_playerNameTextBox.Update(frameDeltaTime);
			if (!_showingMultiplayerOptions)
			{
				_singlePlayerButton.MouseHover(x, y, frameDeltaTime);
				_multiPlayerButton.MouseHover(x, y, frameDeltaTime);
				_exitButton.MouseHover(x, y, frameDeltaTime);
			}
			else
			{
				_ipAddressTextBox.Update(frameDeltaTime);
				_hostOrConnectButton.MouseHover(x, y, frameDeltaTime);
				_cancelButton.MouseHover(x, y, frameDeltaTime);
			}

			_debugText.SetText("Num of Asteroids: " + _gameMain.AsteroidManager.Asteroids.Count);
		}

		public void MouseDown(int x, int y)
		{
			_playerNameTextBox.MouseDown(x, y);
			if (!_showingMultiplayerOptions)
			{
				_singlePlayerButton.MouseDown(x, y);
				_multiPlayerButton.MouseDown(x, y);
				_exitButton.MouseDown(x, y);
			}
			else
			{
				_ipAddressTextBox.MouseDown(x, y);
				_hostOrConnectButton.MouseDown(x, y);
				_cancelButton.MouseDown(x, y);
			}
		}

		public void MouseUp(int x, int y)
		{
			_playerNameTextBox.MouseUp(x, y);
			if (!_showingMultiplayerOptions)
			{
				if (_singlePlayerButton.MouseUp(x, y))
				{
					if (_gameMain.PlayerManager.Players.Count == 0)
					{
						_gameMain.PlayerManager.AddPlayer(new Player(1, 1, Color.Red));
					}
					else
					{
						_gameMain.PlayerManager.Players[0] = new Player(1, 1, Color.Red);
					}
					_gameMain.MainPlayerID = 0;
					var player = _gameMain.MainPlayer;
					player.Bank = 1000;
					player.IsDead = true;
					player.Name = _playerNameTextBox.Text;
					_gameMain.ResetGame();
					_gameMain.PlayerManager.ResetPlayerPositions();
					//Start the game!
					_gameMain.ChangeToScreen(Screen.Upgrade);
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
				_ipAddressTextBox.MouseUp(x, y);
				if (_hostOrConnectButton.MouseUp(x, y))
				{
					//If client, initialize connection at this point, then change screen.  Otherwise, set up listen on port
					
					if (_ipAddressTextBox.Text.Length > 0)
					{
						IPAddress hostAddress;
						if (IPAddress.TryParse(_ipAddressTextBox.Text, out hostAddress))
						{
							_gameMain.ConnectToHostAt(hostAddress);
						}
					}
					else
					{
						_gameMain.ChangeToScreen(Screen.MultiplayerPreGameServer);
					}
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
	}
}
