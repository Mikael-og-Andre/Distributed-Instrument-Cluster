using System.Collections.Generic;

namespace Blazor_Instrument_Cluster.Shared.DeviceSelection {

	/// <summary>
	/// Represents a device that can be connected to
	/// <author>Mikael Nilssen</author>
	/// </summary>
    public class DisplayRemoteDeviceModel {

		public string ip { get; set; }
		public string name { get; set; }
		public string location { get; set; }
		public string type { get; set; }

		public List<int> videoPorts { get; set; }
		public bool hasCrestron { get; set; }
		public bool pingResult { get; set; }

		public DisplayRemoteDeviceModel() {
	        
        }
		public DisplayRemoteDeviceModel(string ip,string name, string location, string type,List<int> videoPorts ,bool hasCrestron,bool pingResult) {
			this.ip = ip;
			this.name = name;
			this.location = location;
			this.type = type;
			this.videoPorts = videoPorts;
			this.hasCrestron = hasCrestron;
			this.pingResult = pingResult;
		}
		
    }
}
