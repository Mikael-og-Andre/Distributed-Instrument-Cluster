using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Crestron_Library;
using MAIN_Program;
using OpenCvSharp;
using Remote_Server.Crestron;
using Video_Library;

namespace Remote_Server {

	/// <summary>
	/// Main class for starting the system controlling data flow between hardware side libraries
	/// and communication libraries.
	/// Produces cli information about system/program status.
	/// </summary>
	/// <author>Andre Helland, Mikael Nilssen</author>
	internal class Program {
		private readonly List<VideoConnection> videoConnections = new();
		private CommandParser commandParser;
		private JsonClasses.ServerSettings serverSettings;
		private static string configFile = "config.json";
		private CrestronListener crestronListener { get; set; }

		private static void Main(string[] args) {
			parsArgs(args);
			_ = new Program(configFile);
		}

		private Program(string configFile) {
			var json = parsConfigFile(configFile);
			//Testing class
			//TestCrestron crestron = new TestCrestron();

			setupSerialCable(json.crestronCable);
			var crestron = new CrestronControl(commandParser);

			//Start crestron listener
			crestronListener = new CrestronListener(new IPEndPoint(IPAddress.Parse(serverSettings.ip), serverSettings.crestronPort), crestron);
			var _ = crestronListener.run();

			foreach (var device in json.videoDevices) {
				setupVideoDevice(device);
			}

			//Set up all video streams.
			for (var i = 0; i < videoConnections.Count; i++) {
				new Thread(videoThread).Start(i);
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
			serverSettings = json.serverSettings;
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
		private bool setupSerialCable(JsonClasses.CrestronCable crestronCable) {
			Console.WriteLine("Initializing serial cable...");
			try {
				var serialPort = new SerialPortInterface(crestronCable.portName);
				commandParser = new CommandParser(serialPort, crestronCable.largeMagnitude, crestronCable.smallMagnitude, crestronCable.maxDelta);

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
				writeWarning("Failed to connect to port: " + crestronCable.portName);
				writeWarning($"Available ports: {string.Join(",",SerialPortInterface.GetAvailablePorts())}");
				return false;
			}

			writeSuccess("Successfully connected to port: " + crestronCable.portName);
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
		/// <returns>If setup was successful.</returns>
		private async Task<bool> setupVideoDevice(JsonClasses.VideoDevice device) {
			Console.WriteLine($"Initializing video device{device.deviceIndex}...");

			var videoDevice = new VideoDeviceInterface(index: device.deviceIndex, API: (VideoCaptureAPIs) device.apiIndex, frameWidth: device.width, frameHeight: device.height);

			//Wait for video device frames.
			var temp = videoDevice.readFrame();

			// Checking if frame has more than 1 color channel (hacky way to check if frames are being produced properly)
			if (temp.Channels() < 2) {
				writeWarning("No output from video device or no device found");

				//Remove device to stop/prevent memory leak.
				videoDevice.Dispose();

				return false;
			}
			videoConnections.Add(new VideoConnection(videoDevice, device.quality, device.fps));


			writeSuccess("Detecting frames from video device");
			return true;
		}

		#endregion setup methods

		#region Threads

		/// <summary>
		/// Thread for sending frames to server at a constant fps.
		/// </summary>
		/// <param name="index">Video device index</param>
		private void videoThread(object index) {
			var i = (int) index;
			var device = videoConnections[i].device;
			var quality = videoConnections[i].quality;
			var fps = videoConnections[i].fps;
			var stream = new MJPEG_Streamer(serverSettings.videoPort+i);


			var timer = Stopwatch.GetTimestamp();
			while (true) {
				try {
					var jpg = device.readJpg(quality);
					stream.Image = jpg;
				}
				catch (Exception e) {
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