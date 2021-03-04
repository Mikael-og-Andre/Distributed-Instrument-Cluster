using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Threading;

//TODO: Handle Unplugging w/out crashing.
//TODO: Make device health method (method for checking if device is working properly/producing frames).
namespace Video_Library {

	/// <summary>
	/// Class for interfacing with video capture devices (usb webcam, capture card, etc).
	/// Class maintains a frame buffer where frames can be read from concurrently.
	/// </summary>
	/// <author>Andre Helland</author>
	public class VideoDeviceInterface: IDisposable {
		private VideoCapture capture;
		private readonly ConcurrentQueue<Mat> frameBuffer;
		private bool captureFrames;

		//DSHOW: Windows api for video devices.
		//TODO: refactor (deprecate resolution input)
		public VideoDeviceInterface(int index = 0, VideoCaptureAPIs API = VideoCaptureAPIs.DSHOW, int frameWidth=1280, int frameHeight=720) {
			capture = new VideoCapture(index, API);
			capture.Set(VideoCaptureProperties.FrameWidth, frameWidth);
			capture.Set(VideoCaptureProperties.FrameHeight, frameHeight);
			//Console.WriteLine(capture.Get(3));


			frameBuffer = new ConcurrentQueue<Mat>();
			Thread thread = new Thread(FrameCaptureThread) { IsBackground = true };
			captureFrames = true;
			thread.Start();
		}

		/// <summary>
		/// Thread starting from class constructor to concurrently fill frame buffer with frames.
		/// </summary>
		private void FrameCaptureThread() {
			var frame = new Mat();
			while (captureFrames) {
				capture.Read(frame);
				frameBuffer.Enqueue(frame);
			}
			capture.Release();
		}

		/// <summary>
		/// Method for accessing frames in frame buffer concurrently.
		/// </summary>
		/// <param name="framePointer">Pointer for method to fill with frame data</param>
		/// <returns>If frame buffer contained new frames and was read successfully</returns>
		public bool tryReadFrameBuffer(out Mat framePointer) {
			return frameBuffer.TryDequeue(out framePointer);
		}

		public bool tryReadJpg(out byte[] buffer) {
			if (frameBuffer.TryDequeue(out Mat frame)) {
				Cv2.ImEncode(".jpg", frame, out buffer, new ImageEncodingParam(ImwriteFlags.JpegQuality, 80));

				//TODO remove
				//Cv2.ImShow("test", Cv2.ImDecode(buffer,ImreadModes.Color));
				//Cv2.WaitKey(1);


				return true;
			}
			buffer = new byte[] { };
			return false;
		}

		/// <summary>
		/// Stops capturing frames and release capture device resources.
		/// </summary>
		public void Dispose() {
			captureFrames = false;
			GC.SuppressFinalize(this);
		}

	}
}
