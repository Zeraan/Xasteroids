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
		private BBTextBox _debuggingText;
		private BBStretchableImage _miniMapBackground;
		private RenderImage _miniMapTarget;
		private BBSprite _dot;

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

			_debuggingText = new BBTextBox();
			if (!_debuggingText.Initialize(0, 0, 300, 300, true, false, "DebugText", gameMain.Random, out reason))
			{
				return false;
			}

			_miniMapTarget = new RenderImage("MiniMapRender", 230, 230, ImageBufferFormats.BufferRGB888A8);
			_miniMapTarget.BlendingMode = BlendingModes.Modulated;

			_dot = SpriteManager.GetSprite("BackgroundStar", _gameMain.Random);
			if (_dot == null)
			{
				reason = "Star sprite doesn't exist.";
				return false;
			}

			return true;
		}

		public void DrawScreen()
		{
			_gameMain.DrawObjects();
			_shipStatsBackground.Draw(125);
			_miniMapBackground.Draw(125);
			var currentPlayer = _gameMain.PlayerManager.MainPlayer;
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
				_dot.Draw(player.PositionX / _gameMain.LevelSize.X * 230, player.PositionY / _gameMain.LevelSize.Y * 230, player.ShipSize / 10f, player.ShipSize / 10f, player.ShipColor);
			}
			GorgonLibrary.Gorgon.CurrentRenderTarget = old;
			//Blit the render to screen
			_miniMapTarget.Blit(_gameMain.ScreenSize.X - 240, _gameMain.ScreenSize.Y - 240);
		}

		public void Update(int x, int y, float frameDeltaTime)
		{
			var player = _gameMain.PlayerManager.MainPlayer;
			_gameMain.AsteroidManager.UpdatePhysics(_gameMain.PlayerManager.Players, frameDeltaTime, _gameMain.Random);
			_gameMain.PlayerManager.UpdatePhysics(frameDeltaTime);
			//TODO: Update Physic Objects (explosions, bullets, etc)

			//After every object's physics are updated, proceed to move/rotate/etc
			_gameMain.AsteroidManager.Update(frameDeltaTime);
			_gameMain.PlayerManager.Update(frameDeltaTime);

			_gameMain.MoveStars(-player.VelocityX * frameDeltaTime, -player.VelocityY * frameDeltaTime);

			//Update the stats
			_bankAmount.SetText(string.Format("${0}", player.Bank));
			_energyAmount.SetText(string.Format("{0}/{1}", (int)player.Energy, player.MaxEnergy));

			//poll the keyboard for movement for main player's ship
			if (_gameMain.IsKeyDown(KeyboardKeys.Left))
			{
				player.Angle -= (player.RotationSpeed * frameDeltaTime);
			}
			else if (_gameMain.IsKeyDown(KeyboardKeys.Right))
			{
				player.Angle += (player.RotationSpeed * frameDeltaTime);
			}
			if (_gameMain.IsKeyDown(KeyboardKeys.Up))
			{
				player.VelocityX += (float)Math.Cos(((player.Angle - 90) / 180) * Math.PI) * player.Acceleration * frameDeltaTime;
				player.VelocityY += (float)Math.Sin(((player.Angle - 90) / 180) * Math.PI) * player.Acceleration * frameDeltaTime;
			}
			string debugText = string.Empty;
			debugText += "Pos: " + player.PositionX + ", " + player.PositionY + "\n\r\n\r";
			debugText += "Vel: " + player.VelocityX + ", " + player.VelocityY + "\n\r\n\r";
			debugText += "Angle: " + player.Angle;
			_debuggingText.SetText(debugText);
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
