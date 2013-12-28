using System;
using System.Drawing;

namespace Xasteroids.Screens
{
	public class ShipSelectionWindow : WindowInterface
	{
		private BBLabel _bankLabel;
		private BBLabel _shipCostLabel;
		private BBLabel[] _colorLabels;
		private BBScrollBar[] _colorSliders;
		private BBButton _leftButton;
		private BBButton _rightButton;
		private BBButton _upButton;
		private BBButton _downButton;
		private BBButton _selectShipButton;

		private BBStretchableImage _shipStatsBackground;
		private BBTextBox _shipStatsTextBox;

		private float _angle;
		private int _size;
		private int _style;
		private int _bank;
		private BBSprite _shipSprite;
		private float[] _convertedColors;

		public Action<int, int, Color, int> OnSelectShip;

		public bool Initialize(GameMain gameMain, out string reason)
		{
			int x = (gameMain.ScreenSize.X / 2) - 300;
			int y = (gameMain.ScreenSize.Y / 2) - 300;

			if (!base.Initialize(x, y, 600, 600, StretchableImageType.MediumBorder, gameMain, false, gameMain.Random, out reason))
			{
				return false;
			}

			x += 20;
			y += 20;

			_leftButton = new BBButton();
			_rightButton = new BBButton();
			_upButton = new BBButton();
			_downButton = new BBButton();
			_colorLabels = new BBLabel[3];
			_colorSliders = new BBScrollBar[3];
			_shipCostLabel = new BBLabel();
			_bankLabel = new BBLabel();
			_shipStatsBackground = new BBStretchableImage();
			_shipStatsTextBox = new BBTextBox();
			_selectShipButton = new BBButton();

			if (!_leftButton.Initialize("ScrollLeftBGButton", "ScrollLeftFGButton", string.Empty, ButtonTextAlignment.CENTER, x + 5, y + 141, 16, 16, gameMain.Random, out reason))
			{
				return false;
			}
			if (!_rightButton.Initialize("ScrollRightBGButton", "ScrollRightFGButton", string.Empty, ButtonTextAlignment.CENTER, x + 277, y + 141, 16, 16, gameMain.Random, out reason))
			{
				return false;
			}
			if (!_upButton.Initialize("ScrollUpBGButton", "ScrollUpFGButton", string.Empty, ButtonTextAlignment.CENTER, x + 141, y + 5, 16, 16, gameMain.Random, out reason))
			{
				return false;
			}
			if (!_downButton.Initialize("ScrollDownBGButton", "ScrollDownFGButton", string.Empty, ButtonTextAlignment.CENTER, x + 141, y + 277, 16, 16, gameMain.Random, out reason))
			{
				return false;
			}

			if (!_shipCostLabel.Initialize(x + 133, y + 310, "Ship Cost:", Color.Green, out reason))
			{
				return false;
			}

			for (int i = 0; i < 3; i++)
			{
				_colorLabels[i] = new BBLabel();
				_colorSliders[i] = new BBScrollBar();
				if (!_colorLabels[i].Initialize(x + 5, y + 340 + (i * 50), string.Empty, Color.White, out reason))
				{
					return false;
				}
				if (!_colorSliders[i].Initialize(x + 5, y + 365 + (i * 50), 288, 1, 256, true, true, gameMain.Random, out reason))
				{
					return false;
				}
				_colorSliders[i].TopIndex = 255;
			}
			_colorLabels[0].SetColor(Color.Red, Color.Empty);
			_colorLabels[1].SetColor(Color.Green, Color.Empty);
			_colorLabels[2].SetColor(Color.Blue, Color.Empty);

			if (!_shipStatsBackground.Initialize(x + 300, y + 5, 250, 500, StretchableImageType.ThinBorderBG, gameMain.Random, out reason))
			{
				return false;
			}
			if (!_shipStatsTextBox.Initialize(x + 305, y + 10, 240, 490, false, false, "ShipStatsTextBox", gameMain.Random, out reason))
			{
				return false;
			}
			if (!_bankLabel.Initialize(x + 300, y + 510, "Bank:", Color.White, out reason))
			{
				return false;
			}
			if (!_selectShipButton.Initialize("ConfirmBG", "ConfirmFG", string.Empty, ButtonTextAlignment.CENTER, x + 485, y + 530, 75, 35, _gameMain.Random, out reason))
			{
				return false;
			}

			RefreshColorValues();
			_size = 1;
			_style = 1;
			_angle = 0;

			_shipSprite = SpriteManager.GetShipSprite(_size, _style, _gameMain.Random);

			return true;
		}

