
namespace Instrument_Communicator_Library {
	/// <summary>
	/// Class for storing and loading information about the Instrument Device
	/// <author>Mikael Nilssen</author>
	/// </summary>

	public class InstrumentInformation {
        
        public int id { get; private set; }         //id
        public string name { get; private set; }    //Designated name of client device
        public string location { get; private set; }    //Location of the device
        public string type { get; private set; }   //Type of device, e.g GPS

        public InstrumentInformation(string name, string location, string type) {
            this.name = name;
            this.location = location;
            this.type = type;
        }

        /// <summary>
        /// Check if the information in the object is the same as in this object
        /// </summary>
        /// <param name="info"> Instrument Information object u want to check</param>
        /// <returns>boolean, true if matching</returns>
        public bool Equals(InstrumentInformation info) {
            //Check if fields match
            if (
                (this.name.Equals(info.name))
                && (this.location.Equals(info.location))
                && (this.type.Equals(info.type))
                && (this.id.Equals(info.id))
                ) {
                return true;
            } else {
                return false;
            }

        }
    }
}