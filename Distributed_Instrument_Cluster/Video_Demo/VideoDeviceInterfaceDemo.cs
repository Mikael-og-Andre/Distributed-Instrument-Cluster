using OpenCvSharp;
using Video_Library;

namespace Video_Demo {

	/// <summary>
	/// Demo for VideoLibrary.
	/// </summary>
	/// <author>Andre Helland</author>
	internal class VideoDeviceInterfaceDemo {
		private static readonly int deviceIndex = 0;

		private static void Main(string[] args) {
			_ = new VideoDeviceInterfaceDemo();
		}

		private VideoDeviceInterfaceDemo() {
			var videoDevice = new VideoDeviceInterface(deviceIndex);

			while (true) {
				var frame = videoDevice.readFrame();
				Cv2.ImShow("Video Device Interface Demo", frame);
				Cv2.WaitKey(1);
			}
		}
	}
}