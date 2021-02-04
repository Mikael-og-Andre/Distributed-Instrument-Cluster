using System;
using System.IO.Ports;
using System.Threading;
using Crestron_Library;


namespace Crestron_Library {
	class test {

		public static void Main(string[] args) {
			SerialPortInterface serialPortInterface = new SerialPortInterface();
			serialPortInterface.setSerialPort("COM5");

			String[] S = serialPortInterface.getAvailablePorts();

			foreach (string s in S) {
				Console.WriteLine(s);
			}

			while(true) {
				Console.ReadKey();
				Console.WriteLine("sent");
				//serialPortInterface.sendBytes(new byte[] { 0x32, 0xB2 });
				//serialPortInterface.sendBytes(new byte[] { 0x42 });
				serialPortInterface.sendBytesSafe(new byte[] { 0x33, 0xB3, 0x18, 0x98, 0x32, 0xB2, 0x32, 0xB2, 0x1F, 0x9F });

			}

			}
		}
	}
