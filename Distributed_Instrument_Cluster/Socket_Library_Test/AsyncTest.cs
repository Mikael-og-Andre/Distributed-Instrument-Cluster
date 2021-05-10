using Microsoft.VisualStudio.TestTools.UnitTesting;
using Socket_Library;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Networking_Library_Test {

	[TestClass]
	public class AsyncTest {

		[TestMethod]
		public async Task byteTransferTest() {
			Random rnd = new Random(DateTime.UtcNow.Millisecond);

			//random data
			byte[] clientData = new byte[Int32.MaxValue / 1000];
			rnd.NextBytes(clientData);
			byte[] listenerData = new byte[Int32.MaxValue / 1000];
			rnd.NextBytes(listenerData);

			Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			listenerSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9897));
			listenerSocket.Listen(4);
			
			Task<byte[]> serverTask = Task.Run(async () => {
				Socket connectedClient = await listenerSocket.AcceptAsync();
				NetworkStream stream = new NetworkStream(connectedClient);
				byte[] receivedData = await SocketOperation.receiveBytesAsync(stream);
				
				await SocketOperation.sendBytesAsync(stream, listenerData);
				return receivedData;
			});

			Task<byte[]> clientTask = Task.Run(async () => {
				Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9897));
				NetworkStream clientStream = new NetworkStream(client);
				await SocketOperation.sendBytesAsync(clientStream, clientData);

				byte[] receivedListenerData = await SocketOperation.receiveBytesAsync(clientStream);
				return receivedListenerData;
			});
			
			byte[] serverRec = await serverTask;
			byte[] clientRec = await clientTask;

			CollectionAssert.AreEqual(serverRec,clientData);
			CollectionAssert.AreEqual(clientRec,listenerData);

			clientTask.Dispose();
			serverTask.Dispose();
			listenerSocket.Dispose();
		}

		[TestMethod]
		public async Task stringTransferTest() {
			//setup data
			string s = "adfuhipaihöfsfådnioaainöpsfnipasgnipasprfijk9e0pghjweri0gei9rgjspdoijf";
			string ss = "@£€£@$[{[}]{$£€{[{[}[}}´]´]{[]{[]}{";
			string sss = "::::::://///////!#¤%&/()=`!!!";
			string longShakespear = @"As I remember, Adam, it was upon this fashion
bequeathed me by will but poor a thousand crowns,
and, as thou sayest, charged my brother, on his
blessing, to breed me well: and there begins my
sadness. My brother Jaques he keeps at school, and
report speaks goldenly of his profit: for my part,
he keeps me rustically at home, or, to speak more
properly, stays me here at home unkept; for call you
that keeping for a gentleman of my birth, that
differs not from the stalling of an ox? His horses
are bred better; for, besides that they are fair
with their feeding, they are taught their manage,
and to that end riders dearly hired: but I, his
brother, gain nothing under him but growth; for the
which his animals on his dunghills are as much
bound to him as I. Besides this nothing that he so
plentifully gives me, the something that nature gave
me his countenance seems to take from me: he lets
me feed with his hinds, bars me the place of a
brother, and, as much as in him lies, mines my
gentility with my education. This is it, Adam, that
grieves me; and the spirit of my father, which I
think is within me, begins to mutiny against this
servitude: I will no longer endure it, though yet I
know no wise remedy how to avoid it.";


			Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			listenerSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9028));
			listenerSocket.Listen(4);

			Task serverTask = new Task(async () => {
				Socket connectedClient = await listenerSocket.AcceptAsync();
				NetworkStream stream = new NetworkStream(connectedClient);

				await SocketOperation.sendStringAsync(s,stream);
				string receivedSS = await SocketOperation.receiveStringAsync(stream);
				Assert.AreEqual(receivedSS,ss);

				await SocketOperation.sendStringAsync(longShakespear, stream);
				await SocketOperation.sendStringAsync(sss, stream);

			});

			Task clientTask = new Task(async () => {
				Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9028));
				NetworkStream clientStream = new NetworkStream(client);

				string receivedS = await SocketOperation.receiveStringAsync(clientStream);
				await SocketOperation.sendStringAsync(ss,clientStream);
				Assert.AreEqual(receivedS,s);

				string receivedLongShake = await SocketOperation.receiveStringAsync(clientStream);
				Assert.AreEqual(receivedLongShake,longShakespear);
				string receivedSSS = await SocketOperation.receiveStringAsync(clientStream);
				Assert.AreEqual(receivedSSS,sss);
			});

			serverTask.Start();
			clientTask.Start();

			await Task.WhenAll(serverTask, clientTask);

			clientTask.Dispose();
			serverTask.Dispose();
			listenerSocket.Dispose();
		}

		[TestMethod]
		public async Task jsonObjectTransferTest() {

			//Data
			TestJsonObject jsonObject1 = new TestJsonObject("asduhiahisufbiau",3487925,"helloooasdoasdoaosdoasdoa");
			TestJsonObject jsonObject2 = new TestJsonObject("asduhiahisufbiau",Int32.MaxValue, "helloooasdoasdoaosdoasdoa");
			TestJsonObject jsonObject3 = new TestJsonObject(null,Int32.MinValue, string.Empty);


			Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			listenerSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9043));
			listenerSocket.Listen(4);

			Task serverTask = new Task(async () => {
				Socket connectedClient = await listenerSocket.AcceptAsync();
				NetworkStream stream = new NetworkStream(connectedClient);

				await SocketOperation.sendObjectAsJsonAsync(stream,jsonObject1);
				await SocketOperation.sendObjectAsJsonAsync(stream, jsonObject2);

				TestJsonObject rTestObj = await SocketOperation.receiveObjectAsJson<TestJsonObject>(stream);
				Assert.AreEqual(rTestObj.address,jsonObject3.address);
				Assert.AreEqual(rTestObj.age,jsonObject3.age);
				Assert.AreEqual(rTestObj.name,jsonObject3.name);

			});

			Task clientTask = new Task(async () => {
				Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9043));
				NetworkStream clientStream = new NetworkStream(client);

				TestJsonObject obj1 = await SocketOperation.receiveObjectAsJson<TestJsonObject>(clientStream);
				Assert.AreEqual(obj1.address,jsonObject1.address);
				Assert.AreEqual(obj1.age,jsonObject1.age);
				Assert.AreEqual(obj1.name,jsonObject1.name);

				TestJsonObject obj2 = await SocketOperation.receiveObjectAsJson<TestJsonObject>(clientStream);
				Assert.AreEqual(obj2.address,jsonObject2.address);
				Assert.AreEqual(obj2.age,jsonObject2.age);
				Assert.AreEqual(obj2.name,jsonObject2.name);
			});

			serverTask.Start();
			clientTask.Start();

			await Task.WhenAll(serverTask, clientTask);

			clientTask.Dispose();
			serverTask.Dispose();
			listenerSocket.Dispose();
		}
	}
}