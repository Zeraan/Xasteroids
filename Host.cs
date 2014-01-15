using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Xasteroids
{
	public class Host
	{
		public const int TCP_PORT = 7127;

		public IPAddress IPAddress { get { return Dns.GetHostEntry(string.Empty).AddressList.Where(address => address.AddressFamily == AddressFamily.InterNetwork).First(); } }
		public bool IsShutDown { get; private set; }
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

		public bool HasConnectionTo(IPAddress clientAddress)
		{
			foreach (var item in _clientsAndUdpEndPoints)
			{
				if (item.Value.Address.Equals(clientAddress))
				{
					return true;
				}
			}
			return false;
		}

		public void SendObjectTCP(IConfigurable obj)
		{
			string data = _objectStringConverter.ObjectToString(obj);
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

		public void SendObjectTcpToClient(IConfigurable obj, IPAddress clientAddress)
		{
			TcpClient client = null;
			foreach (var item in _clientsAndUdpEndPoints)
			{
				if (item.Value.Address.Equals(clientAddress))
				{
					client = item.Key;
					SendObjectTcpToClient(obj, client);
					return;
				}
			}
		}

		private void SendObjectTcpToClient(IConfigurable obj, TcpClient client)
		{
			NetworkStream stream = client.GetStream();
			string data = _objectStringConverter.ObjectToString(obj);
			byte[] bytes = System.Text.Encoding.ASCII.GetBytes(data);
			stream.BeginWrite(bytes, 0, bytes.Length, OnTcpDataSent, client);
		}

		public void SendObjectUDP(IConfigurable obj)
		{
			string data = _objectStringConverter.ObjectToString(obj);
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
			IsShutDown = true;
		}

		private void OnAcceptTcpClient(IAsyncResult ar)
		{
			TcpClient client = null;
			try
			{
				client = _tcpListener.EndAcceptTcpClient(ar);
			}
			catch (ObjectDisposedException)
			{
				//just eating this because I think it's Microsoft's bad
			}
			if (_tcpListener.Server.IsBound)
			{
				_tcpListener.BeginAcceptTcpClient(OnAcceptTcpClient, _tcpListener);
			}
			if (client == null)
			{
				return;
			}
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

		/* gimme an IP address or I'll tell you to and close the connection.
		 * This is supposed to be the first real data that we get 
		 */
		private void OnInitialTcpDataReceived(IAsyncResult asyncResult)
		{
			TcpClient client = (TcpClient)asyncResult.AsyncState;
			NetworkStream stream = client.GetStream();
			byte[] bytes = _clientsAndBytes[client];
			int numberOfBytesRead = stream.EndRead(asyncResult);
			string dataReceived = System.Text.Encoding.ASCII.GetString(bytes, 0, numberOfBytesRead);
			Match match = _sloppyIPAddressRegex.Match(dataReceived);
			if (!match.Success)
			{
				try
				{
					SendObjectTcpToClient(new NetworkMessage { Content = "Problem. No IP address." }, client);
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
				try
				{
					SendObjectTcpToClient(new NetworkMessage { Content = "Problem. Could not parse IP address." }, client);
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
				IConfigurable objectSent;
				if (null == WhatToTellPlayersThatCantJoin)
				{
					objectSent = new GameMessage { Content = String.Empty };
				}
				else
				{
					objectSent = new GameMessage { Content = WhatToTellPlayersThatCantJoin };
				}

				try
				{
					SendObjectTcpToClient(objectSent, client);
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
			if (!client.Connected)
			{
				_clientsAndBytes.Remove(client);
				IPAddress addressOfDisconnectd = _clientsAndUdpEndPoints[client].Address;
				_clientsAndUdpEndPoints.Remove(client);
				if (_sendersAndData.ContainsKey(addressOfDisconnectd))
				{
					_sendersAndData.Remove(addressOfDisconnectd);
				}
				return;
			}
			NetworkStream stream = client.GetStream();
			byte[] bytes = _clientsAndBytes[client];		
			int numberOfBytesRead = stream.EndRead(asyncResult);
			if (numberOfBytesRead == 0)
			{
				_clientsAndBytes.Remove(client);
				IPAddress addressOfDisconnectd = _clientsAndUdpEndPoints[client].Address;
				_clientsAndUdpEndPoints.Remove(client);
				if (_sendersAndData.ContainsKey(addressOfDisconnectd))
				{
					_sendersAndData.Remove(addressOfDisconnectd);
				}
				return;
			}
			string data = System.Text.Encoding.ASCII.GetString(bytes, 0, numberOfBytesRead);
			ReceiveData(_clientsAndUdpEndPoints[client].Address, data);	//this is the one
			stream.BeginRead(bytes, 0, bytes.Length, OnTcpDataReceived, client);
		}

		private void OnUdpDataSent(IAsyncResult asyncResult)
		{
			_udpClient.EndSend(asyncResult);
		}

		public event Action<IPAddress, IConfigurable> ObjectReceived;

		public Dictionary<IPAddress, string> _sendersAndData = new Dictionary<IPAddress, string>();

		public void ReceiveData(IPAddress senderIPAddress, string data)
		{
			if (data == null)
			{
				throw new ArgumentException();
			}

			if (!_sendersAndData.ContainsKey(senderIPAddress))
			{
				_sendersAndData.Add(senderIPAddress, null);
			}
			string portionReceived = _sendersAndData[senderIPAddress];
			if (portionReceived == null)
			{
				Match objectStartMatch = ObjectStringConverter.ObjectStartRegex.Match(data);
				if (!objectStartMatch.Success)
				{
					//our object string was malformed
					return;
				}
				if (objectStartMatch.Index > 0)
				{
					string fragmentOnFront = data.Substring(0, objectStartMatch.Index);
					//our object string was malformed
					data = data.Substring(objectStartMatch.Index);
				}
				portionReceived = data;
			}
			else
			{
				portionReceived += data;
			}

			/* Given the asynchronous nature of our network transfers, it is possible
			* that we've got multiple objects in our data string. Unlikely, but possible.
			*/
			string[] parts = ObjectStringConverter.ObjectEndRegex.Split(portionReceived);
			if (parts.Length > 1)
			{
				int index = 0;

				string completeString = parts[index];
				ReceiveObjectString(senderIPAddress, completeString);

				++index;
				int indexOfLast = parts.Length - 1;
				while (index < indexOfLast)
				{
					/* A closed object string is one ending with an object closing character.
					 * It might not be a valid object string, but if it were, the object
					 * would be correctly closed.
					 */
					string closedObjectString = parts[index];
					Match objectStartMatch = ObjectStringConverter.ObjectStartRegex.Match(closedObjectString);
					if (objectStartMatch.Success)
					{
						if (objectStartMatch.Index > 0)
						{
							string fragmentOnFront = closedObjectString.Substring(0, objectStartMatch.Index);
							//our object string was malformed
							closedObjectString = closedObjectString.Substring(objectStartMatch.Index);
						}
						ReceiveObjectString(senderIPAddress, closedObjectString);
					}
					else
					{
						//our object string was malformed
					}
					++index;
				}
				/* The last part must be either an empty string or the starting piece of
				 * another object.
				 */
				string lastPart = parts[indexOfLast];
				if (lastPart.Length == 0)
				{
					_sendersAndData[senderIPAddress] = null;
					return;
				}
				Match objStartMatch = ObjectStringConverter.ObjectStartRegex.Match(lastPart);
				if (objStartMatch.Success)
				{
					if (objStartMatch.Index > 0)
					{
						string fragmentOnFront = lastPart.Substring(0, objStartMatch.Index);
						//our object string was malformed
						_sendersAndData[senderIPAddress] = lastPart.Substring(objStartMatch.Index);
					}
				}
				else
				{
					//our object string was malformed
					_sendersAndData[senderIPAddress] = null;
				}
			}
			else
			{
				_sendersAndData[senderIPAddress] = portionReceived;
			}
		}

		public void ReceiveObjectString(IPAddress senderIPAddress, string objectString)
		{
			if (_objectStringConverter.ObjectStringMalformed(objectString))
			{
				//our object string was malformed
			}
			else if (ObjectReceived != null)
			{
				IConfigurable theObject = _objectStringConverter.StringToObject(objectString);
				ObjectReceived(senderIPAddress, theObject);
			}
		}

		public void StopMonitoringDataFromSender(IPAddress senderIPAddress)
		{
			_sendersAndData.Remove(senderIPAddress);
		}

		private ObjectStringConverter _objectStringConverter = new ObjectStringConverter();
	}
}
