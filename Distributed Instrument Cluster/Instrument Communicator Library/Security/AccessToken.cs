

namespace Instrument_Communicator_Library {
	/// <summary>
	/// Class that represents a authorization token for the server
	/// <author>Mikael Nilssen</author>
	/// </summary>
    public class AccessToken {

        /// <summary>
		/// Intended for use when connecting to remote server. Generate on website and put manually in app settings
		/// </summary>
        private string connectionHash { get; set; }

        public AccessToken(string connectionHash) {
            this.connectionHash = connectionHash;
        }
		/// <summary>
		/// Returns the string used for authentication
		/// </summary>
		/// <returns></returns>
        public string getAccessString() {
            return connectionHash;
        }
    }
}