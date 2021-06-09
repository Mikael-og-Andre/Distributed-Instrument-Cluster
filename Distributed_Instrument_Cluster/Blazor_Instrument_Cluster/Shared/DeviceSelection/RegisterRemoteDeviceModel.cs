using System;
using System.ComponentModel.DataAnnotations;

namespace Blazor_Instrument_Cluster.Shared.DeviceSelection {

	public class RegisterRemoteDeviceModel {

		[Required]
		public string ip { get; set; }

		[Required]
		[Range(1024, 65535, ErrorMessage = "Must be in range 1024-65535")]
		public int videoBasePort { get; set; }

		[Required] public int videoDeviceNumber { get; set; } = 1;

		[Required]
		[Range(typeof(bool), "false", "true")]
		public bool hasCrestron { get; set; } = false;

		[Range(1024, 65535, ErrorMessage = "Must be in range 1024-65535")]
		public int crestronPort { get; set; }

		[Required]
		public string name { get; set; }

		[Required]
		public string location { get; set; }

		[Required]
		public string type { get; set; }

		public RegisterRemoteDeviceModel() {
			
		}
	}
}