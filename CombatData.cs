using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Xasteroids
{
	class CombatData : IConfigurable
	{
		//Same as number of public properties (excluding configuration)
		public const int CONFIG_LENGTH = 5;
		public List<Asteroid> Asteroids { get; set; }
		public List<Bullet> Bullets { get; set; }
		public List<Player> Players { get; set; }
		public List<Shockwave> Shockwaves { get; set; }
		public Point LevelSize { get; set; }

		public string[] Configuration 
		{
			get
			{
				string[] config = new string[CONFIG_LENGTH];
				
				config[0] = ObjectStringConverter.IConfigurableListToArrayString(Asteroids);
				config[1] = ObjectStringConverter.IConfigurableListToArrayString(Bullets);
				config[2] = ObjectStringConverter.IConfigurableListToArrayString(Players);
				config[3] = ObjectStringConverter.IConfigurableListToArrayString(Shockwaves);
				config[4] = "[" + LevelSize.X + "," + LevelSize.Y + "]";

				return config;
			}
			set 
			{
				if (value.Length < CONFIG_LENGTH)
				{
					return;
				}

				Asteroids = new List<Asteroid>();
				string asteroidsString = value[0];
				string contents = asteroidsString.Substring(1, asteroidsString.Length - 2);
				if (contents.Length != 0)
				{
					string[] asteroidConfigStrings = ObjectStringConverter.ConfigurationFromStringOfConfigurations(contents);
					foreach (string asteroidConfigString in asteroidConfigStrings)
					{
						contents = asteroidConfigString.Substring(1, asteroidConfigString.Length - 2);
						string[] config = ObjectStringConverter.ConfigurationFromMixedString(contents);
						Asteroids.Add(new Asteroid(config));
					}
				}

				Bullets = new List<Bullet>();
				string bulletString = value[1];
				contents = bulletString.Substring(1, bulletString.Length - 2);
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

				Players = new List<Player>();
				string playerString = value[2];
				contents = playerString.Substring(1, playerString.Length - 2);
				if (contents.Length != 0)
				{
					string[] playerConfigStrings = ObjectStringConverter.ConfigurationFromStringOfConfigurations(contents);
					foreach (string playerConfigString in playerConfigStrings)
					{
						contents = playerConfigString.Substring(1, playerConfigString.Length - 2);
						string[] config = ObjectStringConverter.ConfigurationFromMixedString(contents);
						Players.Add(new Player(config));
					}
				}

				Shockwaves = new List<Shockwave>();
				string shockwaveString = value[3];
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

				string[] sizes = value[4].Split(new[] {','});
				int x, y;
				if (sizes.Length == 2 && int.TryParse(sizes[0].Substring(1, sizes[0].Length - 1), out x) && int.TryParse(sizes[1].Substring(0, sizes[0].Length - 1), out y))
				{
					LevelSize = new Point(x, y);
				}
				else
				{
					LevelSize = new Point(-1, -1);
				}
			}
		}
	}
}
