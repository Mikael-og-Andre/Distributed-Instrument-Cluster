using System;
using System.Collections.Concurrent;
using Instrument_Communicator_Library;
using Instrument_Communicator_Library.Remote_Device_side_Communicators;
using System.Threading;
using Instrument_Communicator_Library.Authorization;
using Instrument_Communicator_Library.Interface;

namespace HardwareServer_Demo_Project {

	internal class VideoCommunicatorTestingClass {

		public static void Main(string[] args) {
			Thread.Sleep(10000);
			//Communicator
			InstrumentInformation info = new InstrumentInformation("Radar1", "loc", "type");
			AccessToken accessToken = new AccessToken("access");
			CancellationToken comCancellationToken = new CancellationToken(false);

			VideoCommunicator vidCom = new VideoCommunicator("127.0.0.1", 5051, info, accessToken, comCancellationToken);
			Thread vidComThread = new Thread(() => vidCom.Start());
			vidComThread.Start();

			ConcurrentQueue<VideoFrame> inputQueue = vidCom.GetInputQueue();

			while (true) {
				Console.WriteLine("Fix with img");
			}

		}
	}
}