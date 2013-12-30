using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Xasteroids
{
	public class Client
	{
		public IPAddress IPAddress { get { return Dns.GetHostEntry(Dns.GetHostName()).AddressList[0]; } }

		public Client()
		{
			IPEndPoint myEndPoint = new IPEndPoint(IPAddress, 8307);
			_udpClient = new UdpClient(myEndPoint);
			List<object> state = new List<object> { myEndPoint, _udpClient };
			_udpClient.BeginReceive(OnUdpDataReceived, state);
		}

		private IPAddress _serverIPAddress;
		public IPAddress ServerIPAddress 
		{
			get { return _serverIPAddress; }
			set
			{
				if (_serverIPAddress == value)
				{
					return;
				}
				_tcpClient.Close();
				_serverIPAddress = value;
				ResetTcpClient();
			}
		}

		public void ResetTcpClient()
		{
			try
			{
				_tcpClient = new TcpClient(_serverIPAddress.ToString(), 7127);
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
			SendDataTCP("IP address: " + IPAddress.ToString());
		}

		public void SendDataTCP(string data, bool tryAgainOnFailure = true)
		{
			byte[] bytes = System.Text.Encoding.ASCII.GetBytes(data);
			NetworkStream stream = _tcpClient.GetStream();
			try
			{
				stream.BeginWrite(bytes, 0, bytes.Length, OnTcpDataSent, _tcpClient);
			}
			catch (IOException exception)
			{
				if (tryAgainOnFailure)
				{
					ResetTcpClient();
					SendDataTCP(data, false);
					return;
				}
				SocketException innerException = exception.InnerException as SocketException;
				if (innerException != null && innerException.SocketErrorCode == SocketError.ConnectionReset)
				{
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

		private TcpClient _tcpClient;
		private UdpClient _udpClient;

		private void OnTcpDataSent(IAsyncResult asyncResult)
		{
			TcpClient tcpClient = (TcpClient)asyncResult.AsyncState;
			NetworkStream stream = tcpClient.GetStream();
			stream.EndWrite(asyncResult);
		}

		private void OnUdpDataReceived(IAsyncResult asyncResult)
		{
			List<object> container = (List<object>)asyncResult.AsyncState;
			IPEndPoint myEndPoint = (IPEndPoint)container[0];
			byte[] bytes = _udpClient.EndReceive(asyncResult, ref myEndPoint);
			string data = System.Text.Encoding.ASCII.GetString(bytes);
			_udpClient.BeginReceive(OnUdpDataReceived, container);
		}
	}
}
