using PackageClasses;
using Server_Library.Socket_Clients;
using Video_Library;

namespace MAIN_Program {

	/// <summary>
	/// Object for storing video capture device info and socket connection info.
	/// </summary>
	internal class VideoConnection {
		public VideoDeviceInterface device { get;}
		public SendingClient<Jpeg> connection { get;}

		public VideoConnection(VideoDeviceInterface device, SendingClient<Jpeg> connection) {
			this.device = device;
			this.connection = connection;
		}
	}
}
