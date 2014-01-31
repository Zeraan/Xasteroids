using System.Collections.Generic;

namespace Xasteroids
{
	class CombatData : IConfigurable
	{
		//Same as number of public properties (excluding configuration)
		public const int CONFIG_LENGTH = 5;
		public List<Bullet> Bullets { get; set; }
		public ShipList ShipList { get; set; }
		public List<Shockwave> Shockwaves { get; set; }
		public Point LevelSize { get; set; }
		public bool OverrideClient { get; set; }

		public string[] Configuration 
		{
			get
			{
				string[] config = new string[CONFIG_LENGTH];
				
				config[0] = ObjectStringConverter.IConfigurableListToArrayString(Bullets);
				config[1] = "[" + string.Join(",", ShipList.Configuration )+ "]";
				config[2] = ObjectStringConverter.IConfigurableListToArrayString(Shockwaves);
				config[3] = "[" + LevelSize.X + "," + LevelSize.Y + "]";
				config[4] = OverrideClient.ToString();

				return config;
			}
			set 
			{
				if (value.Length < CONFIG_LENGTH)
				{
					return;
				}

				Bullets = new List<Bullet>();
				string bulletString = value[0];
				string contents = bulletString.Substring(1, bulletString.Length - 2);
				if (contents.Length != 0)
				{
					string[] bulletConfigStrings = ObjectStringConverter.ConfigurationFromStringOfConfigurations(contents);
					foreach (string bulletConfigString in bulletConfigStrings)
					{
						contents = bulletConfigString.Substring(1, bulletConfigString.Length - 2);
						string[] config = ObjectStringConverter.ConfigurationFromMixedString(contents);
						Bullets.Add(new Bullet(config));
					}
				}

				ShipList = new ShipList();
				string shipListConfigString = value[1];
				contents = shipListConfigString.Substring(1, shipListConfigString.Length - 2);
				if (contents.Length != 0)
				{
					string[] shipListConfig = ObjectStringConverter.ConfigurationFromStringOfConfigurations(contents);
					ShipList.Configuration = shipListConfig;
				}

				Shockwaves = new List<Shockwave>();
				string shockwaveString = value[2];
				contents = shockwaveString.Substring(1, shockwaveString.Length - 2);
				if (contents.Length != 0)
				{
					string[] shockwaveConfigStrings = ObjectStringConverter.ConfigurationFromStringOfConfigurations(contents);
					foreach (string shockwaveConfigString in shockwaveConfigStrings)
					{
						contents = shockwaveConfigString.Substring(1, shockwaveConfigString.Length - 2);
						string[] config = ObjectStringConverter.ConfigurationFromMixedString(contents);
						Shockwaves.Add(new Shockwave(config));
					}
				}

				string[] sizes = value[3].Split(new[] {','});
				int x, y;
				if (sizes.Length == 2 && int.TryParse(sizes[0].Substring(1, sizes[0].Length - 1), out x) && int.TryParse(sizes[1].Substring(0, sizes[0].Length - 1), out y))
				{
					LevelSize = new Point(x, y);
				}
				else
				{
					LevelSize = new Point(-1, -1);
				}

				OverrideClient = bool.Parse(value[4]);
			}
		}
	}

	public class PlayerFired : IConfigurable
	{
		public int PlayerID;
		public float PositionX;
		public float PositionY;
		public float VelocityX;
		public float VelocityY;
		public float Angle;
		public float Energy; //Remaining energy for the player

		public const int CONFIG_LENGTH = 7;

		public string[] Configuration
		{
			get
			{
				string[] config = new string[CONFIG_LENGTH];

				config[0] = PlayerID.ToString();
				config[1] = PositionX.ToString();
				config[2] = PositionY.ToString();
				config[3] = VelocityX.ToString();
				config[4] = VelocityY.ToString();
				config[5] = Angle.ToString();
				config[6] = Energy.ToString();

				return config;
			}
			set
			{
				if (value.Length < CONFIG_LENGTH)
				{
					return;
				}

				PlayerID = int.Parse(value[0]);
				PositionX = float.Parse(value[1]);
				PositionY = float.Parse(value[2]);
				VelocityX = float.Parse(value[3]);
				VelocityY = float.Parse(value[4]);
				Angle = float.Parse(value[5]);
				Energy = float.Parse(value[6]);
			}
		}
	}
}
