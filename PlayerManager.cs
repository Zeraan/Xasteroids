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

		public PlayerManager(GameMain gameMain)
		{
			_gameMain = gameMain;
		}

		public void AddPlayer(Player player)
		{
			_players.Add(player);
		}

		public void ResetPlayerPositions()
		{
			//Puts the players in a circle in the middle of level
			if (Players.Count == 1)
			{
				var player = Players[0];
				//Smack dab in middle
				player.PositionX = _gameMain.LevelSize.X / 2;
				player.PositionY = _gameMain.LevelSize.Y / 2;
				player.VelocityX = 0;
				player.VelocityY = 0;
				player.Angle = 0;
				player.Energy = player.MaxEnergy;
			}
			else
			{
				//Have the ships be in a circle, facing outward
			}
		}

		public void UpdatePhysics(float frameDeltaTime)
		{
			foreach (var player in Players)
			{
				if (player.IsDead)
				{
					continue;
				}
				float tx2 = player.PositionX + player.VelocityX * frameDeltaTime;
				float ty2 = player.PositionY + player.VelocityY * frameDeltaTime;
				foreach (var shockwave in _gameMain.ObjectManager.Shockwaves)
				{
					float rx = tx2 - shockwave.PositionX;
					float ry = ty2 - shockwave.PositionY;
					if ((float)Math.Sqrt(rx * rx + ry * ry) < (shockwave.Radius)) //Shockwave hits player
					{
						player.Energy -= (shockwave.Size * 100) * (1 - (0.05f * player.HardnessLevel));
					}
				}
			}
			foreach (var player in Players)
			{
				//Just died from shockwave, add after processing shockwaves
				if (player.Energy < 0 && !player.IsDead)
				{
					player.IsDead = true;
					_gameMain.ObjectManager.AddShockwave(player.PositionX, player.PositionY, player.ShipSize, null);
				}
			}
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
						v1x += k1 * rx * (1 - Players[i].InertialLevel * 0.05f);
						v1y += k1 * ry * (1 - Players[i].InertialLevel * 0.05f);
						v2x += k2 * rx * (1 - Players[j].InertialLevel * 0.05f);
						v2y += k2 * ry * (1 - Players[j].InertialLevel * 0.05f);

						float xDiff = Math.Abs(Players[i].VelocityX) - Math.Abs(v1x);
						float yDiff = Math.Abs(Players[i].VelocityY) - Math.Abs(v1y);
						float amount = (float)Math.Sqrt(xDiff * xDiff + yDiff * yDiff) / 2;
						Players[i].Energy -= amount * (1 - (Players[i].HardnessLevel * 0.05f));
						if (Players[i].Energy < 0)
						{
							Players[i].IsDead = true;
							_gameMain.ObjectManager.AddShockwave(Players[i].PositionX, Players[i].PositionY, Players[i].ShipSize, null);
						}
						xDiff = Math.Abs(Players[j].VelocityX) - Math.Abs(v2x);
						yDiff = Math.Abs(Players[j].VelocityY) - Math.Abs(v2y);
						amount = (float)Math.Sqrt(xDiff * xDiff + yDiff * yDiff) / 2;
						Players[j].Energy -= amount * (1 - (Players[j].HardnessLevel * 0.05f));
						if (Players[j].Energy < 0)
						{
							Players[j].IsDead = true;
							_gameMain.ObjectManager.AddShockwave(Players[j].PositionX, Players[j].PositionY, Players[j].ShipSize, null);
						}

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
				if (player.IsDead)
				{
					continue;
				}
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

	public class Player : IConfigurable
	{
		/* There is a lot in here, and as of this writing I
		 * am only putting a limited amount into the config.
		 */
		public const int CONFIG_LENGTH = 7;
		public const int EXTENDED_CONFIG_LENGTH = 12;

		public string Name { get; set; }

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
		public float Acceleration { get { return (AccelerationLevel * (25f / ShipSize) + 50); } }
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
		public float RotationSpeed { get { return ((100.0f + (15 * RotationLevel)) / ShipSize); } } //90 degress per sec
		public int MaxEnergy { get { return CapacityLevel * 50 + ShipSize * 50; } }
		public float Energy { get; set; }
		public float RechargeRate { get { return RechargeLevel * 5;} }
		public float ShieldAlpha { get; set; }

		private bool _isDead;
		public bool IsDead 
		{ 
			get { return _isDead; }
			set { 
					_isDead = value;
					if (_isDead)
					{
						_shipSprite = null;
						_shieldSprite = null;
					}
				}
		}

		public float CoolDownPeriod { get { return 0.5f - (CooldownLevel * 0.025f); } }
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
		private BBSprite _shipSprite;
		public BBSprite ShipSprite 
		{
			get
			{
				if (_shipSprite == null)
				{
					_shipSprite = SpriteManager.GetShipSprite(ShipSize, ShipStyle, new Random());
				}
				return _shipSprite;
			}
			set
			{
				_shipSprite = value;
			}
		}

		private BBSprite _shieldSprite;
		public BBSprite ShieldSprite 
		{ 
			get
			{
				if (_shieldSprite == null)
				{
					_shieldSprite = SpriteManager.GetShieldSprite(ShipSize, new Random());
				}
				return _shieldSprite;
			}
			set { _shipSprite = value; }
		}

		public int Mass { get; set; }

		public int Bank { get; set; }

		public string[] Configuration
		{
			//Name, Color, Style, Size, Mass can be one-time
			get
			{
				string[] config = new string[CONFIG_LENGTH];

				config[0] = IsDead.ToString();
				config[1] = PositionX.ToString();
				config[2] = PositionY.ToString();
				config[3] = VelocityX.ToString();
				config[4] = VelocityY.ToString();
				config[5] = ShieldAlpha.ToString();
				config[6] = Bank.ToString();

				return config;
			}
			set
			{
				if (value.Length < CONFIG_LENGTH)
				{
					return;
				}

				bool outBool;
				if (bool.TryParse(value[0], out outBool))
				{
					IsDead = outBool;
				}
				float outFloat;
				if (float.TryParse(value[1], out outFloat))
				{
					PositionX = outFloat;
				}
				if (float.TryParse(value[2], out outFloat))
				{
					PositionY = outFloat;
				}
				if (float.TryParse(value[3], out outFloat))
				{
					VelocityX = outFloat;
				}
				if (float.TryParse(value[4], out outFloat))
				{
					VelocityY = outFloat;
				}
				if (float.TryParse(value[5], out outFloat))
				{
					ShieldAlpha = outFloat;
				}
				int outInt;
				if (int.TryParse(value[6], out outInt))
				{
					Bank = outInt;
				}
			}
		}
		
		public Player(int shipSize, int shipStyle, Color shipColor)
		{
			ShipSize = shipSize;
			ShipStyle = shipStyle;
			ShipColor = shipColor;
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

		public Player(string[] configuration)
		{
			Configuration = configuration;
			ShipSize = 1;
			ShipStyle = 1;
			ShipColor = Color.Red;
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

	public class Ship : IConfigurable
	{
		public const int CONFIG_LENGTH = 6;

		public string OwnerName { get; set; }
		public float ShieldAlpha { get; set; }
		public int Size { get; set; }
		public int Style { get; set; }
		public Color Color { get; set; }
		public int Mass { get; set; }

		public string[] Configuration
		{
			get
			{
				string[] config = new string[CONFIG_LENGTH];

				config[0] = OwnerName;
				config[1] = ShieldAlpha.ToString();
				config[2] = Size.ToString();
				config[3] = Style.ToString();
				config[4] = Color.ToArgb().ToString();
				config[5] = Mass.ToString();

				return config;
			}
			set
			{
				if (value.Length < CONFIG_LENGTH)
				{
					return;
				}

				OwnerName = value[0];
				float outFloat;
				if (float.TryParse(value[1], out outFloat))
				{
					ShieldAlpha = outFloat;
				}
				int outInt;
				if (int.TryParse(value[2], out outInt))
				{
					Size = outInt;
				}
				if (int.TryParse(value[3], out outInt))
				{
					Style = outInt;
				}
				if (int.TryParse(value[4], out outInt))
				{
					Color = Color.FromArgb(outInt);
				}
				if (int.TryParse(value[5], out outInt))
				{
					Mass = outInt;
				}
			}
		}
	}
}
