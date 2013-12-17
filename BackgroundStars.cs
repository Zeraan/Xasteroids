using System.Drawing;

namespace Xasteroids
{
	public class BackgroundStars
	{
		private class BackgroundStar
		{
			public float XPos { get; private set; }
			public float YPos { get; private set; }
			public Color Color { get; private set; }

			private int _layer;

			public BackgroundStar(float xPos, float yPos, int Layer, Color color)
			{
				XPos = xPos;
				YPos = yPos;
				_layer = Layer;
				Color = color;
			}

			public void Move(float xAmount, float yAmount, int screenWidth, int screenHeight)
			{
				XPos += (xAmount / _layer);
				YPos += (yAmount / _layer);

				if (XPos < -16)
				{
					XPos += screenWidth + 32;
				}
				else if (XPos > screenWidth + 16)
				{
					XPos -= screenWidth + 32;
				}
				if (YPos < -16)
				{
					YPos += screenHeight + 32;
				}
				else if (YPos > screenHeight + 16)
				{
					YPos -= screenHeight + 32;
				}
			}
		}

		private BBSprite _backGroundStar;
		private GameMain _gameMain;
		private BackgroundStar[] _backGroundStars;

		public bool Initialize(GameMain gameMain, out string reason)
		{
			_gameMain = gameMain;
			_backGroundStar = SpriteManager.GetSprite("BackgroundStar", _gameMain.Random);
			if (_backGroundStar == null)
			{
				reason = "Star sprite doesn't exist.";
				return false;
			}

			int numOfStars = (_gameMain.ScreenSize.X * _gameMain.ScreenSize.Y) / 500;

			_backGroundStars = new BackgroundStar[numOfStars];
			for (int i = 0; i < numOfStars; i++)
			{
				Color color = Color.White;
				switch (_gameMain.Random.Next(6))
				{
					case 0:
						color = Color.OrangeRed;
						break;
					case 1:
						color = Color.LightBlue;
						break;
					case 2:
						color = Color.Violet;
						break;
					case 3:
						color = Color.LightGreen;
						break;
					case 4:
						color = Color.Yellow;
						break;
				}
				BackgroundStar newStar = new BackgroundStar(gameMain.Random.Next(gameMain.ScreenSize.X), gameMain.Random.Next(gameMain.ScreenSize.Y), _gameMain.Random.Next(1, 100), color);
				_backGroundStars[i] = newStar;
			}

			reason = null;
			return true;
		}

		public void Draw()
		{
			for (int i = 0; i < _backGroundStars.Length; i++)
			{
				var star = _backGroundStars[i];
				_backGroundStar.Draw(star.XPos, star.YPos, 0.1f, 0.1f, star.Color);
			}
		}

		public void Move(float xAmount, float yAmount)
		{
			foreach (var star in _backGroundStars)
			{
				star.Move(xAmount, yAmount, _gameMain.ScreenSize.X, _gameMain.ScreenSize.Y);
			}
		}
	}
}
