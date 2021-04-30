using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Video_Library {

	/// <summary>
	/// Class for interacting with ffmpeg process/exe.
	/// </summary>
	/// <author>Andre Helland</author>
	public class FFMPEG_Streamer {

		/// <summary>
		/// Launch arguments passed into ffmpeg process when running start method.
		/// </summary>
		public string launchArgs {
			get => startInfo.Arguments;
			set => startInfo.Arguments = value;
		}

		private ProcessStartInfo startInfo = new();

		public FFMPEG_Streamer() {
			startInfo.FileName = "ffmpeg-4.3.2\\bin\\ffmpeg.exe";
		}

		public void start() {
			using (var process = Process.Start(startInfo)) {
				process.WaitForExit();
			}
		}



	}
}
