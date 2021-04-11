using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using Video_Library;

namespace Video_Demo {
	class MJPEG_Demo {

		public static void Main(string[] args) {
			var device = new VideoDeviceInterface(0, (VideoCaptureAPIs) 700, 1920, 1080);
			var streamer = new MJPEG_Streamer(30, 8080);

			while (true) {
				streamer.image = device.readJpg(40);
			}
		}
	}
}
