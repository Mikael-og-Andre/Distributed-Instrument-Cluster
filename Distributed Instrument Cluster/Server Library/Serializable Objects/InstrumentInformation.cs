using Networking_Library;

namespace Server_Library {
	/// <summary>
	/// Class for storing and loading information about the Instrument Device
	/// <author>Mikael Nilssen</author>
	/// </summary>

	public class InstrumentInformation {

		/// <summary>
		/// Designated name of client device
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Location of the device
		/// </summary>
		public string Location { get; private set; }

		/// <summary>
		/// Type of device, e.g GPS
		/// </summary>
		public string Type { get; private set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name"></param>
		/// <param name="location"></param>
		/// <param name="type"></param>
		public InstrumentInformation(string name, string location, string type) {
			this.Name = name;
			this.Location = location;
			this.Type = type;
		}
		
	}
}