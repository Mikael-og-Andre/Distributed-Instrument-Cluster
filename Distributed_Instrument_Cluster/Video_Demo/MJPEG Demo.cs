using OpenCvSharp;
using System;
using System.Threading;
using Video_Library;

namespace Video_Demo {

	internal class MJPEG_Demo {

		public static void Main(string[] args) {
			var thread = new Thread(startServer);
			thread.Start();
			Console.WriteLine("Goto: http://localhost:8080/");
		}

		private static void startServer() {
			var device = new VideoDeviceInterface(0, (VideoCaptureAPIs)700, 1920, 1080);
			var streamer = new MJPEG_Streamer(8080);

			while (true) {
				streamer.Image = device.readJpg(10);
			}
		}
	}
}