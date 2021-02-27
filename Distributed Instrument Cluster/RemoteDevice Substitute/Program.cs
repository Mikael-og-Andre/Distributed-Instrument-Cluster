using System;
using System.Collections.Concurrent;
using Instrument_Communicator_Library;
using System.Net;
using System.Threading;

namespace RemoteDevice_Substitute {
    class Program {
        static void Main(string[] args) {

			Thread.Sleep(100000);

            Console.WriteLine("Starting communicator");

            CancellationToken token = new CancellationToken(false);
            VideoCommunicator<string> videoCommunicator = new VideoCommunicator<string>("127.0.0.1",44323,new InstrumentInformation("TestName","TestLocation","TestType"),new AccessToken("access"),token);

            ConcurrentQueue<string> videoConcurrentQueue = videoCommunicator.GetInputQueue();

            Thread videoThread = new Thread(() => videoCommunicator.Start());
			videoThread.Start();
			int i = 0;
			while (!token.IsCancellationRequested) {
				string sendingString = "Current i is " + i;
				videoConcurrentQueue.Enqueue(sendingString);
				i++;
				Thread.Sleep(100);
			}

        }
    }
}
