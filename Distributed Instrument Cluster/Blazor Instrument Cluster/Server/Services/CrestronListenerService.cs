using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server_Library.Connection_Classes;
using Server_Library.Connection_Types;
using Server_Library.Server_Listeners;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement;
using Server_Library.Connection_Types.Async;
using Server_Library.Server_Listeners.Async;

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
		/// DuplexListener
		/// </summary>
		private DuplexListenerAsync duplexListenerAsync;

		private string ip { get; set; }
		private int port { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		public CrestronListenerService(ILogger<CrestronListenerService> logger, IServiceProvider services) {
			this.logger = logger;
			this.services = services;
			//Get Remote devices from services
			remoteDeviceManager = (RemoteDeviceManager)services.GetService(typeof(RemoteDeviceManager));
			//Init Listener
			var jsonString = File.ReadAllText(@"config.json");
			var json = JsonSerializer.Deserialize<Json>(jsonString);
			ip = json.serverIP;
			port = json.crestronPort;
		}

		private Json parsConfigFile(string file) {
			Console.WriteLine("Parsing config file...");
			var jsonString = File.ReadAllText(file);
			var json = JsonSerializer.Deserialize<Json>(jsonString);
			return json;
		}

		/// <summary>
		/// Start listener
		/// </summary>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
			Task listenerTask = listenerLoop(stoppingToken).ContinueWith(task => {
				switch (task.Status) {
					case TaskStatus.RanToCompletion:
						logger.LogDebug("Crestron ListenerTask Ended with state RanToCompletion");
						break;

					case TaskStatus.Canceled:
						logger.LogDebug("Crestron ListenerTask Ended with state cancel");
						break;

					case TaskStatus.Faulted:
						logger.LogDebug("Crestron ListenerTask Ended with state Faulted");
						Exception exception = task.Exception?.Flatten();
						if (exception != null) throw exception;
						break;

					default:
						logger.LogWarning("Crestron listenerTask ended without the status canceled, faulted, or ran to completion");
						break;
				}

				//Do something when ended

			}, stoppingToken);

			Task incomingConnectionTask = incomingDeviceLoop(stoppingToken).ContinueWith(task => {
				switch (task.Status) {
					case TaskStatus.RanToCompletion:
						logger.LogDebug("Crestron incomingConnectionTask Ended with state RanToCompletion");
						break;

					case TaskStatus.Canceled:
						logger.LogDebug("Crestron incomingConnectionTask Ended with state cancel");
						break;

					case TaskStatus.Faulted:
						logger.LogDebug("Crestron incomingConnectionTask Ended with state Faulted");
						Exception exception = task.Exception?.Flatten();
						if (exception != null) throw exception;
						break;

					default:
						logger.LogWarning("Crestron incomingConnectionTask ended without the status canceled, faulted, or ran to completion");
						break;
				}

				//Do something when ended

			}, stoppingToken);
		}

		private async Task listenerLoop(CancellationToken stoppingToken) {
			while (!stoppingToken.IsCancellationRequested) {
				//Create new listener
				duplexListenerAsync = new DuplexListenerAsync(new IPEndPoint(IPAddress.Parse(ip), port));
				//listen
				await duplexListenerAsync.run();
				logger.LogWarning("Crestron Listener ended");
			}
		}

		private async Task incomingDeviceLoop(CancellationToken stoppingToken) {
			while (!stoppingToken.IsCancellationRequested) {
				if (duplexListenerAsync.getIncomingConnection(out ConnectionBaseAsync output)) {
					DuplexConnectionAsync connection = (DuplexConnectionAsync)output;

					(string name, string location, string type) result = await queryDatabase();

					await remoteDeviceManager.addConnectionToRemoteDevices(connection, false, result.name, result.location, result.type);
				}
				else {
					await Task.Delay(1000, stoppingToken);
				}
			}
		}

		private async Task<(string name, string location, string type)> queryDatabase() {
			return ("hardcodedname","hardcodedlocation","hardcodedtype");
		}
	}
}