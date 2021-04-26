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
		/// ReceivingListener for accepting SendingClients
		/// </summary>
		private ReceivingListener receivingListener;

		/// <summary>
		/// Constructor, injects logger and remote device connections
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		public VideoListenerService(ILogger<VideoListenerService> logger, IServiceProvider services) {
			this.logger = logger;
			//Get Remote devices from services
			remoteDeviceManager = (RemoteDeviceManager)services.GetService(typeof(IRemoteDeviceManager));
			//Init ReceivingListener
			var jsonString = File.ReadAllText(@"config.json");
			var json = JsonSerializer.Deserialize<Json>(jsonString);
			var ip = json.serverIP;
			var port = json.videoPort;

			receivingListener = new ReceivingListener(new IPEndPoint(IPAddress.Parse(ip), port));
		}

		/// <summary>
		/// Start listener
		/// </summary>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
			//Run server
			Thread listenerThread = new Thread(() => receivingListener.start());
			listenerThread.Start();

			//Get incoming connections and start providers for them
			while (!stoppingToken.IsCancellationRequested) {
				if (receivingListener.getIncomingConnection(out ConnectionBase output)) {
					//Cast to correct connection
					ReceivingConnection receivingConnection = (ReceivingConnection)output;
					//Add to list of remote devices
					remoteDeviceManager.addConnectionToRemoteDevices(receivingConnection);
				}
				else {
					await Task.Delay(5);
				}
			}
		}
	}
}