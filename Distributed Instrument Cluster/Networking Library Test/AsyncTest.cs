using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Networking_Library;

namespace Networking_Library_Test {
	[TestClass]
	public class AsyncTest {

		[TestMethod]
		public async Task byteTransferTest() {

			Random rnd = new Random(DateTime.UtcNow.Millisecond);

			//random data
			byte[] clientData = new byte[Int32.MaxValue/100];
			rnd.NextBytes(clientData);
			byte[] listenerData = new byte[Int32.MaxValue/100];
			rnd.NextBytes(listenerData);


			Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			listenerSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9088));
			listenerSocket.Listen(4);

			Task serverTask = new Task(async () => {
				Socket connectedClient = await listenerSocket.AcceptAsync();
				NetworkStream stream = new NetworkStream(connectedClient);
				byte[] receivedData = await NetworkingOperations.receiveBytesAsync(stream);

				CollectionAssert.AreEqual(receivedData, clientData);

				await NetworkingOperations.sendBytesAsync(stream, listenerData);
			});

			Task clientTask = new Task(async () => {
				Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9088));
				NetworkStream clientStream = new NetworkStream(client);
				await NetworkingOperations.sendBytesAsync(clientStream, clientData);

				byte[] receivedListenerData = await NetworkingOperations.receiveBytesAsync(clientStream);
				CollectionAssert.AreEqual(receivedListenerData,listenerData);
			});
			
			serverTask.Start();
			clientTask.Start();

			await Task.WhenAll(serverTask, clientTask);

		}
	}
}
