using Blazor_Instrument_Cluster.Server.Injection;
using Server_Library;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.RemoteDevice;
using Server_Library.Connection_Types;
using Server_Library.Connection_Types.deprecated;
using Server_Library.Enums;

namespace Blazor_Instrument_Cluster.Server.WebSockets {

	/// <summary>
	/// Websocket handler for crestron control connections
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class CrestronWebsocketHandler<T,U> : ICrestronSocketHandler {

		/// <summary>
		/// Logger
		/// </summary>
		private ILogger<CrestronWebsocketHandler<T,U>> logger;

		/// <summary>
		///Services
		/// </summary>
		private IServiceProvider services;

		/// <summary>
		/// Remote devices
		/// </summary>
		private RemoteDeviceConnections<T,U> remoteDeviceConnectionses;

		/// <summary>
		/// Constructor, Injects Logger and service provider and gets Remote device connection Singleton
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		public CrestronWebsocketHandler(ILogger<CrestronWebsocketHandler<T,U>> logger, IServiceProvider services) {
			this.logger = logger;
			remoteDeviceConnectionses = (RemoteDeviceConnections<T,U>)services.GetService(typeof(IRemoteDeviceConnections<T,U>));
		}

		/// <summary>
		/// Handles the incoming connection
		/// </summary>
		/// <param name="websocket"></param>
		/// <param name="socketFinishedTcs"></param>
		public async void StartCrestronWebsocketProtocol(WebSocket websocket, TaskCompletionSource<object> socketFinishedTcs) {
			//Create cancellation token
			CancellationToken token = new CancellationToken(false);
			
		}
	}
}