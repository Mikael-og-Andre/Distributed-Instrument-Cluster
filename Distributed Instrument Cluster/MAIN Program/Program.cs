﻿using Crestron_Library;
using Server_Library;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Server_Library.Authorization;
using Server_Library.Socket_Clients;
using Video_Library;
using PackageClasses;

namespace MAIN_Program {

	/// <summary>
	/// Main class for starting the system controlling data flow between hardware side libraries
	/// and communication libraries.
	/// Produces cli information about system/program status.
	/// </summary>
	/// <author>Andre Helland, Mikael Nilssen</author>
	internal class Program {
		private readonly List<VideoConnection> videoConnections = new();
		private CommandParser commandParser;
		//private ReceivingClient<ExampleVideoObject> videoClient;
		private ReceivingClient crestronClient;

		private static string configFile = "config.json";
		private static void Main(string[] args) {
			parsArgs(args);
			_ = new Program(configFile);
		}

		private Program(string configFile) {
			var json = parsConfigFile(configFile);


			while (!setupSerialCable(json.serialCable)) {
				Thread.Sleep(3000);
				Console.WriteLine("Retrying...");
			}

			foreach (var device in json.videoDevices) {
				setupVideoDevice(device);
			}

			//Start crestron command relay thread. (this should be event based as an optimal solution).
			var relayThread = new Thread(this.relayThread) { IsBackground = true };
			relayThread.Start();


			//Start video relay threads. 
			for (int i = 0; i < videoConnections.Count; i++) {
				var videoThread = new Thread(this.videoThread) {IsBackground = true};
				videoThread.Start(i);
			}

			//Start CLI loop.
			while (true) {
				parsCLI(Console.ReadLine());
			}
		}

		#region parsers

		private static void parsArgs(string[] args) {
			if (args.Length > 0) {
				configFile = args[0].ToString();
			}
		}

		private JsonClasses parsConfigFile(string file) {
			Console.WriteLine("Parsing config file...");
			var jsonString = File.ReadAllText(file);
			var json = JsonSerializer.Deserialize<JsonClasses>(jsonString);

			writeSuccess("Read config file.");
			return json;
		}

		private void parsCLI(string s) {
			s = s.ToLower();
			switch (s) {
				case "help":
					Console.WriteLine("CLI not implemented");
					break;
				case "q":
				case "quit":
					Console.WriteLine("Program shutdown");
					Environment.Exit(0);
					break;
				default:
					Console.WriteLine($"\"{s}\" not recognized as a command, try \"help\" to see available commands.");
					break;
			}
		}

		#endregion

		#region setup methods

		/// <summary>
		/// Method tries connecting to serial cable and gives console feedback on fail or success.
		/// </summary>
		/// <param name="port">Com port to connect to.</param>
		/// <returns>If setup was successful.</returns>
		private bool setupSerialCable(JsonClasses.SerialCable serialCable) {
			Console.WriteLine("Initializing serial cable...");

			//Try to set up communication socket.
			var communicator = serialCable.communicator;
			try {
				setupCrestronCommunicator(communicator.ip, communicator.port, communicator.name, communicator.location, communicator.type, communicator.subName ,communicator.accessHash);
			} catch {
				writeWarning("Failed to connect to server.");
				return false;
			}

			try {
				var serialPort = new SerialPortInterface(serialCable.portName);
				commandParser = new CommandParser(serialPort, serialCable.largeMagnitude, serialCable.smallMagnitude, serialCable.maxDelta);

				//NumLock check may be unnecessary.
				var task = Task.Run(() => numLockCheck(serialPort));
				if (task.Wait(TimeSpan.FromSeconds(3))) {
					if (task.Result)
						writeWarning("NumLock check failed, com port may not be a crestron cable.");
				} else {
					writeWarning("Com port timed out");
					//Ignore issue for now (dispose not functioning properly).
					//task.Dispose();
					//serialPort.Dispose();
					//return false;
				}

				//Release all keys.
				serialPort.SendBytes(0x38);
			} catch {
				writeWarning("Failed to connect to port: " + serialCable.portName);
				writeWarning($"Available ports: {string.Join(",",SerialPortInterface.GetAvailablePorts())}");
				return false;
			}

			writeSuccess("Successfully connected to port: " + serialCable.portName);
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

			//Try to set up communication socket.
			var communicator = device.communicator;
			SendingClient connection;
			try {
				connection = setupVideoCommunicator(communicator.ip, communicator.port, communicator.name, communicator.location, communicator.type, communicator.subName ,communicator.accessHash);
			}
			catch (Exception e) {
				writeWarning("Failed to connect to server.");
				Console.WriteLine(e);
				return false;
			}

			var videoDevice = new VideoDeviceInterface(index: device.deviceIndex, API: (VideoCaptureAPIs) device.apiIndex, frameWidth: device.width, frameHeight: device.height);

			//Wait for video device frames.
			Mat temp = videoDevice.readFrame();

			// Checking if frame has more than 1 color channel (hacky way to check if frames are being produced properly)
			if (temp.Channels() < 2) {
				writeWarning("No output from video device or no device found");

				//Remove device to stop/prevent memory leak.
				videoDevice.Dispose();

				return false;
			}
			videoConnections.Add(new VideoConnection(videoDevice, connection, device.quality, device.fps));

			writeSuccess("Detecting frames from video device");
			return true;
		}

