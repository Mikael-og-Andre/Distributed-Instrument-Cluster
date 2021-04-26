using PackageClasses;
using Server_Library.Connection_Types.Async;
using Server_Library.Socket_Clients;
using Server_Library.Socket_Clients.Async;
using Video_Library;

namespace MAIN_Program {

	/// <summary>
	/// Object for storing video capture device info and socket connection info.
	/// </summary>
	internal class VideoConnection {
		public VideoDeviceInterface device { get;}
		public DuplexClientAsync connection { get;}
		public int quality { get; }
		public int fps { get; }

		public VideoConnection(VideoDeviceInterface device, DuplexClientAsync connection, int quality=50, int fps=30) {
			this.device = device;
			this.connection = connection;
			this.quality = quality;
			this.fps = fps;
		}
	}
}
