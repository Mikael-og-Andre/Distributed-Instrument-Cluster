using OpenCvSharp;
using System;
using System.Text;

//TODO: Handle Unplugging w/out crashing.
namespace Video_Library {

	/// <summary>
	/// Class for interfacing with video capture devices (usb webcam, capture card, etc).
	/// </summary>
	/// <author>Andre Helland</author>
	public class VideoDeviceInterface: IDisposable {
		private readonly VideoCapture capture;

		//DSHOW: Windows api for video devices.
		public VideoDeviceInterface(int index = 0, VideoCaptureAPIs API = VideoCaptureAPIs.DSHOW, int frameWidth=1280, int frameHeight=720) {
			capture = new VideoCapture(index, API);
			capture.Set(VideoCaptureProperties.FrameWidth, frameWidth);
			capture.Set(VideoCaptureProperties.FrameHeight, frameHeight);
		}

		/// <summary>
		/// Method for accessing frames from video device.
		/// </summary>
		/// <returns>Frame from video device (in OpenCV's "Mat" data type).</returns>
		public Mat readFrame() {
			var frame = new Mat();
			lock (capture) {
				capture.Read(frame);
			}
			return frame;
		}

		/// <summary>
		/// Read frame from video device and encode it as a jpg and chose image quality.
		/// </summary>
		/// <param name="quality">Image quality 0 to 100, higher is better(more data).</param>
		/// <returns></returns>
		public byte[] readJpg(int quality=95) {
			var frame = readFrame();
			Cv2.ImEncode(".jpg", frame, out var buffer, new ImageEncodingParam(ImwriteFlags.JpegQuality, quality));
			//buffer = Encoding.Convert(Encoding.UTF7, Encoding.UTF7, buffer);
			return buffer;
		}

		/// <summary>
		/// Stops capturing frames and release capture device resources.
		/// </summary>
		public void Dispose() {
			lock (capture) {
				capture.Release();
			}
			GC.SuppressFinalize(this);
		}

	}
}
