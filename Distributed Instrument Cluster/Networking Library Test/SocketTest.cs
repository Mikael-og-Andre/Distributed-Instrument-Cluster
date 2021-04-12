using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Networking_Library;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Networking_Library_Test {

	[TestClass]
	public class SocketTest {

		[TestMethod]
		public void testJsonObjectSending() {
			//Setup listener
			IPEndPoint ipEnd = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4069);
			Socket listenerSocket = new Socket(ipEnd.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			listenerSocket.Bind(ipEnd);
			listenerSocket.Listen(10);

			List<TestJsonObject> listOfObjects = new List<TestJsonObject>();
			listOfObjects.Add(new TestJsonObject("harald", 33, "ogandistan street 45"));
			listOfObjects.Add(new TestJsonObject("ogogogon@£$€{[]}[]}[]$£@£dsfhgoidefhgno@£@$@$@$£@£", 222222200, "boggi street 45 \0\0\0"));
			listOfObjects.Add(new TestJsonObject("heheiheihe", -0, "ogandistan street 45"));
			listOfObjects.Add(new TestJsonObject("benjamin", 1, "Streetname"));
			listOfObjects.Add(new TestJsonObject("gundert", -19239829, "ogandistan 45"));
			listOfObjects.Add(new TestJsonObject("gun///////////////1241251245125::::dert", -19239829, "ogandistan 45"));

			Task.Run(() => {
				Socket sock = listenerSocket.Accept();
				foreach (var objs in listOfObjects) {
					NetworkingOperations.sendJsonObjectWithSocket(objs, sock);
				}
			});

			Thread.Sleep(1000);
			Socket connectingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			connectingSocket.Connect(IPAddress.Parse("127.0.0.1"), 4069);

			foreach (var obj in listOfObjects) {
				TestJsonObject jsonObject = NetworkingOperations.receiveJsonObjectWithSocket<TestJsonObject>(connectingSocket);

				Assert.AreEqual(obj.address, jsonObject.address);
				Assert.AreEqual(obj.name, jsonObject.name);
				Assert.AreEqual(obj.age, jsonObject.age);
			}
			connectingSocket.Dispose();
			listenerSocket.Dispose();
		}

		[TestMethod]
		public void testStringSending() {
			//Setup listener
			IPEndPoint ipEnd = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4069);
			Socket listenerSocket = new Socket(ipEnd.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			listenerSocket.Bind(ipEnd);
			listenerSocket.Listen(10);

			List<string> listOfObjects = new List<string>();
			listOfObjects.Add("@£@$helllooooo");
			listOfObjects.Add("Hello my name is kevin 123124124");
			listOfObjects.Add("ghabjnksdlmsahilbfgapisdfibuaspidfabinsuf");
			listOfObjects.Add("\0\0\0\0\0\0 ogga");
			listOfObjects.Add("");
			listOfObjects.Add("\\");

			Task.Run(() => {
				Socket sock = listenerSocket.Accept();
				foreach (var objs in listOfObjects) {
					NetworkingOperations.sendStringWithSocket(objs, sock);
				}
			});

			Thread.Sleep(1000);
			Socket connectingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			connectingSocket.Connect(IPAddress.Parse("127.0.0.1"), 4069);

			foreach (var obj in listOfObjects) {
				string str = NetworkingOperations.receiveStringWithSocket(connectingSocket);

				Assert.AreEqual(obj, str);
			}
		}

		[TestMethod]
		public void testSendAndReceiveBytes() {
			//Data samples
			Random randNum = new Random(DateTime.Now.Millisecond);

			byte[] arrayOne = new byte[2000000];
			byte[] arrayTwo = new byte[100000000];
			byte[] arrayThree = new byte[randNum.Next(int.MaxValue/30000)];
			byte[] arrayFour = new byte[randNum.Next(int.MaxValue/1000)];

			for (int i = 0; i < arrayOne.Length; i++) {
				arrayOne[i] = (byte) randNum.Next();
			}
			for (int i = 0; i < arrayTwo.Length; i++) {
				arrayTwo[i] = (byte) randNum.Next();
			}
			for (int i = 0; i < arrayThree.Length; i++) {
				arrayThree[i] = (byte) randNum.Next();
			}
			for (int i = 0; i < arrayFour.Length; i++) {
				arrayFour[i] = (byte) randNum.Next();
			}

			//Setup connections
			Socket listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			EndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6789);
			listeningSocket.Bind(ep);
			listeningSocket.Listen(200);

			Socket senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			senderSocket.Connect("127.0.0.1",6789);

			Socket receivingSocket = listeningSocket.Accept();

			//Streams
			NetworkStream receivingStream = new NetworkStream(receivingSocket, true);

			NetworkStream senderStream = new NetworkStream(senderSocket, true);

			//Send first array
			NetworkingOperations.sendBytes(senderStream,arrayOne);
			//Check results
			byte[] receivedArrayOne = NetworkingOperations.receiveBytes(receivingStream,arrayOne.Length);
			for (int i = 0; i < receivedArrayOne.Length; i++) {
				Assert.AreEqual((byte)receivedArrayOne[i],(byte)arrayOne[i]);
			}

			//Send second array
			NetworkingOperations.sendBytes(senderStream,arrayTwo);
			//Check results
			byte[] receivedArrayTwo = NetworkingOperations.receiveBytes(receivingStream,arrayTwo.Length);
			for (int i = 0; i < receivedArrayTwo.Length; i++) {
				Assert.AreEqual((byte)receivedArrayTwo[i],(byte)arrayTwo[i]);
			}

			//Send third array
			NetworkingOperations.sendBytes(senderStream,arrayThree);
			//Check results
			byte[] receivedArrayThree = NetworkingOperations.receiveBytes(receivingStream,arrayThree.Length);
			for (int i = 0; i < receivedArrayThree.Length; i++) {
				Assert.AreEqual((byte)receivedArrayThree[i],(byte)arrayThree[i]);
			}

			//Send fourth array
			NetworkingOperations.sendBytes(senderStream,arrayFour);
			//Check results
			byte[] receivedArrayFour = NetworkingOperations.receiveBytes(receivingStream, arrayFour.Length);
			for (int i = 0; i < receivedArrayFour.Length; i++) {
				Assert.AreEqual((byte)receivedArrayFour[i],(byte)arrayFour[i]);
			}

			//Test sending other way
			//Send first array
			NetworkingOperations.sendBytes(receivingStream,arrayFour);
			//Check results
			byte[] receivedReverse = NetworkingOperations.receiveBytes(senderStream, arrayFour.Length);
			for (int i = 0; i < receivedArrayFour.Length; i++) {
				Assert.AreEqual((byte)receivedReverse[i],(byte)arrayFour[i]);
			}

		}

	}
}