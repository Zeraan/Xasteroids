﻿using System;
using System.Collections.Generic;
using System.Drawing;

namespace Xasteroids
{
	public class ObjectManager
	{
		private GameMain _gameMain;

		//This class is for handling bullets, nukes, and other various objects
		public List<Bullet> Bullets { get; set; }
		public BBSprite BulletSprite { get; private set; }
		public BBSprite ShockwaveSprite { get; private set; }

		public List<Explosion> Explosions { get; private set; }
		public List<Shockwave> Shockwaves { get; set; } 

		public ObjectManager(GameMain gameMain)
		{
			Bullets = new List<Bullet>();
			Explosions = new List<Explosion>();
			Shockwaves = new List<Shockwave>();
			_gameMain = gameMain;
			BulletSprite = SpriteManager.GetSprite("Bullet", _gameMain.Random);
			ShockwaveSprite = SpriteManager.GetSprite("Shockwave", _gameMain.Random);
		}

		public void AddBullet(Player player)
		{
			for (int i = 0; i < player.NumberOfMounts + 1; i++)
			{
				float degree = (float)((((player.Angle - (15f * player.NumberOfMounts / 2) + (15f * i)) - 90) / 180) * Math.PI);
				Bullet bullet = new Bullet(player, degree);
				Bullets.Add(bullet);
			}
		}

		public void AddExplosion(float xPos, float yPos, float xVel, float yVel, int size)
		{
			var explosion = new Explosion(xPos, yPos, xVel, yVel, size, _gameMain.Random);
			Explosions.Add(explosion);
		}

		public void AddShockwave(float xPos, float yPos, int size, Player owner)
		{
			var shockwave = new Shockwave(xPos, yPos, size, owner);
			Shockwaves.Add(shockwave);
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
				if (!bullet.IsShrapnel)
				{
					if (bullet.ShrapnelLevel > 0)
					{
						for (int i = 0; i < bullet.ShrapnelLevel; i++)
						{
							//This is a constructor for adding shrapnel
							Bullets.Add(new Bullet(bullet, (float)((_gameMain.Random.Next(360) / 180.0f) * Math.PI)));
						}
					}
				}
				Bullets.Remove(bullet);
			}

			var explosionsToRemove = new List<Explosion>();
			foreach (var explosion in Explosions)
			{
				explosion.PositionX += explosion.VelocityX * frameDeltaTime;
				explosion.PositionY += explosion.VelocityY * frameDeltaTime;

				while (explosion.PositionX < 0)
				{
					explosion.PositionX += width;
				}
				while (explosion.PositionX >= width)
				{
					explosion.PositionX -= width;
				}
				while (explosion.PositionY < 0)
				{
					explosion.PositionY += height;
				}
				while (explosion.PositionY >= height)
				{
					explosion.PositionY -= height;
				}

				explosion.Update(frameDeltaTime, _gameMain.Random);
				if (explosion.LifeTime <= 0)
				{
					explosionsToRemove.Add(explosion);
				}
			}
			foreach (var explosionToRemove in explosionsToRemove)
			{
				Explosions.Remove(explosionToRemove);
			}

			var shockwavesToRemove = new List<Shockwave>();
			foreach (var shockwave in Shockwaves)
			{
				shockwave.Update(frameDeltaTime);
				if (shockwave.Boomed)
				{
					shockwavesToRemove.Add(shockwave);
				}
			}
			foreach (var shockwaveToRemove in shockwavesToRemove)
			{
				Shockwaves.Remove(shockwaveToRemove);
			}
		}

