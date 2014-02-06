using System;
using System.Drawing;
using GorgonLibrary.Graphics;
using GorgonLibrary.InputDevices;

namespace Xasteroids.Screens
{
	public class InGame : ScreenInterface
	{
		private GameMain _gameMain;
		private BBStretchableImage _shipStatsBackground;
		private BBSprite _horizontalEnergyBar;
		private int _energyX;
		private BBLabel _bankAmount;
		private BBLabel _energyAmount;
		//private BBTextBox _debuggingText;
		private BBStretchableImage _miniMapBackground;
		private RenderImage _miniMapTarget;
		private BBSprite _dot;
		private float _delay;

		public bool Initialize(GameMain gameMain, out string reason)
		{
			_gameMain = gameMain;

			_energyX = (_gameMain.ScreenSize.X / 2) - 190;
			_shipStatsBackground = new BBStretchableImage();
			_miniMapBackground = new BBStretchableImage();
			if (!_shipStatsBackground.Initialize((_gameMain.ScreenSize.X / 2) - 200, -20, 400, 90, StretchableImageType.ThinBorderBG, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_miniMapBackground.Initialize(_gameMain.ScreenSize.X - 250, _gameMain.ScreenSize.Y - 250, 300, 300, StretchableImageType.ThinBorderBG, _gameMain.Random, out reason))
			{
				return false;
			}

			_bankAmount = new BBLabel();
			_energyAmount = new BBLabel();

			if (!_bankAmount.Initialize(_energyX, 40, string.Empty, Color.White, out reason))
			{
				return false;
			}
			if (!_energyAmount.Initialize(_energyX + 380, 40, string.Empty, Color.White, out reason))
			{
				return false;
			}
			_energyAmount.SetAlignment(true);

			_horizontalEnergyBar = SpriteManager.GetSprite("ScrollHorizontalBar", gameMain.Random);
			if (_horizontalEnergyBar == null)
			{
				reason = "ScrollHorizontalBar sprite not found";
				return false;
			}

			/*_debuggingText = new BBTextBox();
			if (!_debuggingText.Initialize(0, 0, 300, 300, true, false, "DebugText", gameMain.Random, out reason))
			{
				return false;
			}*/

			_miniMapTarget = new RenderImage("MiniMapRender", 230, 230, ImageBufferFormats.BufferRGB888A8);
			_miniMapTarget.BlendingMode = BlendingModes.Modulated;

			_dot = SpriteManager.GetSprite("BackgroundStar", _gameMain.Random);
			if (_dot == null)
			{
				reason = "Star sprite doesn't exist.";
				return false;
			}
		    _delay = 0;
			return true;
		}

		public void DrawScreen()
		{
			_gameMain.DrawObjects();
			_shipStatsBackground.Draw(125);
			_miniMapBackground.Draw(125);
			var currentPlayer = _gameMain.MainPlayer;
			float percentage = currentPlayer.Energy / currentPlayer.MaxEnergy;
			_horizontalEnergyBar.Draw(_energyX, 7, (380.0f / _horizontalEnergyBar.Width) * percentage, 2, Color.FromArgb(200, Color.LawnGreen));
			_bankAmount.Draw();
			_energyAmount.Draw();
			//_debuggingText.Draw();
			DrawMiniMap();
		}

		private void DrawMiniMap()
		{
			_miniMapTarget.Clear(Color.FromArgb(0, Color.Black));
			RenderTarget old = GorgonLibrary.Gorgon.CurrentRenderTarget;
			GorgonLibrary.Gorgon.CurrentRenderTarget = _miniMapTarget;
			//Draw minimap
			foreach (var asteroid in _gameMain.AsteroidManager.Asteroids)
			{
				//TODO: If have scanner upgrades, show colors and black asteroids (black asteroids are not visible on minimap as well until upgraded)
				if (asteroid is BlackAsteroid)
				{
					continue; //Don't show it if no scanner upgrade, mwhahaha!
				}
				_dot.Draw(asteroid.PositionX / _gameMain.LevelSize.X * 230, asteroid.PositionY / _gameMain.LevelSize.Y * 230, asteroid.Size / 10f, asteroid.Size / 10f, Color.White); //Later upgrades will add color
			}
			foreach (var player in _gameMain.PlayerManager.Players)
			{
				if (player.IsDead)
				{
					//Don't draw dead players
					continue;
				}
				_dot.Draw(player.PositionX / _gameMain.LevelSize.X * 230, player.PositionY / _gameMain.LevelSize.Y * 230, 0.4f, 0.4f, player.ShipColor);
			}
			GorgonLibrary.Gorgon.CurrentRenderTarget = old;
			//Blit the render to screen
			_miniMapTarget.Blit(_gameMain.ScreenSize.X - 240, _gameMain.ScreenSize.Y - 240);
		}

		public void Update(int x, int y, float frameDeltaTime)
		{
			var mainPlayer = _gameMain.MainPlayer;
			bool isDead = mainPlayer.IsDead;
			_gameMain.AsteroidManager.UpdatePhysics(_gameMain.PlayerManager.Players, _gameMain.ObjectManager.Bullets, _gameMain.ObjectManager.Shockwaves, frameDeltaTime, _gameMain.Random);
			_gameMain.PlayerManager.UpdatePhysics(frameDeltaTime);
			//TODO: Update Physic Objects (explosions, bullets, etc)

			//After every object's physics are updated, proceed to move/rotate/etc
			_gameMain.AsteroidManager.Update(frameDeltaTime);
			_gameMain.PlayerManager.Update(frameDeltaTime);
			_gameMain.ObjectManager.Update(frameDeltaTime);

            if (_gameMain.AsteroidManager.Asteroids.Count == 0 && (!_gameMain.IsMultiplayer || _gameMain.IsHost) && _delay == 0)
			{
				//No asteroids left, move to upgrade window
			    _delay = 5;
			}
            if (_gameMain.MainPlayer.IsDead && !isDead && (!_gameMain.IsMultiplayer || _gameMain.IsHost) && _delay == 0)
			{
				_delay = 5;
				//Player died, return to main menu if single player and insufficient funds to buy a new ship
				//_gameMain.ChangeToScreen(Screen.MainMenu);
			}
            if (_delay > 0)
            {
                _delay -= frameDeltaTime;
                if (_delay <= 0)
                {
                    _delay = 0;
                    //Change to appropriate screen
                    if (_gameMain.AsteroidManager.Asteroids.Count == 0)
                    {
                        _gameMain.ChangeToScreen(Screen.Upgrade);
                    }
                    else if (_gameMain.AllPlayersDead)
                    {
                        foreach (var player in _gameMain.PlayerManager.Players)
                        {
                            if (player.Bank >= 520)
                            {
                                _gameMain.LevelNumber--;
                                _gameMain.ChangeToScreen(Screen.Upgrade);
                                return;
                            }
                        }
                        //At this point, nobody have enough money to buy a new ship, game over
                        _gameMain.ChangeToScreen(Screen.MainMenu);
                    }
                }
            }

		    //Update the stats
			_bankAmount.SetText(string.Format("${0}", mainPlayer.Bank));
			_energyAmount.SetText(string.Format("{0}/{1}", (int)mainPlayer.Energy, mainPlayer.MaxEnergy));

			if (!mainPlayer.IsDead)
			{
				_gameMain.MoveStars(-mainPlayer.VelocityX * frameDeltaTime, -mainPlayer.VelocityY * frameDeltaTime);

				//poll the keyboard for movement for main player's ship
				if (_gameMain.IsKeyDown(KeyboardKeys.Left))
				{
					mainPlayer.Angle -= (mainPlayer.RotationSpeed * frameDeltaTime);
				}
				else if (_gameMain.IsKeyDown(KeyboardKeys.Right))
				{
					mainPlayer.Angle += (mainPlayer.RotationSpeed * frameDeltaTime);
				}
				if (_gameMain.IsKeyDown(KeyboardKeys.Up))
				{
					if (mainPlayer.BoostingLevel > 0 && _gameMain.IsKeyDown(KeyboardKeys.ShiftKey) && mainPlayer.Energy > mainPlayer.Acceleration * frameDeltaTime)
					{
						//Boosting, double the acceleration but drain the energy
						mainPlayer.VelocityX += (float)Math.Cos(((mainPlayer.Angle - 90) / 180) * Math.PI) * mainPlayer.Acceleration * frameDeltaTime * (1 + 0.5f * mainPlayer.BoostingLevel);
						mainPlayer.VelocityY += (float)Math.Sin(((mainPlayer.Angle - 90) / 180) * Math.PI) * mainPlayer.Acceleration * frameDeltaTime * (1 + 0.5f * mainPlayer.BoostingLevel);
						mainPlayer.Energy -= mainPlayer.Acceleration * frameDeltaTime * 0.1f;
					}
					else
					{
						mainPlayer.VelocityX += (float)Math.Cos(((mainPlayer.Angle - 90) / 180) * Math.PI) * mainPlayer.Acceleration * frameDeltaTime;
						mainPlayer.VelocityY += (float)Math.Sin(((mainPlayer.Angle - 90) / 180) * Math.PI) * mainPlayer.Acceleration * frameDeltaTime;
					}
				}
				if (_gameMain.IsKeyDown(KeyboardKeys.Down))
				{
					if (mainPlayer.BoostingLevel > 0 && _gameMain.IsKeyDown(KeyboardKeys.ShiftKey) && mainPlayer.Energy > mainPlayer.Acceleration * frameDeltaTime)
					{
						//Boosting, double the acceleration but drain the energy
						mainPlayer.VelocityX -= (float)Math.Cos(((mainPlayer.Angle - 90) / 180) * Math.PI) * mainPlayer.Acceleration * frameDeltaTime * (1 + 0.5f * mainPlayer.BoostingLevel) * (0.25f * mainPlayer.ReverseLevel);
						mainPlayer.VelocityY -= (float)Math.Sin(((mainPlayer.Angle - 90) / 180) * Math.PI) * mainPlayer.Acceleration * frameDeltaTime * (1 + 0.5f * mainPlayer.BoostingLevel) * (0.25f * mainPlayer.ReverseLevel);
						mainPlayer.Energy -= mainPlayer.Acceleration * frameDeltaTime * 0.1f;
					}
					else
					{
						mainPlayer.VelocityX -= (float)Math.Cos(((mainPlayer.Angle - 90) / 180) * Math.PI) * mainPlayer.Acceleration * frameDeltaTime * (0.25f * mainPlayer.ReverseLevel);
						mainPlayer.VelocityY -= (float)Math.Sin(((mainPlayer.Angle - 90) / 180) * Math.PI) * mainPlayer.Acceleration * frameDeltaTime * (0.25f * mainPlayer.ReverseLevel);
					}
				}
				if (_gameMain.IsKeyDown(KeyboardKeys.Space) && mainPlayer.CoolDown == 0 && mainPlayer.Energy >= (20 * mainPlayer.DamageLevel) * (mainPlayer.NumberOfMounts + 1) * (1 - (mainPlayer.ConsumptionLevel * 0.05f)))
				{
					_gameMain.ObjectManager.AddBullet(mainPlayer);
					mainPlayer.Energy -= (20 * mainPlayer.DamageLevel) * (mainPlayer.NumberOfMounts + 1) * (1 - (mainPlayer.ConsumptionLevel * 0.05f));
					mainPlayer.CoolDown += mainPlayer.CoolDownPeriod;
					if (_gameMain.IsMultiplayer && !_gameMain.IsHost)
					{
						var playerFired = new PlayerFired();
						playerFired.Angle = mainPlayer.Angle;
						playerFired.PositionX = mainPlayer.PositionX;
						playerFired.PositionY = mainPlayer.PositionY;
						playerFired.VelocityX = mainPlayer.VelocityX;
						playerFired.VelocityY = mainPlayer.VelocityY;
						playerFired.Energy = mainPlayer.Energy;
						playerFired.PlayerID = _gameMain.MainPlayerID;
						_gameMain.SendFired(playerFired);
					}
				}
			}
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
