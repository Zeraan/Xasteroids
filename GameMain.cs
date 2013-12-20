using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using GorgonLibrary.InputDevices;
using Xasteroids.Screens;
using MainMenu = Xasteroids.Screens.MainMenu;
using MouseButtons = System.Windows.Forms.MouseButtons;

namespace Xasteroids
{
	public enum Screen
	{
		MainMenu
	};

	public class GameMain
	{
		#region Screens
		private ScreenInterface _screenInterface;
		private MainMenu _mainMenu;

		private Screen _currentScreen;
		#endregion

		private Form _parentForm;

		public Random Random { get; private set; }
		public Point MousePos;
		public Point ScreenSize { get; private set; }
		public GorgonLibrary.Graphics.FXShader ShipShader { get; private set; }

		private BBSprite Cursor;

		bool IAmTheClient;
		bool IAmTheServer;
		IPAddress ServerIPAddress;
		TcpListener TcpListener;
		TcpClient TcpClient;
		UdpClient UdpClient;
		TextBox TheTextBox = new TextBox { WordWrap = true, Width = 400 };
		int buttonPushCount = 0;

		public bool Initialize(int screenWidth, int screenHeight, Form parentForm, out string reason)
		{
			_parentForm = parentForm;
			_parentForm.Controls.Add(TheTextBox);
			Random = new Random();
			MousePos = new Point();
			ScreenSize = new Point(screenWidth, screenHeight);

			ShipShader = GorgonLibrary.Graphics.FXShader.FromFile("ColorShader.fx", GorgonLibrary.Graphics.ShaderCompileOptions.OptimizationLevel3);

			if (!SpriteManager.Initialize(out reason))
			{
				return false;
			}
			if (!FontManager.Initialize(out reason))
			{
				return false;
			}

			_mainMenu = new MainMenu();
			if (!_mainMenu.Initialize(this, out reason))
			{
				return false;
			}

			_screenInterface = _mainMenu;
			_currentScreen = Screen.MainMenu;

			Cursor = SpriteManager.GetSprite("Cursor", Random);
			if (Cursor == null)
			{
				reason = "Cursor is not defined in sprites.xml";
				return false;
			}

			StreamReader file = new StreamReader("config");
			IAmTheClient = bool.Parse(file.ReadLine());
			IAmTheServer = bool.Parse(file.ReadLine());
			if (IAmTheServer)
			{
				ServerIPAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
			}
			else
			{
				ServerIPAddress = IPAddress.Parse(file.ReadLine());
			}

			if (IAmTheServer)
			{
				TcpListener = new TcpListener(Dns.Resolve(Dns.GetHostName()).AddressList[0], 7127);
				TcpListener.Start();
				TcpListener.BeginAcceptTcpClient(OnAcceptTcpClient, TcpListener);
				StreamsAndBytes = new Dictionary<NetworkStream, byte[]>();
				UdpClient = new UdpClient();
			}
			if (IAmTheClient)
			{
				try
				{
					TcpClient = new TcpClient(ServerIPAddress.ToString(), 7127);
				}
				catch (SocketException e)
				{
					if (e.SocketErrorCode == SocketError.TimedOut)
					{
						MessageBox.Show("The host computer did not respond to your connection request. " +
							"You might double check the IP address you entered. " +
							"It is also possible that the firewall on the host computer would not let your connection request through."
						);
					}
					else
					{
						MessageBox.Show("You were unable to connect to the host computer. Here is what .Net says about the error: " +
							e.Message
						);
					}
				}
				NetworkStream stream = TcpClient.GetStream();
				byte[] toSend = System.Text.Encoding.ASCII.GetBytes("IPAddress:" + Dns.Resolve(Dns.GetHostName()).AddressList[0].ToString());
				stream.Write(toSend, 0, toSend.Length);
				IPEndPoint myEndPoint = new IPEndPoint(Dns.Resolve(Dns.GetHostName()).AddressList[0], 8307);
				UdpClient = new UdpClient(myEndPoint);
				List<object> state = new List<object> { myEndPoint, UdpClient };
				UdpClient.BeginReceive(OnUdpDataReceived, state);
			}

			return true;
		}

		public void ExitGame()
		{
			//dispose of any resources in use
			if (IAmTheServer)
			{
				TcpListener.Stop();
			}
			if (IAmTheClient)
			{
				TcpClient.GetStream().Close();
				TcpClient.Close();
			}
			UdpClient.Close();
			_parentForm.Close();
		}

