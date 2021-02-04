using System;
using System.IO.Ports;
using System.Threading;


/* 
 * Class to interface with serial port and send bytes over serial connections.
 * 
 * @Author Andre Helland
 */

namespace Crestron_Library {
	class SerialPortInterface {

		private static SerialPort serialPort;


		public SerialPortInterface() {
			serialPort = new SerialPort();
		}

		/*
		 * Close connection with current port and
		 * change what serial port the class is connected to.
		 */
		public void setSerialPort(String port) {
			//TODO generate exception for invalid input.
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
		 * Sends byte array of bites to serial port.
		 * !Unreliable!
		 */
		public void sendBytes(byte[] bytes) {
			//TODO make key/byte transmition reliable. (current method has ~50% success rate).


			serialPort.Open();
			serialPort.Write(new byte[] {0x38}, 0, 1);              //0x38: USB buffer clear command (release all keys).

			serialPort.Write(bytes, 0, bytes.Length);
			serialPort.Close();

		}




	}
}
