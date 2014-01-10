using System.Net;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xasteroids
{
	[TestFixture]
	public class NetworkUnitTests
	{
		private string _messageContent = @"Hi, Mom! Here is a curly bracket - }. Junky chars too! $%^\\,\,";
		private string _otherMessageContent = @"Hi, Mom! Here is some junk! $%^\\,\,";
		private string _malformedObjectString = "utter nonsense}";
		private IPAddress _someAddress = IPAddress.Parse("1.2.3.4");
		private string _validObjectString =		"{" +
													"Type:BugBear," +
													"[" +
														"a string valu	e, 24," +
														"[nested string value, 58]" +
													"]" +	
												"}";
		private ObjectStringConverter _converter = new ObjectStringConverter();

		[Test]
		public void ObjectStringConverter_ConversionTests()
		{ 
			NetworkMessage input = new NetworkMessage { Content = _messageContent };
			NetworkMessage output = _converter.StringToObject(_converter.ObjectToString(input)) as NetworkMessage;
			Assert.That(input.Content.Equals(output.Content), "input.Content was " + input.Content + ", but output.Content was " +
				output.Content);
		}

		[Test]
		public void ObjectStringConverter_ObjectStringMalformedTest()
		{
			Assert.That(_converter.ObjectStringMalformed(_malformedObjectString));
			Assert.That(_converter.ObjectStringMalformed(_validObjectString) == false);
		}

		[Test]
		public void Host_ReceiveDataTest()
		{
			GameMessage inMessage = new GameMessage { Content = _messageContent };
			GameMessage outMessage = new GameMessage();
			Host host = new Host();
			Action<IPAddress, IConfigurable> theAction = (notImportant, receivedObject) => outMessage = (GameMessage)receivedObject;
			host.ObjectReceived += theAction;
			host.ReceiveData(_someAddress, _converter.ObjectToString(inMessage));
			Assert.That(outMessage.Content.Equals(inMessage.Content));
			host.ObjectReceived -= theAction;
			host.StopMonitoringDataFromSender(_someAddress);

			bool malformedEventNotRaised = true;
			Action<IPAddress, string> shouldNotExecute = (notImportant, whoCares) => malformedEventNotRaised = false;
			object outObject = new object();
			theAction = (notImportant, receivedObject) => outObject = receivedObject;
			host.ObjectReceived += theAction;
			host.ReceiveData(_someAddress, _validObjectString);
			Assert.That(malformedEventNotRaised);
			Assert.That(outObject == null);
			host.ObjectReceived -= theAction;
			host.StopMonitoringDataFromSender(_someAddress);

			string mixedString = _validObjectString + _validObjectString + _malformedObjectString + _otherMessageContent;
			List<object> validObjects = new List<object>();
			Action<IPAddress, IConfigurable> onValidReceived = (notImportant, receivedObject) => validObjects.Add(receivedObject);
			host.ObjectReceived += onValidReceived;
			host.ReceiveData(_someAddress, mixedString);
			Assert.That(validObjects[0] == null);
			Assert.That(validObjects[1] == null);
		}
	}
}
