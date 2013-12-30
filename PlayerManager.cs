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

		public void ResetPlayerPositions()
		{
			//Puts the players in a circle in the middle of level
			if (Players.Count == 1)
			{
				//Smack dab in middle
				MainPlayer.PositionX = _gameMain.LevelSize.X / 2;
				MainPlayer.PositionY = _gameMain.LevelSize.Y / 2;
				MainPlayer.VelocityX = 0;
				MainPlayer.VelocityY = 0;
				MainPlayer.Angle = 0;
				MainPlayer.Energy = MainPlayer.MaxEnergy;
			}
			else
			{
				//Have the ships be in a circle, facing outward
			}
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

					Utility.GetClosestDistance(x1, y1, x2, y2, _gameMain.LevelSize.X, _gameMain.LevelSize.Y, out x2, out y2);

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
		//Energy Upgrades
		public int RechargeLevel { get; set; }
		public int CapacityLevel { get; set; }

		//Engine Upgrades
		public int AccelerationLevel { get; set; }
		public int RotationLevel { get; set; }
		public int ReverseLevel { get; set; }
		public int BoostingLevel { get; set; }

		//Shield Upgrades
		public int ShreddingLevel { get; set; }
		public int HardnessLevel { get; set; }
		public int InertialLevel { get; set; }
		public int PhasingLevel { get; set; }

		//Weapon Upgrades
		public int CooldownLevel { get; set; }
		public int ConsumptionLevel { get; set; }
		public int DamageLevel { get; set; }
		public int NumberOfMounts { get; set; }
		public int VelocityLevel { get; set; }
		public int PenetratingLevel { get; set; }
		public int ShrapnelLevel { get; set; }
		public int NumberOfNukes { get; set; }

		public float PositionX { get; set; }
		public float PositionY { get; set; }
		public float VelocityX { get; set; }
		public float VelocityY { get; set; }
		public float Acceleration { get { return (AccelerationLevel * (25f / ShipSize) + 25); } }
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
		public float RotationSpeed { get { return ((75.0f + (15 * RotationLevel)) / ShipSize); } } //90 degress per sec
		public int MaxEnergy { get { return CapacityLevel * 50 + ShipSize * 50; } }
		public float Energy { get; set; }
		public float RechargeRate { get { return RechargeLevel * 5;} }
		public float ShieldAlpha { get; set; }

		public bool IsDead { get; set; }

		public float CoolDownPeriod { get { return 1.0f - (CooldownLevel * 0.05f); } }
		public float CoolDown { get; set; }

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

			Energy = MaxEnergy;

			RechargeLevel = 1;
			CapacityLevel = 0;

			AccelerationLevel = 1;
			RotationLevel = 1;
			ReverseLevel = 0;
			BoostingLevel = 0;

			ShreddingLevel = 0;
			HardnessLevel = 0;
			InertialLevel = 0;
			PhasingLevel = 0;

			CooldownLevel = 1;
			ConsumptionLevel = 0;
			DamageLevel = 1;
			NumberOfMounts = 0;
			VelocityLevel = 1;
			PenetratingLevel = 0;
			ShrapnelLevel = 0;
			NumberOfNukes = 0;
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

			if (CoolDown > 0)
			{
				CoolDown -= frameDeltaTime;
				if (CoolDown < 0)
				{
					CoolDown = 0;
				}
			}
		}
	}
}
