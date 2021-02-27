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

namespace Blazor_Instrument_Cluster.Server.Worker {

	/// <summary>
	/// Background service for running the video listener for the remote devices
	/// <author>Mikael Nilssen</author>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class VideoListenerService<T> : BackgroundService {
		private const int Delay = 10;   //Delay after each loop

		private readonly ILogger<VideoListenerService<T>> logger;      //Injected logger
		private readonly IServiceProvider services;                     //Injected Service provider
		private ListenerVideo<T> videoListener;                       //Video listener server accepting incoming device video connections
		private RemoteDeviceConnection<T> remoteDeviceConnection;       //Remote device connection

		/// <summary>
		/// Inject the logger and the thing used to share information with hubs
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		public VideoListenerService(ILogger<VideoListenerService<T>> logger, IServiceProvider services) {
			this.logger = logger;
			//Get Remote devices from services
			remoteDeviceConnection = (RemoteDeviceConnection<T>)services.GetService(typeof(IRemoteDeviceConnections<T>));

			//Create endpoint
			IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6969);
			this.videoListener = new ListenerVideo<T>(ipEndPoint);

			//Set remoteDevice connections
			List<VideoConnection<T>> videoConnections = videoListener.getVideoConnectionList();
			if (remoteDeviceConnection != null) {
				remoteDeviceConnection.SetVideoConnectionList(videoConnections);
			} else {
				this.logger.LogError("Remote Device connection input was null");
			}
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
			logger.LogDebug($"VideoListenerService is starting.");

			stoppingToken.Register(() => logger.LogDebug($" VideoListener background task is stopping."));

			//Create a new thread for the listener and start it
			Thread videoListenerThread = new Thread(() => videoListener.start());
			videoListenerThread.Start();

			List<VideoConnection<T>> videoConnectionList = videoListener.getVideoConnectionList();

			while (!stoppingToken.IsCancellationRequested) {
				logger.LogDebug($"VideoListener task doing background work.");
				//Lock connection list

				await Task.Delay(Delay, stoppingToken);
			}

			logger.LogDebug($"VideoListener background task is stopping.");
		}
	}
}