using Server_Library;
using Server_Library.Authorization;
using Server_Library.Connection_Types;
using Server_Library.Server_Listeners;
using Server_Library.Socket_Clients;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace serverDemo {

	/// <summary>
	/// Demo for testing Sending Listener And Receiving client combo
	/// <Author>Mikael Nilssen</Author>
	/// </summary>
	public class SendingServerDemo {

		public static void Main(string[] args) {
			SendingListener sender = new SendingListener(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5051));

			Task serverTask = new Task(() => sender.start());
			serverTask.Start();

			Thread.Sleep(1000);

			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			ReceivingClient receivingClient = new ReceivingClient("127.0.0.1", 5051, new ClientInformation("receivingClient", "here", "testing","testingSubname"), new AccessToken("access"), cancellationTokenSource.Token);
			Task receivingClientTask = new Task(() => receivingClient.run());
			receivingClientTask.Start();

			List<SendingConnection> connections = sender.getListOfSendingConnections();

			while (connections.Count < 1) {
				Thread.Sleep(100);
			}

			List<exampleObject> objectsForSending = new List<exampleObject>();
			objectsForSending.Add(new exampleObject("kevin", 12));
			objectsForSending.Add(new exampleObject("bob", 32));
			objectsForSending.Add(new exampleObject("gudrun", 69));
			objectsForSending.Add(new exampleObject("veronica", 18));
			objectsForSending.Add(new exampleObject("randall", 33));

			lock (connections) {
				foreach (var connection in connections) {
					foreach (var obj in objectsForSending) {
						byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(obj);
						connection.queueByteArrayForSending(jsonBytes);
					}
				}
			}

			while (!cancellationTokenSource.Token.IsCancellationRequested) {
				if (receivingClient.getBytesFromClient(out byte[] output)) {
					exampleObject obj = JsonSerializer.Deserialize<exampleObject>(output);
					Console.WriteLine("Received Object: Name: {0} Age: {1}", obj.name, obj.age);
				}

				Thread.Sleep(100);
			}
		}
	}
}