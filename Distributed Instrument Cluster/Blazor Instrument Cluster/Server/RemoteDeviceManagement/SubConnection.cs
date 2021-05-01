using System;
using Server_Library.Connection_Types.Async;

namespace Blazor_Instrument_Cluster.Server.RemoteDeviceManagement {
	/// <summary>
	/// A connection from the remote device
	/// <Author>Mikael Nilssen</Author>
	/// </summary>
	public class SubConnection {

		public Guid id { get; set; }
		public bool videoDevice { get; set; }
		public int port { get; set; }
		public string streamType { get; set; }
		public ConnectionBaseAsync connection { get; private set; }

		public SubConnection(ConnectionBaseAsync connection,bool videoDevice = false, int port = 0, string streamType = "none") {
			id = Guid.NewGuid();
			this.connection = connection;
			this.videoDevice = videoDevice;
			this.port = port;
			this.streamType = streamType;
		}

	}
}
