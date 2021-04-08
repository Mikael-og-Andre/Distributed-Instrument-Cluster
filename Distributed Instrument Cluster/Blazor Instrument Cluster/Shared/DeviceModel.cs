using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Shared {

	/// <summary>
	/// Represents a device that can be connected to
	/// <author>Mikael Nilssen</author>
	/// </summary>
    public class DeviceModel {

		public string name { get; set; }
		public string location { get; set; }
		public string type { get; set; }

        public List<SubDeviceModel> subDevice;

		public DeviceModel(string name, string location, string type, List<SubDeviceModel> subDeviceList) {
			this.name = name;
			this.location = location;
			this.type = type;
            this.subDevice = subDeviceList;
        }
		
    }
}
