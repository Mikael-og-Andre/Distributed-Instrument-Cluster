using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server_Library.Connection_Types.Async;
using Server_Library.Server_Listeners.Async;
using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server.Services {

	/// <summary>
	/// Starts a receiving listener for accepting incoming sending connections
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class VideoListenerService : BackgroundService {

		/// <summary>
		/// Logger
		/// </summary>
		private ILogger<VideoListenerService> logger;

		/// <summary>
		/// Injected Service provider
		/// </summary>
		private readonly IServiceProvider services;

		/// <summary>
		/// Remote device connection
		/// </summary>
		private RemoteDeviceManager remoteDeviceManager;

		/// <summary>
		/// Listener for incoming connections
		/// </summary>
		private DuplexListenerAsync duplexListenerAsync;

		private string ip { get; set; }
		private int port { get; set; }

		/// <summary>
		/// Constructor, injects logger and remote device connections
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		public VideoListenerService(ILogger<VideoListenerService> logger, IServiceProvider services) {
			this.logger = logger;
			//Get Remote devices from services
			remoteDeviceManager = (RemoteDeviceManager)services.GetService(typeof(RemoteDeviceManager));
			//Init ReceivingListener
			var jsonString = File.ReadAllText(@"config.json");
			var json = JsonSerializer.Deserialize<Json>(jsonString);
			ip = json.serverIP;
			port = json.videoPort;
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
						logger.LogDebug("Video ListenerTask Ended with state RanToCompletion");
						break;

					case TaskStatus.Canceled:
						logger.LogDebug("Video ListenerTask Ended with state cancel");
						break;

					case TaskStatus.Faulted:
						logger.LogDebug("Video ListenerTask Ended with state Faulted");
						Exception exception = task.Exception?.Flatten();
						if (exception != null) throw exception;
						break;

					default:
						logger.LogWarning("Video listenerTask ended without the status canceled, faulted, or ran to completion");
						break;
				}

				//Do something when ended

			}, stoppingToken);

			Task incomingConnectionTask = incomingDeviceLoop(stoppingToken).ContinueWith(task => {
				switch (task.Status) {
					case TaskStatus.RanToCompletion:
						logger.LogDebug("Video incomingConnectionTask Ended with state RanToCompletion");
						break;

					case TaskStatus.Canceled:
						logger.LogDebug("Video incomingConnectionTask Ended with state cancel");
						break;

					case TaskStatus.Faulted:
						logger.LogDebug("Video incomingConnectionTask Ended with state Faulted");
						Exception exception = task.Exception?.Flatten();
						if (exception != null) throw exception;
						break;

					default:
						logger.LogWarning("Video incomingConnectionTask ended without the status canceled, faulted, or ran to completion");
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
				logger.LogWarning("Video Listener ended");
			}
		}

		private async Task incomingDeviceLoop(CancellationToken stoppingToken) {
			while (!stoppingToken.IsCancellationRequested) {
				if (duplexListenerAsync.getIncomingConnection(out ConnectionBaseAsync output)) {
					DuplexConnectionAsync connection = (DuplexConnectionAsync)output;

					(string name, string location, string type) result = await queryDatabase();

					await remoteDeviceManager.addConnectionToRemoteDevices(connection, true, result.name, result.location, result.type);
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