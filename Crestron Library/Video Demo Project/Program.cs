using System;
using Video_Library;
using OpenCvSharp;

namespace Video_Demo_Project {
	class Program {
		static void Main(string[] args) {
			var videoDevice = new VideoDeviceInterface(0);

			Mat frame = new Mat();
			while (true) {
				if (videoDevice.tryReadFrameBuffer(out frame)) {
					Cv2.ImShow("test", frame);
					Cv2.WaitKey(1);
				}
			}
		}
	}
}
