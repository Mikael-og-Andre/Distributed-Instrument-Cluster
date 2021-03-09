using Blazor_Instrument_Cluster.Server.Events;
using Blazor_Instrument_Cluster.Server.Injection;
using Instrument_Communicator_Library;
using Instrument_Communicator_Library.Server_Listener;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server.Worker {

	/// <summary>
	/// Background service for running the video listener for the remote devices
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class VideoListenerService : BackgroundService {

		/// <summary>
		/// Delay after each loop
		/// </summary>
		private const int Delay = 10;

		/// <summary>
		/// Injected logger
		/// </summary>
		private readonly ILogger<VideoListenerService> logger;

		/// <summary>
		/// Injected Service provider
		/// </summary>
		private readonly IServiceProvider services;

		/// <summary>
		/// Video listener server accepting incoming device video connections
		/// </summary>
		private ListenerVideo videoListener;

		/// <summary>
		/// Remote device connection
		/// </summary>
		private RemoteDeviceConnection remoteDeviceConnection;

		/// <summary>
		/// Inject the logger and the thing used to share information with hubs
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		public VideoListenerService(ILogger<VideoListenerService> logger, IServiceProvider services) {
			this.logger = logger;
			//Get Remote devices from services
			remoteDeviceConnection = (RemoteDeviceConnection)services.GetService(typeof(IRemoteDeviceConnections));

			//Create endpoint
			IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5051);
			this.videoListener = new ListenerVideo(ipEndPoint);

			//Set remoteDevice connections
			List<VideoConnection> videoConnections = videoListener.getVideoConnectionList();
			if (remoteDeviceConnection != null) {
				remoteDeviceConnection.SetVideoConnectionList(videoConnections);
			}
			else {
				this.logger.LogError("Remote Device connection input was null");
				throw new NullReferenceException(
					"Class: VideoListenerService - remoteDeviceConnection Injection was null");
			}
		}

		/// <summary>
		/// Method that launches when the Hosted service is initialized
		/// Start the Video listener thread, and gets all incoming connections to the listener and starts a Video Frame provider for the incoming connection
		/// </summary>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
			logger.LogDebug($"VideoListenerService is starting.");

			stoppingToken.Register(() => logger.LogDebug($" VideoListener background task is stopping."));

			//Create a new thread for the listener and start it
			Thread videoListenerThread = new Thread(() => videoListener.start());
			videoListenerThread.IsBackground = false;
			videoListenerThread.Start();
			//Get concurrentQueue of incoming connections to the video listener thread
			ConcurrentQueue<VideoConnection> incomingConnections = videoListener.getIncomingConnectionQueue();

			//Start a provider for each new stream
			while (!stoppingToken.IsCancellationRequested) {
				logger.LogDebug($"VideoListener task doing background work.");
				//check if there is a connection in the queue
				if (incomingConnections.TryPeek(out _)) {
					//Start a new thread that pushes frames from the connection to a provider
					incomingConnections.TryDequeue(out VideoConnection connection);
					Thread connectionThread = new Thread(providerStart);
					connectionThread.Start(connection);
				}

				await Task.Delay(Delay, stoppingToken);
			}

			logger.LogDebug($"VideoListener background task is stopping.");
		}

		/// <summary>
		/// Starts a new provider and push frames from connection queue
		/// </summary>
		private void providerStart(object input) {
			//Cast connection
			VideoConnection connection = (VideoConnection)input;
			//Wait for the instrument to have done authorization and get the instrument information
			while (!connection.hasInstrument) {
				Thread.Sleep(100);
			}
			//Instrument information
			InstrumentInformation info = connection.GetInstrumentInformation();
			//Create Provider with the name of the device
			VideoConnectionFrameProvider provider = new VideoConnectionFrameProvider(info.Name);
			//Add provider to list of running providers so i can be found by connecting ui's and subscribed to
			remoteDeviceConnection.AddFrameProviderToListOfProviders(provider);
			CancellationToken token = new CancellationToken();
			//Get Queue
			ConcurrentQueue<VideoFrame> queue = connection.GetOutputQueue();
			while (!token.IsCancellationRequested) {
				if (queue.TryDequeue(out VideoFrame frameResult)) {
					//Send frame to all subscribers
					provider.PushFrame(frameResult);
				}
			}
		}
	}
}