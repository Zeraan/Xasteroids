using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Xasteroids
{
	[TestFixture]
	class CombatDataUnitTests
	{
		Random _theRandom = new Random();
		List<Asteroid> _someAsteroids;
		List<Player> _somePlayers;
		List<Bullet> _someBullets;
		List<Shockwave> _someShockwaves;

		[SetUp]
		public void Setup()
		{
			_someAsteroids = new List<Asteroid> {
				new Asteroid(2000, 2000, _theRandom) { PositionX = 77.0f, VelocityX = -22.9f },
				new Asteroid(3000, 3000, _theRandom) { PositionY = -123.12f, VelocityY = 0.86f },
			};
			_somePlayers = new List<Player> {
				new Player(20, 1, Color.Pink) { RechargeLevel = 3, CapacityLevel = 1, Name = "Pete" },
				new Player(20, 1, Color.Orange) { IsDead = true, NumberOfNukes = 99, Name = "Little Pete" },
			};
			_someBullets = new List<Bullet> {
				new Bullet(_somePlayers[0], 23.5f),
				new Bullet(_somePlayers[1], 45.0f),
			};
			_someShockwaves = new List<Shockwave> {
				new Shockwave(55, 56, 57, _somePlayers[0]),
				new Shockwave(22, 23, 42, _somePlayers[1]),
			};
		}

		[Test]
		public void WorldData_ConfigurationTest()
		{
			CombatData someWorldData = new CombatData { Asteroids = _someAsteroids, Bullets = _someBullets, Players = _somePlayers, Shockwaves = _someShockwaves };
			string[] someWorldConfig = someWorldData.Configuration;
			CombatData fromSomeConfig = new CombatData { Configuration = someWorldConfig };

			var fromConfigAsteroids = fromSomeConfig.Asteroids;
			Assert.That(fromConfigAsteroids[0].PositionX == _someAsteroids[0].PositionX);
			Assert.That(fromConfigAsteroids[0].VelocityX == _someAsteroids[0].VelocityX);
			Assert.That(fromConfigAsteroids[1].PositionY == _someAsteroids[1].PositionY);
			Assert.That(fromConfigAsteroids[1].VelocityY == _someAsteroids[1].VelocityY);

			var fromConfigBullets = fromSomeConfig.Bullets;
			Assert.That(fromConfigBullets[0].OwnerName == _someBullets[0].OwnerName);
			Assert.That(fromConfigBullets[0].ShrapnelLevel == _someBullets[0].ShrapnelLevel);
			Assert.That(fromConfigBullets[1].Damage == _someBullets[1].Damage);
			Assert.That(fromConfigBullets[1].Lifetime == _someBullets[1].Lifetime);

			var fromConfigPlayers = fromSomeConfig.Players;
			Assert.That(fromConfigPlayers[0].PositionX == _somePlayers[0].PositionX);
			Assert.That(fromConfigPlayers[0].VelocityX == _somePlayers[0].VelocityX);
			Assert.That(fromConfigPlayers[1].IsDead == _somePlayers[1].IsDead);
			Assert.That(fromConfigPlayers[1].VelocityY == _somePlayers[1].VelocityY);

			var fromConfigShockwaves = fromSomeConfig.Shockwaves;
			Assert.That(fromConfigShockwaves[0].OwnerName == _someShockwaves[0].OwnerName);
			Assert.That(fromConfigShockwaves[0].PositionY == _someShockwaves[0].PositionY);
			Assert.That(fromConfigShockwaves[1].OwnerName == _someShockwaves[1].OwnerName);
			Assert.That(fromConfigShockwaves[1].Size == _someShockwaves[1].Size);
		}
	}
}
