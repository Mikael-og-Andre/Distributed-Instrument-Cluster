using Instrument_Communicator_Library.Server_Listener;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Instrument_Communicator_Library;
using Instrument_Communicator_Library.Enums;

namespace Testing_Server {

	internal class Program {

		private static void Main(string[] args) {
			Console.WriteLine("Setting up Crestron");

			IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5051);

			ListenerCrestron listenerCrestron = new ListenerCrestron(ipEndPoint);

			//Start thread
			Thread thread = new Thread(() => listenerCrestron.start());
			thread.Start();
			Thread.Sleep(1000);
			CrestronConnection connection;
			string name = "Radar1";
			Console.WriteLine("Searching for connection with name: "+name);
			while (!listenerCrestron.getCrestronConnectionWithName(out connection, name)) {
				Thread.Sleep(100);
			}
			Console.WriteLine("Found device");
			//Get queue
			ConcurrentQueue<Message> queue = connection.getSendingQueue();

			//loop for input
			while (true) {
				string input = Console.ReadLine();
				Message message = new Message(ProtocolOption.message,input);
				Console.WriteLine("Queueing message: "+input);
				queue.Enqueue(message);
			}
		}
	}
}