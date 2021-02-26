using System;
using System.Collections.Generic;
using Crestron_Library;
using OpenCvSharp;
using Video_Library;

namespace MAIN_Program {

	/// <summary>
	/// Main class for starting the system controlling data flow between hardware side libraries
	/// and communication libraries.
	/// Produces cli information about system/program status.
	/// </summary>
	/// <author>Andre Helland</author>
	class Program {
		private readonly List<VideoDeviceInterface> videoDevices = new List<VideoDeviceInterface>();
		private CommandParser commandParser;


		static void Main(string[] args) {


			//TODO: read config file here and pass it into "Program" constructor.


			_ = new Program(new string[] {"test"});

		}


		// TODO: make input config file.
		private static string port = "com4";
		private Program(string[] args) {
			
			//TODO: pars config file:
			
			//TODO: construct/init based on config file

			setupSerialCable(port);
			setupVideoDevice(0);
			setupVideoDevice(2);





		}

		#region setup methods

		/// <summary>
		/// Method tries connecting to serial cable and gives feedback on fail or success.
		/// </summary>
		/// <param name="port">Com port to connect to.</param>
		/// <returns>If setup was successful.</returns>
		private bool setupSerialCable(string port) {
			Console.WriteLine("Initializing serial cable...");
			try {
				var serialPort = new SerialPortInterface(port);
				//TODO: test if com port is a crestron cable (add test using getLEDStatus()).
				
				this.commandParser = new CommandParser(serialPort);

				writeSuccess("Successfully connected to port: " + port);

				return true;
			} catch {
				writeWarning("Failed to connect to port: " + port);

				writeWarning("Available ports: ");
				foreach (var availablePort in SerialPortInterface.GetAvailablePorts()) {
					writeWarning(availablePort);
				}
				return false;
			}
		}

		/// <summary>
		/// Method tries to connect to a video device and gives feedback on fail or success.
		/// </summary>
		/// <param name="index">Index of device from DSHOW API.</param>
		/// <returns>If setup was successful.</returns>
		private bool setupVideoDevice(int index) {
			Console.WriteLine("Initializing video device" + videoDevices.Count + "...");
			this.videoDevices.Add(new VideoDeviceInterface(index));

			//Wait for video device frames.
			Mat temp;
			while (!(videoDevices[^1].tryReadFrameBuffer(out temp))) ;

			// Checking if frame has more than 1 color channel (hacky way to check if frames are being produced properly)
			if (temp.Channels() < 2) {
				writeWarning("No output from video device or no device found");
				
				//Remove device to stop/prevent memory leak.
				videoDevices[^1].Dispose();
				videoDevices.Remove(videoDevices[^1]);
				return false;
			}

			writeSuccess("Detecting frames from video device");
			return true;
		}

		#endregion


		//TODO: make watchDog, checking if all components of the system is functioning as they should and reset components/classes not working.
		private void watchDog() {
			throw new NotImplementedException();
		}

		private void writeWarning(string s) {
			Console.BackgroundColor = ConsoleColor.Red;
			Console.WriteLine(s);
			Console.ResetColor();
		}

		private void writeSuccess(string s) {
			Console.BackgroundColor = ConsoleColor.Green;
			Console.WriteLine(s);
			Console.ResetColor();
		}
	}
}
