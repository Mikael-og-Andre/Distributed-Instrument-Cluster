using Networking_Library;

namespace Instrument_Communicator_Library {
	/// <summary>
	/// Class for storing and loading information about the Instrument Device
	/// <author>Mikael Nilssen</author>
	/// </summary>

	public class InstrumentInformation : ISerializeObject {

		/// <summary>
		/// id
		/// </summary>
		private int Id { get; set; }

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
		/// <param name="id"></param>
		/// <param name="name"></param>
		/// <param name="location"></param>
		/// <param name="type"></param>
		public InstrumentInformation(int id, string name, string location, string type) {
			this.Id = id;
			this.Name = name;
			this.Location = location;
			this.Type = type;
		}

		/// <summary>
		/// Get the bytes representing the object
		/// </summary>
		/// <returns></returns>
		public byte[] getBytes() {
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Get the object from a byte array
		/// </summary>
		/// <param name="arrayBytes"></param>
		/// <returns> object</returns>
		public object getObject(byte[] arrayBytes) {
			throw new System.NotImplementedException();
		}
	}
}