using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using Video_Library;

namespace Video_Demo {
	class test {

		public static void Main(string[] args) {

			var device = new VideoDeviceInterface(1);

			Console.WriteLine("ffs");

			//var ffs = VideoWriter.FourCC('M', 'J', 'P', 'G');
			var ffs = VideoWriter.FourCC('H', '2', '6', '4');


			//var writer = new VideoWriter("http://192.168.1.164:6969/test", ffs, 30.0, new Size(1280, 720));

			var writer = new VideoWriter("test.mp4", ffs, 60.0, new Size(1920, 1080));
			int i = 0;
			while (i<1000) {

				//Console.WriteLine("ffs");
				if (device.tryReadFrameBuffer(out Mat frame)) {
					i++;
					writer.Write(frame);
				}

			}
			writer.Release();
			Console.WriteLine("done");


		}

	}
}
