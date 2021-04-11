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
	}
}