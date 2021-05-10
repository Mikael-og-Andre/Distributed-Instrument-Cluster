using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using Video_Library;
using OpenCvSharp;

namespace Video_Demo {
	class MJPEG_Demo {

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
