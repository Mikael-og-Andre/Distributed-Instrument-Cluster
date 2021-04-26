using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement;
using Blazor_Instrument_Cluster.Shared;
using Microsoft.Extensions.Logging;
using Server_Library;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Shared.Websocket.Enum;

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
		/// Remote devices
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

			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

			CrestronWebsocketState currentState = CrestronWebsocketState.Requesting;

			while (true) {
				try {
					switch (currentState) {
						case CrestronWebsocketState.Requesting:
							currentState = await handleRequesting(websocket,cancellationTokenSource.Token);
							break;
						case CrestronWebsocketState.InQueue:
							currentState = await handleInQueue(websocket,cancellationTokenSource.Token);
							break;
						case CrestronWebsocketState.InControl:
							currentState = await handleInControl(websocket,cancellationTokenSource.Token);
							break;
						case CrestronWebsocketState.WaitingForInput:
							currentState = await handleWaitingForInput(websocket,cancellationTokenSource.Token);
							break;
						case CrestronWebsocketState.TimeLimitExceeded:
							currentState = await handleTimeLimitExceeded(websocket,cancellationTokenSource.Token);
							break;
						case CrestronWebsocketState.Disconnected:
							await handleDisconnected(websocket,cancellationTokenSource.Token);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
				catch (Exception ex) {
					logger.LogError("Error in crestron websocket backend",ex);

				}
			}
		}

		private async Task<CrestronWebsocketState> handleRequesting(WebSocket webSocket,CancellationToken token) {
			try {
				//Send requesting state
				await sendString(webSocket,CrestronWebsocketState.Requesting.ToString(),token);



				return CrestronWebsocketState.InQueue;
			}
			catch (Exception ex) {
				logger.LogDebug(ex,"Error in handleRequesting");
				return CrestronWebsocketState.Disconnected;
			}
		}

		private async Task<CrestronWebsocketState> handleInQueue(WebSocket webSocket,CancellationToken token) {
			try {



				return CrestronWebsocketState.InControl;
			}
			catch (Exception ex) {
				logger.LogDebug(ex,"Error in handleInQueue");
				return CrestronWebsocketState.Disconnected;
			}
		}

		private async Task<CrestronWebsocketState> handleInControl(WebSocket webSocket,CancellationToken token) {
			try {



				return CrestronWebsocketState.TimeLimitExceeded;
			}
			catch (Exception ex) {
				logger.LogDebug(ex,"Error in handleInControl");
				return CrestronWebsocketState.Disconnected;
			}
		}

		private async Task<CrestronWebsocketState> handleTimeLimitExceeded(WebSocket webSocket,CancellationToken token) {
			try {


				return CrestronWebsocketState.WaitingForInput;
			}
			catch (Exception ex) {
				logger.LogDebug(ex,"Error in handleTimeLimitExceeded");
				return CrestronWebsocketState.Disconnected;
			}
		}

		private async Task<CrestronWebsocketState> handleWaitingForInput(WebSocket webSocket,CancellationToken token) {
			try {


				return CrestronWebsocketState.InQueue;
			}
			catch (Exception ex) {
				logger.LogDebug(ex,"Error in handleWaitingForInput");
				return CrestronWebsocketState.Disconnected;
			}
		}

		private async Task handleDisconnected(WebSocket webSocket,CancellationToken token) {
			try {

			}
			catch (Exception ex) {
				logger.LogDebug(ex,"Error in handleDisconnected");
			}
		}

		private async Task sendString(WebSocket webSocket,string s,CancellationToken token) {
			byte[] bytes = Encoding.UTF32.GetBytes(s);
			await webSocket.SendAsync(bytes,WebSocketMessageType.Text,true,token);
		}

		private async Task<string> receiveString(WebSocket webSocket,CancellationToken token) {
			byte[] sizeBytes = new byte[sizeof(int)];
			await webSocket.ReceiveAsync(sizeBytes,token);
			int size = BitConverter.ToInt32(sizeBytes);
			byte[] stringBytes = new byte[size];
			await webSocket.ReceiveAsync(stringBytes,token);
			return Encoding.UTF32.GetString(stringBytes);
		}
	}
}