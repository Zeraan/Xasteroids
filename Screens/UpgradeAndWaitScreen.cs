using System.Drawing;
using GorgonLibrary.InputDevices;

namespace Xasteroids.Screens
{
	public class UpgradeAndWaitScreen : ScreenInterface
	{
		private const int RECHARGE_COST = 150;
		private const int CAPACITY_COST = 300;
		private const int BATTERY_COST = 1000;
		private const int ACCELERATION_COST = 150;
		private const int ROTATION_COST = 100;
		private const int REVERSE_COST = 200;
		private const int BOOST_COST = 1500;
		private const int COOLDOWN_COST = 100;
		private const int CONSUMPTION_COST = 200;
		private const int DAMAGE_COST = 200;
		private const int VELOCITY_COST = 100;
		private const int PENETRATING_COST = 1000;
		private const int MOUNTS_COST = 300;
		private const int SHRAPNEL_COST = 1000;
		private const int NUKE_COST = 1000;
		private const int SHREDDING_COST = 500;
		private const int HARDNESS_COST = 250;
		private const int INERTIAL_COST = 1000; //Inertial reduces the velocity change, which in turn reduces damage as well (if 50% reduction in velocity change, then also 50% reduction in damage)
		private const int PHASING_COST = 5000;

		private GameMain _gameMain;

		private BBStretchableImage _background;

		private BBStretchableImage _energyUpgradeBackground;
		private BBStretchableImage _engineUpgradeBackground;
		private BBStretchableImage _weaponUpgradeBackground;
		private BBStretchableImage _shieldUpgradeBackground;

		private BBStretchableImage _playerStatusBackground;
		private BBStretchableImage _chatBackground;

		private BBTextBox _playerStatusTextBox;
		private BBTextBox _chatTextBox;
		private BBSingleLineTextBox _messageTextBox;

		private BBStretchButton[] _energyButtons;
		private BBStretchButton[] _engineButtons;
		private BBStretchButton[] _weaponButtons;
		private BBStretchButton[] _shieldButtons;
		private BBLabel[] _energyLabels;
		private BBLabel[] _engineLabels;
		private BBLabel[] _weaponLabels;
		private BBLabel[] _shieldLabels;

		private BBLabel[] _upgradeLabels;

		private BBButton _readyButton;

