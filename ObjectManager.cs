using System;
using System.Collections.Generic;
using System.Drawing;

namespace Xasteroids
{
	public class ObjectManager
	{
		private GameMain _gameMain;

		//This class is for handling bullets, nukes, and other various objects
		public List<Bullet> Bullets { get; private set; }
		public BBSprite BulletSprite { get; private set; }

		public ObjectManager(GameMain gameMain)
		{
			Bullets = new List<Bullet>();
			_gameMain = gameMain;
			BulletSprite = SpriteManager.GetSprite("Bullet", gameMain.Random);
		}

		public void AddBullet(Player player)
		{
			Bullet bullet = new Bullet(player);
			Bullets.Add(bullet);
		}

		public void Update(float frameDeltaTime)
		{
			int width = _gameMain.LevelSize.X;
			int height = _gameMain.LevelSize.Y;

			var bulletsToRemove = new List<Bullet>();
			foreach (var bullet in Bullets)
			{
				if (bullet.Damage <= 0)
				{
					bulletsToRemove.Add(bullet);
					continue;
				}
				bullet.PositionX += bullet.VelocityX * frameDeltaTime;
				bullet.PositionY += bullet.VelocityY * frameDeltaTime;

				while (bullet.PositionX < 0)
				{
					bullet.PositionX += width;
				}
				while (bullet.PositionX >= width)
				{
					bullet.PositionX -= width;
				}
				while (bullet.PositionY < 0)
				{
					bullet.PositionY += height;
				}
				while (bullet.PositionY >= height)
				{
					bullet.PositionY -= height;
				}

				bullet.Update(frameDeltaTime); //regeneration and stuff here
				if (bullet.Lifetime <= 0)
				{
					bulletsToRemove.Add(bullet);
				}
			}
			foreach (var bullet in bulletsToRemove)
			{
				Bullets.Remove(bullet);
			}
		}

		public void Clear()
		{
			Bullets.Clear();
		}
	}

	public class Bullet
	{
		public float PositionX { get; set; }
		public float PositionY { get; set; }
		public float VelocityX { get; set; }
		public float VelocityY { get; set; }
		public int Damage { get; set; }
		public float Scale { get; set; }
		public Color Color { get; set; }
		public Player Owner { get; set; } //So it don't hit the ship that fired it
		public float Lifetime { get; set; } //Decrements until it reaches 0, then despawn

		public Bullet(Player owner)
		{
			//Derive data from the owner, since it's obvious that this owner fired the bullet, aka construction call
			Owner = owner;
			PositionX = Owner.PositionX;
			PositionY = Owner.PositionY;
			VelocityX = Owner.VelocityX + (float)(Math.Cos(((Owner.Angle - 90) / 180) * Math.PI) * 250);
			VelocityY = Owner.VelocityY + (float)(Math.Sin(((Owner.Angle - 90) / 180) * Math.PI) * 250);
			Color = Color.White;
			Scale = 1;
			Lifetime = 1;
			Damage = 5;
		}

		public void Update(float frameDeltaTime)
		{
			Lifetime -= frameDeltaTime;
		}
	}
}
