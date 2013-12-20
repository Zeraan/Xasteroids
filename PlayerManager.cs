using System;
using System.Collections.Generic;
using System.Drawing;

namespace Xasteroids
{
	public class PlayerManager
	{
		private GameMain _gameMain;
		private List<Player> _players = new List<Player>();
		public List<Player> Players { get { return _players; } }
		public Player MainPlayer { get { return _players[0]; } } 

		public PlayerManager(GameMain gameMain)
		{
			_gameMain = gameMain;
		}

		public void AddPlayer(Player player)
		{
			_players.Add(player);
		}

		public void ClearMainPlayer(Random r)
		{
			if (_players.Count == 0)
			{
				_players.Add(new Player(1, 1, Color.Red, SpriteManager.GetShipSprite(1, 1, r)));
			}
			MainPlayer.Bank = 1000;
		}

		public void UpdatePhysics(List<Asteroid> asteroids, float frameDeltaTime)
		{
			//TODO: Implement collision handling between asteroid and ship, and ship vs ship.
		}

		public void Update(float frameDeltaTime)
		{
			int width = _gameMain.LevelSize.X;
			int height = _gameMain.LevelSize.Y;
			foreach (var player in Players)
			{
				player.PositionX += player.VelocityX;
				player.PositionY += player.VelocityY;
				while (player.PositionX < 0)
				{
					player.PositionX += width;
				}
				while (player.PositionX >= width)
				{
					player.PositionX -= width;
				}
				while (player.PositionY < 0)
				{
					player.PositionY += height;
				}
				while (player.PositionY >= height)
				{
					player.PositionY -= height;
				}
			}
		}
	}

	public class Player
	{
		public float PositionX { get; set; }
		public float PositionY { get; set; }
		public float VelocityX { get; set; }
		public float VelocityY { get; set; }
		public float Acceleration { get { return 1; } } //10 pixels per sec per sec acceleration
		private float _angle;
		public float Angle
		{
			get { return _angle; }
			set
			{
				_angle = value;
				while (_angle < 0)
				{
					_angle += 360;
				}
				while (_angle >= 360)
				{
					_angle -= 360;
				}
			}
		}
		public float RotationSpeed { get { return 90; } } //90 degress per sec

		public int MaxEnergy { get; private set; }
		public float Energy { get; private set; }
		public float RechargeRate { get; private set; }

		public float CoolDownPeriod { get; private set; }
		public float CoolDown { get; private set; }

		public int ShipSize { get; set; }
		public int ShipStyle { get; set; }
		private Color _shipColor;
		public Color ShipColor
		{
			get { return _shipColor; }
			set 
			{ 
				_shipColor = value;
				ShipConvertedColor = new[]
								{
									_shipColor.R / 255.0f,
									_shipColor.G / 255.0f,
									_shipColor.B / 255.0f,
									1
								};
			}
		}
		public float[] ShipConvertedColor { get; private set; }
		public BBSprite ShipSprite { get; set; }

		public int Bank { get; set; }

		public Player(int shipSize, int shipStyle, Color shipColor, BBSprite shipSprite)
		{
			ShipSize = shipSize;
			ShipStyle = shipStyle;
			ShipColor = shipColor;
			ShipSprite = shipSprite;

			MaxEnergy = ShipSize * 100;
			Energy = MaxEnergy;
		}
	}
}
