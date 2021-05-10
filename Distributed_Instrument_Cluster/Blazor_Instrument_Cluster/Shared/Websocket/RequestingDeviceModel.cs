using System;

namespace Blazor_Instrument_Cluster.Shared.Websocket {
	/// <summary>
	/// Class used for serializing and sending when requesting a connection from the server
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class RequestingDeviceModel {
		/// <summary>
		/// Name value of the remote device
		/// </summary>
		public string name { get; set; }
		/// <summary>
		/// Location value of the remote device
		/// </summary>
		public string location { get; set; }
		/// <summary>
		/// Type value of the remote device
		/// </summary>
		public string type { get; set; }

		/// <summary>
		/// Constructor for json
		/// </summary>
		public RequestingDeviceModel() {
			
		}

		/// <summary>
		/// Constructor with input
		/// </summary>
		/// <param name="name"></param>
		/// <param name="location"></param>
		/// <param name="type"></param>
		public RequestingDeviceModel(string name, string location, string type) {
			this.name = name;
			this.location = location;
			this.type = type;
		}
	}
}