		public bool Initialize(GameMain gameMain, out string reason)
		{
			

			_gameMain = gameMain;

			int x = _gameMain.ScreenSize.X / 2 - 400;
			int y = _gameMain.ScreenSize.Y / 2 - 300;

			_background = new BBStretchableImage();

			_energyUpgradeBackground = new BBStretchableImage();
			_engineUpgradeBackground = new BBStretchableImage();
			_weaponUpgradeBackground = new BBStretchableImage();
			_shieldUpgradeBackground = new BBStretchableImage();

			_playerStatusBackground = new BBStretchableImage();
			_chatBackground = new BBStretchableImage();

			_playerStatusTextBox = new BBTextBox();
			_chatTextBox = new BBTextBox();
			_messageTextBox = new BBSingleLineTextBox();

			_readyButton = new BBButton();

			_energyButtons = new BBStretchButton[3];
			_energyLabels = new BBLabel[3];
			for (int i = 0; i < 3; i++)
			{
				_energyButtons[i] = new BBStretchButton();
				_energyLabels[i] = new BBLabel();
			}
			_engineButtons = new BBStretchButton[4];
			_engineLabels = new BBLabel[4];
			for (int i = 0; i < 4; i++)
			{
				_engineButtons[i] = new BBStretchButton();
				_engineLabels[i] = new BBLabel();
			}
			_weaponButtons = new BBStretchButton[8];
			_weaponLabels = new BBLabel[8];
			for (int i = 0; i < 8; i++)
			{
				_weaponButtons[i] = new BBStretchButton();
				_weaponLabels[i] = new BBLabel();
			}
			_shieldButtons = new BBStretchButton[4];
			_shieldLabels = new BBLabel[4];
			for (int i = 0; i < 4; i++)
			{
				_shieldButtons[i] = new BBStretchButton();
				_shieldLabels[i] = new BBLabel();
			}

			_upgradeLabels = new BBLabel[4];
			for (int i = 0; i < 4; i++)
			{
				_upgradeLabels[i] = new BBLabel();
			}

			if (!_background.Initialize(x - 30, y - 30, 860, 660, StretchableImageType.ThickBorder, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_energyUpgradeBackground.Initialize(x, y, 400, 150, StretchableImageType.ThinBorderBG, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_engineUpgradeBackground.Initialize(x, y + 150, 400, 190, StretchableImageType.ThinBorderBG, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_weaponUpgradeBackground.Initialize(x + 400, y, 400, 340, StretchableImageType.ThinBorderBG, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_shieldUpgradeBackground.Initialize(x, y + 340, 800, 110, StretchableImageType.ThinBorderBG, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_playerStatusBackground.Initialize(x, y + 450, 400, 150, StretchableImageType.ThinBorderBG, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_chatBackground.Initialize(x + 400, y + 450, 400, 150, StretchableImageType.ThinBorderBG, _gameMain.Random, out reason))
			{
				return false;
			}

			if (!_playerStatusTextBox.Initialize(x + 410, y + 460, 380, 95, false, true, "PlayerStatusTextBox", _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_chatTextBox.Initialize(x + 10, y + 460, 380, 100, true, true, "UpgradeChatTextBox", _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_messageTextBox.Initialize(string.Empty, x + 10, y + 560, 380, 30, false, _gameMain.Random, out reason))
			{
				return false;
			}

			if (!_energyButtons[0].Initialize("Upgrade Recharge Rate", ButtonTextAlignment.LEFT, StretchableImageType.TinyButtonBG, StretchableImageType.TinyButtonFG, x + 10, y + 32, 380, 35, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_energyButtons[1].Initialize("Upgrade Capacity", ButtonTextAlignment.LEFT, StretchableImageType.TinyButtonBG, StretchableImageType.TinyButtonFG, x + 10, y + 69, 380, 35, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_energyButtons[2].Initialize("Buy Emergency Battery", ButtonTextAlignment.LEFT, StretchableImageType.TinyButtonBG, StretchableImageType.TinyButtonFG, x + 10, y + 106, 380, 35, _gameMain.Random, out reason))
			{
				return false;
			}
			for (int i = 0; i < 3; i++)
			{
				if (!_energyLabels[i].Initialize(x + 375, y + 40 + (i * 37), string.Empty, Color.GreenYellow, out reason))
				{
					return false;
				}
				_energyLabels[i].SetAlignment(true);
			}

			if (!_engineButtons[0].Initialize("Upgrade Acceleration", ButtonTextAlignment.LEFT, StretchableImageType.TinyButtonBG, StretchableImageType.TinyButtonFG, x + 10, y + 182, 380, 35, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_engineButtons[1].Initialize("Upgrade Rotation Speed", ButtonTextAlignment.LEFT, StretchableImageType.TinyButtonBG, StretchableImageType.TinyButtonFG, x + 10, y + 219, 380, 35, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_engineButtons[2].Initialize("Upgrade Reverse Thrusters", ButtonTextAlignment.LEFT, StretchableImageType.TinyButtonBG, StretchableImageType.TinyButtonFG, x + 10, y + 256, 380, 35, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_engineButtons[3].Initialize("Upgrade Boosters", ButtonTextAlignment.LEFT, StretchableImageType.TinyButtonBG, StretchableImageType.TinyButtonFG, x + 10, y + 293, 380, 35, _gameMain.Random, out reason))
			{
				return false;
			}
			for (int i = 0; i < 4; i++)
			{
				if (!_engineLabels[i].Initialize(x + 375, y + 190 + (i * 37), string.Empty, Color.GreenYellow, out reason))
				{
					return false;
				}
				_engineLabels[i].SetAlignment(true);
			}

			if (!_weaponButtons[0].Initialize("Reduce Cooldown", ButtonTextAlignment.LEFT, StretchableImageType.TinyButtonBG, StretchableImageType.TinyButtonFG, x + 410, y + 32, 380, 35, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_weaponButtons[1].Initialize("Reduce Energy Consumption", ButtonTextAlignment.LEFT, StretchableImageType.TinyButtonBG, StretchableImageType.TinyButtonFG, x + 410, y + 69, 380, 35, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_weaponButtons[2].Initialize("Upgrade Damage", ButtonTextAlignment.LEFT, StretchableImageType.TinyButtonBG, StretchableImageType.TinyButtonFG, x + 410, y + 106, 380, 35, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_weaponButtons[3].Initialize("Upgrade Velocity", ButtonTextAlignment.LEFT, StretchableImageType.TinyButtonBG, StretchableImageType.TinyButtonFG, x + 410, y + 143, 380, 35, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_weaponButtons[4].Initialize("Upgrade Penetration", ButtonTextAlignment.LEFT, StretchableImageType.TinyButtonBG, StretchableImageType.TinyButtonFG, x + 410, y + 180, 380, 35, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_weaponButtons[5].Initialize("Buy Additional Mount", ButtonTextAlignment.LEFT, StretchableImageType.TinyButtonBG, StretchableImageType.TinyButtonFG, x + 410, y + 217, 380, 35, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_weaponButtons[6].Initialize("Add Shrapnel", ButtonTextAlignment.LEFT, StretchableImageType.TinyButtonBG, StretchableImageType.TinyButtonFG, x + 410, y + 252, 380, 35, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_weaponButtons[7].Initialize("Buy Nuclear Missile", ButtonTextAlignment.LEFT, StretchableImageType.TinyButtonBG, StretchableImageType.TinyButtonFG, x + 410, y + 287, 380, 35, _gameMain.Random, out reason))
			{
				return false;
			}
			for (int i = 0; i < 8; i++)
			{
				if (!_weaponLabels[i].Initialize(x + 775, y + 40 + (i * 37), string.Empty, Color.GreenYellow, out reason))
				{
					return false;
				}
				_weaponLabels[i].SetAlignment(true);
			}

			if (!_shieldButtons[0].Initialize("Upgrade Shredding", ButtonTextAlignment.LEFT, StretchableImageType.TinyButtonBG, StretchableImageType.TinyButtonFG, x + 10, y + 372, 390, 35, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_shieldButtons[1].Initialize("Upgrade Hardness", ButtonTextAlignment.LEFT, StretchableImageType.TinyButtonBG, StretchableImageType.TinyButtonFG, x + 10, y + 409, 390, 35, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_shieldButtons[2].Initialize("Upgrade Inertial Stabilizer", ButtonTextAlignment.LEFT, StretchableImageType.TinyButtonBG, StretchableImageType.TinyButtonFG, x + 400, y + 372, 390, 35, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_shieldButtons[3].Initialize("Buy Phasing Cloak", ButtonTextAlignment.LEFT, StretchableImageType.TinyButtonBG, StretchableImageType.TinyButtonFG, x + 400, y + 409, 390, 35, _gameMain.Random, out reason))
			{
				return false;
			}
			for (int i = 0; i < 4; i++)
			{
				if (!_shieldLabels[i].Initialize(x + 385 + ((i / 2) * 390), y + 380 + ((i % 2) * 37), string.Empty, Color.GreenYellow, out reason))
				{
					return false;
				}
				_shieldLabels[i].SetAlignment(true);
			}

			if (!_readyButton.Initialize("ConfirmBG", "ConfirmFG", string.Empty, ButtonTextAlignment.CENTER, x + 715, y + 555, 75, 35, _gameMain.Random, out reason))
			{
				return false;
			}

			return true;
		}

		public void DrawScreen()
		{
			_background.Draw();
			_energyUpgradeBackground.Draw();
			_engineUpgradeBackground.Draw();
			_weaponUpgradeBackground.Draw();
			_shieldUpgradeBackground.Draw();
			_playerStatusBackground.Draw();
			_chatBackground.Draw();

			for (int i = 0; i < 3; i++)
			{
				_energyButtons[i].Draw();
				_energyLabels[i].Draw();
			}
			for (int i = 0; i < 4; i++)
			{
				_engineButtons[i].Draw();
				_engineLabels[i].Draw();
			}
			for (int i = 0; i < 8; i++)
			{
				_weaponButtons[i].Draw();
				_weaponLabels[i].Draw();
			}
			for (int i = 0; i < 4; i++)
			{
				_shieldButtons[i].Draw();
				_shieldLabels[i].Draw();
			}

			_playerStatusTextBox.Draw();
			_chatTextBox.Draw();
			_messageTextBox.Draw();

			_readyButton.Draw();
		}

		public void Update(int x, int y, float frameDeltaTime)
		{
			for (int i = 0; i < 3; i++)
			{
				_energyButtons[i].MouseHover(x, y, frameDeltaTime);
			}
			for (int i = 0; i < 4; i++)
			{
				_engineButtons[i].MouseHover(x, y, frameDeltaTime);
			}
			for (int i = 0; i < 8; i++)
			{
				_weaponButtons[i].MouseHover(x, y, frameDeltaTime);
			}
			for (int i = 0; i < 4; i++)
			{
				_shieldButtons[i].MouseHover(x, y, frameDeltaTime);
			}
			_readyButton.MouseHover(x, y, frameDeltaTime);
		}

		public void MouseDown(int x, int y)
		{
			foreach (var button in _energyButtons)
			{
				button.MouseDown(x, y);
			}
			foreach (var button in _engineButtons)
			{
				button.MouseDown(x, y);
			}
			foreach (var button in _weaponButtons)
			{
				button.MouseDown(x, y);
			}
			foreach (var button in _shieldButtons)
			{
				button.MouseDown(x, y);
			}
			_readyButton.MouseDown(x, y);
		}

		public void MouseUp(int x, int y)
		{
			var player = _gameMain.PlayerManager.MainPlayer;
			if (_energyButtons[0].MouseUp(x, y))
			{
				int cost = (player.RechargeLevel + 1) * RECHARGE_COST;
				player.RechargeLevel++;
				player.Bank -= cost;
				RefreshLabels();
			}
			if (_energyButtons[1].MouseUp(x, y))
			{
				int cost = (player.CapacityLevel + 1) * CAPACITY_COST;
				player.CapacityLevel++;
				player.Bank -= cost;
				RefreshLabels();
			}
			if (_energyButtons[2].MouseUp(x, y))
			{
				//int cost = BATTERY_COST;
				//player.Battery++;
				//player.Bank -= cost;
				//RefreshLabels();
			}
			if (_engineButtons[0].MouseUp(x, y))
			{
				int cost = (player.AccelerationLevel + 1) * ACCELERATION_COST;
				player.AccelerationLevel++;
				player.Bank -= cost;
				RefreshLabels();
			}
			if (_engineButtons[1].MouseUp(x, y))
			{
				int cost = (player.RotationLevel + 1) * ROTATION_COST;
				player.RotationLevel++;
				player.Bank -= cost;
				RefreshLabels();
			}
			if (_engineButtons[2].MouseUp(x, y))
			{
				int cost = (player.ReverseLevel + 1) * REVERSE_COST;
				player.ReverseLevel++;
				player.Bank -= cost;
				RefreshLabels();
			}
			if (_engineButtons[3].MouseUp(x, y))
			{
				int cost = (player.BoostingLevel + 1) * BOOST_COST;
				player.BoostingLevel++;
				player.Bank -= cost;
				RefreshLabels();
			}
			if (_weaponButtons[0].MouseUp(x, y))
			{
				int cost = (player.CooldownLevel + 1) * COOLDOWN_COST;
				player.CooldownLevel++;
				player.Bank -= cost;
				RefreshLabels();
			}
			if (_weaponButtons[1].MouseUp(x, y))
			{
				int cost = (player.ConsumptionLevel + 1) * CONSUMPTION_COST;
				player.ConsumptionLevel++;
				player.Bank -= cost;
				RefreshLabels();
			}
			if (_weaponButtons[2].MouseUp(x, y))
			{
				int cost = (player.DamageLevel + 1) * DAMAGE_COST;
				player.DamageLevel++;
				player.Bank -= cost;
				RefreshLabels();
			}
			if (_weaponButtons[3].MouseUp(x, y))
			{
				int cost = (player.VelocityLevel + 1) * VELOCITY_COST;
				player.VelocityLevel++;
				player.Bank -= cost;
				RefreshLabels();
			}
			if (_weaponButtons[4].MouseUp(x, y))
			{
				int cost = (player.PenetratingLevel + 1) * PENETRATING_COST;
				player.PenetratingLevel++;
				player.Bank -= cost;
				RefreshLabels();
			}
			if (_weaponButtons[5].MouseUp(x, y))
			{
				int cost = (player.NumberOfMounts + 1) * MOUNTS_COST;
				player.NumberOfMounts++;
				player.Bank -= cost;
				RefreshLabels();
			}
			if (_weaponButtons[6].MouseUp(x, y))
			{
				int cost = (player.ShrapnelLevel + 1) * SHRAPNEL_COST;
				player.ShrapnelLevel++;
				player.Bank -= cost;
				RefreshLabels();
			}
			if (_weaponButtons[7].MouseUp(x, y))
			{
				player.NumberOfNukes++;
				player.Bank -= NUKE_COST;
				RefreshLabels();
			}
			if (_shieldButtons[0].MouseUp(x, y))
			{
				int cost = (player.ShreddingLevel + 1) * SHREDDING_COST;
				player.ShreddingLevel++;
				player.Bank -= cost;
				RefreshLabels();
			}
			if (_shieldButtons[1].MouseUp(x, y))
			{
				int cost = (player.HardnessLevel + 1) * HARDNESS_COST;
				player.HardnessLevel++;
				player.Bank -= cost;
				RefreshLabels();
			}
			if (_shieldButtons[2].MouseUp(x, y))
			{
				int cost = (player.InertialLevel + 1) * INERTIAL_COST;
				player.InertialLevel++;
				player.Bank -= cost;
				RefreshLabels();
			}
			if (_shieldButtons[3].MouseUp(x, y))
			{
				player.PhasingLevel++;
				player.Bank -= PHASING_COST;
				RefreshLabels();
			}
			if (_readyButton.MouseUp(x, y))
			{
				//Advance to In Game if single player, otherwise notify others player is ready
				_gameMain.LevelNumber++;
				_gameMain.SetupLevel();
				_gameMain.PlayerManager.ResetPlayerPositions();
				_gameMain.ChangeToScreen(Screen.InGame);
			}
		}

		public void MouseScroll(int direction, int x, int y)
		{
			
		}

		public void KeyDown(KeyboardInputEventArgs e)
		{
			
		}

		public void RefreshLabels()
		{
			var player = _gameMain.PlayerManager.MainPlayer;
			int cost = (player.RechargeLevel + 1) * RECHARGE_COST;

			if (player.RechargeLevel == 40 + (player.ShipSize * 4))
			{
				_energyLabels[0].SetText("Max");
				_energyLabels[0].SetColor(Color.Red, Color.Empty);
				_energyButtons[0].Enabled = false;
			}
			else
			{
				_energyLabels[0].SetText(string.Format("${0}", cost));
				if (player.Bank >= cost)
				{
					_energyLabels[0].SetColor(Color.GreenYellow, Color.Empty);
					_energyButtons[0].Enabled = true;
				}
				else
				{
					_energyLabels[0].SetColor(Color.Red, Color.Empty);
					_energyButtons[0].Enabled = false;
				}
			}

			if (player.CapacityLevel == 20 + (player.ShipSize * 10))
			{
				_energyLabels[1].SetText("Max");
				_energyLabels[1].SetColor(Color.Red, Color.Empty);
				_energyButtons[1].Enabled = false;
			}
			else
			{
				cost = (player.CapacityLevel + 1) * CAPACITY_COST;
				_energyLabels[1].SetText(string.Format("${0}", cost));
				if (player.Bank >= cost)
				{
					_energyLabels[1].SetColor(Color.GreenYellow, Color.Empty);
					_energyButtons[1].Enabled = true;
				}
				else
				{
					_energyLabels[1].SetColor(Color.Red, Color.Empty);
					_energyButtons[1].Enabled = false;
				}
			}

			cost = BATTERY_COST;
			_energyLabels[2].SetText(string.Format("${0}", cost));
			if (player.Bank >= cost)
			{
				_energyLabels[2].SetColor(Color.GreenYellow, Color.Empty);
				_energyButtons[2].Enabled = true;
			}
			else
			{
				_energyLabels[2].SetColor(Color.Red, Color.Empty);
				_energyButtons[2].Enabled = false;
			}

			if (player.AccelerationLevel == 10)
			{
				_engineLabels[0].SetText("Max");
				_engineLabels[0].SetColor(Color.Red, Color.Empty);
				_engineButtons[0].Enabled = false;
			}
			else
			{
				cost = (player.AccelerationLevel + 1) * ACCELERATION_COST;
				_engineLabels[0].SetText(string.Format("${0}", cost));
				if (player.Bank >= cost)
				{
					_engineLabels[0].SetColor(Color.GreenYellow, Color.Empty);
					_engineButtons[0].Enabled = true;
				}
				else
				{
					_engineLabels[0].SetColor(Color.Red, Color.Empty);
					_engineButtons[0].Enabled = false;
				}
			}

			if (player.RotationLevel == 10)
			{
				_engineLabels[1].SetText("Max");
				_engineLabels[1].SetColor(Color.Red, Color.Empty);
				_engineButtons[1].Enabled = false;
			}
			else
			{
				cost = (player.RotationLevel + 1) * ROTATION_COST;
				_engineLabels[1].SetText(string.Format("${0}", cost));
				if (player.Bank >= cost)
				{
					_engineLabels[1].SetColor(Color.GreenYellow, Color.Empty);
					_engineButtons[1].Enabled = true;
				}
				else
				{
					_engineLabels[1].SetColor(Color.Red, Color.Empty);
					_engineButtons[1].Enabled = false;
				}
			}

			if (player.ReverseLevel == 4)
			{
				_engineLabels[2].SetText("Max");
				_engineLabels[2].SetColor(Color.Red, Color.Empty);
				_engineButtons[2].Enabled = false;
			}
			else
			{
				cost = (player.ReverseLevel + 1) * REVERSE_COST;
				_engineLabels[2].SetText(string.Format("${0}", cost));
				if (player.Bank >= cost)
				{
					_engineLabels[2].SetColor(Color.GreenYellow, Color.Empty);
					_engineButtons[2].Enabled = true;
				}
				else
				{
					_engineLabels[2].SetColor(Color.Red, Color.Empty);
					_engineButtons[2].Enabled = false;
				}
			}

			if (player.BoostingLevel == 4)
			{
				_engineLabels[3].SetText("Max");
				_engineLabels[3].SetColor(Color.Red, Color.Empty);
				_engineButtons[3].Enabled = false;
			}
			else
			{
				cost = (player.BoostingLevel + 1) * BOOST_COST;
				_engineLabels[3].SetText(string.Format("${0}", cost));
				if (player.Bank >= cost)
				{
					_engineLabels[3].SetColor(Color.GreenYellow, Color.Empty);
					_engineButtons[3].Enabled = true;
				}
				else
				{
					_engineLabels[3].SetColor(Color.Red, Color.Empty);
					_engineButtons[3].Enabled = false;
				}
			}

			if (player.CooldownLevel == 15)
			{
				_weaponLabels[0].SetText("Max");
				_weaponLabels[0].SetColor(Color.Red, Color.Empty);
				_weaponButtons[0].Enabled = false;
			}
			else
			{
				cost = (player.CooldownLevel + 1) * COOLDOWN_COST;
				_weaponLabels[0].SetText(string.Format("${0}", cost));
				if (player.Bank >= cost)
				{
					_weaponLabels[0].SetColor(Color.GreenYellow, Color.Empty);
					_weaponButtons[0].Enabled = true;
				}
				else
				{
					_weaponLabels[0].SetColor(Color.Red, Color.Empty);
					_weaponButtons[0].Enabled = false;
				}
			}

			if (player.ConsumptionLevel == 15)
			{
				_weaponLabels[1].SetText("Max");
				_weaponLabels[1].SetColor(Color.Red, Color.Empty);
				_weaponButtons[1].Enabled = false;
			}
			else
			{
				cost = (player.ConsumptionLevel + 1) * CONSUMPTION_COST;
				_weaponLabels[1].SetText(string.Format("${0}", cost));
				if (player.Bank >= cost)
				{
					_weaponLabels[1].SetColor(Color.GreenYellow, Color.Empty);
					_weaponButtons[1].Enabled = true;
				}
				else
				{
					_weaponLabels[1].SetColor(Color.Red, Color.Empty);
					_weaponButtons[1].Enabled = false;
				}
			}

			if (player.DamageLevel == 6)
			{
				_weaponLabels[2].SetText("Max");
				_weaponLabels[2].SetColor(Color.Red, Color.Empty);
				_weaponButtons[2].Enabled = false;
			}
			else
			{
				cost = (player.DamageLevel + 1) * DAMAGE_COST;
				_weaponLabels[2].SetText(string.Format("${0}", cost));
				if (player.Bank >= cost && (20 * (player.DamageLevel + 1) * (player.NumberOfMounts + 1) * (1 - (player.ConsumptionLevel * 0.05f))) <= player.MaxEnergy) //Don't want to run in situation where you consume more than you have
				{
					_weaponLabels[2].SetColor(Color.GreenYellow, Color.Empty);
					_weaponButtons[2].Enabled = true;
				}
				else
				{
					_weaponLabels[2].SetColor(Color.Red, Color.Empty);
					_weaponButtons[2].Enabled = false;
				}
			}

			if (player.VelocityLevel == 6)
			{
				_weaponLabels[3].SetText("Max");
				_weaponLabels[3].SetColor(Color.Red, Color.Empty);
				_weaponButtons[3].Enabled = false;
			}
			else
			{
				cost = (player.VelocityLevel + 1) * VELOCITY_COST;
				_weaponLabels[3].SetText(string.Format("${0}", cost));
				if (player.Bank >= cost)
				{
					_weaponLabels[3].SetColor(Color.GreenYellow, Color.Empty);
					_weaponButtons[3].Enabled = true;
				}
				else
				{
					_weaponLabels[3].SetColor(Color.Red, Color.Empty);
					_weaponButtons[3].Enabled = false;
				}
			}

			if (player.PenetratingLevel == 5)
			{
				_weaponLabels[4].SetText("Max");
				_weaponLabels[4].SetColor(Color.Red, Color.Empty);
				_weaponButtons[4].Enabled = false;
			}
			else
			{
				cost = (player.PenetratingLevel + 1) * PENETRATING_COST;
				_weaponLabels[4].SetText(string.Format("${0}", cost));
				if (player.Bank >= cost)
				{
					_weaponLabels[4].SetColor(Color.GreenYellow, Color.Empty);
					_weaponButtons[4].Enabled = true;
				}
				else
				{
					_weaponLabels[4].SetColor(Color.Red, Color.Empty);
					_weaponButtons[4].Enabled = false;
				}
			}

			if (player.NumberOfMounts == player.ShipSize)
			{
				_weaponLabels[5].SetText("Max");
				_weaponLabels[5].SetColor(Color.Red, Color.Empty);
				_weaponButtons[5].Enabled = false;
			}
			else
			{
				cost = (player.NumberOfMounts + 1) * MOUNTS_COST;
				_weaponLabels[5].SetText(string.Format("${0}", cost));
				if (player.Bank >= cost && (20 * (player.DamageLevel) * (player.NumberOfMounts + 2) * (1 - (player.ConsumptionLevel * 0.05f))) <= player.MaxEnergy) //Don't want to run in situation where you consume more than you have
				{
					_weaponLabels[5].SetColor(Color.GreenYellow, Color.Empty);
					_weaponButtons[5].Enabled = true;
				}
				else
				{
					_weaponLabels[5].SetColor(Color.Red, Color.Empty);
					_weaponButtons[5].Enabled = false;
				}
			}

			if (player.ShrapnelLevel == 4)
			{
				_weaponLabels[6].SetText("Max");
				_weaponLabels[6].SetColor(Color.Red, Color.Empty);
				_weaponButtons[6].Enabled = false;
			}
			else
			{
				cost = (player.ShrapnelLevel + 1) * SHRAPNEL_COST;
				_weaponLabels[6].SetText(string.Format("${0}", cost));
				if (player.Bank >= cost)
				{
					_weaponLabels[6].SetColor(Color.GreenYellow, Color.Empty);
					_weaponButtons[6].Enabled = true;
				}
				else
				{
					_weaponLabels[6].SetColor(Color.Red, Color.Empty);
					_weaponButtons[6].Enabled = false;
				}
			}

			cost = NUKE_COST;
			_weaponLabels[7].SetText(string.Format("${0}", cost));
			if (player.Bank >= cost)
			{
				_weaponLabels[7].SetColor(Color.GreenYellow, Color.Empty);
				_weaponButtons[7].Enabled = true;
			}
			else
			{
				_weaponLabels[7].SetColor(Color.Red, Color.Empty);
				_weaponButtons[7].Enabled = false;
			}

			if (player.ShreddingLevel == 4)
			{
				_shieldLabels[0].SetText("Max");
				_shieldLabels[0].SetColor(Color.Red, Color.Empty);
				_shieldButtons[0].Enabled = false;
			}
			else
			{
				cost = (player.ShreddingLevel + 1) * SHREDDING_COST;
				_shieldLabels[0].SetText(string.Format("${0}", cost));
				if (player.Bank >= cost)
				{
					_shieldLabels[0].SetColor(Color.GreenYellow, Color.Empty);
					_shieldButtons[0].Enabled = true;
				}
				else
				{
					_shieldLabels[0].SetColor(Color.Red, Color.Empty);
					_shieldButtons[0].Enabled = false;
				}
			}

			if (player.HardnessLevel == 15)
			{
				_shieldLabels[1].SetText("Max");
				_shieldLabels[1].SetColor(Color.Red, Color.Empty);
				_shieldButtons[1].Enabled = false;
			}
			else
			{
				cost = (player.HardnessLevel + 1) * HARDNESS_COST;
				_shieldLabels[1].SetText(string.Format("${0}", cost));
				if (player.Bank >= cost)
				{
					_shieldLabels[1].SetColor(Color.GreenYellow, Color.Empty);
					_shieldButtons[1].Enabled = true;
				}
				else
				{
					_shieldLabels[1].SetColor(Color.Red, Color.Empty);
					_shieldButtons[1].Enabled = false;
				}
			}

			if (player.InertialLevel == 15)
			{
				_shieldLabels[2].SetText("Max");
				_shieldLabels[2].SetColor(Color.Red, Color.Empty);
				_shieldButtons[2].Enabled = false;
			}
			else
			{
				cost = (player.InertialLevel + 1) * INERTIAL_COST;
				_shieldLabels[2].SetText(string.Format("${0}", cost));
				if (player.Bank >= cost)
				{
					_shieldLabels[2].SetColor(Color.GreenYellow, Color.Empty);
					_shieldButtons[2].Enabled = true;
				}
				else
				{
					_shieldLabels[2].SetColor(Color.Red, Color.Empty);
					_shieldButtons[2].Enabled = false;
				}
			}

			if (player.PhasingLevel == 1)
			{
				_shieldLabels[3].SetText("Max");
				_shieldLabels[3].SetColor(Color.Red, Color.Empty);
				_shieldButtons[3].Enabled = false;
			}
			else
			{
				cost = PHASING_COST;
				_shieldLabels[3].SetText(string.Format("${0}", cost));
				if (player.Bank >= cost)
				{
					_shieldLabels[3].SetColor(Color.GreenYellow, Color.Empty);
					_shieldButtons[3].Enabled = true;
				}
				else
				{
					_shieldLabels[3].SetColor(Color.Red, Color.Empty);
					_shieldButtons[3].Enabled = false;
				}
			}

			//Refresh the player status text box
			string status = player.Name + " ($" + player.Bank + ") - Shopping";
			//TODO- Add other players to status
			_playerStatusTextBox.SetText(status);
		}
	}
}
