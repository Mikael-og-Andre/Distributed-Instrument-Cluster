using System;
using System.IO.Ports;
using System.Threading;


namespace Crestron_Library {
	/// <summary>
	/// Class to interface with serial port and send bytes over serial connections.
	/// </summary>
	/// <author>Andre Helland</author>
	public class SerialPortInterface {

		private static SerialPort serialPort;

		public SerialPortInterface() {
			serialPort = new SerialPort();
			Console.WriteLine(serialPort.WriteBufferSize);
		}

		/// <summary>
		///Close connection with current port and change what serial port the class is connected to.
		/// </summary>
		/// <param name="port">Serial port to connect to.</param>
		public void setSerialPort(String port) {
			//Check if port given is valid and throw exception if not.
			String[] ports = getAvailablePorts();
			bool portValid = false;
			foreach (String s in ports) {
				if (port.ToLower().Equals(s.ToLower())) {
					portValid = true;
					break;
				}
			}
			if(!portValid) {
				throw new ArgumentException("Invalid port: \"" + port + "\"");
			}

			serialPort.Close();
			serialPort.PortName = port;
		}

		/// <summary>
		/// Function returns all available serial ports.
		/// </summary>
		/// <returns>String array of all available serial ports.</returns>
		public String[] getAvailablePorts() {
			return SerialPort.GetPortNames();
		}


		/// <summary>
		/// Sends byte array of bytes to serial port.
		/// Unreliable when sending more than 2 bytes!!!
		/// Use "sendBytesSafe" when sending more than 1 byte.
		/// TODO: Potential fix (investigate).
		/// After each command is sent to the CBL-USB-RS232KM-6,
		/// the CBL-USB-RS232KM-6 returns a response code, which is the 1’s complement of the command received.
		/// Use this response byte to indicate when the next command may be sent to the  CBL-USB-RS232KM-6. 
		/// </summary>
		/// <param name="bytes">Array of bytes to send.</param>
		public void sendBytes(byte[] bytes) {
			serialPort.Open();
			serialPort.Write(bytes, 0, bytes.Length);
			serialPort.Close();
		}

		/// <summary>
		/// Reliably transmit multiple bytes.
		/// (Function is kinda slow due to serial port limitation/baud rate)
		/// </summary>
		/// <param name="bytes">Array of bytes to send.</param>
		/// <param name="keySafety">If function will send clear key buffer command to release all keys to prevent having keys accidentally stuck.</param>
		public void sendBytesSafe(byte[] bytes, bool keySafety=true) {
			//Iterate over all bytes in array and send them one at a time.
			//(Done for reliable transmission. Sending more than 2 bytes = unreliable transmission)
			foreach (byte b in bytes) {
				sendBytes(new byte[] {b});
			}

			//Send buffer clear command releasing any potentially stuck key.
			if (keySafety) {
				serialPort.Open();
				serialPort.Write(new byte[] { 0x38 }, 0, 1);              //0x38: USB buffer clear command (release all keys).
				serialPort.Close();
			}
		}

	}
}
