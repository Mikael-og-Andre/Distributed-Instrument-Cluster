using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;


namespace Crestron_Library {
	/// <summary>
	/// Class to interface with serial port and send bytes over serial connections.
	/// </summary>
	/// <author>Andre Helland</author>
	public class SerialPortInterface: IDisposable {
		private readonly ConcurrentQueue<byte> byteQueue = new ConcurrentQueue<byte>();
		private static SerialPort serialPort;
		private bool SendData = true;

		public SerialPortInterface(string port) {
			serialPort = new SerialPort();
			setSerialPort(port);

			Thread sendThread = new Thread(SendDataThread);
			sendThread.Start();
		}

		/// <summary>
		/// Function returns all available serial ports.
		/// </summary>
		/// <returns>String array of all available serial ports.</returns>
		public static String[] GetAvailablePorts() {
			return SerialPort.GetPortNames();
		}

		/// <summary>
		/// Method sends byte to receive information on button state for:
		/// Caps Lock, num lock and scroll lock.
		/// </summary>
		/// <returns>Binary "truth table" for what buttons are on and off in the form of a byte.</returns>
		public byte GetLEDStatus() {
			while (!serialPort.IsOpen) ;

			byte result = sendByte(0x7f);	// 0x7f: Byte to get status response.
			return (byte)(result & 0x0f);	// Only takes first 4 bits. (0011 0011 and 0000 1111 = 0000 0011).
		}

		/// <summary>
		/// TODO:
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public bool GetLEDStatusBitwise(int index) {
			throw new NotImplementedException();

			return false;
		}

		/// <summary>
		/// Adds bytes to byte queue.
		/// Bytes are then sent by SendingDataThread.
		/// </summary>
		/// <param name="bytes"></param>
		public void SendBytes(List<byte> bytes) {
			foreach (byte b in bytes) {
				byteQueue.Enqueue(b);
			}
		}

		/// <summary>
		/// Stops data sending thread and releases com port.
		/// </summary>
		public void Dispose() {
			SendData = false;
			GC.SuppressFinalize(this);
		}


		/// <summary>
		/// Thread sending bytes queued up in byteQueue.
		/// Method sends byte one by one and waits for response byte from crestron cable before sending.
		/// </summary>
		private void SendDataThread() {
			serialPort.Open();
			while(SendData) {
				if (byteQueue.TryDequeue(out byte b)) {
					sendByte(b);
				}
			}
			serialPort.Close();
		}

		/// <summary>
		/// Sends byte to serial port and waits for response.
		/// 
		/// After each command is sent to the CBL-USB-RS232KM-6,
		/// the CBL-USB-RS232KM-6 returns a response code, which is the 1’s complement of the command received.
		/// Use this response byte to indicate when the next command may be sent to the  CBL-USB-RS232KM-6. 
		/// </summary>
		/// <param name="bytes">List of bytes to send.</param>
		/// <returns>Response byte from byte sent</returns>
		private byte sendByte(byte b) {
			//Critical zone, only one thread can access serial port.
			lock (serialPort) {
				byte[] bytes = new byte[] { b };
			
				serialPort.Write(bytes, 0, bytes.Length);

				//Wait for response byte before continuing.
				var responseByte = new byte[1];
				while (true) {
					serialPort.Read(responseByte, 0, 1);
					if (responseByte != null)
						break;
				}
				return responseByte[0];
			}
		}

		/// <summary>
		/// Method for validating selected port and connecting to it.
		/// </summary>
		/// <param name="port">Serial port to connect to.</param>
		private void setSerialPort(String port) {
			//Check if port given is valid and throw exception if not.
			String[] ports = GetAvailablePorts();
			bool portValid = false;
			foreach (String s in ports) {
				if (port.ToLower().Equals(s.ToLower())) {
					portValid = true;
					break;
				}
			}
			if (!portValid) {
				throw new ArgumentException("Invalid port: \"" + port + "\"");
			} else {
				serialPort.PortName = port;
			}
		}
	}
}
