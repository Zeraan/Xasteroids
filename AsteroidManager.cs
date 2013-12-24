using System;
using System.Collections.Generic;
using System.Drawing;

namespace Xasteroids
{
	public enum AsteroidType { GENERIC, CLUMPY, MAGNETIC, EXPLOSIVE, BLACK, DENSE, GRAVITIC, ZIPPY, REPULSER, PHASING, GOLD }

	public class AsteroidManager
	{
		//This is used for performance reasons
		private class AstCell
		{
			List<Asteroid> _asteroidsInCell = new List<Asteroid>();

			public void Reset()
			{
				_asteroidsInCell.Clear();
			}

			public void AddAsteroid(Asteroid asteroid, int levelWidth, int levelHeight, Random r, float frameDeltaTime)
			{
				foreach (var ast in _asteroidsInCell)
				{
					HandleCollision(ast, asteroid, levelWidth, levelHeight, r, frameDeltaTime);
				}
				_asteroidsInCell.Add(asteroid);
			}

			private static void HandleCollision(Asteroid ast1, Asteroid ast2, int levelWidth, int levelHeight, Random r, float frameDeltaTime)
			{
				if (ast1.ToBeRemoved || ast2.ToBeRemoved)
				{
					//Some asteroids have clumped together, don't calculate between any asteroids with the asteroid to be removed;
					return;
				}

				//create variables that'd be easier to read than function calls
				float x1 = ast1.PositionX;
				float y1 = ast1.PositionY;
				float x2 = ast2.PositionX;
				float y2 = ast2.PositionY;

				Utility.GetClosestDistance(x1, y1, x2, y2, levelWidth, levelHeight, out x2, out y2);

				float v1x = ast1.VelocityX; //e.FrameDeltaTime is the time between frames, less than 1
				float v1y = ast1.VelocityY;
				float v2x = ast2.VelocityX;
				float v2y = ast2.VelocityY;

				//get the position plus velocity
				float tx1 = x1 + v1x * frameDeltaTime;
				float ty1 = y1 + v1y * frameDeltaTime;
				float tx2 = x2 + v2x * frameDeltaTime;
				float ty2 = y2 + v2y * frameDeltaTime;

				//if (Utility.QuickDistCollisionCheck(tx1, ty1, Asteroids[i].Size * 16, tx2, ty2, Asteroids[j].Size * 16))
				{
					float dx = tx2 - tx1;
					float dy = ty2 - ty1;
					float dx2 = x2 - x1;
					float dy2 = y2 - y1;
					float r1 = dx * dx + dy * dy; //Get the distance between centers of asteroids
					float r2 = dx2 * dx2 + dy2 * dy2;

					if (r1 < Math.Pow(ast1.Radius + ast2.Radius, 2) && r1 < r2) //Collision!
					{
						r1 = (float)Math.Sqrt(r1); //Didn't square root earlier for performance
						if (!((ast1 is ClumpyAsteroid && ast2 is ClumpyAsteroid) && ast1.Size + ast2.Size <= 5)) //Make sure it's not clumpy asteroids that can clump together
						{
							//Calculate the impulse or change of momentum, or whatever people call it
							float rx = dx / r1;
							float ry = dy / r1;
							float k1 = 2 * ast2.Mass * (rx * (v2x - v1x) + ry * (v2y - v1y)) / (ast1.Mass + ast2.Mass);
							float k2 = 2 * ast1.Mass * (rx * (v1x - v2x) + ry * (v1y - v2y)) / (ast1.Mass + ast2.Mass);

							//Adjust the velocities
							v1x += k1 * rx;
							v1y += k1 * ry;
							v2x += k2 * rx;
							v2y += k2 * ry;

							//Assign the final value to asteroids
							ast1.VelocityX = v1x;
							ast1.VelocityY = v1y;
							ast2.VelocityX = v2x;
							ast2.VelocityY = v2y;
						}
						else //it's clumpy, clump them together
						{
							float firstAsteroidFactor = (ast1.Size * 1.0f) / (ast1.Size + ast2.Size);
							float secondAsteroidFactor = 1.0f - firstAsteroidFactor;
							ast1.Size += ast2.Size;
							ast1.Radius = ast1.Size * 16;
							ast1.Mass = ast1.Size * 300;
							ast1.PositionX = (ast1.PositionX + ast2.PositionX) / 2;
							ast1.PositionY = (ast1.PositionY + ast2.PositionY) / 2;
							//Combine position and velocity:
							ast1.VelocityX = (ast1.VelocityX * firstAsteroidFactor) + (ast2.VelocityX * secondAsteroidFactor);
							ast1.VelocityY = (ast1.VelocityY * firstAsteroidFactor) + (ast2.VelocityY * secondAsteroidFactor);
							ast1.AsteroidSprite = SpriteManager.GetAsteroidSprite(ast1.Size, ast1.Style, r);
							//TODO: Combine the remaining HP

							ast2.ToBeRemoved = true;
						}
					}
				}
			}

