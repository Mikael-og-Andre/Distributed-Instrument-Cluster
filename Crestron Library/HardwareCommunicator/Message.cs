using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Represents a message to put in the queue concurrent queue when sending to the client
/// <author>Mikael Nilssen</author>
/// </summary>

namespace InstrumentCommunicator {
    public class Message {

        private protocolOption option;  //protocol option enum that tells server what protocol to use when sending
        private string messageString;     //string that will be sent to server
        private string[] messageStringArray;  //Array of strings that will be sent to server
        public bool multiMessage;

        public Message(protocolOption option, string messageString) {
            this.option = option;
            this.messageString = messageString;
            multiMessage = false;
        }

        public Message(protocolOption option, string[] messageStringArray) {
            this.option = option;
            this.messageStringArray = messageStringArray;
            multiMessage = true;
        }

    }
}
