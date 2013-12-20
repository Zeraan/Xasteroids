using System;
using System.Collections.Generic;
using System.Drawing;

namespace Xasteroids
{
	public enum AsteroidType { GENERIC, CLUMPY, MAGNETIC, EXPLOSIVE, BLACK, DENSE, GRAVITIC, ZIPPY, REPULSER, PHASING, GOLD }

	public class AsteroidManager
	{
		private GameMain _gameMain;

		public List<Asteroid> Asteroids { get; private set; }

		public AsteroidManager(GameMain gameMain)
		{
			_gameMain = gameMain;
			Asteroids = new List<Asteroid>();
		}

		public void SetUpLevel(AsteroidType[] types, int asteroidPoints, Random r)
		{
			Asteroids.Clear(); //Just to make sure it's really empty
			int width = _gameMain.LevelSize.X;
			int height = _gameMain.LevelSize.Y;
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

		public void UpdatePhysics(List<Player> players, float frameDeltaTime, Random r)
		{
			//First, update the asteroids' collision
			UpdateAsteroidPhysics(frameDeltaTime, r);
		}

		private void UpdateAsteroidPhysics(float frameDeltaTime, Random r)
		{
			List<Asteroid> asteroidsToRemove = new List<Asteroid>();

			for (int i = 0; i < Asteroids.Count; i++)
			{
				for (int j = i + 1; j < Asteroids.Count; j++)
				{
					if (Asteroids[i].ToBeRemoved || Asteroids[j].ToBeRemoved)
					{
						//Some asteroids have clumped together, don't calculate between any asteroids with the asteroid to be removed;
						continue;
					}
					//create variables that'd be easier to read than function calls
					float x1 = Asteroids[i].PositionX;
					float y1 = Asteroids[i].PositionY;
					float x2 = Asteroids[j].PositionX;
					float y2 = Asteroids[j].PositionY;

					float v1x = Asteroids[i].VelocityX; //e.FrameDeltaTime is the time between frames, less than 1
					float v1y = Asteroids[i].VelocityY;
					float v2x = Asteroids[j].VelocityX;
					float v2y = Asteroids[j].VelocityY;

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

					if (r1 < (Asteroids[i].Size * 16 + Asteroids[j].Size * 16) && r1 < r2) //Collision!
					{
						if (!((Asteroids[i] is ClumpyAsteroid && Asteroids[j] is ClumpyAsteroid) && Asteroids[i].Size + Asteroids[j].Size <= 5)) //Make sure it's not clumpy asteroids that can clump together
						{
							//Calculate the impulse or change of momentum, or whatever people call it
							float rx = dx / r1;
							float ry = dy / r1;
							float k1 = 2 * Asteroids[j].Mass * (rx * (v2x - v1x) + ry * (v2y - v1y)) / (Asteroids[i].Mass + Asteroids[j].Mass);
							float k2 = 2 * Asteroids[i].Mass * (rx * (v1x - v2x) + ry * (v1y - v2y)) / (Asteroids[i].Mass + Asteroids[j].Mass);

							//Adjust the velocities
							v1x += k1 * rx;
							v1y += k1 * ry;
							v2x += k2 * rx;
							v2y += k2 * ry;

							//Assign the final value to asteroids
							Asteroids[i].VelocityX = v1x;
							Asteroids[i].VelocityY = v1y;
							Asteroids[j].VelocityX = v2x;
							Asteroids[j].VelocityY = v2y;
						}
						else //it's clumpy, clump them together
						{
							float firstAsteroidFactor = (Asteroids[i].Size * 1.0f) / (Asteroids[i].Size + Asteroids[j].Size);
							float secondAsteroidFactor = 1.0f - firstAsteroidFactor;
							Asteroids[i].Size += Asteroids[j].Size;
							Asteroids[i].Mass = Asteroids[i].Size * 300;
							Asteroids[i].PositionX = (Asteroids[i].PositionX + Asteroids[j].PositionX) / 2;
							Asteroids[i].PositionY = (Asteroids[i].PositionY + Asteroids[j].PositionY) / 2;
							//Combine position and velocity:
							Asteroids[i].VelocityX = (Asteroids[i].VelocityX * firstAsteroidFactor) + (Asteroids[j].VelocityX * secondAsteroidFactor);
							Asteroids[i].VelocityY = (Asteroids[i].VelocityY * firstAsteroidFactor) + (Asteroids[j].VelocityY * secondAsteroidFactor);
							Asteroids[i].AsteroidSprite = SpriteManager.GetAsteroidSprite(Asteroids[i].Size, Asteroids[i].Style, r);
							//TODO: Combine the remaining HP

							Asteroids[j].ToBeRemoved = true;
							asteroidsToRemove.Add(Asteroids[j]);
						}
					}
				}
			}

			foreach (var asteroid in asteroidsToRemove)
			{
				Asteroids.Remove(asteroid);
			}
		}

		public void UpdateAsteroids(float frameDeltaTime)
		{
			//Rotate/move asteroids
			foreach (var asteroid in Asteroids)
			{
				asteroid.Update(_gameMain.LevelSize.X, _gameMain.LevelSize.Y, frameDeltaTime);
			}
		}
	}

	public class Asteroid
	{
		public bool ToBeRemoved { get; set; }
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
			ToBeRemoved = false;
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

		public void Update(int width, int height, float frameDeltaTime)
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
			PositionX += VelocityX * frameDeltaTime;
			PositionY += VelocityY * frameDeltaTime;
			while (PositionX < 0)
			{
				PositionX += width;
			}
			while (PositionX >= width)
			{
				PositionX -= width;
			}
			while (PositionY < 0)
			{
				PositionY += height;
			}
			while (PositionY >= height)
			{
				PositionY -= height;
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

			Mass = Size * 200;
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

			Mass = Size * 300;
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

			Mass = Size * 160;
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

			Mass = Size * 240;
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

			Mass = Size * 200;
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

			Mass = Size * 1100;
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

			Mass = Size * 300;
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

			Mass = Size * 30;
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

			Mass = Size * 260;
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

			Mass = Size * 220;
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

			Mass = Size * 400;
		}
	}
}
