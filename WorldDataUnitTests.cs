using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Xasteroids
{
	[TestFixture]
	class WorldDataUnitTests
	{
		Random _theRandom = new Random();
		List<Asteroid> _someAsteroids;
		List<Player> _somePlayers;
		List<Bullet> _someBullets;
		List<Explosion> _someExplosions;
		List<Shockwave> _someShockwaves;

		[SetUp]
		public void Setup()
		{
			_someAsteroids = new List<Asteroid> {
				new Asteroid(20, 20, _theRandom) { PositionX = 77.0f, VelocityX = -22.9f },
				new Asteroid(30, 30, _theRandom) { PositionY = -123.12f, VelocityY = 0.86f },
			};
			_somePlayers = new List<Player> {
				new Player(20, 1, Color.Pink, SpriteManager.GetShipSprite(1, 1, _theRandom), SpriteManager.GetShieldSprite(1, _theRandom)) { RechargeLevel = 3, CapacityLevel = 1 },
				new Player(20, 1, Color.Orange, SpriteManager.GetShipSprite(1, 2, _theRandom), SpriteManager.GetShieldSprite(2, _theRandom)) { IsDead = true, NumberOfNukes = 99 },
			};
			_someBullets = new List<Bullet> {
				new Bullet(_somePlayers[0], 23.5f),
				new Bullet(_somePlayers[1], 45.0f),
			};
			_someExplosions = new List<Explosion> {
				new Explosion(23, 46, 79, 125, 99, _theRandom),
				new Explosion(78, 79, 79, 80, 81, _theRandom),
			};
			_someShockwaves = new List<Shockwave> {
				new Shockwave(55, 56, 57, _somePlayers[0]),
				new Shockwave(22, 23, 42, _somePlayers[1]),
			};
		}

		[Test]
		public void WorldData_ConfigurationTest()
		{
			WorldData someWorldData = new WorldData { Asteroids = _someAsteroids, Bullets = _someBullets, Players = _somePlayers };
			string[] someWorldConfig = someWorldData.Configuration;
			WorldData fromSomeConfig = new WorldData { Configuration = someWorldConfig };

			var fromConfigAsteroids = fromSomeConfig.Asteroids;
			Assert.That(fromConfigAsteroids[0].PositionX == _someAsteroids[0].PositionX);
			Assert.That(fromConfigAsteroids[0].VelocityX == _someAsteroids[0].VelocityX);
			Assert.That(fromConfigAsteroids[1].PositionY == _someAsteroids[1].PositionY);
			Assert.That(fromConfigAsteroids[1].VelocityY == _someAsteroids[1].VelocityY);

			var fromConfigPlayers = fromSomeConfig.Players;
			Assert.That(fromConfigPlayers[0].PositionX == _somePlayers[0].PositionX);
			Assert.That(fromConfigPlayers[0].VelocityX == _somePlayers[0].VelocityX);
			Assert.That(fromConfigPlayers[1].IsDead == _somePlayers[1].IsDead);
			Assert.That(fromConfigPlayers[1].NumberOfNukes == _somePlayers[1].NumberOfNukes);
		}
	}
}
