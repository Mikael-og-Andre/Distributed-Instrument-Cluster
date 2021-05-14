using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server.RemoteDeviceManagement.JsonReading {
	public class jsonDevice {
		public string ip { get; set; }
		public string name { get; set; }
		public string location { get; set; }
		public string type { get; set; }
		public int VideoDevices { get; set; }
		public int VideoBasePort { get; set; }
		public bool hasCrestron { get; set; }
		public int CrestronBasePort { get; set; }
	}
}
