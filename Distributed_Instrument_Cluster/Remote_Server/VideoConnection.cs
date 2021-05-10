using Packet_Classes;
using Server_Library.Socket_Clients;
using Video_Library;

namespace MAIN_Program {

	/// <summary>
	/// Object for storing video capture device info and socket connection info.
	/// </summary>
	internal class VideoConnection {
		public VideoDeviceInterface device { get;}
		public int quality { get; }
		public int fps { get; }

		public VideoConnection(VideoDeviceInterface device, int quality=50, int fps=30) {
			this.device = device;
			this.quality = quality;
			this.fps = fps;
		}
	}
}
