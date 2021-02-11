/// <summary>
/// Class for storing and loading information about the client
/// <author>Mikael Nilssen</author>
/// </summary>

namespace InstrumentCommunicator {

    public class InstrumentInformation {
        private string name;    //Designated name of client device
        private string location;    //Location of the device
        private string type;    //Type of device, e.g GPS

        public InstrumentInformation(string name, string location, string type) {
            this.name = name;
            this.location = location;
            this.type = type;
        }

    }
}