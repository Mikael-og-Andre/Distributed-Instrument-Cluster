using Crestron_Library;
using Instrument_Communicator_Library;
using Instrument_Communicator_Library.Remote_Device_side_Communicators;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading;
using Video_Library;

namespace MAIN_Program {

	/// <summary>
	/// Main class for starting the system controlling data flow between hardware side libraries
	/// and communication libraries.
	/// Produces cli information about system/program status.
	/// </summary>
	/// <author>Andre Helland, Mikael Nilssen</author>
	internal class Program {
		private readonly List<VideoDeviceInterface> videoDevices = new List<VideoDeviceInterface>();
		private CommandParser commandParser;
		private VideoCommunicator videoCommunicator;
		private CrestronCommunicator crestronCommunicator;

		private static void Main(string[] args) {
			//TODO INSTCLUST-103: read config file here and pass it into "Program" constructor.

			_ = new Program(new string[] { "test" });
		}

		// TODO INSTCLUST-103: make input config file.
		private static string serialComPort = "com4";

		private Program(string[] args) {


			//TODO INSTCLUST-103: pars config file:

			//TODO INSTCLUST-103: construct/init based on config file

			setupSerialCable(serialComPort);
			//setupVideoDevice(0);
			setupVideoDevice(1);

			try {
				//Setup communicators
				Thread.Sleep(1000);
				setupVideoCommunicator("127.0.0.1", 5051, "Radar1", "device location", "device type", "access");
				setupCrestronCommunicator("127.0.0.1", 5050, "Radar1", "device location", "device type", "access");
			}
			catch (Exception e) {
				Console.WriteLine(e);
				throw;
			}

			var relayThread = new Thread(this.relayThread) {IsBackground = true};
			relayThread.Start();


			while (true) {
				//TODO: RENAME/REDO
				//if (videoDevices[0].tryReadFrameBuffer(out Mat ooga)) {
				if (videoDevices[0].tryReadJpg(out byte[] ooga, 70)) {
					Thread.Sleep(100);
					videoCommunicator.GetInputQueue().Enqueue(new VideoFrame(ooga));
				}
			}


		}

		#region setup methods

		/// <summary>
		/// Method tries connecting to serial cable and gives console feedback on fail or success.
		/// </summary>
		/// <param name="port">Com port to connect to.</param>
		/// <returns>If setup was successful.</returns>
		private bool setupSerialCable(string port) {
			Console.WriteLine("Initializing serial cable...");
			try {
				var serialPort = new SerialPortInterface(port);
				commandParser = new CommandParser(serialPort);
				writeSuccess("Successfully connected to port: " + port);

				//NumLock check may be unnecessary. 
				if (numLockCheck(serialPort))
					writeWarning("NumLock check failed, com port may not be a crestron cable.");

				//Release any keys
				serialPort.SendBytes(0x38);

				return true;
			} catch {
				writeWarning("Failed to connect to port: " + port);
				writeWarning($"Available ports: {string.Join(",",SerialPortInterface.GetAvailablePorts())}");
				return false;
			}
		}

		/// <summary>
		/// Checks if serial port is connected to a crestron cable by using numlock state
		/// and checking if it changes when sending a make numlock byte.
		/// </summary>
		/// <param name="serialPort"></param>
		/// <returns></returns>
		private bool numLockCheck(SerialPortInterface serialPort) {
			var numLock0 = serialPort.GetLEDStatus()[0];
			serialPort.SendBytes(new List<byte>() {0x5a, 0xda});
			while (serialPort.isExecuting()) ;
			var numLock1 = serialPort.GetLEDStatus()[0];
			return (numLock0 == numLock1);
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

		private void setupVideoCommunicator(string ip, int port, string name, string location, string type, string accessHash) {
			//Video networking info
			string videoIP = ip;
			int videoPort = port;
			//Instrument Information
			InstrumentInformation info = new InstrumentInformation(name, location, type);
			//AccessToken -
			AccessToken accessToken = new AccessToken(accessHash);
			//cancellation tokens
			CancellationToken videoCancellationToken = new CancellationToken(false);

			//Video Communicator
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