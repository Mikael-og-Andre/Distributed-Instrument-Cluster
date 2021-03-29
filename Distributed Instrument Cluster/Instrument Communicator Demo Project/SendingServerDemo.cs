using Server_Library;
using Server_Library.Authorization;
using Server_Library.Connection_Types;
using Server_Library.Server_Listeners;
using Server_Library.Socket_Clients;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace serverDemo {

	/// <summary>
	/// Demo for testing Sending Listener And Receiving client combo
	/// <Author>Mikael Nilssen</Author>
	/// </summary>
	public class SendingServerDemo {

		public static void Main(string[] args) {
			SendingListener<exampleObject> sender = new SendingListener<exampleObject>(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5051));

			Task serverTask = new Task(() => sender.start());
			serverTask.Start();

			Thread.Sleep(1000);

			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			ReceivingClient<exampleObject> receivingClient = new ReceivingClient<exampleObject>("127.0.0.1", 5051, new ClientInformation("receivingClient", "here", "testing","testingSubname"), new AccessToken("access"), cancellationTokenSource.Token);
			Task receivingClientTask = new Task(() => receivingClient.run());
			receivingClientTask.Start();

			List<SendingConnection<exampleObject>> connections = sender.getListOfSendingConnections();

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
						connection.queueObjectForSending(obj);
					}
				}
			}

			while (!cancellationTokenSource.Token.IsCancellationRequested) {
				if (receivingClient.getObjectFromClient(out exampleObject output)) {
					Console.WriteLine("Received Object: Name: {0} Age: {1}", output.name, output.age);
				}

				Thread.Sleep(100);
			}
		}
	}
}