using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
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
		InGame,
		Upgrade,
	};

	public class GameMain
	{
		public const int CELL_SIZE = 320;

		#region Screens
		private ScreenInterface _screenInterface;
		private MainMenu _mainMenu;
		private MultiplayerGameSetup _multiplayerGameSetup;
		private InGame _inGame;
		private UpgradeAndWaitScreen _upgradeAndWaitScreen;

		private Screen _currentScreen;
		#endregion

		private Xasteroids _parentForm;

		private BackgroundStars _backgroundStars;

		public PlayerManager PlayerManager { get; private set; }
		public AsteroidManager AsteroidManager { get; private set; }
		public ObjectManager ObjectManager { get; private set; }
		public Random Random { get; private set; }
		public Point MousePos;
		public Point ScreenSize { get; private set; }
		public GorgonLibrary.Graphics.FXShader ShipShader { get; private set; }

		private BBSprite Cursor;
		private Host _host;
		private Client _client;
		public ShipSelectionWindow ShipSelectionWindow { get; private set; }
		
		public object ChatLock = new object();
		public bool NewChatMessage;
		public StringBuilder ChatText;

		public object PlayerListLock = new object();
		public bool NewPlayerListUpdate;
		public PlayerList PlayerList { get; private set; }

		private int _mainPlayerID;
		public int MainPlayerID //Used by client to know which player instance to update
		{
			get { return _mainPlayerID; }
			set
			{
				_mainPlayerID = value;
				MainPlayer = PlayerManager.Players[MainPlayerID];
			}
		} 
		public Player MainPlayer { get; private set; } 

		public bool IsHost
		{ 
			get { return _host != null; }
		}
		public bool IsConnected
		{
			get { return IsHost ? !_host.IsShutDown : _client != null ? !_client.IsShutDown : false; }
		}
		public int LevelNumber { get; set; }
		public Point LevelSize { get; private set; }

		public bool Initialize(int screenWidth, int screenHeight, Xasteroids parentForm, out string reason)
		{
			_parentForm = parentForm;
			Random = new Random();
			MousePos = new Point();
			ScreenSize = new Point(screenWidth, screenHeight);
			PlayerList = new PlayerList();

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

			PlayerManager = new PlayerManager(this);
			AsteroidManager = new AsteroidManager(this);
			ObjectManager = new ObjectManager(this);
			ShipSelectionWindow = new ShipSelectionWindow();
			ShipSelectionWindow.ShipSelected += OnShipSelected;
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

			ChatText = new StringBuilder();
			NewChatMessage = false;

			return true;
		}

		public void ExitGame()
		{
			//dispose of any resources in use
			_parentForm.Close();
		}

		public void PayPlayer(string playerName, int amount)
		{
			foreach (Player player in PlayerManager.Players)
			{
				if (player.Name.Equals(playerName))
				{
					player.Bank += amount;
					break;
				}
			}
		}

		//Handle events
		public void ProcessGame(float frameDeltaTime)
		{
			_backgroundStars.Draw();

			_screenInterface.Update(MousePos.X, MousePos.Y, frameDeltaTime);
			_screenInterface.DrawScreen();

			if (_currentScreen != Screen.InGame || MainPlayer.IsDead)
			{
				Cursor.Draw(MousePos.X, MousePos.Y);
				Cursor.Update(frameDeltaTime, Random);
			}
		}

		public void ConnectToHostAt(IPAddress address)
		{
			if (_host != null)
			{
				_host.ShutDown();
				_host.ObjectReceived -= HandleObject;
				_host = null;
			}
			if (_client != null)
			{
				_client.ShutDown();
				_client.Disconnected -= OnDisconnected;
				_client.ObjectReceived -= HandleObject;
			}
			_client = new Client { ServerIPAddress = address };
			/* Client will only have non-null values for ServerIPAddress
			 * if it is connected.
			 */
			if (_client.ServerIPAddress != null)
			{
				SendPlayerNameToHost();
				ChangeToScreen(Screen.MultiplayerPreGameClient);
				_client.Disconnected += OnDisconnected;
				_client.ObjectReceived += HandleObject;
			}
		}

		public void SendPlayerNameToHost()
		{
			_client.SendObjectTcp(new NetworkMessage { Content = "Name:" + _mainMenu.PlayerName });
		}

		public void OnPlayerReady(Player player)
		{
			//single player
			if (_host == null || _client == null)
			{
				LevelNumber++;
				SetupLevel();
				PlayerManager.ResetPlayerPositions();
				ChangeToScreen(Screen.InGame);
				return;
			}

			if (_host != null)
			{
				List<Ship> ships = new List<Ship>();
				foreach (Player p in PlayerManager.Players)
				{
					ships.Add(new Ship {
						OwnerName = p.Name,
						ShieldAlpha = p.ShieldAlpha,
						Size = p.ShipSize,
						Style = p.ShipStyle,
						Color = p.ShipColor,
						Mass = p.Mass
					});
				}
				_host.SendObjectTCP(new ShipList { Ships = ships });
				return;
			}

			if (_client != null && _client.ServerIPAddress != null)
			{
				_client.SendObjectTcp(new NetworkMessage { Content = "Ready" });
				return;
			}
		}

		public void OnShipSelected(Ship ship)
		{
			if (_client != null && _client.ServerIPAddress != null)
			{
				_client.SendObjectTcp(ship);
				PlayerManager.Players[MainPlayerID] = new Player(ship.Style, ship.Size, ship.Color);
			}
		}

		private void OnDisconnected()
		{
			if (_currentScreen != Screen.MainMenu)
			{
				ChangeToScreen(Screen.MainMenu);
			}
			MessageBox.Show("Your connection with the host has been severed.");
		}

		private void HandleObject(IPAddress senderIPAddress, IConfigurable theObject)
		{
			if (theObject is GameMessage)
			{
				lock (ChatLock)
				{
					NewChatMessage = true;
					var gameMessage = (GameMessage)theObject;
					ChatText.AppendLine(gameMessage.Content);
					if (IsHost)
					{
						_host.SendObjectTCP(gameMessage);
					}
				}
				return;
			}

			if (theObject is PlayerList)
			{
				lock (PlayerListLock)
				{
					NewPlayerListUpdate = true;
					PlayerList = (PlayerList)theObject;
				}
			}

			if (theObject is NetworkMessage)
			{
				ReceiveNetworkMessage(senderIPAddress, (NetworkMessage)theObject);
				return;
			}

			if (theObject is Ship)
			{
				var theShip = (Ship)theObject;
				string ownerName = theShip.OwnerName;
				foreach (var item in _clientAddressesAndMonikers)
				{
					if (item.Value[NAME].Equals(ownerName))
					{
						var thatPlayer = PlayerManager.Players[(int)item.Value[ID]];
						if (thatPlayer == null)
						{
							var player = new Player(theShip.Size, theShip.Style, theShip.Color);
							PlayerManager.AddPlayer(player);
						}
						else
						{
							thatPlayer.ShipSize = theShip.Size;
							thatPlayer.ShipStyle = theShip.Style;
							thatPlayer.ShipColor = theShip.Color;
						}
						return;
					}
				}
			}

			if (theObject is ShipList)
			{
				var ships = ((ShipList)theObject).Ships;
				var thePlayers = PlayerManager.Players;
				while (thePlayers.Count < ships.Count)
				{
					thePlayers.Add(null);
				}
				for (int j = 0; j < ships.Count; ++j)
				{
					if (j == MainPlayerID)
					{
						continue;
					}
					var theShip = ships[j];
					thePlayers[j] = new Player(theShip.Size, theShip.Style, theShip.Color);
				}
				return;
			}
		}

		private Regex _nameRegex = new Regex(@"^Name:(.*)$", RegexOptions.Compiled);
		private Regex _yourIDRegex = new Regex(@"^Your ID:(\d)$", RegexOptions.Compiled);
		private Regex _changeScreenRegex = new Regex(@"Change to (\w+) Screen\.", RegexOptions.Compiled);

		// The monikers are a name string and an int ID, in that order.
		private const int NAME = 0;
		private const int ID = 1;
		private Dictionary<IPAddress, List<object>> _clientAddressesAndMonikers = new Dictionary<IPAddress, List<object>>();
		public Dictionary<IPAddress, List<object>> ShoppingPlayers;

		public void ReceiveNetworkMessage(IPAddress senderAddress, NetworkMessage message)
		{
			Match match = _nameRegex.Match(message.Content);
			if (match.Success)
			{
				if (_host == null)
				{
					//Only the host keeps track of clients and ids and etc
					return;
				}

				string clientPlayerName = match.Groups[1].Value;
				if (_clientAddressesAndMonikers.ContainsKey(senderAddress))
				{
					_clientAddressesAndMonikers[senderAddress][NAME] = clientPlayerName;
				}
				else
				{
					var monikers = new List<object> {
						clientPlayerName,
						null
					};
					_clientAddressesAndMonikers.Add(senderAddress, monikers);
				}
				//Make a list of player names and send to clients
				string list = string.Empty;
				foreach (var moniker in _clientAddressesAndMonikers)
				{
					list = list + moniker.Value[NAME] + "|";
				}
				PlayerList.Configuration = new[] {list};
				_host.SendObjectTCP(PlayerList);
				return;
			}

			match = _yourIDRegex.Match(message.Content);
			if (match.Success)
			{
				int id = int.Parse(match.Groups[1].Value);
				while (PlayerManager.Players.Count < id + 1)
				{
					PlayerManager.AddPlayer(new Player(1, 1, Color.Red));
				}
				MainPlayerID = id;
				return;
			}

			match = _changeScreenRegex.Match(message.Content);
			if (match.Success)
			{
				if (match.Groups[1].Value.Equals(Screen.Upgrade.ToString()))
				{
					ChangeToScreen(Screen.Upgrade);
				}
				return;
			}

			if (_host != null && message.Content.Equals("Ready"))
			{
				ShoppingPlayers.Remove(senderAddress);
				if (ShoppingPlayers.Count == 0)
				{
					_upgradeAndWaitScreen.EnableTheReadyButton();
				}
			}
		}

		public void AssignPlayerIDs()
		{
			var themPlayers = PlayerManager.Players;
			int id = 1;
			foreach (var item in _clientAddressesAndMonikers)
			{
				var address = item.Key;
				if (_host.HasConnectionTo(item.Key))
				{
					_clientAddressesAndMonikers[address][ID] = id;
					_host.SendObjectTcpToClient(new NetworkMessage { Content = "Your ID:" + id.ToString() }, address);
					++id;
				}
			}
		}

		public void SendChat(string message)
		{
			var gameMessage = new GameMessage();
			gameMessage.Content = message;
			if (IsHost)
			{
				_host.SendObjectTCP(gameMessage);
				ChatText.AppendLine(message);
				NewChatMessage = true;
			}
			else
			{
				_client.SendObjectTcp(gameMessage);
			}
		}

		public void ChangeToScreen(Screen whichScreen)
		{
			_currentScreen = whichScreen;
			switch (whichScreen)
			{
				case Screen.MainMenu:
				{
					//Main Menu is always initialized before this point
					PlayerManager.Players.Clear();
					LevelNumber = 100;
					SetupLevel();
					_screenInterface = _mainMenu;
					// Clean up _host if we have left MultiplayerPreGameServer
					if (_host != null)
					{
						if (!_host.IsShutDown)
						{
							_host.ShutDown();
						}
						_host.ObjectReceived -= HandleObject;
						_host = null;
						_clientAddressesAndMonikers.Clear();
					}
					// Clean up _client if we have left MultiplayerPreGameServer
					if (_client != null)
					{
						if (!_client.IsShutDown)
						{
							_client.ShutDown();
						}
						_client.ObjectReceived -= HandleObject;
						_client = null;
					}
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
					if (_client != null)
					{
						_client.ShutDown();
						_client = null;
					}
					_host = new Host { CurrentlyAcceptingPlayers = true };
					_host.ObjectReceived += HandleObject;
					_screenInterface = _multiplayerGameSetup;
					break;
				}
				case Screen.InGame:
				{
					if (_inGame == null)
					{
						string reason;
						_inGame = new InGame();
						if (!_inGame.Initialize(this, out reason))
						{
							MessageBox.Show("Error in loading In-Game Screen.  Reason: " + reason);
							ExitGame();
						}
					}
					ObjectManager.Clear();
					_screenInterface = _inGame;
					break;
				}
				case Screen.Upgrade:
				{
					if (_upgradeAndWaitScreen == null)
					{
						string reason;
						_upgradeAndWaitScreen = new UpgradeAndWaitScreen();
						if (!_upgradeAndWaitScreen.Initialize(this, out reason))
						{
							MessageBox.Show("Error in loading Upgrade Screen.  Reason: " + reason);
							ExitGame();
						}
					}
					if (IsHost)
					{
						if (PlayerManager.Players.Count == 0)
						{
							PlayerManager.Players.Add(new Player(1, 1, Color.Red));
						}
						else if (PlayerManager.Players[MainPlayerID] == null)
						{
							PlayerManager.Players[MainPlayerID] = new Player(1, 1, Color.Red);
						}
						MainPlayerID = 0;
					}
					_upgradeAndWaitScreen.RefreshLabels();
					_screenInterface = _upgradeAndWaitScreen;
					AssignPlayerIDs();
					if (IsHost)
					{
						if (ShoppingPlayers == null)
						{
							ShoppingPlayers = new Dictionary<IPAddress, List<Object>>();
						}
						foreach (var item in _clientAddressesAndMonikers)
						{
							if (item.Value[ID] != null)
							{
								ShoppingPlayers.Add(item.Key, item.Value);
							}
						}
						if (ShoppingPlayers.Count > 0)
						{
							_upgradeAndWaitScreen.DisableTheReadyButton();
						}
						_host.SendObjectTCP(new NetworkMessage { Content = "Change to " + Screen.Upgrade.ToString() + " Screen." });
					}
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
			float x, y;
			
			if (_currentScreen != Screen.InGame)
			{
				//Put in center of level
				x = LevelSize.X / 2;
				y = LevelSize.Y / 2;
			}
			else
			{
				x = MainPlayer.PositionX;
				y = MainPlayer.PositionY;
			}

			int screenWidth = ScreenSize.X / 2;
			int screenHeight = ScreenSize.Y / 2;

			float leftBounds = x - screenWidth;
			float rightBounds = x + screenWidth;
			float topBounds = y - screenHeight;
			float bottomBounds = y + screenHeight;

			bool overlapsLeft = (leftBounds - 80) < 0;
			bool overlapsRight = !overlapsLeft && rightBounds + 80 >= LevelSize.X;
			bool overlapsTop = (topBounds - 80) < 0;
			bool overlapsBottom = !overlapsTop && bottomBounds + 80 >= LevelSize.Y;

			foreach (var bullet in ObjectManager.Bullets)
			{
				int size = (int)(bullet.Scale * 2); //For performance, cache the value
				float modifiedX = bullet.PositionX;
				float modifiedY = bullet.PositionY;

				if (overlapsLeft && bullet.PositionX >= rightBounds + size)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedX -= LevelSize.X;
				}
				else if (overlapsRight && bullet.PositionX < leftBounds - size)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedX += LevelSize.X;
				}
				if (overlapsTop && bullet.PositionY >= bottomBounds + size)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedY -= LevelSize.Y;
				}
				else if (overlapsBottom && bullet.PositionY < topBounds - size)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedY += LevelSize.Y;
				}
				if (modifiedX >= leftBounds - size && modifiedX < rightBounds + size && modifiedY >= topBounds - size && modifiedY < bottomBounds + size)
				{
					//It is visible
					ObjectManager.BulletSprite.Draw((modifiedX + screenWidth) - x, (modifiedY + screenHeight) - y, bullet.Scale, bullet.Scale, bullet.Color);
				}
			}

			foreach (var asteroid in AsteroidManager.Asteroids)
			{
				int size = asteroid.Radius; //For performance, cache the value
				float modifiedX = asteroid.PositionX;
				float modifiedY = asteroid.PositionY;

				if (overlapsLeft && asteroid.PositionX >= rightBounds + size)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedX -= LevelSize.X;
				}
				else if (overlapsRight && asteroid.PositionX < leftBounds - size)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedX += LevelSize.X;
				}
				if (overlapsTop && asteroid.PositionY >= bottomBounds + size)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedY -= LevelSize.Y;
				}
				else if (overlapsBottom && asteroid.PositionY < topBounds - size)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedY += LevelSize.Y;
				}
				
				if (modifiedX >= leftBounds - size && modifiedX < rightBounds + size && modifiedY >= topBounds - size && modifiedY < bottomBounds + size)
				{
					//It is visible
					asteroid.AsteroidSprite.Draw((modifiedX + screenWidth) - x, (modifiedY + screenHeight) - y, 1, 1, Color.FromArgb((int)asteroid.Phase, asteroid.Color), asteroid.Angle);
				}
			}

			foreach (var player in PlayerManager.Players)
			{
				if (player.IsDead)
				{
					//No need to draw dead players
					continue;
				}
				if (player == MainPlayer)
				{
					//Always in center of screen, just draw it there
					GorgonLibrary.Gorgon.CurrentShader = ShipShader;
					ShipShader.Parameters["EmpireColor"].SetValue(player.ShipConvertedColor);
					player.ShipSprite.Draw(screenWidth, screenHeight, 1, 1, Color.White, player.Angle);
					GorgonLibrary.Gorgon.CurrentShader = null;
					if (player.ShieldAlpha > 0)
					{
						//Shield was recently activated, display it
						byte alpha = (byte)(player.ShieldAlpha * 255);
						player.ShieldSprite.Draw(screenWidth, screenHeight, 1, 1, Color.FromArgb(alpha, alpha, alpha, alpha));
					}
				}
				int size = 16 * player.ShipSize; //For performance, cache the value
				float modifiedX = player.PositionX;
				float modifiedY = player.PositionY;

				if (overlapsLeft && player.PositionX >= rightBounds + size)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedX -= LevelSize.X;
				}
				else if (overlapsRight && player.PositionX < leftBounds - size)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedX += LevelSize.X;
				}
				if (overlapsTop && player.PositionY >= bottomBounds + size)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedY -= LevelSize.Y;
				}
				else if (overlapsBottom && player.PositionY < topBounds - size)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedY += LevelSize.Y;
				}

				if (modifiedX >= leftBounds - size && modifiedX < rightBounds + size && modifiedY >= topBounds - size && modifiedY < bottomBounds + size)
				{
					//It is visible
					GorgonLibrary.Gorgon.CurrentShader = ShipShader;
					ShipShader.Parameters["EmpireColor"].SetValue(player.ShipConvertedColor);
					player.ShipSprite.Draw((modifiedX + screenWidth) - x, (modifiedY + screenHeight) - y, 1, 1, Color.White, player.Angle);
					GorgonLibrary.Gorgon.CurrentShader = null;
					if (player.ShieldAlpha > 0)
					{
						//Shield was recently activated, display it
						byte alpha = (byte)(player.ShieldAlpha * 255);
						player.ShieldSprite.Draw((modifiedX + screenWidth) - x, (modifiedY + screenHeight) - y, 1, 1, Color.FromArgb(alpha, alpha, alpha, alpha));
					}
				}
			}

			foreach (var explosion in ObjectManager.Explosions)
			{
				int size = explosion.Size;
				float modifiedX = explosion.PositionX;
				float modifiedY = explosion.PositionY;

				if (overlapsLeft && explosion.PositionX >= rightBounds + size)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedX -= LevelSize.X;
				}
				else if (overlapsRight && explosion.PositionX < leftBounds - size)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedX += LevelSize.X;
				}
				if (overlapsTop && explosion.PositionY >= bottomBounds + size)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedY -= LevelSize.Y;
				}
				else if (overlapsBottom && explosion.PositionY < topBounds - size)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedY += LevelSize.Y;
				}
				if (modifiedX >= leftBounds - size && modifiedX < rightBounds + size && modifiedY >= topBounds - size && modifiedY < bottomBounds + size)
				{
					//It is visible
					explosion.Sprite.Draw((modifiedX + screenWidth) - x, (modifiedY + screenHeight) - y);
				}
			}

			foreach (var shockwave in ObjectManager.Shockwaves)
			{
				float radius = shockwave.Radius * (1 - (shockwave.TimeTilBoom / 0.2f));
				float modifiedX = shockwave.PositionX;
				float modifiedY = shockwave.PositionY;

				if (overlapsLeft && shockwave.PositionX >= rightBounds + radius)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedX -= LevelSize.X;
				}
				else if (overlapsRight && shockwave.PositionX < leftBounds - radius)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedX += LevelSize.X;
				}
				if (overlapsTop && shockwave.PositionY >= bottomBounds + radius)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedY -= LevelSize.Y;
				}
				else if (overlapsBottom && shockwave.PositionY < topBounds - radius)
				{
					//It's on other side of screen, check and see if it could be visible due to overlap
					modifiedY += LevelSize.Y;
				}
				if (modifiedX >= leftBounds - radius && modifiedX < rightBounds + radius && modifiedY >= topBounds - radius && modifiedY < bottomBounds + radius)
				{
					float scale = (radius / 100);
					//It is visible
					ObjectManager.ShockwaveSprite.Draw(modifiedX + screenWidth - x, modifiedY + screenHeight - y, scale, scale);
				}
			}
		}

		public void MoveStars(float xAmount, float yAmount)
		{
			_backgroundStars.Move(xAmount, yAmount);
		}

		public void ResetGame()
		{
			LevelNumber = 0;
			ObjectManager.Clear();
		}

		public void SetupLevel()
		{
			/*List<AsteroidType> asteroidsToInlcude = new List<AsteroidType>
			{
				//AsteroidType.GENERIC, 
				//AsteroidType.CLUMPY,
				//AsteroidType.DENSE, 
				AsteroidType.EXPLOSIVE, 
				//AsteroidType.BLACK, 
				//AsteroidType.GOLD,
				//AsteroidType.GRAVITIC, 
				//AsteroidType.MAGNETIC, 
				//AsteroidType.PHASING,
				//AsteroidType.REPULSER, 
				//AsteroidType.ZIPPY
													};*/
			var types = new List<AsteroidType>();
			types.Add(AsteroidType.GENERIC);
			if (LevelNumber > 5)
			{
				types.Add(AsteroidType.CLUMPY);
			}
			if (LevelNumber > 10)
			{
				types.Add(AsteroidType.DENSE);
			}
			if (LevelNumber > 15)
			{
				types.Add(AsteroidType.EXPLOSIVE);
			}
			if (LevelNumber > 20)
			{
				types.Add(AsteroidType.BLACK);
			}
			if (LevelNumber > 25)
			{
				types.Add(AsteroidType.GOLD);
			}
			if (LevelNumber > 30)
			{
				types.Add(AsteroidType.GRAVITIC);
			}
			if (LevelNumber > 35)
			{
				types.Add(AsteroidType.MAGNETIC);
			}
			if (LevelNumber > 40)
			{
				types.Add(AsteroidType.PHASING);
			}
			if (LevelNumber > 45)
			{
				types.Add(AsteroidType.REPULSER);
			}
			if (LevelNumber > 50)
			{
				types.Add(AsteroidType.ZIPPY);
			}

			int numOfTypes = Random.Next(1, 5);
			var asteroidsToInlcude = new List<AsteroidType>();
			for (int i = 0; i < numOfTypes; i++)
			{
				asteroidsToInlcude.Add(types[Random.Next(types.Count)]);
			}
			//Will split the level up into 320x320 sections for performance, 160 is the largest object's size
			LevelSize = new Point(10 * CELL_SIZE, 10 * CELL_SIZE);

			AsteroidManager.SetUpLevel(asteroidsToInlcude.ToArray(), LevelNumber * 10 * (PlayerManager.Players.Count == 0 ? 1 : PlayerManager.Players.Count), Random);
		}

		public bool IsKeyDown(KeyboardKeys whichKey)
		{
			return _parentForm.IsKeyDown(whichKey);
		}
	}
}
