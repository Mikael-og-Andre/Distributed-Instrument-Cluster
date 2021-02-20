using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		CommandParser commandParser = new CommandParser();

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
			//test2();





			//testSerialInterface();
			//Commands c = new Commands();



			//List<string> commands = c.getAllCommands();

			//foreach(string s in commands) {
			//	Console.WriteLine(s);
			//}

			//Console.WriteLine(c.getBreakByte("k"));
			////Console.WriteLine(c.getBreakByte("left"));
			///

			var temp = new Commands();

			test test = new test();

			test.pythonCursorCapture();


		}


		//https://stackoverflow.com/questions/53379866/running-python-script-on-c-sharp-and-getting-output-continuously
		public void pythonCursorCapture() {

			var cmd = "C:\\Users\\Andre\\Desktop\\CODE\\Distributed-Instrument-Cluster\\CursorPosition\\main.py";
			var process = new Process {
				StartInfo = new ProcessStartInfo {
					FileName = "C:\\Users\\Andre\\anaconda3.\\python.exe",
					Arguments = cmd,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				},
				EnableRaisingEvents = true
			};

			process.ErrorDataReceived += commandParser.spamIn;
			process.OutputDataReceived += commandParser.spamIn;

			//process.ErrorDataReceived += Process_OutputDataReceived;
			//process.OutputDataReceived += Process_OutputDataReceived;

			process.Start();
			process.BeginErrorReadLine();
			process.BeginOutputReadLine();
			process.WaitForExit();
			Console.Read();
		}


		private static void test2() {
			SerialPortInterface serialPortInterface = new SerialPortInterface("COM5");




			//serialPortInterface.SendBytes(new List<byte> { 0x6f });	//large magnitude
			serialPortInterface.SendBytes(new List<byte> { 0x6d }); //small magnitude

			//serialPortInterface.enqueuBytes(new List<byte> { 0x3a, 0x1f, 0x38, 0x4c });	//nuke

			//serialPortInterface.enqueuBytes(new List<byte> { 0x33, 0xB3, 0x18, 0x98, 0x32, 0xB2, 0x32, 0xB2, 0x1F, 0x9F });


			for (int i = 0; i < 239; i++) {
				serialPortInterface.SendBytes(new List<byte> { 0x44, 0x42 });   //top left
																				//serialPortInterface.SendBytes(new List<byte> { 0x45, 0x43 });	//bottom right

				//serialPortInterface.sendBytesSafe(new byte[] { 0x44 }, false);    //up
				//serialPortInterface.SendBytes(new List<byte> { 0x45 });    //down

				//serialPortInterface.SendBytes(new List<byte> { 0x43 });		//right
				//serialPortInterface.SendBytes(new List<byte> { 0x42 });		//left

			}
			//Thread.Sleep(100);
			//Console.WriteLine(serialPortInterface.GetLEDStatus()[1]);
			//Thread.Sleep(100);
			//serialPortInterface.SendBytes(new List<byte> { 0x1e, 0x9e });
			//Thread.Sleep(100);
			//Console.WriteLine(serialPortInterface.GetLEDStatus()[1]);

			//serialPortInterface.Dispose();
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

		static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e) {
			Console.WriteLine(e.Data);

		}
	}
}

