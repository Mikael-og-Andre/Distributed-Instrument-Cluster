using Microsoft.VisualStudio.TestTools.UnitTesting;
using Networking_Library_Test;
using Server_Library;
using Server_Library.Authorization;
using Server_Library.Connection_Types;
using Server_Library.Server_Listeners;
using Server_Library.Socket_Clients;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Server_Library_Test {

	[TestClass]
	public class SendingListenerTesting {

		[TestMethod]
		public void testSendingListener() {
			IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5435);

			SendingListener<TestJsonObject> sendingListener = new SendingListener<TestJsonObject>(ipEndPoint);

			Task.Run(() => {
				sendingListener.start();
			});
			Thread.Sleep(5000);
			//Cancellation Token
			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

			ReceivingClient<TestJsonObject> receivingClient1 = new ReceivingClient<TestJsonObject>("127.0.0.1", 5435,
				new ClientInformation("receivingClient1", "location 1", "type 1","receiver1"), new AccessToken("access"), cancellationTokenSource.Token);

			ReceivingClient<TestJsonObject> receivingClient2 = new ReceivingClient<TestJsonObject>("127.0.0.1", 5435,
				new ClientInformation("receivingClient2", "location 2", "type 2","receiver2"), new AccessToken("access"), cancellationTokenSource.Token);

			ReceivingClient<TestJsonObject> receivingClient3 = new ReceivingClient<TestJsonObject>("127.0.0.1", 5435,
				new ClientInformation("receivingClient3", "location 3", "type 3","receiver3"), new AccessToken("access"), cancellationTokenSource.Token);

			Task.Run(() => {
				receivingClient1.run();
			});

			Task.Run(() => {
				receivingClient2.run();
			});

			Task.Run(() => {
				receivingClient3.run();
			});

			Thread.Sleep(1000);
			//List
			List<TestJsonObject> listFor1 = new List<TestJsonObject>();
			listFor1.Add(new TestJsonObject("herilolol", 1010, "oggigigigig"));
			listFor1.Add(new TestJsonObject("jeralo", 12, "obama street"));
			listFor1.Add(new TestJsonObject("torri", 44, "fernando obamium"));

			List<TestJsonObject> listFor2 = new List<TestJsonObject>();
			listFor2.Add(new TestJsonObject("obamium", 33, "hehieheihei"));
			listFor2.Add(new TestJsonObject("asdddd", 345, "trump street"));
			listFor2.Add(new TestJsonObject("aaaaa", 22, "obamium maximus"));

			List<TestJsonObject> listFor3 = new List<TestJsonObject>();
			listFor3.Add(new TestJsonObject("turmpistania", 2233, "gogoogogogogoogog"));
			listFor3.Add(new TestJsonObject("fereeee", 3, "heruuu street"));
			listFor3.Add(new TestJsonObject("Gologogogogo", 2332, "maximus retardicus"));

			List<SendingConnection<TestJsonObject>> listOfSendingConnection =
				sendingListener.getListOfSendingConnections();

			SendingConnection<TestJsonObject> sendingConnection1 = null;

			bool found1 = false;
			while (!found1) {
				lock (listOfSendingConnection) {
					foreach (var connection in listOfSendingConnection) {
						ClientInformation clientInformation = connection.getInstrumentInformation();
						if (clientInformation.Name == "receivingClient1") {
							sendingConnection1 = connection;
							found1 = true;
							break;
						}
					}
				}
			}

			SendingConnection<TestJsonObject> sendingConnection2 = null;

			bool found2 = false;
			while (!found2) {
				lock (listOfSendingConnection) {
					foreach (var connection in listOfSendingConnection) {
						ClientInformation clientInformation = connection.getInstrumentInformation();
						if (clientInformation.Name == "receivingClient2") {
							sendingConnection2 = connection;
							found2 = true;
							break;
						}
					}
				}
			}

			SendingConnection<TestJsonObject> sendingConnection3 = null;

			bool found3 = false;
			while (!found3) {
				lock (listOfSendingConnection) {
					foreach (var connection in listOfSendingConnection) {
						ClientInformation clientInformation = connection.getInstrumentInformation();
						if (clientInformation.Name == "receivingClient3") {
							sendingConnection3 = connection;
							found3 = true;
							break;
						}
					}
				}
			}

			foreach (var obj in listFor1) {
				sendingConnection1.queueObjectForSending(obj);
			}

			foreach (var obj in listFor2) {
				sendingConnection2.queueObjectForSending(obj);
			}

			foreach (var obj in listFor3) {
				sendingConnection3.queueObjectForSending(obj);
			}

			Thread.Sleep(1000);

			foreach (var obj in listFor1) {
				TestJsonObject currentTestJsonObject;
				bool foundObject = false;
				do {
					foundObject = receivingClient1.getObjectFromClient(out TestJsonObject output);
					currentTestJsonObject = output;
				} while (!foundObject);

				Assert.AreEqual(obj.name, currentTestJsonObject.name);
				Assert.AreEqual(obj.age, currentTestJsonObject.age);
				Assert.AreEqual(obj.address, currentTestJsonObject.address);
			}

			foreach (var obj in listFor2) {
				TestJsonObject currentTestJsonObject;
				bool foundObject = false;
				do {
					foundObject = receivingClient2.getObjectFromClient(out TestJsonObject output);
					currentTestJsonObject = output;
				} while (!foundObject);

				Assert.AreEqual(obj.name, currentTestJsonObject.name);
				Assert.AreEqual(obj.age, currentTestJsonObject.age);
				Assert.AreEqual(obj.address, currentTestJsonObject.address);
			}

			foreach (var obj in listFor3) {
				TestJsonObject currentTestJsonObject;
				bool foundObject = false;
				do {
					foundObject = receivingClient3.getObjectFromClient(out TestJsonObject output);
					currentTestJsonObject = output;
				} while (!foundObject);

				Assert.AreEqual(obj.name, currentTestJsonObject.name);
				Assert.AreEqual(obj.age, currentTestJsonObject.age);
				Assert.AreEqual(obj.address, currentTestJsonObject.address);
			}
		}
	}
}