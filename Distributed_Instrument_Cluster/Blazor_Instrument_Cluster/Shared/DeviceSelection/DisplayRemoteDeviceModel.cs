using System.Collections.Generic;

namespace Blazor_Instrument_Cluster.Shared.DeviceSelection {

	/// <summary>
	/// Represents a device that can be connected to
	/// <author>Mikael Nilssen</author>
	/// </summary>
    public class DisplayRemoteDeviceModel {

		public string name { get; set; }
		public string location { get; set; }
		public string type { get; set; }
		public bool hasCrestron { get; set; }
		public bool pingResult { get; set; }

		public DisplayRemoteDeviceModel() {
	        
        }
		public DisplayRemoteDeviceModel(string name, string location, string type, bool hasCrestron,bool pingResult) {
			this.name = name;
			this.location = location;
			this.type = type;
			this.hasCrestron = hasCrestron;
			this.pingResult = pingResult;
		}
		
    }
}
