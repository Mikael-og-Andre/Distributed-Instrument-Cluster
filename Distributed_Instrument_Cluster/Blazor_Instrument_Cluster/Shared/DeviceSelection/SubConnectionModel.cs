using System;
using System.ComponentModel;

namespace Blazor_Instrument_Cluster.Shared.DeviceSelection {
	/// <summary>
	/// <author>Mikael Nilssen</author>
	/// </summary>
	
	public class SubConnectionModel {

		public Guid guid { get; set; }
		public bool isVideoDevice { get; set; }
		public int port { get; set; }
        public string streamType { get; set; }

        public SubConnectionModel() {
            
        }

        public SubConnectionModel(Guid guid,bool isVideoDevice, int port, string streamType) {
	        this.guid = guid;
	        this.isVideoDevice = isVideoDevice;
	        this.port = port;
	        this.streamType = streamType;
        }
    }
}
