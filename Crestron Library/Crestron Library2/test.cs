using System;
using System.IO.Ports;
using System.Threading;


namespace Crestron_Library {
	class test {

		public static void Main() {
			SerialPortInterface serialPortInterface = new SerialPortInterface();
			serialPortInterface.setSerialPort("COM5");

			serialPortInterface.sendBytes(new byte[] { 0x32, 0xB2 });

			}
		}
	}
