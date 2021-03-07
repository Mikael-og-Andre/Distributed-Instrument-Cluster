using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Shared {

	/// <summary>
	/// Represents a device that can be connected to
	/// </summary>
    public class DeviceModel {

		public string name { get; set; }
		public string location { get; set; }
		public string type { get; set; }

		public bool hasCrestron { get; set; }

		public DeviceModel(string name, string location, string type, bool hasCrestron) {
			this.name = name;
			this.location = location;
			this.type = type;
			this.hasCrestron = hasCrestron;	
		}

    }
}
