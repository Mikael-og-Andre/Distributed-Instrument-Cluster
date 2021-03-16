﻿using System;
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

	public class ReceivingListenerService<T,U> : BackgroundService {

		/// <summary>
		/// Logger
		/// </summary>
		private ILogger<ReceivingListenerService<T,U>> logger;

		/// <summary>
		/// Injected Service provider
		/// </summary>
		private readonly IServiceProvider services;

		/// <summary>
		/// Remote device connection
		/// </summary>
		private RemoteDeviceConnections<T,U> remoteDeviceConnections;

		/// <summary>
		/// ReceivingListener for accepting SendingClients
		/// </summary>
		private ReceivingListener<T> receivingListener;

		/// <summary>
		/// Constructor, injects logger and remote device connections
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		public ReceivingListenerService(ILogger<ReceivingListenerService<T,U>> logger, IServiceProvider services) {
			this.logger = logger;
			//Get Remote devices from services
			remoteDeviceConnections = (RemoteDeviceConnections<T,U>)services.GetService(typeof(IRemoteDeviceConnections<T,U>));
			//Init ReceivingListener
			//TODO: Add config for ip of endpoint
			receivingListener = new ReceivingListener<T>(new IPEndPoint(IPAddress.Parse("127.0.0.1"),6980 ));
			

		}

		/// <summary>
		/// Start listener
		/// </summary>
		/// <param name="stoppingToken"></param>
		/// <returns></returns>
		protected override async Task ExecuteAsync(CancellationToken stoppingToken) {

			//Run server
			Task serverTask = new Task(() => {
				receivingListener.start();
			});
			serverTask.Start();

			//Get incoming connections and start providers for them
			while (!stoppingToken.IsCancellationRequested) {
				if (receivingListener.getIncomingConnection(out ConnectionBase output)) {
					ReceivingConnection<T> receivingConnection = (ReceivingConnection<T>) output;

					
				}
				else {
					Thread.Sleep(1000);
				}
			}

			await Task.Delay(1000);

		}
	}
}