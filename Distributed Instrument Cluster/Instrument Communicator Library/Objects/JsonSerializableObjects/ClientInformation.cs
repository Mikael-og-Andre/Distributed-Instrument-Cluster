namespace Server_Library {
	/// <summary>
	/// Class for storing and loading information about the Instrument Device
	/// <author>Mikael Nilssen</author>
	/// </summary>

	public class ClientInformation {

		/// <summary>
		/// Designated name of client device
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Location of the device
		/// </summary>
		public string Location { get; set; }

		/// <summary>
		/// Type of device, e.g GPS
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// The name of the specific connection socket this was sent on, Used to distinguish 
		/// </summary>
		public string SubName {get;set; }

		/// <summary>
		/// Empty constructor
		/// </summary>
		public ClientInformation() {
			
		}

		/// <summary>
		/// Constructor with input
		/// </summary>
		/// <param name="name"></param>
		/// <param name="location"></param>
		/// <param name="type"></param>
		public ClientInformation(string name, string location, string type, string subName) {
			this.Name = name;
			this.Location = location;
			this.Type = type;
			this.SubName = subName;
		}
	}
}