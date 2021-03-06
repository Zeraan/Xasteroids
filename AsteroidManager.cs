﻿using System;
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

				if ((ast1.Phase < 100 && ast2.Phase >= 100) || (ast1.Phase >= 100 && ast2.Phase < 100))
				{
					//Out of phase asteroids won't collide with each other.  Phased asteroids will collide, as non-phased asteroids will.
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

				float dx = tx2 - tx1;
				float dy = ty2 - ty1;
				float dx2 = x2 - x1;
				float dy2 = y2 - y1;
				float r1 = (float)Math.Sqrt(dx * dx + dy * dy); //Get the distance between centers of asteroids
				float r2 = (float)Math.Sqrt(dx2 * dx2 + dy2 * dy2);

				if (r1 < ast1.Radius + ast2.Radius && r1 < r2) //Collision!
				{
					if (!((ast1.AsteroidType == AsteroidType.CLUMPY && ast2.AsteroidType == AsteroidType.CLUMPY) && ast1.Size + ast2.Size <= 5)) //Make sure it's not clumpy asteroids that can clump together
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
				if ((ast1.AsteroidType == AsteroidType.REPULSER || ast2.AsteroidType == AsteroidType.REPULSER) && (r1 < ast1.Size * 64 || r1 < ast2.Size * 64))
				{
					var degree = Math.Atan2(ty2 - ty1, tx2 - tx1);
					float repulsingForce;
					if (ast1.AsteroidType == AsteroidType.REPULSER && ast2.AsteroidType == AsteroidType.REPULSER)
					{
						if (r1 < ast1.Size * 64 && r1 < ast2.Size * 64)
						{
							//Double the power
							repulsingForce = ((ast1.Size * 96) - r1) + ((ast2.Size * 96) - r1);
						}
						else if (r1 < ast1.Size * 64)
						{
							repulsingForce = ((ast1.Size * 96) - r1);
						}
						else
						{
							repulsingForce = ((ast2.Size * 96) - r1);
						}
					}
					else
					{
						if (ast1.AsteroidType == AsteroidType.REPULSER)
						{
							repulsingForce = ((ast1.Size * 96) - r1);
						}
						else
						{
							repulsingForce = ((ast2.Size * 96) - r1);
						}
					}
					ast2.VelocityX += (float)(repulsingForce * Math.Cos(degree) * frameDeltaTime * (ast1.Mass / (float)(ast1.Mass + ast2.Mass)));
					ast2.VelocityY += (float)(repulsingForce * Math.Sin(degree) * frameDeltaTime * (ast1.Mass / (float)(ast1.Mass + ast2.Mass)));
					ast1.VelocityX -= (float)(repulsingForce * Math.Cos(degree) * frameDeltaTime * (ast2.Mass / (float)(ast1.Mass + ast2.Mass)));
					ast1.VelocityY -= (float)(repulsingForce * Math.Sin(degree) * frameDeltaTime * (ast2.Mass / (float)(ast1.Mass + ast2.Mass)));
				}
				if ((ast1.AsteroidType == AsteroidType.GRAVITIC || ast2.AsteroidType == AsteroidType.GRAVITIC) && (r1 < ast1.Size * 64 || r1 < ast2.Size * 64))
				{
					var degree = Math.Atan2(ty2 - ty1, tx2 - tx1);
					float graviticForce;
					if (ast1.AsteroidType == AsteroidType.GRAVITIC && ast2.AsteroidType == AsteroidType.GRAVITIC)
					{
						if (r1 < ast1.Size * 64 && r1 < ast2.Size * 64)
						{
							//Double the power
							graviticForce = ((ast1.Size * 96) - r1) + ((ast2.Size * 96) - r1);
						}
						else if (r1 < ast1.Size * 64)
						{
							graviticForce = ((ast1.Size * 96) - r1);
						}
						else
						{
							graviticForce = ((ast2.Size * 96) - r1);
						}
					}
					else
					{
						if (ast1.AsteroidType == AsteroidType.REPULSER)
						{
							graviticForce = ((ast1.Size * 96) - r1);
						}
						else
						{
							graviticForce = ((ast2.Size * 96) - r1);
						}
					}
					ast2.VelocityX -= (float)(graviticForce * Math.Cos(degree) * frameDeltaTime * (ast1.Mass / (float)(ast1.Mass + ast2.Mass)));
					ast2.VelocityY -= (float)(graviticForce * Math.Sin(degree) * frameDeltaTime * (ast1.Mass / (float)(ast1.Mass + ast2.Mass)));
					ast1.VelocityX += (float)(graviticForce * Math.Cos(degree) * frameDeltaTime * (ast2.Mass / (float)(ast1.Mass + ast2.Mass)));
					ast1.VelocityY += (float)(graviticForce * Math.Sin(degree) * frameDeltaTime * (ast2.Mass / (float)(ast1.Mass + ast2.Mass)));
				}
			}

			public void HandleCollision(Player player, int levelWidth, int levelHeight, Random r, float frameDeltaTime)
			{
				for (int i = 0; i < _asteroidsInCell.Count; i++)
				{
					var asteroid = _asteroidsInCell[i];
					if (asteroid.ToBeRemoved || player.IsDead || asteroid.Phase < 100)
					{
						//No collision possible
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
						v2x += k2 * rx * (1 - player.InertialLevel * 0.05f);
						v2y += k2 * ry * (1 - player.InertialLevel * 0.05f);

						//Assign the final value to asteroids
						asteroid.VelocityX = v1x;
						asteroid.VelocityY = v1y;

						float xDiff = Math.Abs(player.VelocityX) - Math.Abs(v2x);
						float yDiff = Math.Abs(player.VelocityY) - Math.Abs(v2y);
						float amount = (float)Math.Sqrt(xDiff * xDiff + yDiff * yDiff) / 2;
						player.Energy -= amount * (1 - (player.HardnessLevel * 0.05f));
						if (player.Energy < 0)
						{
							player.IsDead = true;
						}
						player.VelocityX = v2x;
						player.VelocityY = v2y;

						//Deal damage and activate shield for player
						player.ShieldAlpha = 1;
					}
					if (asteroid.AsteroidType == AsteroidType.MAGNETIC && r1 < asteroid.Size * 64) //Within magnetic range
					{
						var degree = Math.Atan2(ty2 - ty1, tx2 - tx1);
						asteroid.VelocityX += (float)(((asteroid.Size * 128) - r1) * Math.Cos(degree) * frameDeltaTime);
						asteroid.VelocityY += (float)(((asteroid.Size * 128) - r1) * Math.Sin(degree) * frameDeltaTime);
					}
					if (asteroid.AsteroidType == AsteroidType.REPULSER && r1 < asteroid.Size * 64)
					{
						var degree = Math.Atan2(ty2 - ty1, tx2 - tx1);
						float repulsingForce = ((asteroid.Size * 96) - r1);
						player.VelocityX += (float)(repulsingForce * Math.Cos(degree) * frameDeltaTime * (asteroid.Mass / (float)(asteroid.Mass + player.Mass)));
						player.VelocityY += (float)(repulsingForce * Math.Sin(degree) * frameDeltaTime * (asteroid.Mass / (float)(asteroid.Mass + player.Mass)));
						asteroid.VelocityX -= (float)(repulsingForce * Math.Cos(degree) * frameDeltaTime * (player.Mass / (float)(asteroid.Mass + player.Mass)));
						asteroid.VelocityY -= (float)(repulsingForce * Math.Sin(degree) * frameDeltaTime * (player.Mass / (float)(asteroid.Mass + player.Mass)));
					}
					if (asteroid.AsteroidType == AsteroidType.GRAVITIC && r1 < asteroid.Size * 64)
					{
						var degree = Math.Atan2(ty2 - ty1, tx2 - tx1);
						float graviticForce = ((asteroid.Size * 96) - r1);
						player.VelocityX -= (float)(graviticForce * Math.Cos(degree) * frameDeltaTime * (asteroid.Mass / (float)(asteroid.Mass + player.Mass)));
						player.VelocityY -= (float)(graviticForce * Math.Sin(degree) * frameDeltaTime * (asteroid.Mass / (float)(asteroid.Mass + player.Mass)));
						asteroid.VelocityX += (float)(graviticForce * Math.Cos(degree) * frameDeltaTime * (player.Mass / (float)(asteroid.Mass + player.Mass)));
						asteroid.VelocityY += (float)(graviticForce * Math.Sin(degree) * frameDeltaTime * (player.Mass / (float)(asteroid.Mass + player.Mass)));
					}
				}
			}

			public event Action<string, int> PlayerIsOwedMoney;

			public void HandleBullets(Bullet bullet, ObjectManager objectManager, float frameDeltaTime)
			{
				if (bullet.Damage <= 0)
				{
					return;
				}
				foreach (var asteroid in _asteroidsInCell)
				{
					if (asteroid.HP <= 0 || asteroid.ImpactedBullets.Contains(bullet))
					{
						continue;
						//This asteroid was destroyed, or already impacted by this bullet, skip checking
					}
					float tx1 = bullet.PositionX + bullet.VelocityX * frameDeltaTime;
					float ty1 = bullet.PositionY + bullet.VelocityY * frameDeltaTime;
					float tx2 = asteroid.PositionX + asteroid.VelocityX * frameDeltaTime;
					float ty2 = asteroid.PositionY + asteroid.VelocityY * frameDeltaTime;
					float rx = tx2 - tx1;
					float ry = ty2 - ty1;
					if ((float)Math.Sqrt(rx * rx + ry * ry) < (asteroid.Radius)) //bullet impact!
					{
						float damageDone = asteroid.HP;
						asteroid.HP -= bullet.Damage;
						damageDone -= asteroid.HP;
						asteroid.ImpactedBullets.Add(bullet);
						if (asteroid.HP <= 0)
						{
							damageDone += asteroid.HP;
							//Give money to player who shot it
							int value;
							switch (asteroid.AsteroidType)
							{
								case AsteroidType.GENERIC:
									value = 15;
									break;
								case AsteroidType.CLUMPY:
									value = 10;
									break;
								case AsteroidType.MAGNETIC:
									value = 50;
									break;
								case AsteroidType.REPULSER:
									value = 30;
									break;
								case AsteroidType.GRAVITIC:
									value = 40;
									break;
								case AsteroidType.DENSE:
									value = 30;
									break;
								case AsteroidType.ZIPPY:
									value = 15;
									break;
								case AsteroidType.EXPLOSIVE:
									value = 40;
									break;
								case AsteroidType.BLACK:
									value = 30;
									break;
								case AsteroidType.GOLD:
									value = 200;
									break;
								default:
									value = 50;
									break;
							}
							if (PlayerIsOwedMoney != null)
							{
								PlayerIsOwedMoney(bullet.OwnerName, value * asteroid.Size);
							}
						}
						bullet.Damage -= damageDone * (1 - (bullet.PenetratingLevel * 0.10f));
						objectManager.AddExplosion(bullet.PositionX, bullet.PositionY, asteroid.VelocityX, asteroid.VelocityY, 1);
					}
				}
			}

			public void HandleShockwave(Shockwave shockwave, float frameDeltaTime)
			{
				foreach (var asteroid in _asteroidsInCell)
				{
					if (asteroid.HP <= 0)
					{
						continue;
						//This asteroid was destroyed already, stop checking
					}
					float tx2 = asteroid.PositionX + asteroid.VelocityX * frameDeltaTime;
					float ty2 = asteroid.PositionY + asteroid.VelocityY * frameDeltaTime;
					float rx = tx2 - shockwave.PositionX;
					float ry = ty2 - shockwave.PositionY;
					if ((float)Math.Sqrt(rx * rx + ry * ry) < (shockwave.Radius)) //Shockwave hits it
					{
						asteroid.HP -= shockwave.Size * 100;
						if (asteroid.HP <= 0 && shockwave.OwnerName != null)
						{
							//Give money to player who destroyed it
							int value;
							switch (asteroid.AsteroidType)
							{
								case AsteroidType.GENERIC:
									value = 15;
									break;
								case AsteroidType.CLUMPY:
									value = 10;
									break;
								case AsteroidType.MAGNETIC:
									value = 50;
									break;
								case AsteroidType.REPULSER:
									value = 30;
									break;
								case AsteroidType.GRAVITIC:
									value = 40;
									break;
								case AsteroidType.DENSE:
									value = 30;
									break;
								case AsteroidType.ZIPPY:
									value = 15;
									break;
								case AsteroidType.EXPLOSIVE:
									value = 40;
									break;
								case AsteroidType.BLACK:
									value = 30;
									break;
								case AsteroidType.GOLD:
									value = 200;
									break;
								default:
									value = 50;
									break;
							}
							if (PlayerIsOwedMoney != null)
							{
								PlayerIsOwedMoney(shockwave.OwnerName, value * asteroid.Size);
							}
						}
					}
				}
			}
		}

		private GameMain _gameMain;

		public List<Asteroid> Asteroids { get; set; }
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
					_astCells[i][j].PlayerIsOwedMoney += _gameMain.PayPlayer;
				}
			}
			while (asteroidPoints > 0)
			{
				var type = types[r.Next(types.Length)];
				Asteroid newAst = new Asteroid(type, width, height, r);
				Asteroids.Add(newAst);
				asteroidPoints -= newAst.Point;
			}
		}

		public void UpdatePhysics(List<Player> players, List<Bullet> bullets, List<Shockwave> shockwaves, float frameDeltaTime, Random r)
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
				if (float.IsNaN(ast.PositionX) || float.IsNaN(ast.PositionY))
				{
					//Somehow got corrupted, eject this asteroid from level to avoid corruption of other asteroids and crashing
					ast.ToBeRemoved = true;
					continue;
				}
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
					if (player.IsDead)
					{
						continue;
					}
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
					if (player.IsDead)
					{
						_gameMain.ObjectManager.AddShockwave(player.PositionX, player.PositionY, player.ShipSize, null);
						if (player == _gameMain.MainPlayer)
						{
							player.ClientIsDead = true;
						}
					}
				}
			}
			
			if (bullets != null)
			{
				foreach (var bullet in bullets)
				{
					if (bullet.Damage <= 0)
					{
						continue;
					}
					int x = (int)(bullet.PositionX / GameMain.CELL_SIZE);
					int y = (int)(bullet.PositionY / GameMain.CELL_SIZE);
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
					_astCells[x1][y1].HandleBullets(bullet, _gameMain.ObjectManager, frameDeltaTime);
					_astCells[x][y1].HandleBullets(bullet, _gameMain.ObjectManager, frameDeltaTime);
					_astCells[x2][y1].HandleBullets(bullet, _gameMain.ObjectManager, frameDeltaTime);
					_astCells[x1][y].HandleBullets(bullet, _gameMain.ObjectManager, frameDeltaTime);
					_astCells[x][y].HandleBullets(bullet, _gameMain.ObjectManager, frameDeltaTime);
					_astCells[x2][y].HandleBullets(bullet, _gameMain.ObjectManager, frameDeltaTime);
					_astCells[x1][y2].HandleBullets(bullet, _gameMain.ObjectManager, frameDeltaTime);
					_astCells[x][y2].HandleBullets(bullet, _gameMain.ObjectManager, frameDeltaTime);
					_astCells[x2][y2].HandleBullets(bullet, _gameMain.ObjectManager, frameDeltaTime);
				}
			}
			if (shockwaves != null)
			{
				foreach (var shockwave in shockwaves)
				{
					if (shockwave.TimeTilBoom > 0)
					{
						continue;
					}
					int x = (int)(shockwave.PositionX / GameMain.CELL_SIZE);
					int y = (int)(shockwave.PositionY / GameMain.CELL_SIZE);
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
					_astCells[x1][y1].HandleShockwave(shockwave, frameDeltaTime);
					_astCells[x][y1].HandleShockwave(shockwave, frameDeltaTime);
					_astCells[x2][y1].HandleShockwave(shockwave, frameDeltaTime);
					_astCells[x1][y].HandleShockwave(shockwave, frameDeltaTime);
					_astCells[x][y].HandleShockwave(shockwave, frameDeltaTime);
					_astCells[x2][y].HandleShockwave(shockwave, frameDeltaTime);
					_astCells[x1][y2].HandleShockwave(shockwave, frameDeltaTime);
					_astCells[x][y2].HandleShockwave(shockwave, frameDeltaTime);
					_astCells[x2][y2].HandleShockwave(shockwave, frameDeltaTime);
				}
			}

			List<Asteroid> newAsteroids = new List<Asteroid>();
			foreach (var asteroid in Asteroids)
			{
				//If HP is 0 or less, remove the asteroid and spawn smaller asteroids, unless it's explosive asteroid, in which case it simply emits a shockwave
				if (asteroid.HP <= 0)
				{
					if (asteroid.AsteroidType == AsteroidType.EXPLOSIVE)
					{
						_gameMain.ObjectManager.AddShockwave(asteroid.PositionX, asteroid.PositionY, asteroid.Size, null);
						_gameMain.ObjectManager.AddExplosion(asteroid.PositionX, asteroid.PositionY, asteroid.VelocityX, asteroid.VelocityY, 4);
						asteroid.ToBeRemoved = true;
					}
					else if (!_gameMain.IsMultiplayer || _gameMain.IsHost)
					{
						newAsteroids.AddRange(SpawnAsteroids(asteroid));
						_gameMain.ObjectManager.AddExplosion(asteroid.PositionX, asteroid.PositionY, asteroid.VelocityX, asteroid.VelocityY, 4);
						asteroid.ToBeRemoved = true;
					}
				}
			}
			Asteroids.AddRange(newAsteroids);
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

		private List<Asteroid> SpawnAsteroids(Asteroid whichAsteroid)
		{
			var newAsteroids = new List<Asteroid>();
			if (whichAsteroid.Size == 1)
			{
				//Smallest asteroid, do nothing unless it's explosive, in which case, spawn an explosion
				//TODO: Add explosion code
				return newAsteroids;
			}
			int point = whichAsteroid.Size;

			while (point > 0)
			{
				//Continue spawning until points run out
				float addVel = 0; //for yellow asteroids, they explode fragments FAST
				int newSize = _gameMain.Random.Next(whichAsteroid.Size - 1) + 1;
				Asteroid newAsteroid = new Asteroid(whichAsteroid.AsteroidType, _gameMain.LevelSize.X, _gameMain.LevelSize.Y, _gameMain.Random);
				switch (whichAsteroid.AsteroidType)
				{
					case AsteroidType.GENERIC:
						newAsteroid.HP = 5 * newSize;
						break;
					case AsteroidType.CLUMPY:
						newAsteroid.HP = 10 * newSize;
						break;
					case AsteroidType.GRAVITIC:
						newAsteroid.HP = 10 * newSize;
						break;
					case AsteroidType.REPULSER:
						newAsteroid.HP = 10 * newSize;
						break;
					case AsteroidType.GOLD:
						newAsteroid.HP = 40 * newSize;
						break;
					case AsteroidType.MAGNETIC:
						newAsteroid.HP = 10 * newSize;
						break;
					case AsteroidType.BLACK:
						newAsteroid.HP = 5 * newSize;
						break;
					case AsteroidType.ZIPPY:
						addVel = 200;
						newAsteroid.HP = 5 * newSize;
						break;
					case AsteroidType.DENSE:
						newAsteroid.HP = 50 * newSize;
						break;
					default:
						newAsteroid.HP = 20 * newSize;
						break;
				}
				newAsteroid.Size = newSize;
				newAsteroid.Radius = newAsteroid.Size * 16;
				newAsteroid.AsteroidSprite = SpriteManager.GetAsteroidSprite(newAsteroid.Size, newAsteroid.Style, _gameMain.Random);
				var randomAngle = (_gameMain.Random.Next(360) / 180.0) * Math.PI;
				float tempXVel = (float)Math.Cos(randomAngle) * (75.0f + addVel);
				float tempYVel = (float)Math.Sin(randomAngle) * (75.0f + addVel);
				newAsteroid.PositionX = whichAsteroid.PositionX;
				newAsteroid.PositionY = whichAsteroid.PositionY;
				newAsteroid.VelocityX = whichAsteroid.VelocityX + tempXVel;
				newAsteroid.VelocityY = whichAsteroid.VelocityY + tempYVel;
				point -= newAsteroid.Size;
				newAsteroids.Add(newAsteroid);
			}
			return newAsteroids;
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

	public class Asteroid : IConfigurable
	{
		private BBSprite _asteroidSprite;

		//config exlcludes itself (naturally) and AsteroidSprite
		public const int CONFIG_LENGTH = 18;

        public AsteroidType AsteroidType { get; set; }
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
		public float Phase { get; set; }
		public bool IsPhasing { get; set; }
		public int PhaseSpeed { get; set; }
		public int Point 
		{ 
			get
			{
				switch (AsteroidType)
				{
					case AsteroidType.GENERIC:
					case AsteroidType.CLUMPY:
						return Size;
					case AsteroidType.BLACK:
					case AsteroidType.GRAVITIC:
					case AsteroidType.REPULSER:
						return (int)(Size * 1.5);
					case AsteroidType.MAGNETIC:
					case AsteroidType.EXPLOSIVE:
					case AsteroidType.DENSE:
						return Size * 2;
					case AsteroidType.ZIPPY:
						return (int)(Size * 2.5);
					case AsteroidType.GOLD:
						return Size * 3;
				}
				return Size;
			} 
		}
		public BBSprite AsteroidSprite 
		{ 
			get
			{
				if (_asteroidSprite == null)
				{
					_asteroidSprite = SpriteManager.GetAsteroidSprite(Size, Style, new Random());
				}
				return _asteroidSprite;
			}
			set
			{
				_asteroidSprite = value;
			}
		}
		public List<Bullet> ImpactedBullets { get; set; }  //SO penetrating bullets won't continue to hit the asteroid every frame
		public string[] Configuration
		{
			get
			{
				string[] config = new string[CONFIG_LENGTH];
				config[0] = ((int)AsteroidType).ToString();
				config[1] = ToBeRemoved.ToString();
				config[2] = PositionX.ToString();
				config[3] = PositionY.ToString();
				config[4] = VelocityX.ToString();
				config[5] = VelocityY.ToString();
				config[6] = Angle.ToString();
				config[7] = RotationSpeed.ToString();
				config[8] = HP.ToString();
				config[9] = Color.ToArgb().ToString();
				config[10] = Radius.ToString();
				config[11] = Mass.ToString();
				config[12] = Size.ToString();
				config[13] = Style.ToString();
				config[14] = Phase.ToString();
				config[15] = IsPhasing.ToString();
				config[16] = PhaseSpeed.ToString();
				string bulletsString;
				if (ImpactedBullets == null || ImpactedBullets.Count == 0)
				{
					bulletsString = "[]";
				}
				else
				{
					string[] asStrings = new string[ImpactedBullets.Count];
					for (int j = 0; j < asStrings.Length; ++j)
					{
						string arrayString = "[" + string.Join(",", ImpactedBullets[j].Configuration) + "]";
						asStrings[j] = arrayString;
					}
					bulletsString = "[" + string.Join(",", asStrings) + "]";
				}
				config[17] = bulletsString;
		
				return config;
			}
			set
			{
				if (value.Length < CONFIG_LENGTH)
				{
					return;
				}
				int outInt;
				if (int.TryParse(value[0], out outInt))
				{
					AsteroidType = (AsteroidType) outInt;
				}
				bool outBool;
				if(bool.TryParse(value[1], out outBool))
				{
					ToBeRemoved = outBool;
				}
				float outFloat;
				if (float.TryParse(value[2], out outFloat))
				{
					PositionX = outFloat;
				}
				if (float.TryParse(value[3], out outFloat))
				{
					PositionY = outFloat;
				}
				if (float.TryParse(value[4], out outFloat))
				{
					VelocityX = outFloat;
				}
				if (float.TryParse(value[5], out outFloat))
				{
					VelocityY = outFloat;
				}
				if (float.TryParse(value[6], out outFloat))
				{
					Angle = outFloat;
				}
				if (float.TryParse(value[7], out outFloat))
				{
					RotationSpeed = outFloat;
				}
				if (float.TryParse(value[8], out outFloat))
				{
					HP = outFloat;
				}
				if (int.TryParse(value[9], out outInt))
				{
					Color = Color.FromArgb(outInt);
				}
				if(int.TryParse(value[10], out outInt))
				{
					Radius = outInt;
				}
				if (int.TryParse(value[11], out outInt))
				{
					Mass = outInt;
				}
				if (int.TryParse(value[12], out outInt))
				{
					Size = outInt;
				}
				if (int.TryParse(value[13], out outInt))
				{
					Style = outInt;
				}
				if (float.TryParse(value[14], out outFloat))
				{
					Phase = outFloat;
				}
				if (bool.TryParse(value[15], out outBool))
				{
					IsPhasing = outBool;
				}
				if (int.TryParse(value[16], out outInt))
				{
					PhaseSpeed = outInt;
				}

				ImpactedBullets = new List<Bullet>();
				string bulletsString = value[17];
				string contents = bulletsString.Substring(1, bulletsString.Length - 2);
				if (contents.Length != 0)
				{
					string[] bulletConfigStrings = ObjectStringConverter.ConfigurationFromStringOfConfigurations(contents);
					foreach (string bulletConfigString in bulletConfigStrings)
					{
						contents = bulletConfigString.Substring(1, bulletConfigString.Length - 2);
						string[] config = ObjectStringConverter.ConfigurationFromMixedString(contents);
						ImpactedBullets.Add(new Bullet(config));
					}
				}				
			}
		}

		public Asteroid(AsteroidType asteroidType, int width, int height, Random r)
		{
			AsteroidType = asteroidType;
			ImpactedBullets = new List<Bullet>();
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
			Phase = 255;
			IsPhasing = true;

			//Here begins asteroid-type specific code
			switch (AsteroidType)
			{
				case AsteroidType.GENERIC:
					{
						Color = Color.White;

						VelocityX = r.Next(20, 200) * (r.Next(2) > 0 ? -1 : 1);
						VelocityY = r.Next(20, 200) * (r.Next(2) > 0 ? -1 : 1);

						Mass = Size * 200;
						HP = Size * 5;
						break;
					}
				case AsteroidType.CLUMPY:
					{
						Color = Color.MediumPurple;

						VelocityX = r.Next(80, 260) * (r.Next(2) > 0 ? -1 : 1);
						VelocityY = r.Next(80, 260) * (r.Next(2) > 0 ? -1 : 1);

						Mass = Size * 300;
						HP = Size * 10;
						break;
					}
				case AsteroidType.MAGNETIC:
					{
						Color = Color.Blue;

						VelocityX = r.Next(80, 240) * (r.Next(2) > 0 ? -1 : 1);
						VelocityY = r.Next(80, 240) * (r.Next(2) > 0 ? -1 : 1);

						Mass = Size * 160;
						HP = Size * 10;
						break;
					}
				case AsteroidType.EXPLOSIVE:
					{
						Color = Color.OrangeRed;

						VelocityX = r.Next(160, 200) * (r.Next(2) > 0 ? -1 : 1);
						VelocityY = r.Next(160, 200) * (r.Next(2) > 0 ? -1 : 1);

						Mass = Size * 240;
						HP = Size * 3;
						break;
					}
				case AsteroidType.BLACK:
					{
						Color = Color.Black;

						VelocityX = r.Next(20, 200) * (r.Next(2) > 0 ? -1 : 1);
						VelocityY = r.Next(20, 200) * (r.Next(2) > 0 ? -1 : 1);

						Mass = Size * 200;
						HP = Size * 5;
						break;
					}
				case AsteroidType.DENSE:
					{
						Color = Color.Gray;

						VelocityX = r.Next(20, 160) * (r.Next(2) > 0 ? -1 : 1);
						VelocityY = r.Next(20, 160) * (r.Next(2) > 0 ? -1 : 1);

						Mass = Size * 1100;
						HP = Size * 50;
						break;
					}
				case AsteroidType.GRAVITIC:
					{
						Color = Color.Cyan;

						VelocityX = r.Next(200, 400) * (r.Next(2) > 0 ? -1 : 1);
						VelocityY = r.Next(200, 400) * (r.Next(2) > 0 ? -1 : 1);

						Mass = Size * 300;
						HP = Size * 10;
						break;
					}
				case AsteroidType.ZIPPY:
					{
						Color = Color.GreenYellow;

						VelocityX = r.Next(400, 1000) * (r.Next(2) > 0 ? -1 : 1);
						VelocityY = r.Next(400, 1000) * (r.Next(2) > 0 ? -1 : 1);

						Mass = Size * 30;
						HP = Size * 5;
						break;
					}
				case AsteroidType.REPULSER:
					{
						Color = Color.HotPink;

						VelocityX = r.Next(200, 400) * (r.Next(2) > 0 ? -1 : 1);
						VelocityY = r.Next(200, 400) * (r.Next(2) > 0 ? -1 : 1);

						Mass = Size * 260;
						HP = Size * 10;
						break;
					}
				case AsteroidType.PHASING:
					{
						Color = Color.DarkRed;

						VelocityX = r.Next(40, 280) * (r.Next(2) > 0 ? -1 : 1);
						VelocityY = r.Next(40, 280) * (r.Next(2) > 0 ? -1 : 1);

						Mass = Size * 220;
						HP = Size * 20;

						PhaseSpeed = r.Next(10, 255);
						break;
					}
				case AsteroidType.GOLD:
					{
						Color = Color.Gold;

						VelocityX = r.Next(100, 300) * (r.Next(2) > 0 ? -1 : 1);
						VelocityY = r.Next(100, 300) * (r.Next(2) > 0 ? -1 : 1);

						Mass = Size * 400;
						HP = Size * 40;
						break;
					}
			}
		}

		public Asteroid(string[] configuration)
		{
			Configuration = configuration;
		}

		public virtual void Update(int width, int height, float frameDeltaTime)
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

			if (AsteroidType == AsteroidType.PHASING)
			{
				if (IsPhasing)
				{
					Phase -= PhaseSpeed * frameDeltaTime;
					if (Phase < 0)
					{
						IsPhasing = false;
						Phase = 0;
					}
				}
				else
				{
					Phase += PhaseSpeed * frameDeltaTime;
					if (Phase > 255)
					{
						IsPhasing = true;
						Phase = 255;
					}
				}
			}
		}
	}

	public class AsteroidsList : IConfigurable
	{
		public List<Asteroid> Asteroids { get; set; }

		public const int CONFIG_LENGTH = 1;
		public string[] Configuration
		{
			get
			{
				string[] config = new string[CONFIG_LENGTH];
				config[0] = ObjectStringConverter.IConfigurableListToArrayString(Asteroids);
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
			}
		}
	}
}
