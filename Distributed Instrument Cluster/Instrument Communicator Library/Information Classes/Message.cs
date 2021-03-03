using Instrument_Communicator_Library.Interface;
using System;
using System.Collections.Generic;

namespace Instrument_Communicator_Library {

	/// <summary>
	/// Represents a message to put in the queue concurrent queue when sending to the client
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class Message : ISerializeableObject {
		private protocolOption option;  //protocol option enum that tells server what protocol to use when sending
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

		public byte[] getBytes() {
			//Byte array with the size of char for each element in
			byte[] completeBytes = new byte[(option.ToString().Length * messageString.Length * sizeof(char)) + 1];
			char[] optionChars = option.ToString().ToCharArray();
			char[] messageChars = messageString.ToCharArray();

			int i = 0;
			//Add Option chars to byte array
			for (i = 0; i < optionChars.Length; i++) {
				completeBytes[i] = (byte)optionChars[i];
			}
			//Add max byte to show where first string ends
			i++;
			completeBytes[i] = byte.MaxValue;
			//Add messageBytes to byte array after the null byte
			for (int j = 0; j < messageChars.Length; i++, j++) {
				completeBytes[i] = (byte)messageChars[j];
			}

			return completeBytes;
		}

		/// <summary>
		/// Convert array Of bytes created by the get btyes method back into a message object
		/// </summary>
		/// <param name="arrayBytes">Array of bytes generated from get bytes method from the same object type</param>
		/// <returns>Object</returns>
		public object getObject(byte[] arrayBytes) {
			List<char> optionChars = new List<char>();
			List<char> messageChars = new List<char>();

			bool seenMaxByte = false;
			for (int i = 0; i < arrayBytes.Length; i++) {
				byte currentByte = arrayBytes[i];
				//Check if max byte is there so you start writing to the message byte list
				if (currentByte.Equals(Byte.MaxValue)) {
					seenMaxByte = true;
				}
				//Write to either option byte or message byte list
				if (seenMaxByte) {
					messageChars.Add((char)currentByte);
				} else {
					optionChars.Add((char)currentByte);
				}
			}
			//Convert to string and option
			char[] optionCharArray = optionChars.ToArray();
			char[] messageCharArray = messageChars.ToArray();
			//Convert to string
			string optionString = optionCharArray.ToString();
			string messageString = messageCharArray.ToString();
			//Recreate enum
			protocolOption option = (protocolOption)Enum.Parse(typeof(protocolOption), optionString, true);


			Message msg = new Message(option, messageString);
			return msg;
		}
	}
}