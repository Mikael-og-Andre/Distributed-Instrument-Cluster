using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.Injection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server_Library.Connection_Classes;
using Server_Library.Connection_Types;
using Server_Library.Server_Listeners;

namespace Blazor_Instrument_Cluster.Server.Services {

	public class SendingListenerService<T,U> :BackgroundService {

		/// <summary>
		/// Logger
		/// </summary>
		private ILogger<SendingListenerService<T,U>> logger;

		/// <summary>
		/// Injected Service provider
		/// </summary>
		private readonly IServiceProvider services;

		/// <summary>
		/// Remote device connection
		/// </summary>
		private RemoteDeviceConnections<T,U> remoteDeviceConnections;

		/// <summary>
		/// Sending listener accepting incoming ReceivingClients
		/// </summary>
		private SendingListener<U> sendingListener;


		public SendingListenerService(ILogger<SendingListenerService<T,U>> logger, IServiceProvider services) {
			this.logger = logger;
			//Get Remote devices from services
			remoteDeviceConnections = (RemoteDeviceConnections<T,U>)services.GetService(typeof(IRemoteDeviceConnections<T,U>));
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
			Task serverTask = new Task(() => {
				sendingListener.start();
			});
			serverTask.Start();

			//Get incoming connections
			while (!stoppingToken.IsCancellationRequested) {
				if (sendingListener.getIncomingConnection(out ConnectionBase output)) {
					SendingConnection<T> sendingConnection = (SendingConnection<T>) output;

					
				}
				else {
					Thread.Sleep(1000);
				}
			}

			await Task.Delay(1000);
		}
	}
}