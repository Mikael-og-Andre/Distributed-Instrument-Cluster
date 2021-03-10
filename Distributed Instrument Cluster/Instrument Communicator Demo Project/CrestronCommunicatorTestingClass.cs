using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using Instrument_Communicator_Library;
using Instrument_Communicator_Library.Authorization;
using Instrument_Communicator_Library.Remote_Device_side_Communicators;

namespace Server_And_Demo_Project {

	internal class CrestronCommunicatorTestingClass {

		public static void Main(string[] args) {

			Thread.Sleep(10000);
			CancellationToken token = new CancellationToken(false);
			CrestronCommunicator crestronCommunicator = new CrestronCommunicator("127.0.0.1",5050,new InstrumentInformation("Radar1","location","Type"),new AccessToken("access"),token);

			Thread crestronThread = new Thread(() => crestronCommunicator.Start());
			crestronThread.Start();
			Console.WriteLine("Started Server and now pushing from queue");
			ConcurrentQueue<string> queue = crestronCommunicator.getCommandOutputQueue();

			while (!token.IsCancellationRequested) {

				if (queue.TryDequeue(out string result)) {
					Console.WriteLine("Pushed from output queue: "+result);
				}

			}

		}
	}
}