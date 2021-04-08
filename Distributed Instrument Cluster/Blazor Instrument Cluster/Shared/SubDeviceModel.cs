using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Shared {
	public class SubDeviceModel {

		public bool isVideoDevice { get; set; }
        public string subname { get; set; }
        public int port { get; set; }
        public string streamType { get; set; }

        public SubDeviceModel() {
            
        }

        public SubDeviceModel(bool isVideoDevice, string subname, int port, string streamType) {
	        this.isVideoDevice = isVideoDevice;
	        this.subname = subname;
	        this.port = port;
	        this.streamType = streamType;
        }
    }
}
