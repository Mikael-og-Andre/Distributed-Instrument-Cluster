using Blazor_Instrument_Cluster.Server.Injection;
using Blazor_Instrument_Cluster.Server.RemoteDevice;
using Blazor_Instrument_Cluster.Server.SendingHandler;
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

namespace Blazor_Instrument_Cluster.Server.WebSockets {

	/// <summary>
	/// Websocket handler for crestron control connections
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class CrestronWebsocketHandler<T, U> : ICrestronSocketHandler {

		/// <summary>
		/// Logger
		/// </summary>
		private ILogger<CrestronWebsocketHandler<T, U>> logger;

		/// <summary>
		///Services
		/// </summary>
		private IServiceProvider services;

		/// <summary>
		/// Remote devices
		/// </summary>
		private RemoteDeviceConnections<T, U> remoteDeviceConnections;

		/// <summary>
		/// Constructor, Injects Logger and service provider and gets Remote device connection Singleton
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		public CrestronWebsocketHandler(ILogger<CrestronWebsocketHandler<T, U>> logger, IServiceProvider services) {
			this.logger = logger;
			remoteDeviceConnections = (RemoteDeviceConnections<T, U>)services.GetService(typeof(IRemoteDeviceConnections<T, U>));
		}

		/// <summary>
		/// Handles the incoming connection
		/// </summary>
		/// <param name="websocket"></param>
		/// <param name="socketFinishedTcs"></param>
		public async void StartCrestronWebsocketProtocol(WebSocket websocket, TaskCompletionSource<object> socketFinishedTcs) {
			//Create cancellation token
			CancellationToken cancellationToken = new CancellationToken(false);

			try {
				//Exchange information about the wanted device
				(bool found, ClientInformation info, RemoteDevice<T, U> device, ControlToken<U> controlToken) result = await setupDeviceAsync(websocket, cancellationToken);
				//Control token
				ControlToken<U> controlToken = result.controlToken;

				if (!result.found) {
					await stopConnectionAsync("Device not found", socketFinishedTcs, cancellationToken, websocket,
						controlToken);
				}
				//enter queue
				bool gaveControl = await handleQueueAsync(controlToken, websocket, cancellationToken);

				//end connection if not in control
				if (!gaveControl) {
					//end connection
					await stopConnectionAsync("Denied Control", socketFinishedTcs, cancellationToken, websocket, controlToken);
				}
				//handle receive
				await handleMessageReceivingAsync(controlToken, websocket, cancellationToken);
			}
			catch (Exception ex) {
				//Log
				logger.LogWarning(ex, "Exception occurred in websocket");
				//Stop connection
				await stopConnectionAsync("Closing Connection", socketFinishedTcs, cancellationToken, websocket);
			}
			//Complete
			socketFinishedTcs.TrySetResult(new object());
		}

		#region Startup information sharing

		/// <summary>
		/// Setup and information sharing with the client websocket
		/// Gets the device information from the client
		/// </summary>
		/// <param name="websocket"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task<(bool, ClientInformation, RemoteDevice<T, U>, ControlToken<U>)> setupDeviceAsync(WebSocket websocket, CancellationToken token) {
			//Send start signal
			byte[] startBytes = Encoding.UTF8.GetBytes("start");
			ArraySegment<byte> startSegment = new ArraySegment<byte>(startBytes);
			await websocket.SendAsync(startSegment, WebSocketMessageType.Text, true, token);

			//Get device info
			byte[] deviceBuffer = new byte[4098];
			ArraySegment<byte> deviceSeg = new ArraySegment<byte>(deviceBuffer);
			await websocket.ReceiveAsync(deviceSeg, token);
			string deviceJson = Encoding.UTF8.GetString(deviceSeg).TrimEnd('\0');

			RequestConnectionModel deviceInfo = null;
			try {
				//Deserialize from json
				deviceInfo = JsonSerializer.Deserialize<RequestConnectionModel>(deviceJson);
			}
			catch (Exception e) {
				logger.LogWarning(e, "failed to deserialize requested device");
				return (false, null, null, null);
			}

			//Check if device exists
			bool foundDevice = false;
			RemoteDevice<T, U> remoteDevice = null;
			//Try to find the requested device
			if (remoteDeviceConnections.getRemoteDeviceWithNameLocationAndType(deviceInfo.name, deviceInfo.location, deviceInfo.type, out RemoteDevice<T, U> outputDevice)) {
				//Device was found, check if the sub name exists on the device
				remoteDevice = outputDevice;

				List<string> listOfSubNames = remoteDevice.getSubNamesList();

				foreach (var obj in listOfSubNames) {
					if (obj.ToLower().Equals(deviceInfo.subname.ToLower())) {
						foundDevice = true;
					}
				}
			}
			//init control token
			ControlToken<U> controlToken = null;
			bool foundController = false;

			if (foundDevice) {
				//get controller
				if (remoteDevice.getControlTokenForDevice(deviceInfo.subname, out ControlToken<U> output)) {
					controlToken = output;
					//Signal that the control token was found
					await sendStringWithWebsocketAsync("Found Device".ToLower(), websocket, token);
					foundController = true;
				}
				else {
					//Controller not found
					await sendStringWithWebsocketAsync("Device not found".ToLower(), websocket, token);
				}
			}
			else {
				//Not found
				await sendStringWithWebsocketAsync("Device not found".ToLower(), websocket, token);
			}

			(bool, ClientInformation, RemoteDevice<T, U>, ControlToken<U>) result = 
				(foundDevice && foundController, new ClientInformation(deviceInfo.name, deviceInfo.location, deviceInfo.type, deviceInfo.subname), remoteDevice, controlToken);

			return result;
		}

