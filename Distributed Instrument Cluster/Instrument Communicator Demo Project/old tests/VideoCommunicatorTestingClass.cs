﻿using System;
using System.Collections.Concurrent;
using Server_Library;
using System.Threading;
using Server_Library.Authorization;
using Server_Library.Socket_Clients;

namespace HardwareServer_Demo_Project {

	internal class VideoCommunicatorTestingClass {

		public static void Main(string[] args) {
			Thread.Sleep(10000);
			//Communicator
			ClientInformation info = new ClientInformation("Radar1", "loc", "type");
			AccessToken accessToken = new AccessToken("access");
			CancellationToken comCancellationToken = new CancellationToken(false);

			VideoClient vidCom = new VideoClient("127.0.0.1", 5051, info, accessToken, comCancellationToken);
			Thread vidComThread = new Thread(() => vidCom.run());
			vidComThread.Start();

			ConcurrentQueue<VideoFrame> inputQueue = vidCom.getInputQueue();

			while (true) {
				Console.WriteLine("Fix with img");
			}

		}
	}
}