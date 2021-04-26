using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Shared {
	public class SubDeviceModel {

		public Guid guid { get; set; }
		public bool isVideoDevice { get; set; }
		public int port { get; set; }
        public string streamType { get; set; }

        public SubDeviceModel() {
            
        }

        public SubDeviceModel(Guid guid,bool isVideoDevice, int port, string streamType) {
	        this.guid = guid;
	        this.isVideoDevice = isVideoDevice;
	        this.port = port;
	        this.streamType = streamType;
        }
    }
}
