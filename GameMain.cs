﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
		public object ChatLock;
		public bool NewChatMessage;
		public StringBuilder ChatText;

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

			if (_currentScreen != Screen.InGame || PlayerManager.MainPlayer.IsDead)
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
				ChangeToScreen(Screen.MultiplayerPreGameClient);
				_client.Disconnected += OnDisconnected;
				_client.ObjectReceived += HandleObject;
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
			var castObject = theObject as GameMessage;
			if (castObject != null)
			{
				lock (ChatLock)
				{
					NewChatMessage = true;
					ChatText.AppendLine(castObject.Content);
					if (IsHost)
					{
						_host.SendObjectTCP(theObject);
					}
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
					_upgradeAndWaitScreen.RefreshLabels();
					_screenInterface = _upgradeAndWaitScreen;
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
				x = PlayerManager.MainPlayer.PositionX;
				y = PlayerManager.MainPlayer.PositionY;
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
				if (player == PlayerManager.MainPlayer)
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
