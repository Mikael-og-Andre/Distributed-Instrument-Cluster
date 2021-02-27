using Instrument_Communicator_Library.Server_Listener;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Instrument_Communicator_Library;

namespace Testing_Server {

	internal class Program {

		private static void Main(string[] args) {
			Console.WriteLine("Setting up Crestron");

			IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5051);

			ListenerCrestron listenerCrestron = new ListenerCrestron(ipEndPoint);

			//Start thread
			Thread thread = new Thread(() => listenerCrestron.Start());
			thread.Start();
			Thread.Sleep(1000);
			CrestronConnection connection;
			string name = "crestron";
			Console.WriteLine("Searching for connection with name: "+name);
			while (!listenerCrestron.getCrestronConnectionWithName(out connection, name)) {
				Thread.Sleep(100);
			}
			Console.WriteLine("Found device");
			//Get queue
			ConcurrentQueue<Message> queue = connection.GetInputQueue();

			//loop for input
			while (true) {
				string input = Console.ReadLine();
				string[] strings = new string[] { input};
				Message message = new Message(protocolOption.message,strings);
				Console.WriteLine("Queueing message: "+input);
				queue.Enqueue(message);
			}


		}
	}
}