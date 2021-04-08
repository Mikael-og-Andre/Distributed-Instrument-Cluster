using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server.RemoteDevice {
	public class SubDevice {


		public bool videoDevice { get; set; }
		public string subname { get; set; }
		public int port { get; set; }
		public string streamType { get; set; }

		public SubDevice(bool videoDevice, string subname, int port, string streamType) {
			this.videoDevice = videoDevice;
			this.subname = subname;
			this.port = port;
			this.streamType = streamType;
		}

	}
}
