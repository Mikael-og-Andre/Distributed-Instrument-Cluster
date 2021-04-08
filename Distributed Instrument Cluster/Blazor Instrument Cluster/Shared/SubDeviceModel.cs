using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Shared {
	public class SubDeviceModel {

        public string subname { get; set; }
        public int port { get; set; }
        public string streamType { get; set; }

        public SubDeviceModel() {
            
        }

        public SubDeviceModel(string subname, int port, string streamType) {
	        this.subname = subname;
	        this.port = port;
	        this.streamType = streamType;
        }
    }
}
