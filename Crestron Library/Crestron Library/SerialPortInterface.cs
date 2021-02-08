using System;
using System.IO.Ports;
using System.Threading;

// TODO: Redo comments/documentation.
/* 
 * Class to interface with serial port and send bytes over serial connections.
 * 
 * @Author Andre Helland
 */

namespace Crestron_Library {
	public class SerialPortInterface {

		private static SerialPort serialPort;

		public SerialPortInterface() {
			serialPort = new SerialPort();
			Console.WriteLine(serialPort.WriteBufferSize);
		}

		/*
		 * Close connection with current port and
		 * change what serial port the class is connected to.
		 */
		public void setSerialPort(String port) {
			//TODO: generate exception for invalid input.
			serialPort.Close();
			serialPort.PortName = port;
		}
		
		/*
		 * Function returns all available serial ports.
		 */
		public String[] getAvailablePorts() {
			return SerialPort.GetPortNames();
		}


		/*
		 * Sends byte array of bytes to serial port.
		 * Unreliable when sending more than 2 bytes!!!
		 * Use "sendBytesSafe" when sending more than 1 byte.
		 */
		public void sendBytes(byte[] bytes) {
			serialPort.Open();
			serialPort.Write(bytes, 0, bytes.Length);
			serialPort.Close();
		}

		/*
		 * Reliably transmit multiple bytes.
		 * @param keySafety enable or disable release key command.
		 */
		public void sendBytesSafe(byte[] bytes) {
			sendBytesSafe(bytes, true);
		}
		public void sendBytesSafe(byte[] bytes, bool keySafety) {
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
