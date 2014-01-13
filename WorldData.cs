using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xasteroids
{
	class WorldData : IConfigurable
	{
		//Same as number of public properties (excluding configuration)
		public const int CONFIG_LENGTH = 3;
		public List<Asteroid> Asteroids { get; set; }
		public List<Bullet> Bullets { get; set; }
		public List<Player> Players { get; set; }
		public List<Explosion> Explosions { get; set; }
		public List<Shockwave> Shockwaves { get; set; }

		public string[] Configuration 
		{
			get
			{
				string[] config = new string[CONFIG_LENGTH];
				string asteroidsString;
				if (Asteroids == null || Asteroids.Count == 0)
				{
					asteroidsString = "[]";
				}
				else
				{
					string[] asStrings = new string[Asteroids.Count];
					for (int j = 0; j < asStrings.Length; ++j)
					{
						string arrayString = "[" + string.Join(",", Asteroids[j].Configuration) + "]";
						asStrings[j] = arrayString;
					}
					asteroidsString = "[" + string.Join(",", asStrings) + "]";
				}
				config[0] = asteroidsString;

				return config;
			}
			set 
			{
				if (value.Length < CONFIG_LENGTH)
				{
					return;
				}

				string asteroidsString = value[0];
				string contents = asteroidsString.Substring(1, asteroidsString.Length - 2);
				string[] asStrings = contents.Split(',');
				Asteroids = new List<Asteroid>();
				foreach (string arrayString in asStrings)
				{
					contents = arrayString.Substring(1, arrayString.Length - 2);
					string[] asteroidConfig = contents.Split(',');
					Asteroids.Add(new Asteroid(asteroidConfig));
				}
			}
		}
	}
}
