
namespace Instrument_Communicator_Library {
	/// <summary>
	/// Class for storing and loading information about the Instrument Device
	/// <author>Mikael Nilssen</author>
	/// </summary>

	public class InstrumentInformation {
        
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

        public InstrumentInformation(string name, string location, string type) {
            this.Name = name;
            this.Location = location;
            this.Type = type;
        }

        /// <summary>
        /// Check if the information in the object is the same as in this object
        /// </summary>
        /// <param name="info"> Instrument Information object u want to check</param>
        /// <returns>boolean, true if matching</returns>
        public bool Equals(InstrumentInformation info) {
            //Check if fields match
            if (
                (this.Name.Equals(info.Name))
                && (this.Location.Equals(info.Location))
                && (this.Type.Equals(info.Type))
                && (this.Id.Equals(info.Id))
                ) {
                return true;
            } else {
                return false;
            }

        }
    }
}