			public void HandleCollision(Player player, int levelWidth, int levelHeight, Random r, float frameDeltaTime)
			{
				for (int i = 0; i < _asteroidsInCell.Count; i++)
				{
					var asteroid = _asteroidsInCell[i];
					if (asteroid.ToBeRemoved || player.IsDead)
					{
						//Some asteroids have clumped together, don't calculate between any asteroids with the asteroid to be removed;
						continue;
					}
					//create variables that'd be easier to read than function calls
					float x1 = asteroid.PositionX;
					float y1 = asteroid.PositionY;
					float x2 = player.PositionX;
					float y2 = player.PositionY;

					Utility.GetClosestDistance(x1, y1, x2, y2, levelWidth, levelHeight, out x2, out y2);

					float v1x = asteroid.VelocityX; //e.FrameDeltaTime is the time between frames, less than 1
					float v1y = asteroid.VelocityY;
					float v2x = player.VelocityX;
					float v2y = player.VelocityY;

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

					if (r1 < (asteroid.Radius + player.ShipSize * 16) && r1 < r2) //Collision!
					{
						//Calculate the impulse or change of momentum, or whatever people call it
						float rx = dx / r1;
						float ry = dy / r1;
						float k1 = 2 * player.Mass * (rx * (v2x - v1x) + ry * (v2y - v1y)) / (asteroid.Mass + player.Mass);
						float k2 = 2 * asteroid.Mass * (rx * (v1x - v2x) + ry * (v1y - v2y)) / (asteroid.Mass + player.Mass);

						//Adjust the velocities
						v1x += k1 * rx;
						v1y += k1 * ry;
						v2x += k2 * rx;
						v2y += k2 * ry;

						//Assign the final value to asteroids
						asteroid.VelocityX = v1x;
						asteroid.VelocityY = v1y;
						player.VelocityX = v2x;
						player.VelocityY = v2y;

						//Deal damage and activate shield for player
						player.ShieldAlpha = 1;
					}
				}
			}
		}

		private GameMain _gameMain;

