using Video_Library;
using OpenCvSharp;

namespace Video_Demo {
	/// <summary>
	/// Demo for VideoLibrary.
	/// </summary>
	/// <author>Andre Helland</author>
	class VideoDeviceInterfaceDemo {
		private static readonly int deviceIndex = 0;

		static void Main(string[] args) {
			_ = new VideoDeviceInterfaceDemo();
		}

		private VideoDeviceInterfaceDemo() {
			var videoDevice = new VideoDeviceInterface(deviceIndex);

			while (true) {
				if (videoDevice.tryReadFrameBuffer(out var frame)) {
					Cv2.ImShow("Video Device Interface Demo", frame);
					Cv2.WaitKey(1);
				}
			}
		}
	}
}
