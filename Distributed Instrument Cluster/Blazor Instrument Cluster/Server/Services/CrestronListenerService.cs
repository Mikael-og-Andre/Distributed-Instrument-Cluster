using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server_Library.Connection_Classes;
using Server_Library.Connection_Types;
using Server_Library.Server_Listeners;
using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement;

namespace Blazor_Instrument_Cluster.Server.Services {

	/// <summary>
	/// Starts a Sending listener, that will send commands to incoming receiving listeners
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class CrestronListenerService: BackgroundService {

		/// <summary>
		/// Logger
		/// </summary>
		private ILogger<CrestronListenerService> logger;

		/// <summary>
		/// Injected Service provider
		/// </summary>
		private readonly IServiceProvider services;

		/// <summary>
		/// Remote device connection
		/// </summary>
		private RemoteDeviceManager remoteDeviceManager;

		/// <summary>
		/// Sending listener accepting incoming ReceivingClients
		/// </summary>
		private SendingListener sendingListener;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		public CrestronListenerService(ILogger<CrestronListenerService> logger, IServiceProvider services) {
			this.logger = logger;
			//Get Remote devices from services
			remoteDeviceManager = (RemoteDeviceManager)services.GetService(typeof(IRemoteDeviceManager));
			//Init Listener
			var jsonString = File.ReadAllText(@"config.json");
			var json = JsonSerializer.Deserialize<Json>(jsonString);
			var ip = json.serverIP;
			var port = json.crestronPort;

			sendingListener = new SendingListener(new IPEndPoint(IPAddress.Parse(ip), port));
		}

		/// <summary>
		/// Start the listener
		/// </summary>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
			//Run server
			Thread listenerThread = new Thread(() => sendingListener.start());
			listenerThread.Start();

			//Get incoming connections
			while (!stoppingToken.IsCancellationRequested) {
				if (sendingListener.getIncomingConnection(out ConnectionBase output)) {
					//Cast to correct type
					SendingConnection sendingConnection = (SendingConnection)output;
					//Add to remote devices
					remoteDeviceManager.addConnectionToRemoteDevices(sendingConnection);
				}
				else {
					await Task.Delay(5);
				}
			}
			
		}

		private Json parsConfigFile(string file) {
			Console.WriteLine("Parsing config file...");
			var jsonString = File.ReadAllText(file);
			var json = JsonSerializer.Deserialize<Json>(jsonString);
			return json;
		}
	}
}