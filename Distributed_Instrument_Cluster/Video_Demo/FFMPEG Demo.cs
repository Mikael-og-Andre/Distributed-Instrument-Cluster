using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Video_Library;

namespace Video_Demo {
	class FFMPEG_Demo {

		public static void Main(string[] args) {
			var ffmpeg = new FFMPEG_Streamer();

			ffmpeg.launchArgs =
				"-re -f lavfi -i aevalsrc=\"sin(400*2*PI*t)\" -ar 8000 -f mulaw -f rtp rtp://127.0.0.1:1234";
			Console.WriteLine(ffmpeg.launchArgs);
			ffmpeg.start();
		}

	}
}
