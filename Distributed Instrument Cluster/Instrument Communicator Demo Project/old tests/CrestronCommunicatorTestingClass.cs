using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using Server_Library;
using Server_Library.Authorization;
using Server_Library.Socket_Clients;

namespace Server_And_Demo_Project {

	internal class CrestronCommunicatorTestingClass {

		public static void Main(string[] args) {

			Thread.Sleep(10000);
			CancellationToken token = new CancellationToken(false);
			CrestronClient crestronClient = new CrestronClient("127.0.0.1",5050,new ClientInformation("Radar1","location","Type"),new AccessToken("access"),token);

			Thread crestronThread = new Thread(() => crestronClient.run());
			crestronThread.Start();
			Console.WriteLine("Started Server and now pushing from queue");
			ConcurrentQueue<string> queue = crestronClient.getCommandOutputQueue();

			while (!token.IsCancellationRequested) {

				if (queue.TryDequeue(out string result)) {
					Console.WriteLine("Pushed from output queue: "+result);
				}

			}

		}
	}
}