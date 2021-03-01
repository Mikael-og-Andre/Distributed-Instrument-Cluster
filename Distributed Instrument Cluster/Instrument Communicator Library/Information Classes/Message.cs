

using System;
using System.Text;
using Instrument_Communicator_Library.Interface;

namespace Instrument_Communicator_Library {
	/// <summary>
	/// Represents a message to put in the queue concurrent queue when sending to the client
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class Message : ISerializeableObject {
        private protocolOption option;  //protocol option enum that tells server what protocol to use when sending
        private string messageString;	//String to be sent to the server

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="option">protocol option for the message</param>
        /// <param name="messageStringArray">array of strings that will be sent to the client</param>
        public Message(protocolOption option, string messageString) {
            this.option = option;
            this.messageString = messageString;
        }

        /// <summary>
        /// Returns array of strings, throws exception if not meant to be a multiMessage object
        /// </summary>
        /// <returns>String array with messages</returns>
        public string getMessage() {
            return this.messageString;
        }

        /// <summary>
        /// Get the protocol intended for the message
        /// </summary>
        /// <returns></returns>
        public protocolOption getProtocol() {
            return this.option;
        }

        public byte[] getBytes() {
	        byte[] completeBytes = new byte[2048];
	        byte[] optionBytes = Encoding.ASCII.GetBytes(this.option.ToString(), 0, 1024);
	        byte[] messageBytes = Encoding.ASCII.GetBytes(this.messageString,0,1024);
			
			System.Buffer.BlockCopy(optionBytes,0,completeBytes,0,1024);
			System.Buffer.BlockCopy(messageBytes,0,completeBytes,1024,1024);

			return completeBytes;
        }

        public object getObject(byte[] arrayBytes) {
	        byte[] optionBytes = new byte[1024];
	        byte[] messagebytes = new byte[1024];
			System.Buffer.BlockCopy(arrayBytes,0,optionBytes,0,1024);
			System.Buffer.BlockCopy(arrayBytes, 1024, messagebytes, 0, 1024);

			string optionString = Encoding.ASCII.GetString(optionBytes);
			string messageString = Encoding.ASCII.GetString(messagebytes);

			protocolOption option = (protocolOption)Enum.Parse(typeof(protocolOption), optionString, true);
			Message msg = new Message(option, messageString);
			return msg;
        }
    }
}