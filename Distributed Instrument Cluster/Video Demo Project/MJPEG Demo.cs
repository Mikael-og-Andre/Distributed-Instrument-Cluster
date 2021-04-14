using OpenCvSharp;
using Video_Library;

namespace Video_Demo {
	class MJPEG_Demo {

		public static void Main(string[] args) {
			var device = new VideoDeviceInterface(0, (VideoCaptureAPIs) 700, 1920, 1080);
			var streamer = new MJPEG_Streamer(30, 8080);

			while (true) {
				streamer.Image = device.readJpg(40);
			}
		}
	}
}
