using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Xasteroids
{
	public class Client
	{
		public IPAddress IPAddress { get { return Dns.GetHostEntry(string.Empty).AddressList.Where(address => address.AddressFamily == AddressFamily.InterNetwork).First(); } }
		public bool IsShutDown { get; private set; }

		public event Action Disconnected;
		public event Action<IPAddress, IConfigurable> ObjectReceived;

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
				if (_tcpClient != null && _tcpClient.Connected)
				{
					_tcpClient.GetStream().Close();
					_tcpClient.Close();
				}
				_serverIPAddress = value;
				if (_serverIPAddress != null)
				{
					ResetTcpClient();
				}
			}
		}

		private string _tcpDataFromServer;

		public void ReceiveData(IPAddress senderIPAddress, string data)
		{
			if (data == null)
			{
				throw new ArgumentException();
			}

			string portionReceived = _tcpDataFromServer;
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
					_tcpDataFromServer = null;
					return;
				}
				Match objStartMatch = ObjectStringConverter.ObjectStartRegex.Match(lastPart);
				if (objStartMatch.Success)
				{
					if (objStartMatch.Index > 0)
					{
						string fragmentOnFront = lastPart.Substring(0, objStartMatch.Index);
						//our object string was malformed
						_tcpDataFromServer = lastPart.Substring(objStartMatch.Index);
					}
				}
				else
				{
					//our object string was malformed
					_tcpDataFromServer = null;
				}
			}
			else
			{
				_tcpDataFromServer = portionReceived;
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

		public void ResetTcpClient()
		{
			try
			{
				_tcpClient = new TcpClient(_serverIPAddress.ToString(), Host.TCP_PORT);
			}
			catch (SocketException e)
			{
				ServerIPAddress = null;
				if (e.SocketErrorCode == SocketError.TimedOut)
				{
					MessageBox.Show("The host computer did not respond to your connection request. " +
						"You might double check the IP address you entered. " +
						"It is also possible that the firewall on the host computer would not let your connection request through."
					);
				}
				else if (e.SocketErrorCode == SocketError.NetworkUnreachable)
				{
					MessageBox.Show("Your computer cannot reach the IP address you have specified. " +
						"You might want to double check what you have entered."
					);
				}
				else
				{
					MessageBox.Show("You were unable to connect to the host computer. Here is what .Net says about the error: " +
						e.Message
					);
				}
			}
			if (_tcpClient == null)
			{
				return;
			}
			NetworkStream stream = _tcpClient.GetStream();
			byte[] bytes = new byte[1024];
			_streamAndBytesRead[0] = stream;
			_streamAndBytesRead[1] = bytes;
			stream.BeginRead(bytes, 0, bytes.Length, ReadCallback, _streamAndBytesRead);
			SendObjectTcp(new NetworkMessage { Content = "IP Address:" + IPAddress });
		}

		public void SendObjectTcp(IConfigurable obj, bool tryAgainOnFailure = true)
		{
			string data = _objectStringConverter.ObjectToString(obj);
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
					SendObjectTcp(obj, false);
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

		public void ShutDown()
		{
			_udpClient.Close();
			if (_tcpClient != null && _tcpClient.Connected)
			{
				_tcpClient.GetStream().Close();
				_tcpClient.Close();
			}
			IsShutDown = true;
		}

		private TcpClient _tcpClient;
		private UdpClient _udpClient;

		private void OnTcpDataSent(IAsyncResult asyncResult)
		{
			TcpClient tcpClient = (TcpClient)asyncResult.AsyncState;
			NetworkStream stream = tcpClient.GetStream();
			stream.EndWrite(asyncResult);
		}

		private List<object> _streamAndBytesRead = new List<object> {
			null,
			null
		};

		// Called when we bet data over TCP connection
		private void ReadCallback(IAsyncResult asyncResult)
		{
			NetworkStream stream = (NetworkStream)_streamAndBytesRead[0];
			byte[] bytes = (byte[])_streamAndBytesRead[1];
			int numberOfBytesRead = 0;
			if (!stream.CanRead)
			{
				ServerIPAddress = null;
				if (Disconnected != null)
				{
					Disconnected();
				}
				return;
			}
			try
			{
				numberOfBytesRead = stream.EndRead(asyncResult);
			}
			catch (IOException)
			{
				ServerIPAddress = null;
				if (Disconnected != null)
				{
					Disconnected();
				}
				return;
			}
			if (numberOfBytesRead == 0)
			{
				ServerIPAddress = null;
				if (Disconnected != null)
				{
					Disconnected();
				}
				return;
			}
			string data = System.Text.Encoding.ASCII.GetString(bytes, 0, numberOfBytesRead);
			ReceiveData(ServerIPAddress, data);
			stream.BeginRead(bytes, 0, bytes.Length, ReadCallback, _streamAndBytesRead);
		}

		private void OnUdpDataReceived(IAsyncResult asyncResult)
		{
			if (IsShutDown)
			{
				return;
			}
			List<object> container = (List<object>)asyncResult.AsyncState;
			IPEndPoint myEndPoint = (IPEndPoint)container[0];
			byte[] bytes = null;
			try
			{
				bytes = _udpClient.EndReceive(asyncResult, ref myEndPoint);
			}
			catch (ObjectDisposedException)
			{
				//Another Microsoft problem I'm just going to eat
			}
			catch (NullReferenceException)
			{
				//Another Microsoft problem I'm just going to eat
			}
			if (bytes == null)
			{
				return;
			}
			_udpClient.BeginReceive(OnUdpDataReceived, container);
			if (ObjectReceived != null)
			{
				string data = System.Text.Encoding.ASCII.GetString(bytes);
				IConfigurable obj = _objectStringConverter.StringToObject(data);
				ObjectReceived(ServerIPAddress, obj);
			}
		}

		private ObjectStringConverter _objectStringConverter = new ObjectStringConverter();
	}
}
