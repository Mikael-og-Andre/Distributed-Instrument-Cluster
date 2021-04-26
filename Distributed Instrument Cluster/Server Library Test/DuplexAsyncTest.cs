using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server_Library.Authorization;
using Server_Library.Connection_Types.Async;
using Server_Library.Server_Listeners.Async;
using Server_Library.Socket_Clients.Async;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Server_Library_Test {

	[TestClass]
	public class DuplexAsyncTest {

		[TestMethod]
		public async Task authTokenTest() {
			IPEndPoint ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5601);
			DuplexListenerAsync listener = new DuplexListenerAsync(ip);

			AccessToken aToken = new AccessToken("Authorize");
			DuplexClientAsync client = new DuplexClientAsync("127.0.0.1", 5601, aToken);

			Task server = Task.Run(async () => await listener.run());
			Task setupTask = Task.Run(async () => await client.setup());

			await Task.WhenAll(setupTask);
			await Task.Delay(200);
			bool f = listener.getIncomingConnection(out ConnectionBaseAsync con);
			Assert.IsTrue(f);
			DuplexConnectionAsync connection = (DuplexConnectionAsync)con;
			AccessToken serverToken = con.accessToken;

			Assert.AreEqual(aToken.getAccessString(), serverToken.getAccessString());

			listener.stop();
			server.Dispose();
		}

		[TestMethod]
		public async Task duplexReadingBytesTest() {
			IPEndPoint ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5601);
			DuplexListenerAsync listener = new DuplexListenerAsync(ip);

			AccessToken aToken = new AccessToken("Authorize");
			DuplexClientAsync client = new DuplexClientAsync("127.0.0.1", 5601, aToken);

			Task server = Task.Run(async () => await listener.run());
			Task setupTask = Task.Run(async () => await client.setup());

			await Task.WhenAll(setupTask);
			await Task.Delay(200);
			bool f = listener.getIncomingConnection(out ConnectionBaseAsync con);
			Assert.IsTrue(f);
			DuplexConnectionAsync connection = (DuplexConnectionAsync)con;

			Random rnd = new Random(DateTime.UtcNow.Millisecond);
			byte[] randomBytes = new byte[Int32.MaxValue / 1000];
			byte[] randomBytes2 = new byte[100000];

			List<byte[]> sentbyBig = new List<byte[]>();
			List<byte[]> receivedBig = new List<byte[]>();
			Task bigTask = Task.Run(async () => {
				for (int i = 0; i < 10; i++) {
					rnd.NextBytes(randomBytes);
					//Send to client from server
					sentbyBig.Add(randomBytes);
					await connection.sendBytesAsync(randomBytes);
					//Receive and check
					byte[] receivedBytes = await client.receiveBytesAsync();
					receivedBig.Add(receivedBytes);
				}
			});
			List<byte[]> sentbySmall = new List<byte[]>();
			List<byte[]> receivedSmall = new List<byte[]>();
			Task smallTask = Task.Run(async () => {
				for (int i = 0; i < 100; i++) {
					rnd.NextBytes(randomBytes2);
					//Send to server from client
					sentbySmall.Add(randomBytes2);
					await client.sendBytesAsync(randomBytes2);
					//receive on server
					byte[] receivedBytes = await connection.receiveBytesAsync();
					receivedSmall.Add(receivedBytes);
				}
			});

			await Task.WhenAll(smallTask, bigTask);

			CollectionAssert.AreEqual(sentbySmall,receivedSmall);
			CollectionAssert.AreEqual(sentbyBig,receivedBig);
		}
	}
}