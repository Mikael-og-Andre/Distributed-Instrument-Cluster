using System;
using Crestron_Library;

namespace Crestron_Demo {
	/// <summary>
	/// Demo fro testing command parser in crestron library.
	/// </summary>
	/// <author>Andre Helland</author>
	internal class CommandParserDemo {

		public static void Main(string[] args) {
			_ = new CommandParserDemo();
		}

		private CommandParserDemo() {
			var commandParser = new CommandParser(pickPort());

			while (true) {
				var line  = Console.ReadLine();

				try {
					commandParser.pars(line);
				} catch (Exception e) {
					Console.WriteLine(e);
				}
			}
		}


		private static SerialPortInterface pickPort() {

			Console.WriteLine($"Available ports: {string.Join(",",SerialPortInterface.GetAvailablePorts())}");
			var portName = Console.ReadLine();

			try {
				Console.WriteLine($"Connected to port: {portName}");
				 return new SerialPortInterface(portName);
			} catch {
				Console.WriteLine($"Failed to connect to port: {portName}");
				pickPort();
			}
			return null;
		}
	}
}

