using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Xasteroids
{
	public class Host
	{
		public IPAddress IPAddress { get { return Dns.GetHostEntry(Dns.GetHostName()).AddressList[0]; } }
		public bool CurrentlyAcceptingPlayers { get; set; }
		public string WhatToTellPlayersThatCantJoin { get; set; }
		private TcpListener _tcpListener;
		private UdpClient _udpClient = new UdpClient();
		private List<IPAddress> _ipAddressesWithGameAccess = new List<IPAddress>();
		private Dictionary<TcpClient, byte[]> _clientsAndBytes = new Dictionary<TcpClient, byte[]>();
		private Dictionary<TcpClient, IPEndPoint> _clientsAndUdpEndPoints = new Dictionary<TcpClient, IPEndPoint>();
		private List<TcpClient> _clientsToClose = new List<TcpClient>();
		private Regex _sloppyIPAddressRegex = new Regex(@"\d+.\d+.\d+.\d+", RegexOptions.Compiled);

		public Host()
		{
			_tcpListener = new TcpListener(IPAddress, 7127);
			_tcpListener.Start();
			_tcpListener.BeginAcceptTcpClient(OnAcceptTcpClient, _tcpListener);
		}

		public void SendDataTCP(string data)
		{
			byte[] bytes = System.Text.Encoding.ASCII.GetBytes(data);
			var keys = _clientsAndUdpEndPoints.Keys.ToArray();
			for (int j = keys.Length - 1; j >= 0; --j)
			{
				TcpClient tcpClient = keys[j];
				NetworkStream stream = tcpClient.GetStream();
				try
				{
					stream.BeginWrite(bytes, 0, bytes.Length, OnTcpDataSent, tcpClient);
				}
				/* If we can't write to this stream something is seriously wrong.
				 * I don't think we can redeem this client, so let's shut it down.
				 * When the client discovers that its connection is gone it will try 
				 * to make a new one. If the new connection succeeds everything should
				 * be fine.
				 */
				catch
				{
					tcpClient.Close();
					_clientsAndBytes.Remove(tcpClient);
					if (_clientsAndUdpEndPoints.ContainsKey(tcpClient))
					{
						_clientsAndUdpEndPoints.Remove(tcpClient);
					}
				}
			}
		}

		public void SendDataTCPToClient(string data, TcpClient client)
		{
			NetworkStream stream = client.GetStream();
			byte[] bytes = System.Text.Encoding.ASCII.GetBytes(data);
			stream.BeginWrite(bytes, 0, bytes.Length, OnTcpDataSent, client);
		}

		public void SendDataUDP(string data)
		{
			byte[] bytes = System.Text.Encoding.ASCII.GetBytes(data);
			foreach (var kvp in _clientsAndUdpEndPoints)
			{
				_udpClient.BeginSend(bytes, bytes.Length, kvp.Value, OnUdpDataSent, _udpClient);
			}
		}

		public void ShutDown()
		{
			foreach (var kvp in _clientsAndBytes)
			{
				TcpClient client = kvp.Key;
				client.GetStream().Close();
				client.Close();
			}
			_clientsAndBytes.Clear();
			_udpClient.Close();
			_clientsAndUdpEndPoints.Clear();
			_ipAddressesWithGameAccess.Clear();
			_tcpListener.Stop();
		}
		
		private void OnAcceptTcpClient(IAsyncResult ar)
		{
			TcpClient client = _tcpListener.EndAcceptTcpClient(ar);
			byte[] bytes = new byte[1024];
			NetworkStream stream = client.GetStream();
			_clientsAndBytes.Add(client, bytes);
			stream.BeginRead(bytes, 0, bytes.Length, OnInitialTcpDataReceived, client);
		}

		private void OnTcpDataSent(IAsyncResult asyncResult)
		{
			TcpClient tcpClient = (TcpClient)asyncResult.AsyncState;
			NetworkStream stream = tcpClient.GetStream();
			stream.EndWrite(asyncResult);
			if (_clientsToClose.Contains(tcpClient))
			{
				tcpClient.GetStream().Close();
				tcpClient.Close();
				_clientsToClose.Remove(tcpClient);
				_clientsAndBytes.Remove(tcpClient);
				if (_clientsAndUdpEndPoints.ContainsKey(tcpClient))
				{
					_clientsAndUdpEndPoints.Remove(tcpClient);
				}
			}
		}

		//gimme an IP address or I'll tell you to and close the connection
		private void OnInitialTcpDataReceived(IAsyncResult asyncResult)
		{
			TcpClient client = (TcpClient)asyncResult.AsyncState;
			NetworkStream stream = client.GetStream();
			byte[] bytes = new byte[1024];
			int numberOfBytesRead = stream.EndRead(asyncResult);
			string dataReceived = System.Text.Encoding.ASCII.GetString(bytes, 0, numberOfBytesRead);
			Match match = _sloppyIPAddressRegex.Match(dataReceived);
			if (!match.Success)
			{
				string dataSent = "Problem. No IP address.";
				try
				{
					SendDataTCPToClient(dataSent, client);
				}
				/* My connection to this client might have broken already.
				 * If it has, I just want to prevent a crash on this end.
				 * Nothing else needs be done.
				 */
				catch { }
				_clientsToClose.Add(client);
				return;
			}
			IPAddress ipAddress;
			if (!IPAddress.TryParse(match.Value, out ipAddress))
			{
				string dataSent = "Problem. Could not parse IP address.";
				try
				{
					SendDataTCPToClient(dataSent, client);
				}
				/* My connection to this client might have broken already.
				 * If it has, I just want to prevent a crash on this end.
				 * Nothing else needs be done.
				 */
				catch { }
				_clientsToClose.Add(client);
				return;
			}

			if (CurrentlyAcceptingPlayers || _ipAddressesWithGameAccess.Contains(ipAddress))
			{
				string dataSent = "OK";
				byte[] toSend = System.Text.Encoding.ASCII.GetBytes(dataSent);
				try
				{
					SendDataTCPToClient(dataSent, client);
				}
				catch
				{
					client.Close();
					return;
				}

				_clientsAndBytes.Add(client, bytes);
				bool updatedClientsAndUdpEndPoints = false;
				var keys = _clientsAndUdpEndPoints.Keys.ToArray();
				var values = _clientsAndUdpEndPoints.Values.ToArray();
				for (int j = values.Length - 1; j >= 0; --j)
				{
					IPEndPoint endPoint = values[j];
					if (endPoint.Address.Equals(ipAddress))
					{
						_clientsAndUdpEndPoints.Remove(keys[j]);
						_clientsAndUdpEndPoints.Add(client, endPoint); 
						updatedClientsAndUdpEndPoints = true;
					}
				}
				if (!updatedClientsAndUdpEndPoints)
				{
					_clientsAndUdpEndPoints.Add(client, new IPEndPoint(ipAddress, 8307));
				}
				if (!_ipAddressesWithGameAccess.Contains(ipAddress))
				{
					_ipAddressesWithGameAccess.Add(ipAddress);
				}
				stream.BeginRead(bytes, 0, bytes.Length, OnTcpDataReceived, client);
			}
			else
			{
				string dataSent;
				if (null == WhatToTellPlayersThatCantJoin)
				{
					dataSent = String.Empty;
				}
				else
				{
					dataSent = WhatToTellPlayersThatCantJoin;
				}

				try
				{
					SendDataTCPToClient(dataSent, client);
				}
				/* My connection to this client might have broken already.
				 * If it has, I just want to prevent a crash on this end.
				 * Nothing else needs be done.
				 */
				catch { }
				_clientsToClose.Add(client);
			}
		}

		private void OnTcpDataReceived(IAsyncResult asyncResult)
		{
			TcpClient client = (TcpClient)asyncResult.AsyncState;
			NetworkStream stream = client.GetStream();
			byte[] bytes = _clientsAndBytes[client];
			string data = String.Empty;		
			int numberOfBytesRead = stream.EndRead(asyncResult);
			data += System.Text.Encoding.ASCII.GetString(bytes, 0, numberOfBytesRead);
			while (stream.DataAvailable)
			{
				stream.BeginRead(bytes, 0, bytes.Length, OnTcpDataReceived, stream);
			}
			stream.BeginRead(bytes, 0, bytes.Length, OnTcpDataReceived, stream);
		}

		private void OnUdpDataSent(IAsyncResult asyncResult)
		{
			_udpClient.EndSend(asyncResult);
		}
	}
}
