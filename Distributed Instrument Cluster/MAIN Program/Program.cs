using Crestron_Library;
using Server_Library;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using Server_Library.Authorization;
using Server_Library.Socket_Clients;
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
		private VideoClient videoClient;
		private CrestronClient crestronClient;

		private static string configFile = "config.json";
		private static void Main(string[] args) {
			parsArgs(args);
			_ = new Program(configFile);
		}

		private static void parsArgs(string[] args) {
			if (args.Length > 0) {
				configFile = args[0].ToString();
			}
		}

		private Program(string configFile) {
			var json = parsConfigFile(configFile);

			setupSerialCable(json.serialCable);

			foreach (var device in json.videoDevices) {
				setupVideoDevice(device);
			}
			//setupVideoDevice(1);

			//try {
			//	//Setup communicators
			//	Thread.Sleep(1000);
			//	setupVideoCommunicator("127.0.0.1", 5051, "Radar1", "device location", "device type", "access");
			//	setupCrestronCommunicator("127.0.0.1", 5050, "Radar1", "device location", "device type", "access");
			//}
			//catch (Exception e) {
			//	Console.WriteLine(e);
			//	throw;
			//}

			var relayThread = new Thread(this.relayThread) {IsBackground = true};
			relayThread.Start();


			while (true) {
				//TODO: RENAME/REDO
				//if (videoDevices[0].tryReadFrameBuffer(out Mat ooga)) {
				if (videoDevices[0].tryReadJpg(out byte[] ooga, 70)) {
					Thread.Sleep(100);
					videoClient.getInputQueue().Enqueue(new VideoFrame(ooga));
				}
			}
		}

		private JsonClasses parsConfigFile(string file) {
			Console.WriteLine("Parsing config file...");
			var jsonString = File.ReadAllText(file);
			var json = JsonSerializer.Deserialize<JsonClasses>(jsonString);

			writeSuccess("Read config file.");
			return json;
		}


		#region setup methods

		/// <summary>
		/// Method tries connecting to serial cable and gives console feedback on fail or success.
		/// </summary>
		/// <param name="port">Com port to connect to.</param>
		/// <returns>If setup was successful.</returns>
		private bool setupSerialCable(JsonClasses.SerialCable serialCable) {
			Console.WriteLine("Initializing serial cable...");
			try {
				var serialPort = new SerialPortInterface(serialCable.portName);
				commandParser = new CommandParser(serialPort, serialCable.largeMagnitude, serialCable.smallMagnitude, serialCable.maxDelta);
				writeSuccess("Successfully connected to port: " + serialCable.portName);

				//NumLock check may be unnecessary. 
				if (numLockCheck(serialPort))
					writeWarning("NumLock check failed, com port may not be a crestron cable.");

				//Release all keys.
				serialPort.SendBytes(0x38);
			} catch {
				writeWarning("Failed to connect to port: " + serialCable.portName);
				writeWarning($"Available ports: {string.Join(",",SerialPortInterface.GetAvailablePorts())}");
				return false;
			}

			//Try to set up communication socket.
			var communicator = serialCable.communicator;
			try {
				setupCrestronCommunicator(communicator.ip, communicator.port, communicator.name, communicator.location, communicator.type, communicator.accessHash);
			} catch (Exception e) {
				writeWarning("Failed to connect to server.");
				Console.WriteLine(e);
				return false;
			}

			return true;
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
		private bool setupVideoDevice(JsonClasses.VideoDevice device) {
			Console.WriteLine($"Initializing video device{device.deviceIndex}...");
			videoDevices.Add(new VideoDeviceInterface(index: device.deviceIndex, API: (VideoCaptureAPIs) device.apiIndex, frameWidth: device.width, frameHeight: device.height));

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

			var communicator = device.communicator;
			try {
				setupVideoCommunicator(communicator.ip, communicator.port, communicator.name, communicator.location, communicator.type, communicator.accessHash);
			} catch (Exception e) {
				writeWarning("Failed to connect to server.");
				Console.WriteLine(e);
				return false;
			}
			return true;
		}

		public void setupVideoCommunicator(string ip, int port, string name, string location, string type,string subName, string accessHash) {
			//Video networking info
			string videoIP = ip;
			int videoPort = port;
			//Instrument Information
			ClientInformation info = new ClientInformation(name, location, type, subName);
			//AccessToken -
			AccessToken accessToken = new AccessToken(accessHash);
			//cancellation tokens
			CancellationToken videoCancellationToken = new CancellationToken(false);

			//Video Communicator
			videoClient =
				new VideoClient(videoIP, videoPort, info, accessToken, videoCancellationToken);

			//TODO: refactor threading
			Thread videoThread = new Thread(() => videoClient.run());
			videoThread.Start();
		}

		public void setupCrestronCommunicator(string ip, int port, string name, string location, string type, string subName, string accessHash) {
			//Video networking info
			string crestronIP = ip;
			int crestronPort = port;
			//Instrument Information
			ClientInformation info = new ClientInformation(name, location, type,subName);
			//AccessToken -
			AccessToken accessToken = new AccessToken(accessHash);
			//Crestron Cancellation Token
			CancellationToken crestronCancellationToken = new CancellationToken(false);
			//Crestron Communicator
			crestronClient = new CrestronClient(crestronIP, crestronPort, info, accessToken, crestronCancellationToken);



			//TODO: refactor threading
			Thread crestronThread = new Thread(() => crestronClient.run());
			crestronThread.Start();
		}

		#endregion setup methods

		/// <summary>
		/// Thread for relaying commands coming from internet socket to command parser.
		/// </summary>
		private void relayThread() {
			var queue = crestronClient.getCommandOutputQueue();
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
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(s);
			Console.ResetColor();
		}

		private void writeSuccess(string s) {
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine(s);
			Console.ResetColor();
		}
	}
}