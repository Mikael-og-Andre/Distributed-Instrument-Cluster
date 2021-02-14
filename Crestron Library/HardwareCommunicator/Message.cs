/// <summary>
/// Represents a message to put in the queue concurrent queue when sending to the client
/// <author>Mikael Nilssen</author>
/// </summary>

namespace Instrument_Communicator_Library {

    public class Message {
        private protocolOption option;  //protocol option enum that tells server what protocol to use when sending
        private string[] messageStringArray;  //Array of strings that will be sent to server

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="option">protocol option for the message</param>
        /// <param name="messageStringArray">array of strings that will be sent to the client</param>
        public Message(protocolOption option, string[] messageStringArray) {
            this.option = option;
            this.messageStringArray = messageStringArray;
        }

        /// <summary>
        /// Returns array of strings, throws exception if not meant to be a multiMessage object
        /// </summary>
        /// <returns>String array with messages</returns>
        public string[] getMessageArray() {
            return this.messageStringArray;
        }

        /// <summary>
        /// Get the protocol intended for the message
        /// </summary>
        /// <returns></returns>
        public protocolOption getProtocol() {
            return this.option;
        }
    }
}