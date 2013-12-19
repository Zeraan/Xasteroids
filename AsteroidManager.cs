using System;
using System.Collections.Generic;
using System.Drawing;

namespace Xasteroids
{
	public enum AsteroidType { GENERIC, CLUMPY, MAGNETIC, EXPLOSIVE, BLACK, DENSE, GRAVITIC, ZIPPY, REPULSER, PHASING, GOLD }

	public class AsteroidManager
	{
		public List<Asteroid> Asteroids { get; private set; }
		public Point LevelSize { get; private set; }

		public AsteroidManager()
		{
			Asteroids = new List<Asteroid>();
			LevelSize = new Point();
		}

		public void SetUpLevel(int width, int height, AsteroidType[] types, int asteroidPoints, Random r)
		{
			LevelSize = new Point(width, height);
			Asteroids.Clear(); //Just to make sure it's really empty
			while (asteroidPoints > 0)
			{
				var type = types[r.Next(types.Length)];
				Asteroid newAst;
				switch (type)
				{
					case AsteroidType.GENERIC:
						newAst = new GenericAsteroid(width, height, r);
						Asteroids.Add(newAst);
						asteroidPoints -= newAst.Point;
						break;
					case AsteroidType.CLUMPY:
						newAst = new ClumpyAsteroid(width, height, r);
						Asteroids.Add(newAst);
						asteroidPoints -= newAst.Point;
						break;
					case AsteroidType.MAGNETIC:
						newAst = new MagneticAsteroid(width, height, r);
						Asteroids.Add(newAst);
						asteroidPoints -= newAst.Point;
						break;
					case AsteroidType.EXPLOSIVE:
						newAst = new ExplosiveAsteroid(width, height, r);
						Asteroids.Add(newAst);
						asteroidPoints -= newAst.Point;
						break;
					case AsteroidType.BLACK:
						newAst = new BlackAsteroid(width, height, r);
						Asteroids.Add(newAst);
						asteroidPoints -= newAst.Point;
						break;
					case AsteroidType.DENSE:
						newAst = new DenseAsteroid(width, height, r);
						Asteroids.Add(newAst);
						asteroidPoints -= newAst.Point;
						break;
					case AsteroidType.GRAVITIC:
						newAst = new GraviticAsteroid(width, height, r);
						Asteroids.Add(newAst);
						asteroidPoints -= newAst.Point;
						break;
					case AsteroidType.ZIPPY:
						newAst = new ZippyAsteroid(width, height, r);
						Asteroids.Add(newAst);
						asteroidPoints -= newAst.Point;
						break;
					case AsteroidType.REPULSER:
						newAst = new RepulserAsteroid(width, height, r);
						Asteroids.Add(newAst);
						asteroidPoints -= newAst.Point;
						break;
					case AsteroidType.PHASING:
						newAst = new PhasingAsteroid(width, height, r);
						Asteroids.Add(newAst);
						asteroidPoints -= newAst.Point;
						break;
					case AsteroidType.GOLD:
						newAst = new GoldAsteroid(width, height, r);
						Asteroids.Add(newAst);
						asteroidPoints -= newAst.Point;
						break;
				}
			}
		}

		public void UpdateAsteroids(float frameDeltaTime)
		{
			foreach (var asteroid in Asteroids)
			{
				asteroid.UpdateRotation(frameDeltaTime);
			}
		}
	}

	public class Asteroid
	{
		public float PositionX { get; set; }
		public float PositionY { get; set; }
		public float VelocityX { get; set; }
		public float VelocityY { get; set; }
		public float Angle { get; set; }
		public float RotationSpeed { get; set; }
		public float HP { get; set; }
		public Color Color { get; set; }
		public int Mass { get; set; }
		public int Size { get; set; }
		public int Style { get; set; }
		public virtual int Point { get; set; }
		public BBSprite AsteroidSprite { get; set; }

		public Asteroid(int width, int height, Random r)
		{
			//Safe zone can't be spawned in, 400x400 in middle of level
			int safeWidth = (width / 2) - 200;
			int safeHeight = (height / 2) - 200;

			//Determine which side to spawn on
			int temp = r.Next(4);

			switch (temp)
			{
				case 0: //left side
					PositionX = r.Next(safeWidth);
					PositionY = r.Next(height);
					break;
				case 1: //right side
					PositionX = width - r.Next(safeWidth);
					PositionY = r.Next(height);
					break;
				case 2: //top side
					PositionX = r.Next(width);
					PositionY = r.Next(safeHeight);
					break;
				case 3: //bottom side
					PositionX = r.Next(width);
					PositionY = height -r.Next(safeHeight);
					break;
			}

			Size = r.Next(5) + 1;
			Style = r.Next(3) + 1;
			RotationSpeed = (r.Next(1800) / 10.0f) * (r.Next(2) > 0 ? -1 : 1);
			AsteroidSprite = SpriteManager.GetAsteroidSprite(Size, Style, r);
		}

		public void UpdateRotation(float frameDeltaTime)
		{
			Angle += RotationSpeed * frameDeltaTime;
			while (Angle < 0)
			{
				Angle += 360;
			}
			while (Angle > 360)
			{
				Angle -= 360;
			}
		}
	}

