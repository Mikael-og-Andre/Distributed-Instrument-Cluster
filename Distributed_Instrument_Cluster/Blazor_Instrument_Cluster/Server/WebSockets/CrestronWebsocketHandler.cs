using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement;
using Microsoft.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server.WebSockets {

	/// <summary>
	/// Websocket handler for crestron control connections
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class CrestronWebsocketHandler {

		/// <summary>
		/// Logger
		/// </summary>
		private ILogger<CrestronWebsocketHandler> logger;

		/// <summary>
		///Services
		/// </summary>
		private IServiceProvider services;

		/// <summary>
		/// Remote device manager
		/// </summary>
		private RemoteDeviceManager remoteDeviceManager;

		/// <summary>
		/// Constructor, Injects Logger and service provider and gets Remote device connection Singleton
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		public CrestronWebsocketHandler(ILogger<CrestronWebsocketHandler> logger, IServiceProvider services) {
			this.logger = logger;
			remoteDeviceManager = (RemoteDeviceManager)services.GetService(typeof(RemoteDeviceManager));
		}

		/// <summary>
		/// Handles the incoming connection
		/// </summary>
		/// <param name="websocket"></param>
		/// <param name="socketFinishedTcs"></param>
		public async Task startProtocol(WebSocket websocket, TaskCompletionSource<object> socketFinishedTcs) {
			CrestronWebsocketBackend crestronWebsocketBackend = new CrestronWebsocketBackend(websocket,remoteDeviceManager,socketFinishedTcs);

			await crestronWebsocketBackend.start();
		}
	}
}
