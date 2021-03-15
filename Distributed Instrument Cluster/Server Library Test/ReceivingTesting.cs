﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Networking_Library;
using Networking_Library_Test;
using Server_Library;
using Server_Library.Authorization;
using Server_Library.Connection_Types;
using Server_Library.Server_Listeners;
using Server_Library.Socket_Clients;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Server_Library_Test {

	[TestClass]
	public class ReceivingTesting {

		[TestMethod]
		public void testReceivingListener() {
			IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4090);
			ReceivingListener<TestJsonObject> receivingListener = new ReceivingListener<TestJsonObject>(ipEndPoint);

			Task.Run(() => {
				receivingListener.start();
			});

			//CancellationTokenSource
			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

			SendingClient<TestJsonObject> sendingClient1 = new SendingClient<TestJsonObject>("127.0.0.1", 4090,
				new ClientInformation("sendingClient1", "location 1", "type 1"), new AccessToken("access"),
				cancellationTokenSource.Token);

			SendingClient<TestJsonObject> sendingClient2 = new SendingClient<TestJsonObject>("127.0.0.1", 4090,
				new ClientInformation("sendingClient2", "location 2", "type 2"), new AccessToken("access"),
				cancellationTokenSource.Token);

			SendingClient<TestJsonObject> sendingClient3 = new SendingClient<TestJsonObject>("127.0.0.1", 4090,
				new ClientInformation("sendingClient3", "location 3", "type 3"), new AccessToken("access"),
				cancellationTokenSource.Token);

			Task.Run(() => {
				sendingClient1.run();
			});

			Task.Run(() => {
				sendingClient2.run();
			});

			Task.Run(() => {
				sendingClient3.run();
			});

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

			foreach (var obj in listFor1) {
				sendingClient1.queueObjectForSending(obj);
			}

			foreach (var obj in listFor2) {
				sendingClient2.queueObjectForSending(obj);
			}

			foreach (var obj in listFor3) {
				sendingClient3.queueObjectForSending(obj);
			}

			Thread.Sleep(5000);
			List<ReceivingConnection<TestJsonObject>> listOfReceivingConnections = receivingListener.getListOfReceivingConnections();

			ReceivingConnection<TestJsonObject> receivingConnection1 = null;

			bool found1 = false;
			while (!found1) {
				lock (listOfReceivingConnections) {
					foreach (var connection in listOfReceivingConnections) {
						ClientInformation clientInformation = connection.getInstrumentInformation();
						if (clientInformation.Name == "sendingClient1") {
							receivingConnection1 = connection;
							found1 = true;
							break;
						}
					}
				}
			}

			ReceivingConnection<TestJsonObject> receivingConnection2 = null;

			bool found2 = false;
			while (!found2) {
				lock (listOfReceivingConnections) {
					foreach (var connection in listOfReceivingConnections) {
						ClientInformation clientInformation = connection.getInstrumentInformation();
						if (clientInformation.Name == "sendingClient2") {
							receivingConnection2 = connection;
							found2 = true;
							break;
						}
					}
				}
			}

			ReceivingConnection<TestJsonObject> receivingConnection3 = null;

			bool found3 = false;
			while (!found3) {
				lock (listOfReceivingConnections) {
					foreach (var connection in listOfReceivingConnections) {
						ClientInformation clientInformation = connection.getInstrumentInformation();
						if (clientInformation.Name == "sendingClient3") {
							receivingConnection3 = connection;
							found3 = true;
							break;
						}
					}
				}
			}

			foreach (var obj in listFor1) {
				TestJsonObject currenTestJsonObject;
				bool foundObject = false;
				do {
					foundObject = receivingConnection1.getObjectFromConnection(out TestJsonObject output);
					currenTestJsonObject = output;
				} while (!foundObject);

				Assert.AreEqual(obj.name, currenTestJsonObject.name);
				Assert.AreEqual(obj.age, currenTestJsonObject.age);
				Assert.AreEqual(obj.address, currenTestJsonObject.address);
			}


			foreach (var obj in listFor2) {
				TestJsonObject currenTestJsonObject;
				bool foundObject = false;
				do {
					foundObject = receivingConnection2.getObjectFromConnection(out TestJsonObject output);
					currenTestJsonObject = output;
				} while (!foundObject);

				Assert.AreEqual(obj.name, currenTestJsonObject.name);
				Assert.AreEqual(obj.age, currenTestJsonObject.age);
				Assert.AreEqual(obj.address, currenTestJsonObject.address);
			}

			foreach (var obj in listFor3) {
				TestJsonObject currenTestJsonObject;
				bool foundObject = false;
				do {
					foundObject = receivingConnection3.getObjectFromConnection(out TestJsonObject output);
					currenTestJsonObject = output;
				} while (!foundObject);

				Assert.AreEqual(obj.name, currenTestJsonObject.name);
				Assert.AreEqual(obj.age, currenTestJsonObject.age);
				Assert.AreEqual(obj.address, currenTestJsonObject.address);
			}

		}

		[TestMethod]
		public void testSingleConnection() {
			Thread thread = new Thread(() => { });
			//Setup listener
			IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4091);
			Socket listenerSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			listenerSocket.Bind(ipEndPoint);
			listenerSocket.Listen(10);

			//Client info
			ClientInformation clientInformation = new ClientInformation("Random Name", "testing location", "testing type");

			//Create client
			SendingClient<TestJsonObject> sendingClient = new SendingClient<TestJsonObject>("127.0.0.1", 4091,
				clientInformation, new AccessToken("access"), new CancellationToken(false));

			//Start client
			Task.Run(() => {
				sendingClient.run();
			});
			//Accept socket and authorize
			Socket socket = listenerSocket.Accept();

			//Send start Auth signal
			NetworkingOperations.sendStringWithSocket("auth", socket);

			//Get Authorization Token info
			string connectionHash = NetworkingOperations.receiveStringWithSocket(socket);
			AccessToken accessToken = new AccessToken(connectionHash);

			//Get Client information
			ClientInformation info = NetworkingOperations.receiveJsonObjectWithSocket<ClientInformation>(socket);

			ReceivingConnection<TestJsonObject> receivingConnection = new ReceivingConnection<TestJsonObject>(thread,
				socket, accessToken, info, new CancellationToken(false)) {
				isSetupCompleted = true, isAuthorized = true
			};

			List<TestJsonObject> listOfObjects = new List<TestJsonObject>();
			listOfObjects.Add(new TestJsonObject("heihei", 120, ""));
			listOfObjects.Add(new TestJsonObject("bob", 12, "a"));
			listOfObjects.Add(new TestJsonObject("kevin", 4, "bjert"));
			listOfObjects.Add(new TestJsonObject("randis", 5551, "hyundai"));

			//Queue objects for sending
			foreach (var obj in listOfObjects) {
				sendingClient.queueObjectForSending(obj);
			}
			Thread.Sleep(1000);
			//Receive and check
			foreach (var obj in listOfObjects) {
				while (!receivingConnection.receive()) {
				}

				receivingConnection.getObjectFromConnection(out TestJsonObject output);
				Assert.AreEqual(obj.name, output.name);
				Assert.AreEqual(obj.age, output.age);
				Assert.AreEqual(obj.address, output.address);
			}
		}
	}
}