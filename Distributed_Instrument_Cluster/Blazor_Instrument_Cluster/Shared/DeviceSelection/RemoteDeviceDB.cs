namespace Blazor_Instrument_Cluster.Shared.DeviceSelection {

	public class RemoteDeviceDB {
		public int RemoteDeviceDBID { get; set; }

		public string ip { get; set; }

		public int videoBasePort { get; set; }
		public int videoDeviceNumber { get; set; }

		public bool hasCrestron { get; set; }

		public int crestronPort { get; set; }

		public string name { get; set; }

		public string location { get; set; }

		public string type { get; set; }
	}
}