		public List<Asteroid> Asteroids { get; private set; }
		private AstCell[][] _astCells;

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
			_astCells = new AstCell[width / GameMain.CELL_SIZE][];
			for (int i = 0; i < _astCells.Length; i++)
			{
				_astCells[i] = new AstCell[height / GameMain.CELL_SIZE];
				for (int j = 0; j < _astCells[i].Length; j++)
				{
					_astCells[i][j] = new AstCell();
				}
			}
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
			for (int i = 0; i < _astCells.Length; i++)
			{
				for (int j = 0; j < _astCells[i].Length; j++)
				{
					_astCells[i][j].Reset();
				}
			}
			int levelWidth = _gameMain.LevelSize.X;
			int levelHeight = _gameMain.LevelSize.Y;
			foreach (var ast in Asteroids)
			{
				int x = (int)(ast.PositionX / GameMain.CELL_SIZE);
				int y = (int)(ast.PositionY / GameMain.CELL_SIZE);
				int x1 = x - 1;
				int x2 = x + 1;
				int y1 = y - 1;
				int y2 = y + 1;
				if (x1 < 0)
				{
					x1 = _astCells.Length - 1;
				}
				if (y1 < 0)
				{
					y1 = _astCells[x].Length - 1;
				}
				if (x2 >= _astCells.Length)
				{
					x2 = 0;
				}
				if (y2 >= _astCells[x].Length)
				{
					y2 = 0;
				}
				_astCells[x1][y1].AddAsteroid(ast, levelWidth, levelHeight, r, frameDeltaTime);
				_astCells[x][y1].AddAsteroid(ast, levelWidth, levelHeight, r, frameDeltaTime);
				_astCells[x2][y1].AddAsteroid(ast, levelWidth, levelHeight, r, frameDeltaTime);
				_astCells[x1][y].AddAsteroid(ast, levelWidth, levelHeight, r, frameDeltaTime);
				_astCells[x][y].AddAsteroid(ast, levelWidth, levelHeight, r, frameDeltaTime);
				_astCells[x2][y].AddAsteroid(ast, levelWidth, levelHeight, r, frameDeltaTime);
				_astCells[x1][y2].AddAsteroid(ast, levelWidth, levelHeight, r, frameDeltaTime);
				_astCells[x][y2].AddAsteroid(ast, levelWidth, levelHeight, r, frameDeltaTime);
				_astCells[x2][y2].AddAsteroid(ast, levelWidth, levelHeight, r, frameDeltaTime);
			}

			if (players != null)
			{
				//Second, update the asteroid vs ship collisions
				foreach (var player in players)
				{
					int x = (int)(player.PositionX / GameMain.CELL_SIZE);
					int y = (int)(player.PositionY / GameMain.CELL_SIZE);
					int x1 = x - 1;
					int x2 = x + 1;
					int y1 = y - 1;
					int y2 = y + 1;
					if (x1 < 0)
					{
						x1 = _astCells.Length - 1;
					}
					if (y1 < 0)
					{
						y1 = _astCells[x].Length - 1;
					}
					if (x2 >= _astCells.Length)
					{
						x2 = 0;
					}
					if (y2 >= _astCells[x].Length)
					{
						y2 = 0;
					}
					_astCells[x1][y1].HandleCollision(player, levelWidth, levelHeight, r, frameDeltaTime);
					_astCells[x][y1].HandleCollision(player, levelWidth, levelHeight, r, frameDeltaTime);
					_astCells[x2][y1].HandleCollision(player, levelWidth, levelHeight, r, frameDeltaTime);
					_astCells[x1][y].HandleCollision(player, levelWidth, levelHeight, r, frameDeltaTime);
					_astCells[x][y].HandleCollision(player, levelWidth, levelHeight, r, frameDeltaTime);
					_astCells[x2][y].HandleCollision(player, levelWidth, levelHeight, r, frameDeltaTime);
					_astCells[x1][y2].HandleCollision(player, levelWidth, levelHeight, r, frameDeltaTime);
					_astCells[x][y2].HandleCollision(player, levelWidth, levelHeight, r, frameDeltaTime);
					_astCells[x2][y2].HandleCollision(player, levelWidth, levelHeight, r, frameDeltaTime);
				}
			}

			List<Asteroid> asteroidsToRemove = new List<Asteroid>();
			//Remove all the asteroids to be removed
			for (int i = 0; i < Asteroids.Count; i++)
			{
				if (Asteroids[i].ToBeRemoved)
				{
					asteroidsToRemove.Add(Asteroids[i]);
				}
			}
			foreach (var asteroid in asteroidsToRemove)
			{
				Asteroids.Remove(asteroid);
			}
		}

		public void Update(float frameDeltaTime)
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
		public int Radius { get; set; }
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
					PositionX = width - (r.Next(safeWidth) + 1);
					PositionY = r.Next(height);
					break;
				case 2: //top side
					PositionX = r.Next(width);
					PositionY = r.Next(safeHeight);
					break;
				case 3: //bottom side
					PositionX = r.Next(width);
					PositionY = height - (r.Next(safeHeight) + 1);
					break;
			}

			Size = r.Next(5) + 1;
			Radius = Size * 16;
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