	public class GenericAsteroid : Asteroid
	{
		public override int Point
		{
			get { return Size; }
		}

		public GenericAsteroid(int width, int height, Random r)
			: base(width, height, r)
		{
			Color = Color.White;

			VelocityX = r.Next(10, 100) * (r.Next(2) > 0 ? -1 : 1);
			VelocityY = r.Next(10, 100) * (r.Next(2) > 0 ? -1 : 1);
		}
	}
	public class ClumpyAsteroid : Asteroid
	{
		public override int Point
		{
			get { return Size; }
		}

		public ClumpyAsteroid(int width, int height, Random r)
			: base(width, height, r)
		{
			Color = Color.MediumPurple;

			VelocityX = r.Next(40, 130) * (r.Next(2) > 0 ? -1 : 1);
			VelocityY = r.Next(40, 130) * (r.Next(2) > 0 ? -1 : 1);
		}
	}
	public class MagneticAsteroid : Asteroid
	{
		public override int Point
		{
			get { return Size * 2; }
		}

		public MagneticAsteroid(int width, int height, Random r)
			: base(width, height, r)
		{
			Color = Color.Blue;

			VelocityX = r.Next(40, 120) * (r.Next(2) > 0 ? -1 : 1);
			VelocityY = r.Next(40, 120) * (r.Next(2) > 0 ? -1 : 1);
		}
	}
	public class ExplosiveAsteroid : Asteroid
	{
		public override int Point
		{
			get { return Size * 2; }
		}

		public ExplosiveAsteroid(int width, int height, Random r)
			: base(width, height, r)
		{
			Color = Color.OrangeRed;

			VelocityX = r.Next(80, 100) * (r.Next(2) > 0 ? -1 : 1);
			VelocityY = r.Next(80, 100) * (r.Next(2) > 0 ? -1 : 1);
		}
	}
	public class BlackAsteroid : Asteroid
	{
		public override int Point
		{
			get { return (int)(Size * 1.5); }
		}

		public BlackAsteroid(int width, int height, Random r)
			: base(width, height, r)
		{
			Color = Color.Black;

			VelocityX = r.Next(10, 100) * (r.Next(2) > 0 ? -1 : 1);
			VelocityY = r.Next(10, 100) * (r.Next(2) > 0 ? -1 : 1);
		}
	}
	public class DenseAsteroid : Asteroid
	{
		public override int Point
		{
			get { return Size * 2; }
		}

		public DenseAsteroid(int width, int height, Random r)
			: base(width, height, r)
		{
			Color = Color.Gray;

			VelocityX = r.Next(10, 80) * (r.Next(2) > 0 ? -1 : 1);
			VelocityY = r.Next(10, 80) * (r.Next(2) > 0 ? -1 : 1);
		}
	}
	public class GraviticAsteroid : Asteroid
	{
		public override int Point
		{
			get { return (int)(Size * 1.5); }
		}

		public GraviticAsteroid(int width, int height, Random r)
			: base(width, height, r)
		{
			Color = Color.Cyan;

			VelocityX = r.Next(100, 200) * (r.Next(2) > 0 ? -1 : 1);
			VelocityY = r.Next(100, 200) * (r.Next(2) > 0 ? -1 : 1);
		}
	}
	public class ZippyAsteroid : Asteroid
	{
		public override int Point
		{
			get { return (int)(Size * 2.5); }
		}

		public ZippyAsteroid(int width, int height, Random r)
			: base(width, height, r)
		{
			Color = Color.GreenYellow;

			VelocityX = r.Next(200, 500) * (r.Next(2) > 0 ? -1 : 1);
			VelocityY = r.Next(200, 500) * (r.Next(2) > 0 ? -1 : 1);
		}
	}
	public class RepulserAsteroid : Asteroid
	{
		public override int Point
		{
			get { return (int)(Size * 1.5); }
		}

		public RepulserAsteroid(int width, int height, Random r)
			: base(width, height, r)
		{
			Color = Color.HotPink;

			VelocityX = r.Next(100, 200) * (r.Next(2) > 0 ? -1 : 1);
			VelocityY = r.Next(100, 200) * (r.Next(2) > 0 ? -1 : 1);
		}
	}
	public class PhasingAsteroid : Asteroid
	{
		public override int Point
		{
			get { return Size * 2; }
		}

		public PhasingAsteroid(int width, int height, Random r)
			: base(width, height, r)
		{
			Color = Color.DarkRed;

			VelocityX = r.Next(20, 140) * (r.Next(2) > 0 ? -1 : 1);
			VelocityY = r.Next(20, 140) * (r.Next(2) > 0 ? -1 : 1);
		}
	}
	public class GoldAsteroid : Asteroid
	{
		public override int Point
		{
			get { return Size * 3; }
		}

		public GoldAsteroid(int width, int height, Random r)
			: base(width, height, r)
		{
			Color = Color.Gold;

			VelocityX = r.Next(50, 150) * (r.Next(2) > 0 ? -1 : 1);
			VelocityY = r.Next(50, 150) * (r.Next(2) > 0 ? -1 : 1);
		}
	}
}
