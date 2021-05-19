using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server.RemoteDeviceManagement.JsonReading {
	/// <summary>
	/// Class for storing multiple  devices
	/// <author>Mikael Nilssen</author> 
	/// </summary>
	public class RemoteDeviceList {
		public List<jsonDevice> RemoteDevices { get; set; }
	}
}
