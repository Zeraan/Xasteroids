using System.Text;
using GorgonLibrary.InputDevices;

namespace Xasteroids.Screens
{
	public class MultiplayerGameSetup : ScreenInterface
	{
		private GameMain _gameMain;

		private BBStretchableImage _chatBackground;
		private BBStretchableImage _playerListBackground;
		private BBTextBox _chatText;
		private BBSingleLineTextBox _messageTextBox;
		private BBTextBox _playerList;
		private BBStretchButton _shipSelection;
		private BBStretchButton _startGame;
		private BBStretchButton _leaveLobby;

		private bool _isHost;

		public bool Initialize(GameMain gameMain, out string reason)
		{
			_gameMain = gameMain;

			_chatBackground = new BBStretchableImage();
			_playerListBackground = new BBStretchableImage();
			_chatText = new BBTextBox();
			_messageTextBox = new BBSingleLineTextBox();
			_playerList = new BBTextBox();
			_shipSelection = new BBStretchButton();
			_startGame = new BBStretchButton();
			_leaveLobby = new BBStretchButton();

			int chatWidth = _gameMain.ScreenSize.X - 250;
			int chatHeight = _gameMain.ScreenSize.Y - 80;

			if (!_chatBackground.Initialize(20, 20, chatWidth, chatHeight, StretchableImageType.ThinBorderBG, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_chatText.Initialize(25, 25, chatWidth - 10, chatHeight - 10, true, true, "PreGameChatTextBox", _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_messageTextBox.Initialize("Chat Message", 20, 20 + chatHeight, chatWidth, 40, false, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_playerListBackground.Initialize(chatWidth + 30, 20, 200, chatHeight - 245, StretchableImageType.ThinBorderBG, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_playerList.Initialize(chatWidth + 35, 25, 190, chatHeight - 255, false, true, "PlayerListTextBox", _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_shipSelection.Initialize(string.Empty, ButtonTextAlignment.CENTER, StretchableImageType.ThinBorderBG, StretchableImageType.ThinBorderFG, chatWidth + 30, chatHeight - 220, 200, 200, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_startGame.Initialize("Start Game", ButtonTextAlignment.CENTER, StretchableImageType.TinyButtonBG, StretchableImageType.TinyButtonFG, chatWidth + 30, chatHeight - 15, 200, 35, _gameMain.Random, out reason))
			{
				return false;
			}
			if (!_leaveLobby.Initialize("Leave", ButtonTextAlignment.CENTER, StretchableImageType.TinyButtonBG, StretchableImageType.TinyButtonFG, chatWidth + 30, chatHeight + 25, 200, 35, _gameMain.Random, out reason))
			{
				return false;
			}
			_messageTextBox.Select();
			_isHost = false;

			return true;
		}

		public void DrawScreen()
		{
			_chatBackground.Draw();
			_messageTextBox.Draw();
			_playerListBackground.Draw();
			_shipSelection.Draw();
			if (_gameMain.IsHost)
			{
				_startGame.Draw();
			}
			_leaveLobby.Draw();
			_chatText.Draw();
			_playerList.Draw();
		}

		public void Update(int x, int y, float frameDeltaTime)
		{
			if (_gameMain.NewChatMessage)
			{
				_chatText.SetText(_gameMain.ChatText.ToString());
				_chatText.ScrollToBottom();
				_gameMain.NewChatMessage = false;
			}
			_shipSelection.MouseHover(x, y, frameDeltaTime);
			if (_isHost)
			{
				_startGame.MouseHover(x, y, frameDeltaTime);
			}
			_leaveLobby.MouseHover(x, y, frameDeltaTime);
			_chatText.MouseHover(x, y, frameDeltaTime);
			_messageTextBox.Update(frameDeltaTime);
		}

		public void MouseDown(int x, int y)
		{
			_shipSelection.MouseDown(x, y);
			if (_gameMain.IsHost)
			{
				_startGame.MouseDown(x, y);
			}
			_leaveLobby.MouseDown(x, y);
			_chatText.MouseDown(x, y);
		}

		public void MouseUp(int x, int y)
		{
			if (_shipSelection.MouseUp(x, y))
			{
				//Display the ship selection window
			}
			if (_startGame.MouseUp(x, y))
			{
				_gameMain.ChangeToScreen(Screen.Upgrade);
			}
			if (_leaveLobby.MouseUp(x, y))
			{
				_gameMain.ChangeToScreen(Screen.MainMenu);
				//Disconnect
			}
			_chatText.MouseUp(x, y);
		}

		public void MouseScroll(int direction, int x, int y)
		{
			//TODO: Implement mouse scrolling in TextBox
			//_chatText.MouseScroll(x, y);
		}

		public void KeyDown(KeyboardInputEventArgs e)
		{
			if (e.Key == KeyboardKeys.Enter || e.Key == KeyboardKeys.Return)
			{
				_gameMain.SendChat(_messageTextBox.Text);
				_messageTextBox.SetText(string.Empty);
			}
			else
			{
				_messageTextBox.KeyDown(e);
			}
		}

		public void SetHost(bool isHosting)
		{
			_isHost = isHosting;
			_chatText.SetText(string.Empty);
			_messageTextBox.SetText(string.Empty);
		}
	}
}
