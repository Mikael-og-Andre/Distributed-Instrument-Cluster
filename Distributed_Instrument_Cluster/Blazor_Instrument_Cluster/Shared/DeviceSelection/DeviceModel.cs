using System.Collections.Generic;

namespace Blazor_Instrument_Cluster.Shared.DeviceSelection {

	/// <summary>
	/// Represents a device that can be connected to
	/// <author>Mikael Nilssen</author>
	/// </summary>
    public class DeviceModel {

		public string name { get; set; }
		public string location { get; set; }
		public string type { get; set; }

		public List<SubConnectionModel> subDevice { get; set;}

		public DeviceModel() {
	        
        }
		public DeviceModel(string name, string location, string type, List<SubConnectionModel> subDeviceList) {
			this.name = name;
			this.location = location;
			this.type = type;
            this.subDevice = subDeviceList;
        }
		
    }
}