		public void Clear()
		{
			Bullets.Clear();
		}
	}

	public class Shockwave : IConfigurable
	{
		public const int CONFIG_LENGTH = 7;

		public float PositionX { get; set; }
		public float PositionY { get; set; }
		public float TimeTilBoom { get; private set; }
		public int Radius { get; private set; }
		public int Size { get; private set; }
		public bool Boomed { get; private set; }
		public string OwnerName { get; private set; } //For nukes

		public string[] Configuration
		{
			get
			{
				string[] config = new string[CONFIG_LENGTH];
				config[0] = PositionX.ToString();
				config[1] = PositionY.ToString();
				config[2] = TimeTilBoom.ToString();
				config[3] = Radius.ToString();
				config[4] = Size.ToString();
				config[5] = Boomed.ToString();
				config[6] = OwnerName == null ? string.Empty : OwnerName.ToString();
				return config;
			}
			set
			{
				if (value.Length < CONFIG_LENGTH)
				{
					return;
				}

				float outFloat;
				if (float.TryParse(value[0], out outFloat))
				{
					PositionX = outFloat;
				}
				if (float.TryParse(value[1], out outFloat))
				{
					PositionY = outFloat;
				}
				if (float.TryParse(value[2], out outFloat))
				{
					TimeTilBoom = outFloat;
				}
				int outInt;
				if (int.TryParse(value[3], out outInt))
				{
					TimeTilBoom = outFloat;
				}
				if (int.TryParse(value[4], out outInt))
				{
					Size = outInt;
				}
				bool outBool;
				if (bool.TryParse(value[5], out outBool))
				{
					Boomed = outBool;
				}
				OwnerName = string.IsNullOrEmpty(value[6]) ? null : value[6];
			}
		}

		public Shockwave(float xPos, float yPos, int size, Player owner)
		{
			TimeTilBoom = 0.2f;
			PositionX = xPos;
			PositionY = yPos;
			Radius = 72 * size;
			Size = size;
			if (owner != null)
			{
				OwnerName = owner.Name;
			}
		}

		public Shockwave(string[] configuration)
		{
			Configuration = configuration;
		}

		public void Update(float frameDeltaTime)
		{
			if (TimeTilBoom <= 0)
			{
				Boomed = true;
			}
			else
			{
				//Don't check for 0 or below, allowing for one cycle of physics update before this gets removed, so objects gets damaged
				TimeTilBoom -= frameDeltaTime;
			}
		}
	}

	public class Explosion
	{
		//Explosion would be one-time update, just pass in position, velocity, and LifeTime, and the client will handle the rest
		//if it don't show up, no biggie
		//it's just a visual effect
		//oh, and Size as well

		public float PositionX { get; set; }
		public float PositionY { get; set; }
		public float VelocityX { get; set; }
		public float VelocityY { get; set; }
		public BBSprite Sprite { get; private set; }
		public float LifeTime { get; private set; }
		public int Size { get; private set; }

		public Explosion(float xPos, float yPos, float xvel, float yvel, int size, Random r)
		{
			switch (size)
			{
				case 1:
					Sprite = SpriteManager.GetSprite("SmallExplosion", r);
					break;
				case 2:
					Sprite = SpriteManager.GetSprite("MediumExplosion", r);
					break;
				case 4:
					Sprite = SpriteManager.GetSprite("LargeExplosion", r);
					break;
			}
			PositionX = xPos;
			PositionY = yPos;
			VelocityX = xvel;
			VelocityY = yvel;
			LifeTime = 0.25f;
			Size = (size * 8);
		}

		public void Update(float frameDeltaTime, Random r)
		{
			LifeTime -= frameDeltaTime;
			Sprite.Update(frameDeltaTime, r);
		}
	}

	public class Bullet : IConfigurable
	{
		public const int CONFIG_LENGTH = 12;

		public float PositionX { get; set; }
		public float PositionY { get; set; }
		public float VelocityX { get; set; }
		public float VelocityY { get; set; }
		public float Damage { get; set; }
		public float Scale { get; set; }
		public Color Color { get; set; }
		public string OwnerName { get; set; } //So it don't hit the ship that fired it
		public int PenetratingLevel { get; set; }
		public int ShrapnelLevel { get; set; }
		public float Lifetime { get; set; } //Decrements until it reaches 0, then despawn
		public bool IsShrapnel { get; set; }
		public string[] Configuration { 
			get
			{
				string[] config = new string[CONFIG_LENGTH];

				config[0] = PositionX.ToString();
				config[1] = PositionY.ToString();
				config[2] = VelocityX.ToString();
				config[3] = VelocityY.ToString();
				config[4] = Damage.ToString();
				config[5] = Scale.ToString();
				config[6] = Color.ToArgb().ToString();
				config[7] = OwnerName;	//Should do something about this guy
				config[8] = PenetratingLevel.ToString();
				config[9] = ShrapnelLevel.ToString();
				config[10] = Lifetime.ToString();
				config[11] = IsShrapnel.ToString();

				return config;
			} 
			set
			{
				if (value.Length < CONFIG_LENGTH)
				{
					return;
				}

				float outFloat;
				if (float.TryParse(value[0], out outFloat))
				{
					PositionX = outFloat;
				}
				if (float.TryParse(value[1], out outFloat))
				{
					PositionY = outFloat;
				}
				if (float.TryParse(value[2], out outFloat))
				{
					VelocityX = outFloat;
				}
				if (float.TryParse(value[3], out outFloat))
				{
					VelocityY = outFloat;
				}
				if (float.TryParse(value[4], out outFloat))
				{
					Damage = outFloat;
				}
				if (float.TryParse(value[5], out outFloat))
				{
					Scale = outFloat;
				}
				int outInt;
				if (int.TryParse(value[6], out outInt))
				{
					Color = Color.FromArgb(outInt);
				}
				OwnerName = value[7];
				if (int.TryParse(value[8], out outInt))
				{
					PenetratingLevel = outInt;
				}
				if (int.TryParse(value[9], out outInt))
				{
					ShrapnelLevel = outInt;
				}
				if (float.TryParse(value[10], out outFloat))
				{
					Lifetime = outFloat;
				}
				bool outBool;
				if (bool.TryParse(value[11], out outBool))
				{
					IsShrapnel = outBool;
				}
			}
		}

		public Bullet(Player owner, float degree)
		{
			//Derive data from the owner, since it's obvious that this owner fired the bullet, aka construction call
			OwnerName = owner.Name;
			PenetratingLevel = owner.PenetratingLevel;
			ShrapnelLevel = owner.ShrapnelLevel;
			PositionX = owner.PositionX;
			PositionY = owner.PositionY;
			VelocityX = owner.VelocityX + (float)(Math.Cos(degree) * (owner.VelocityLevel * 100 + 200));
			VelocityY = owner.VelocityY + (float)(Math.Sin(degree) * (owner.VelocityLevel * 100 + 200));
			Color = Color.White;
			Scale = 1;
			Lifetime = 1;
			Damage = owner.DamageLevel * 5;
			IsShrapnel = false;
		}

		public Bullet(Bullet bullet, float degree)
		{
			//This is a shrapnel
			//add new bullets in random velocities
			OwnerName = bullet.OwnerName;
			PenetratingLevel = bullet.PenetratingLevel;
			ShrapnelLevel = bullet.ShrapnelLevel;
			PositionX = bullet.PositionX;
			PositionY = bullet.PositionY;
			VelocityX = bullet.VelocityX + (float)(Math.Cos(degree) * 100);
			VelocityY = bullet.VelocityY + (float)(Math.Sin(degree) * 100);
			Color = Color.White;
			Scale = 0.5f;
			Lifetime = 1;
			Damage = 5;
			IsShrapnel = true;
		}

		public Bullet(string[] configuration)
		{
			Configuration = configuration;
		}

		public void Update(float frameDeltaTime)
		{
			Lifetime -= frameDeltaTime;
		}
	}
}
