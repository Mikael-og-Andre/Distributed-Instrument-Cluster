using Blazor_Instrument_Cluster.Server.Injection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server_Library.Connection_Classes;
using Server_Library.Connection_Types;
using Server_Library.Server_Listeners;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.RemoteDevice;

namespace Blazor_Instrument_Cluster.Server.Services {

	/// <summary>
	/// Starts a Sending listener, that will send commands to incoming receiving listeners
	/// </summary>
	/// <typeparam name="T">Type for the receiving listener in the remote device connection</typeparam>
	/// <typeparam name="U">Type for the sending listener</typeparam>
	public class CrestronListenerService<T,U> : BackgroundService {

		/// <summary>
		/// Logger
		/// </summary>
		private ILogger<CrestronListenerService<T,U>> logger;

		/// <summary>
		/// Injected Service provider
		/// </summary>
		private readonly IServiceProvider services;

		/// <summary>
		/// Remote device connection
		/// </summary>
		private RemoteDeviceManager<U> remoteDeviceManager;

		/// <summary>
		/// Sending listener accepting incoming ReceivingClients
		/// </summary>
		private SendingListener<U> sendingListener;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		public CrestronListenerService(ILogger<CrestronListenerService<T,U>> logger, IServiceProvider services) {
			this.logger = logger;
			//Get Remote devices from services
			remoteDeviceManager = (RemoteDeviceManager<U>)services.GetService(typeof(IRemoteDeviceConnections<U>));
			//Init Listener
			//TODO: Add config ip setup
			sendingListener = new SendingListener<U>(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6981));
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
					SendingConnection<U> sendingConnection = (SendingConnection<U>)output;
					//Add to remote devices
					remoteDeviceManager.addConnectionToRemoteDevices(sendingConnection);
				}
				else {
					await Task.Delay(50);
				}
			}
			
		}
	}
}