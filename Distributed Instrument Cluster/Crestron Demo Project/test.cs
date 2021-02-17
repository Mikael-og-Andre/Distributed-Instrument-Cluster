using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using Crestron_Library;
using Microsoft.VisualBasic.FileIO;

//TODO: delete.
namespace Crestron_Library {
	class test {

		public static void Main(string[] args) {
			//Console.WriteLine("wth");
			//SerialPort serialPort = new SerialPort();

			//serialPort.PortName = "COM5";

			//serialPort.Open();
			//serialPort.Write(new byte[] { 0x33, 0xB3, 0x18, 0x98, 0x32, 0xB2, 0x32, 0xB2, 0x1F, 0x9F }, 0, 3);
			//serialPort.BaseStream.Flush();
			//byte[] temp = new byte[1];
			//while (true) {
			//	serialPort.Read(temp, 0, 1);
			//	Console.WriteLine(temp[0]);
			//}



			//testSerialInterface();
			test2();
			




			 //testSerialInterface();
			//Commands c = new Commands();

			

			//List<string> commands = c.getAllCommands();

			//foreach(string s in commands) {
			//	Console.WriteLine(s);
			//}

			//Console.WriteLine(c.getBreakByte("k"));
			////Console.WriteLine(c.getBreakByte("left"));

		}


		private static void test2() {
			SerialPortInterface serialPortInterface = new SerialPortInterface("COM5");




			//serialPortInterface.sendBytesSafe(new byte[] { 0x6f }, false);	//large magnitude
			//serialPortInterface.sendBytesSafe(new byte[] { 0x6d }, false);	//small magnitude

			//serialPortInterface.enqueuBytes(new List<byte> { 0x3a, 0x1f, 0x38, 0x4c });	//nuke

			//serialPortInterface.enqueuBytes(new List<byte> { 0x33, 0xB3, 0x18, 0x98, 0x32, 0xB2, 0x32, 0xB2, 0x1F, 0x9F });


			for (int i = 0; i < 1; i++) {
				//serialPortInterface.enqueuBytes(new List<byte> { 0x44, 0x42 });	//top left
				//serialPortInterface.SendBytes(new List<byte> { 0x45, 0x43 });	//bottom right

				//serialPortInterface.sendBytesSafe(new byte[] { 0x44 }, false);    //up
				//serialPortInterface.sendBytesSafe(new byte[] { 0x45 }, false);    //down
				

			}
			Thread.Sleep(1000);
			Console.WriteLine(serialPortInterface.GetLEDStatus());
			serialPortInterface.SendBytes(new List<byte> { 0x1e, 0x9e });
			Thread.Sleep(1000);
			Console.WriteLine(serialPortInterface.GetLEDStatus());

			serialPortInterface.Dispose();
		}

		private static void testSerialInterface() {
			SerialPortInterface serialPortInterface = new SerialPortInterface("COM5");

			String[] S = SerialPortInterface.GetAvailablePorts();

			foreach (string s in S) {
				Console.WriteLine(s);
			}

			while (true) {
				Console.ReadKey();
				Console.WriteLine("sent");
				//serialPortInterface.sendBytes(new byte[] { 0x32, 0xB2 });
				//serialPortInterface.sendBytes(new byte[] { 0x42 });
				//serialPortInterface.sendBytesSafe(new byte[] { 0x33, 0xB3, 0x18, 0x98, 0x32, 0xB2, 0x32, 0xB2, 0x1F, 0x9F });

			}

		}
	}
}


