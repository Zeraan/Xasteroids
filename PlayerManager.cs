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
				_players.Add(new Player(1, 1, Color.Red, SpriteManager.GetShipSprite(1, 1, r), SpriteManager.GetShieldSprite(1, r)));
			}
			MainPlayer.Bank = 1000;
		}

		public void UpdatePhysics(float frameDeltaTime)
		{
			//Asteroid vs ship are handled in AsteroidManager
			if (Players.Count < 2)
			{
				//No need to process physics
				return;
			}
			for (int i = 0; i < Players.Count - 1; i++)
			{
				for (int j = i + 1; j < Players.Count; j++)
				{
					if (Players[i].IsDead || Players[j].IsDead)
					{
						//Some asteroids have clumped together, don't calculate between any asteroids with the asteroid to be removed;
						continue;
					}
					//create variables that'd be easier to read than function calls
					float x1 = Players[i].PositionX;
					float y1 = Players[i].PositionY;
					float x2 = Players[j].PositionX;
					float y2 = Players[j].PositionY;

					//TODO: Add a simple rectangle bounding check to skip expensive circle calculations, and do opposite side collision checking (5 and Width-5 should collide)

					float v1x = Players[i].VelocityX; //e.FrameDeltaTime is the time between frames, less than 1
					float v1y = Players[i].VelocityY;
					float v2x = Players[j].VelocityX;
					float v2y = Players[j].VelocityY;

					//get the position plus velocity
					float tx1 = x1 + v1x * frameDeltaTime;
					float ty1 = y1 + v1y * frameDeltaTime;
					float tx2 = x2 + v2x * frameDeltaTime;
					float ty2 = y2 + v2y * frameDeltaTime;

					float dx = tx2 - tx1;
					float dy = ty2 - ty1;
					float dx2 = x2 - x1;
					float dy2 = y2 - y1;

					float r1 = (float)Math.Sqrt(dx * dx + dy * dy); //Get the distance between centers of asteroids
					float r2 = (float)Math.Sqrt(dx2 * dx2 + dy2 * dy2);

					if (r1 < (Players[i].ShipSize * 16 + Players[j].ShipSize * 16) && r1 < r2) //Collision!
					{
						//Calculate the impulse or change of momentum, or whatever people call it
						float rx = dx / r1;
						float ry = dy / r1;
						float k1 = 2 * Players[j].Mass * (rx * (v2x - v1x) + ry * (v2y - v1y)) / (Players[i].Mass + Players[j].Mass);
						float k2 = 2 * Players[i].Mass * (rx * (v1x - v2x) + ry * (v1y - v2y)) / (Players[i].Mass + Players[j].Mass);

						//Adjust the velocities
						v1x += k1 * rx;
						v1y += k1 * ry;
						v2x += k2 * rx;
						v2y += k2 * ry;

						//Assign the final value to asteroids
						Players[i].VelocityX = v1x;
						Players[i].VelocityY = v1y;
						Players[j].VelocityX = v2x;
						Players[j].VelocityY = v2y;
					}
				}
			}
		}

		public void Update(float frameDeltaTime)
		{
			int width = _gameMain.LevelSize.X;
			int height = _gameMain.LevelSize.Y;
			foreach (var player in Players)
			{
				player.PositionX += player.VelocityX * frameDeltaTime;
				player.PositionY += player.VelocityY * frameDeltaTime;
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
				player.Update(frameDeltaTime); //regeneration and stuff here
			}
		}
	}

	public class Player
	{
		public float PositionX { get; set; }
		public float PositionY { get; set; }
		public float VelocityX { get; set; }
		public float VelocityY { get; set; }
		public float Acceleration { get { return 100; } } //10 pixels per sec per sec acceleration
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
		public float ShieldAlpha { get; set; }

		public bool IsDead { get; set; }

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
		public BBSprite ShieldSprite { get; set; }

		public int Mass { get; set; }

		public int Bank { get; set; }

		public Player(int shipSize, int shipStyle, Color shipColor, BBSprite shipSprite, BBSprite shieldSprite)
		{
			ShipSize = shipSize;
			ShipStyle = shipStyle;
			ShipColor = shipColor;
			ShipSprite = shipSprite;
			ShieldSprite = shieldSprite;
			Mass = ShipSize * 20;

			MaxEnergy = ShipSize * 100;
			Energy = MaxEnergy;
		}

		public void Update(float frameDeltaTime)
		{
			//Update energy regeneration
			if (IsDead)
			{
				//Dead, nothing to see here, move along
				return;
			}

			Energy += RechargeRate * frameDeltaTime;
			if (Energy > MaxEnergy)
			{
				Energy = MaxEnergy;
			}

			if (ShieldAlpha > 0)
			{
				ShieldAlpha -= frameDeltaTime;
			}
		}
	}
}