		//Handle events
		public void ProcessGame(float frameDeltaTime)
		{
			_screenInterface.Update(MousePos.X, MousePos.Y, frameDeltaTime);
			_screenInterface.DrawScreen();

			Cursor.Draw(MousePos.X, MousePos.Y);
			Cursor.Update(frameDeltaTime, Random);
		}

		private List<IPEndPoint> ClientUdpEndPoints = new List<IPEndPoint>();

		public void MouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				_screenInterface.MouseDown(e.X, e.Y);
			}
			++buttonPushCount;
			if (IAmTheClient)
			{
				string message = "Client has pressed mouse button " + buttonPushCount + " times.";
				byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
				NetworkStream stream = TcpClient.GetStream();
				try
				{
					stream.BeginWrite(data, 0, data.Length, OnTcpDataSent, stream);
				}
				catch (IOException exception)
				{
					SocketException innerException = exception.InnerException as SocketException;
					if (innerException != null && innerException.SocketErrorCode == SocketError.ConnectionReset)
					{
						//try re-setting TCP client, and if that fails...
						MessageBox.Show("Your connection with the host computer appears to have been reset. " +
							"An attempt was made to fix the problem, but it failed. You are currently unable to send data to the host computer"
						);
					}
					else
					{
						MessageBox.Show("You were unable to send data to the host computer. Here is what .Net says about the error: " +
							exception.Message
						);
					}
				}
			}
			if (IAmTheServer)
			{
				string message = "Host has pressed mouse button " + buttonPushCount + " times.";
				byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
				foreach (IPEndPoint ipEndPoint in ClientUdpEndPoints)
				{
					UdpClient.BeginSend(data, data.Length, ipEndPoint, OnUdpSent, UdpClient);
				}
			}
		}

		public void MouseUp(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				_screenInterface.MouseUp(e.X, e.Y);
			}
		}

		public void MouseScroll(int delta)
		{
			_screenInterface.MouseScroll(delta, MousePos.X, MousePos.Y);
		}

		public void KeyDown(KeyboardInputEventArgs e)
		{
			_screenInterface.KeyDown(e);
		}

		private Dictionary<NetworkStream, byte[]> StreamsAndBytes;

		private void OnAcceptTcpClient(IAsyncResult ar)
		{
			TcpClient client = TcpListener.EndAcceptTcpClient(ar);
			byte[] bytes = new byte[1024];
			NetworkStream stream = client.GetStream();
			StreamsAndBytes.Add(stream, bytes);
			stream.BeginRead(bytes, 0, bytes.Length, OnTcpDataReceived, stream);
		}

		private void OnTcpDataSent(IAsyncResult asyncResult)
		{
			NetworkStream stream = (NetworkStream)asyncResult.AsyncState;
			stream.EndWrite(asyncResult);
		}

		private void OnTcpDataReceived(IAsyncResult asyncResult)
		{
			NetworkStream stream = (NetworkStream)asyncResult.AsyncState;
			byte[] bytes = StreamsAndBytes[stream];
			string data = String.Empty;		
			int numberOfBytesRead = stream.EndRead(asyncResult);
			data += System.Text.Encoding.ASCII.GetString(bytes, 0, numberOfBytesRead);
			while (stream.DataAvailable)
			{
				stream.BeginRead(bytes, 0, bytes.Length, OnTcpDataReceived, stream);
			}
			if (data.Contains("IPAddress") && IAmTheServer)
			{
				ClientUdpEndPoints.Add(new IPEndPoint(IPAddress.Parse(data.Substring(data.IndexOf(':') + 1)), 8307));
			}
			else
			{
				TheTextBox.BeginInvoke(new Action(() => TheTextBox.Text = data));
			}
			stream.BeginRead(bytes, 0, bytes.Length, OnTcpDataReceived, stream);
		}

		private void OnUdpSent(IAsyncResult asyncResult)
		{
			UdpClient.EndSend(asyncResult);
		}

		private void OnUdpDataReceived(IAsyncResult asyncResult)
		{
			List<object> container = (List<object>)asyncResult.AsyncState;
			IPEndPoint myEndPoint = (IPEndPoint)container[0];
			byte[] bytes = UdpClient.EndReceive(asyncResult, ref myEndPoint);
			string data = System.Text.Encoding.ASCII.GetString(bytes);
			TheTextBox.BeginInvoke(new Action(() => TheTextBox.Text = data));
			UdpClient.BeginReceive(OnUdpDataReceived, container);
		}
	}
}