		#endregion Startup information sharing

		#region In queue

		private async Task<bool> handleQueueAsync(ControlToken<U> controlToken, WebSocket websocket, CancellationToken cancellationToken) {
			//Send entering queue
			await sendStringWithWebsocketAsync("enter queue", websocket, cancellationToken);
			while (!cancellationToken.IsCancellationRequested) {
				//try to request control
				controlToken.updateTime();
				if (controlToken.requestControl()) {
					//got control
					QueueStatusModel queueStatus = new QueueStatusModel(controlToken.hasControl);

					string json = JsonSerializer.Serialize(queueStatus);
					await sendStringWithWebsocketAsync(json, websocket,cancellationToken);
					return true;
				}
				else {
					//Still waiting for control
					QueueStatusModel queueStatus = new QueueStatusModel(controlToken.hasControl);

					string json = JsonSerializer.Serialize(queueStatus);
					await sendStringWithWebsocketAsync(json, websocket,cancellationToken);
					await Task.Delay(5000, cancellationToken);
				}
			}
			//not in control
			return false;
		}

		#endregion In queue

		#region Controlling

		/// <summary>
		/// Send messages from websockets to remote device
		/// </summary>
		/// <param name="controlToken"></param>
		/// <param name="websocket"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		private async Task handleMessageReceivingAsync(ControlToken<U> controlToken, WebSocket websocket, CancellationToken cancellationToken) {
			while (controlToken.hasControl && (!cancellationToken.IsCancellationRequested)) {
				//get json from websocket
				byte[] receivedBytesBuffer = new byte[1024];
				ArraySegment<byte> receivedBytesSegment = new ArraySegment<byte>(receivedBytesBuffer);
				await websocket.ReceiveAsync(receivedBytesSegment, cancellationToken);
				string jsonObj = Encoding.UTF8.GetString(receivedBytesSegment).TrimEnd('\0');

				try {
					//Send object to remote device
					U obj = JsonSerializer.Deserialize<U>(jsonObj);
					bool sent = controlToken.send(obj);
				}
				catch (Exception e) {
					logger.LogWarning(e, "Exception when deserializing command");
				}
			}
		}

		#endregion Controlling

		#region Websocket actions

		/// <summary>
		/// Stops the connection, and signals the connecting websocket
		/// </summary>
		/// <param name="statusDescription"></param>
		/// <param name="socketFinishedTcs"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="webSocket"></param>
		/// <param name="controlToken"></param>
		private async Task stopConnectionAsync(string statusDescription, TaskCompletionSource<object> socketFinishedTcs, CancellationToken cancellationToken, WebSocket webSocket, ControlToken<U> controlToken = null) {
			//If control token is passed, set as inactive to move the queue along
			controlToken?.abandon();
			//Close websocket
			await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, statusDescription, cancellationToken);
			//signal connection is over
			socketFinishedTcs.TrySetResult(new object());
		}

		/// <summary>
		/// Send a string
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="websocket"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task sendStringWithWebsocketAsync(string msg, WebSocket websocket, CancellationToken token) {
			//Send
			ArraySegment<byte> sendingBytes = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
			await websocket.SendAsync(sendingBytes, WebSocketMessageType.Text, true, token);
		}

		#endregion Websocket actions
	}
}