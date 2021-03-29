using Instrument_Communicator_Library.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Instrument_Communicator_Library {

	/// <summary>
	/// Represents a message to put in the queue concurrent queue when sending to the client
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class Message : ISerializeableObject {
		private protocolOption option = protocolOption.ping;  //protocol option enum that tells server what protocol to use when sending
		private string messageString;   //String to be sent to the server

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="option">protocol option for the message</param>
		/// <param name="messageString">array of strings that will be sent to the client</param>
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
		/// <summary>
		/// Get a byte array representing the object
		/// </summary>
		/// <returns>byte array</returns>
		public byte[] getBytes() {
			//Byte array with the size of char for each element in
			byte[] optionBytes = BitConverter.GetBytes((int)option);
			byte[] messageBytes = Encoding.ASCII.GetBytes(this.messageString);
			byte[] completeBytes = new byte[optionBytes.Length + messageBytes.Length + 1];

			//Put option bytes in complete bytes array
			System.Buffer.BlockCopy(optionBytes, 0, completeBytes, 0, optionBytes.Length);
			//Put nullbyte after option bytes
			completeBytes[optionBytes.Length] = (byte)0;
			//put message bytes after nullbyte
			System.Buffer.BlockCopy(messageBytes, 0, completeBytes, optionBytes.Length + 1, messageBytes.Length);

			return completeBytes;
		}

		/// <summary>
		/// Convert array Of bytes created by the get btyes method back into a message object
		/// </summary>
		/// <param name="arrayBytes">Array of bytes generated from get bytes method from the same object type</param>
		/// <returns>Object</returns>
		public object getObject(byte[] arrayBytes) {
			List<byte> optionChars = new List<byte>();
			List<byte> messageChars = new List<byte>();

			bool nullByte = false;
			for (int i = 0; i < arrayBytes.Length; i++) {
				byte currentByte = arrayBytes[i];
				//Check if max byte is there so you start writing to the message byte list
				if (currentByte == (byte)0) {
					nullByte = true;
					continue;
				}
				//Write to either option byte or message byte list
				if (nullByte) {
					messageChars.Add(currentByte);
				}
				else {
					optionChars.Add(currentByte);
				}
			}
			//Convert to string and option
			byte[] optionArray = optionChars.ToArray();
			byte[] messageArray = messageChars.ToArray();
			//Convert to string
			int optionInt = BitConverter.ToInt32(optionArray);
			string messageString = Encoding.ASCII.GetString(messageArray);
			//Recreate enum
			protocolOption option = (protocolOption)optionInt;

			Message msg = new Message(option, messageString);
			return msg;
		}
	}
}