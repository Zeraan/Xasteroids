using System;
using System.Drawing;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.InputDevices;

namespace Xasteroids
{
	public partial class Xasteroids : Form
	{
		private Input _input;
		private Keyboard _keyboard;

		private GameMain _gameMain;

		public Xasteroids()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				Gorgon.Initialize(true, false);

				VideoMode videoMode;
				bool fullScreen;

				using (Configuration configuration = new Configuration())
				{
					configuration.FillResolutionList();
					configuration.ShowDialog(this);
					if (configuration.DialogResult != DialogResult.OK)
					{
						Close();
						return;
					}
					videoMode = configuration.VideoMode;
					fullScreen = configuration.FullScreen;
				}

				Gorgon.SetMode(this, videoMode.Width, videoMode.Height, BackBufferFormats.BufferRGB888, !fullScreen);

				Gorgon.Idle += Gorgon_Idle;

				_input = Input.LoadInputPlugIn(Environment.CurrentDirectory + @"\GorgonInput.DLL", "Gorgon.RawInput");
				_input.Bind(this);

				_keyboard = _input.Keyboard;
				_keyboard.Enabled = true;
				_keyboard.Exclusive = true;
				_keyboard.KeyDown += KeyboardOnKeyDown;

				_gameMain = new GameMain();

				string reason;
				if (!_gameMain.Initialize(Gorgon.Screen.Width, Gorgon.Screen.Height, this, out reason))
				{
					MessageBox.Show(string.Format("Error loading game resources, error message: {0}", reason));
					Close();
					return;
				}

				Gorgon.Go();
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
				Close();
			}
		}

		private void KeyboardOnKeyDown(object sender, KeyboardInputEventArgs e)
		{
			if (e.Alt && e.Key == KeyboardKeys.Enter)
			{
				Gorgon.Screen.Windowed = !Gorgon.Screen.Windowed;
			}
			_gameMain.KeyDown(e);
		}

		void Gorgon_Idle(object sender, GorgonLibrary.Graphics.FrameEventArgs e)
		{
			Gorgon.Screen.Clear(Color.Black);
			Gorgon.Screen.BeginDrawing();

			_gameMain.ProcessGame(e.FrameDeltaTime);

			Gorgon.Screen.EndDrawing();
		}

		private void BeyondBeyaan_MouseDown(object sender, MouseEventArgs e)
		{
			_gameMain.MouseDown(e);
		}

		private void BeyondBeyaan_MouseUp(object sender, MouseEventArgs e)
		{
			_gameMain.MouseUp(e);
		}

		private void BeyondBeyaan_MouseMove(object sender, MouseEventArgs e)
		{
			_gameMain.MousePos.X = e.X;
			_gameMain.MousePos.Y = e.Y;
		}

		void BeyondBeyaan_MouseWheel(object sender, MouseEventArgs e)
		{
			_gameMain.MouseScroll(e.Delta);
		}

		private void BeyondBeyaan_MouseLeave(object sender, EventArgs e)
		{
			Cursor.Show();
		}

		private void BeyondBeyaan_MouseEnter(object sender, EventArgs e)
		{
			Cursor.Hide();
		}

		public bool IsKeyDown(KeyboardKeys whichKey)
		{
			return _keyboard.KeyStates[whichKey] == KeyState.Down;
		}
	}
}
