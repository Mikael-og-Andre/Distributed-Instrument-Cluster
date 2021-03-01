using Crestron_Library;
using Instrument_Communicator_Library;
using Instrument_Communicator_Library.Remote_Device_side_Communicators;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Threading;
using Instrument_Communicator_Library.Interface;
using Video_Library;

namespace MAIN_Program {

	/// <summary>
	/// Main class for starting the system controlling data flow between hardware side libraries
	/// and communication libraries.
	/// Produces cli information about system/program status.
	/// </summary>
	/// <author>Andre Helland</author>
	internal class Program {
		private readonly List<VideoDeviceInterface> videoDevices = new List<VideoDeviceInterface>();
		private CommandParser commandParser;
		private VideoCommunicator videoCommunicator;
		private CrestronCommunicator crestronCommunicator;

		private static void Main(string[] args) {
			//TODO: read config file here and pass it into "Program" constructor.

			_ = new Program(new string[] { "test" });
		}

		// TODO: make input config file.
		private static string serialComPort = "com4";

		private Program(string[] args) {
			//Setup communicators
			Thread.Sleep(1000);
			//setupVideoCommunicator("127.0.0.1", 5050, "device name", "device location", "device type", "access");
			setupCrestronCommunicator("127.0.0.1", 5051, "crestron", "device location", "device type", "access");

			//TODO: pars config file:

			//TODO: construct/init based on config file

			setupSerialCable(serialComPort);
			//setupVideoDevice(0);
			//setupVideoDevice(2);

			var relayThread = new Thread(this.relayThread) {IsBackground = true};
			relayThread.Start();


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

		public void setupVideoCommunicator(string ip, int port, string name, string location, string type, string accessHash) {
			//Video networking info
			string videoIP = ip;
			int videoPort = port;
			//Instrument Information
			InstrumentInformation info = new InstrumentInformation(name, location, type);
			//AccessToken -
			AccessToken accessToken = new AccessToken(accessHash);
			//cancellation tokens
			CancellationToken videoCancellationToken = new CancellationToken(false);

			//Video Communicator - <TYPE YOU WANT TO SEND>
			videoCommunicator =
				new VideoCommunicator(videoIP, videoPort, info, accessToken, videoCancellationToken);

			//TODO: refactor threading
			Thread videoThread = new Thread(() => videoCommunicator.Start());
			videoThread.Start();
		}

		public void setupCrestronCommunicator(string ip, int port, string name, string location, string type, string accessHash) {
			//Video networking info
			string crestronIP = ip;
			int crestronPort = port;
			//Instrument Information
			InstrumentInformation info = new InstrumentInformation(name, location, type);
			//AccessToken -
			AccessToken accessToken = new AccessToken(accessHash);
			//Crestron Cancellation Token
			CancellationToken crestronCancellationToken = new CancellationToken(false);
			//Crestron Communicator
			crestronCommunicator = new CrestronCommunicator(crestronIP, crestronPort, info, accessToken, crestronCancellationToken);

			//TODO: refactor threading
			Thread crestronThread = new Thread(() => crestronCommunicator.Start());
			crestronThread.Start();
		}

		#endregion setup methods

		/// <summary>
		/// Thread for relaying commands coming from internet socket to command parser.
		/// </summary>
		private void relayThread() {
			var queue = crestronCommunicator.getCommandOutputQueue();
			while (true) {
				try {
					if (queue.TryDequeue(out string temp)) {
						Console.WriteLine(temp);
						commandParser.pars(temp);
					}
				}
				catch (Exception e) {
					Console.WriteLine(e);
				}
			}
		}


		//TODO: make watchDog, checking if all components of the system is functioning as they should and reset components/classes not working.
		private void watchDog() {
			throw new NotImplementedException();
		}

		private void writeWarning(string s) {
			//Console.BackgroundColor = ConsoleColor.Red;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(s);
			Console.ResetColor();
		}

		private void writeSuccess(string s) {
			//Console.BackgroundColor = ConsoleColor.Green;
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine(s);
			Console.ResetColor();
		}
	}
}