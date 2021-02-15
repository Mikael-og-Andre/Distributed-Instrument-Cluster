using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
//using System.Threading.Tasks;
//using LibVLCSharp.Shared;
//using AForge.Video.DirectShow;
using OpenCvSharp;

namespace Video_Library {
	class Test {
		public Test() {

		}

		static void Main(string[] args) {
			var videoDevice = new VideoDeviceInterface(1);

			Mat frame = new Mat();
			while (true) {
				if(videoDevice.tryReadFrameBuffer(out frame)) {
					Cv2.ImShow("test", frame);
				}
				Cv2.WaitKey(1);
			}


			//test.aForge();
			//test.openCV();
			//test.openCV2();
			//test.openCV3();
			//test.openCV4();
		}

		public void openCV4() {
			//var capture = new VideoCapture(3);
			var capture = new VideoCapture(1, VideoCaptureAPIs.DSHOW);

			capture.Set(VideoCaptureProperties.FrameWidth, 1280);
			capture.Set(VideoCaptureProperties.FrameHeight, 720);

			var frame = new Mat();
			while (true) {
				capture.Read(frame);

				Cv2.ImShow("ok", frame);
				Cv2.WaitKey(1);

			}

		}

		public void openCV3() {
			var capture = new VideoCapture(1);

			Console.WriteLine(capture.GetBackendName());

			capture.Open(0);
			using (var normalWindow = new Window("normal"))
			using (var image = new Mat()) {
				while (true) {
					capture.Read(image);
					normalWindow.ShowImage(image);
				}
			}
		}

		public void openCV2() {
			var capture = new VideoCapture(0);
			//capture.Open(0, VideoCaptureAPIs.ANY);
			using (var normalWindow = new Window("normal"))
			using (var image = new Mat()) {
				while (true) {
					/* start camera */
					capture.Read(image);
					if (!image.Empty()) {
						break;
					}
					Console.WriteLine("no frame");
				}
			}
			using (var normalWindow = new Window("normal"))
			using (var image = new Mat()) {
				double counter = 0;
				double seconds = 0;
				var watch = Stopwatch.StartNew();
				while (true) {
					capture.Read(image);
					if (image.Empty()) {
						break;
					}

					normalWindow.ShowImage(image);

					counter++;
					seconds = watch.ElapsedMilliseconds / (double)1000;
					if (seconds >= 3) {
						watch.Stop();
						break;
					}
				}
				var fps = counter / seconds;
				Console.WriteLine(fps);
			}
		}

		public void openCV() {
			var capture = new VideoCapture();
			//capture.Set(VideoCaptureProperties.FrameWidth, 640);
			//capture.Set(VideoCaptureProperties.FrameHeight, 480);
			capture.Open(2);
			if (!capture.IsOpened())
				throw new Exception("capture initialization failed");

			var fs = FrameSource.CreateFrameSource_Camera(2);

			using (var normalWindow = new Window("normal")) {
				var normalFrame = new Mat();
				while (true) {
					Console.WriteLine("ok");
					capture.Read(normalFrame);
					if (normalFrame.Empty()) {
						Console.WriteLine("ffs");
						break;
					}
					//normalWindow.ShowImage(normalFrame);
					//srWindow.ShowImage(srFrame);

					Cv2.ImShow("ok", normalFrame);
					Cv2.WaitKey(100);
				}
			}
		}


		//public void aForge() {
		//	// enumerate video devices
		//	var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
		//	foreach (FilterInfo ffs in videoDevices) {
		//		Console.WriteLine(ffs.Name);
		//	}

		//	VideoCaptureDevice captureDevice = new VideoCaptureDevice(videoDevices[0].MonikerString);

		//	captureDevice.Start();
		//	Thread.Sleep(5000);
		//	captureDevice.Stop();

		//}


		/*
		public static async void vlc() {

			using var libvlc = new LibVLC();
			using var mediaPlayer = new MediaPlayer(libvlc);
			using var media = new Media(libvlc, "dshow://", FromType.FromLocation);

			mediaPlayer.Play(media);

			await Task.Delay(5000);

			mediaPlayer.Stop();


			throw new NotImplementedException();
		}
		*/
	}
}