		public SendingClient setupVideoCommunicator(string ip, int port, string name, string location, string type,string subName, string accessHash) {
			string videoIP = ip;
			int videoPort = port;
			ClientInformation info = new ClientInformation(name, location, type, subName);
			AccessToken accessToken = new AccessToken(accessHash);
			CancellationToken videoCancellationToken = new CancellationToken(false);

			var videoClient = new SendingClient(ip, port, info, accessToken, videoCancellationToken);
			videoClient.run();
			return videoClient;
		}

		public void setupCrestronCommunicator(string ip, int port, string name, string location, string type, string subName, string accessHash) {
			string crestronIP = ip;
			int crestronPort = port;
			ClientInformation info = new ClientInformation(name, location, type,subName);
			AccessToken accessToken = new AccessToken(accessHash);
			CancellationToken crestronCancellationToken = new CancellationToken(false);

			crestronClient = new ReceivingClient(ip, port, info, accessToken, crestronCancellationToken);
			crestronClient.run();
		}

		#endregion setup methods

		#region Threads

		/// <summary>
		/// Thread for relaying commands coming from internet socket to command parser.
		/// </summary>
		private void relayThread() {
			while (true) {
				try {
					if (crestronClient.getBytesFromClient(out var messageObject)) {
						ExampleCrestronMsgObject temp =
							JsonSerializer.Deserialize<ExampleCrestronMsgObject>(
								Encoding.UTF32.GetString(messageObject).Replace("\0",string.Empty));
						if (temp != null) commandParser.pars(temp.msg);
					}
				}
				catch (Exception e) {
					Console.WriteLine(e);
				}
			}
		}

		/// <summary>
		/// Thread for sending frames to server at a constant or dynamic* fps.
		/// *TODO: make dynamic fps option when fps=0.
		/// </summary>
		/// <param name="index">Video device index</param>
		private void videoThread(object index) {
			var i = (int) index;
			var device = videoConnections[i].device;
			var connection = videoConnections[i].connection;
			var quality = videoConnections[i].quality;
			var fps = videoConnections[i].fps;
			
			var timer = Stopwatch.GetTimestamp();
			while (true) {
				try {
					var jpg = device.readJpg(quality);
					connection.queueBytesForSending(jpg.ToArray());
				} catch (Exception e) {
					Console.WriteLine(e);
				}
				timer = fpsLimiter(timer, fps);
			}
		}

		#endregion

		/// <summary>
		/// Calculates delta from previous call and current and sleeps thread
		/// the necessary amount of time to achieve the desired fps.
		/// </summary>
		/// <param name="T">Time returned from previous method call</param>
		/// <param name="fps">Desired frame rate.</param>
		/// <returns>Time with delay for use in next call of this method.</returns>
		private static long fpsLimiter(long T, int fps) {
			var delta = T - Stopwatch.GetTimestamp();
			var sleepTime = (int) (delta / 10000);
			if(sleepTime>0)
				Thread.Sleep(sleepTime);
			return Stopwatch.GetTimestamp() + (10000000) / fps;
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