		public void LoadShip(int size, int style, Color color, int bank)
		{
			_bank = bank;
			_bankLabel.SetText("Amount in bank: " + bank);
			_size = size;
			_style = style;
			_colorSliders[0].TopIndex = color.R;
			_colorSliders[1].TopIndex = color.G;
			_colorSliders[2].TopIndex = color.B;
			RefreshShipSprite();
			RefreshColorValues();
			RefreshShipCost();
		}

		public override void Draw()
		{
			base.Draw();

			GorgonLibrary.Gorgon.CurrentShader = _gameMain.ShipShader;
			_gameMain.ShipShader.Parameters["EmpireColor"].SetValue(_convertedColors);
			_shipSprite.Draw(_xPos + 169, _yPos + 169, 1, 1, Color.White, _angle);
			GorgonLibrary.Gorgon.CurrentShader = null;

			_upButton.Draw();
			_downButton.Draw();
			_leftButton.Draw();
			_rightButton.Draw();

			for (int i = 0; i < 3; i++)
			{
				_colorLabels[i].Draw();
				_colorSliders[i].Draw();
			}

			_shipCostLabel.Draw();
			_shipStatsBackground.Draw();
			_shipStatsTextBox.Draw();
			_bankLabel.Draw();
			_selectShipButton.Draw();
		}

		public override bool MouseHover(int x, int y, float frameDeltaTime)
		{
			_upButton.MouseHover(x, y, frameDeltaTime);
			_downButton.MouseHover(x, y, frameDeltaTime);
			_leftButton.MouseHover(x, y, frameDeltaTime);
			_rightButton.MouseHover(x, y, frameDeltaTime);
			_selectShipButton.MouseHover(x, y, frameDeltaTime);

			for (int i = 0; i < 3; i++)
			{
				if (_colorSliders[i].MouseHover(x, y, frameDeltaTime))
				{
					RefreshColorValues();
				}
			}

			_angle += 45 * frameDeltaTime;
			return true;
		}

		public override bool MouseDown(int x, int y)
		{
			_upButton.MouseDown(x, y);
			_downButton.MouseDown(x, y);
			_leftButton.MouseDown(x, y);
			_rightButton.MouseDown(x, y);
			_selectShipButton.MouseDown(x, y);

			for (int i = 0; i < 3; i++)
			{
				_colorSliders[i].MouseDown(x, y);
			}
			return true;
		}

		public override bool MouseUp(int x, int y)
		{
			if (_upButton.MouseUp(x, y))
			{
				_size--;
				if (_size < 1)
				{
					_size = 5;
				}
				RefreshShipSprite();
				RefreshShipCost();
			}
			if (_downButton.MouseUp(x, y))
			{
				_size++;
				if (_size > 5)
				{
					_size = 1;
				}
				RefreshShipSprite();
				RefreshShipCost();
			}
			if (_leftButton.MouseUp(x, y))
			{
				_style--;
				if (_style < 1)
				{
					_style = 18;
				}
				RefreshShipSprite();
			}
			if (_rightButton.MouseUp(x, y))
			{
				_style++;
				if (_style > 18)
				{
					_style = 1;
				}
				RefreshShipSprite();
			}
			for (int i = 0; i < 3; i++)
			{
				if (_colorSliders[i].MouseUp(x, y))
				{
					RefreshColorValues();
				}
			}
			if (_selectShipButton.MouseUp(x, y))
			{
				if (OnSelectShip != null)
				{
					OnSelectShip(_size, _style, Color.FromArgb(_colorSliders[0].TopIndex, _colorSliders[1].TopIndex, _colorSliders[2].TopIndex), _size * _size * 500);
				}
			}
			return base.MouseUp(x, y);
		}

		private void RefreshColorValues()
		{
			_convertedColors = new []
								{
									_colorSliders[0].TopIndex / 255.0f,
									_colorSliders[1].TopIndex / 255.0f,
									_colorSliders[2].TopIndex / 255.0f,
									1
								};
			_colorLabels[0].SetText(string.Format("{0} Red Value", _colorSliders[0].TopIndex));
			_colorLabels[1].SetText(string.Format("{0} Green Value", _colorSliders[1].TopIndex));
			_colorLabels[2].SetText(string.Format("{0} Blue Value", _colorSliders[2].TopIndex));
		}
		private void RefreshShipCost()
		{
			int cost = _size * 120 + 400;
			_shipCostLabel.SetText(string.Format("Ship Cost: ${0}", cost));
			if (cost <= _bank)
			{
				_shipCostLabel.SetColor(Color.Green, Color.Empty);
			}
			else
			{
				_shipCostLabel.SetColor(Color.Red, Color.Empty);
			}
		}
		private void RefreshShipSprite()
		{
			_shipSprite = SpriteManager.GetShipSprite(_size, _style, _gameMain.Random);
		}
	}
}
