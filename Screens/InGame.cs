using System;
using System.Drawing;
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

		public bool Initialize(GameMain gameMain, out string reason)
		{
			_gameMain = gameMain;

			_energyX = (_gameMain.ScreenSize.X / 2) - 190;
			_shipStatsBackground = new BBStretchableImage();
			if (!_shipStatsBackground.Initialize((_gameMain.ScreenSize.X / 2) - 200, -20, 400, 90, StretchableImageType.ThinBorderBG, _gameMain.Random, out reason))
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

			return true;
		}

		public void DrawScreen()
		{
			_gameMain.DrawObjects();
			_shipStatsBackground.Draw(125);
			var currentPlayer = _gameMain.PlayerManager.MainPlayer;
			float percentage = currentPlayer.Energy / currentPlayer.MaxEnergy;
			_horizontalEnergyBar.Draw(_energyX, 7, (380.0f / _horizontalEnergyBar.Width) * percentage, 2, Color.FromArgb(200, Color.LawnGreen));
			_bankAmount.Draw();
			_energyAmount.Draw();
			_debuggingText.Draw();
		}

		public void Update(int x, int y, float frameDeltaTime)
		{
			var player = _gameMain.PlayerManager.MainPlayer;
			_gameMain.AsteroidManager.UpdatePhysics(_gameMain.PlayerManager.Players, frameDeltaTime, _gameMain.Random);
			//TODO: Update Players and Phsyic Objects (explosions, bullets, etc)

			//After every object's physics are updated, proceed to move/rotate/etc
			_gameMain.AsteroidManager.UpdateAsteroids(frameDeltaTime);
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
