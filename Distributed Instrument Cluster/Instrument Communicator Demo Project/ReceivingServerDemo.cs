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
	/// Demo project for Testing The Receiving Listener and Sending Client
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class ReceivingServerDemo {

		public static void Main(string[] args) {
			ReceivingListener<exampleObject> receiver = new ReceivingListener<exampleObject>(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5050));

			Task serverTask = new Task(() => receiver.start());
			serverTask.Start();

			Thread.Sleep(1000);

			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			SendingClient<exampleObject> sendingClient = new SendingClient<exampleObject>("127.0.0.1", 5050, new ClientInformation("sendingClient", "here", "testing","testingSub"), new AccessToken("access"), cancellationTokenSource.Token);
			Task sendingClientTask = new Task(() => sendingClient.run());
			sendingClientTask.Start();

			List<ReceivingConnection<exampleObject>> connections = receiver.getListOfReceivingConnections();

			List<exampleObject> objectsForSending = new List<exampleObject>();
			objectsForSending.Add(new exampleObject("kevin",12));
			objectsForSending.Add(new exampleObject("bob",32));
			objectsForSending.Add(new exampleObject("gudrun",69));
			objectsForSending.Add(new exampleObject("veronica",18));
			objectsForSending.Add(new exampleObject("randall",33));

			foreach (var obj in objectsForSending) {
				sendingClient.queueBytesForSending(obj);
			}

			while (!cancellationTokenSource.Token.IsCancellationRequested) {
				lock (connections) {
					foreach (var con in connections) {
						if (con.getDataFromConnection(out exampleObject output)) {
							Console.WriteLine("Received Object: name: {0}, age: {1}", output.name, output.age);
						}
					}
				}
				Thread.Sleep(100);
			}
		}
	}
}