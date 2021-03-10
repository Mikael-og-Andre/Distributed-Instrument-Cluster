using Blazor_Instrument_Cluster.Server.Injection;
using Instrument_Communicator_Library;
using Instrument_Communicator_Library.Server_Listener;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Instrument_Communicator_Library.Connection_Types;
using Instrument_Communicator_Library.Server_Listeners;

namespace Blazor_Instrument_Cluster.Server.Worker {

	/// <summary>
	/// Background service for accepting incoming connections from controllers
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class CrestronListenerService : BackgroundService {
		private const int Delay = 10;   //Delay after each loop

		private readonly ILogger<VideoListenerService> logger;      //Injected logger
		private readonly IServiceProvider services;                     //Injected Service provider
		private ListenerCrestron crestronListener;                       //Video listener server accepting incoming device video connections
		private RemoteDeviceConnection remoteDeviceConnection;       //Remote device connection

		/// <summary>
		/// Inject the logger and the thing used to share information with hubs
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		public CrestronListenerService(ILogger<VideoListenerService> logger, IServiceProvider services) {
			this.logger = logger;
			//Get Remote devices from services
			remoteDeviceConnection = (RemoteDeviceConnection)services.GetService(typeof(IRemoteDeviceConnections));

			//Create endpoint
			IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5050);
			this.crestronListener = new ListenerCrestron(ipEndPoint);

			//Set remoteDevice connections
			List<CrestronConnection> crestronConnectionList = crestronListener.getCrestronConnectionList();
			if (remoteDeviceConnection != null) {
				remoteDeviceConnection.setCrestronConnectionList(crestronConnectionList);
			} else {
				this.logger.LogError("Remote Device connection input was null");
				throw new NullReferenceException(
					"Class: VideoListenerService - remoteDeviceConnection Injection was null");
			}
		}
		/// <summary>
		/// Starts the listener thread for the crestron communicators
		/// </summary>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
			logger.LogDebug($"CrestronListenerService is starting.");
			stoppingToken.Register(() => logger.LogDebug($" Crestron Listener background task is stopping."));
			//Create and start new thread
			Thread crestronThread = new Thread(() => crestronListener.start()) {IsBackground = false};
			crestronThread.Start();

			await Task.Delay(10);
		}
	}
}