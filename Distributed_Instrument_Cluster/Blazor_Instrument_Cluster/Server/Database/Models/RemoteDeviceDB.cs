using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server.Database.Models {
	public class RemoteDeviceDB {

		public int RemoteDeviceDBID { get; set; }

		public string name { get; set; }

		public string location { get; set; }

		public string type { get; set; }

		public string ipAddress { get; set; }


	}
}
