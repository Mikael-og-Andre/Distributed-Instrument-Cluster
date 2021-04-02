using Blazor_Instrument_Cluster.Server.Injection;
using Blazor_Instrument_Cluster.Server.RemoteDevice;
using Blazor_Instrument_Cluster.Server.SendingHandler;
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
				(bool found, ClientInformation info, RemoteDevice<T, U> device) result = await getDeviceInfoAsync(websocket, cancellationToken);

				//Tell socket if the device was found or not
				if (result.found) {
					RemoteDevice<T, U> remoteDevice = result.device;
					ClientInformation info = result.info;
					//Queue for control of the sending connection
					(bool enteredQueue, SendingControlHandler<U> handler, ControlToken controlToken) queueResult = enterQueue(remoteDevice, info);

					bool hasQueued = queueResult.enteredQueue;
					SendingControlHandler<U> sendingHandler = queueResult.handler;
					ControlToken controlToken = queueResult.controlToken;

					//Check if successfully queued
					if (!hasQueued) {
						//end connection
						await stopConnectionAsync("Sub device was not found", socketFinishedTcs, cancellationToken, websocket, controlToken);
						return;
					}

					//Signal that they are in queue
					await sendStringWithWebsocketAsync("in queue", websocket, cancellationToken);

					//While you are in the queue update queue position for the client
					while (!controlToken.hasControl) {
						//Update time on token
						controlToken.updateTime();
						//update the controller
						sendingHandler.updateController();
						//Get the position in queue
						int position = sendingHandler.getQueuePosition(controlToken);
						//If returned pos is -1 the control token was not found in the system, so end the connection
						if (position == -1) {
							await stopConnectionAsync("Closing Connection", socketFinishedTcs, cancellationToken, websocket, controlToken);
							return;
						}
						//Check if we have control
						if ((position == 0) && controlToken.hasControl) {
							//Update position for user
							await sendStringWithWebsocketAsync("" + position, websocket, cancellationToken);
							break;
						}
						//Update position for user
						await sendStringWithWebsocketAsync("" + position, websocket, cancellationToken);
						//Wait
						await Task.Delay(10000, cancellationToken);
					}

					//Send signal that they are in control
					await sendStringWithWebsocketAsync("controlling", websocket, cancellationToken);

					//Start receiving commands
					while (controlToken.hasControl) {
						//Update time since last action
						controlToken.updateTime();
						//Receive a command and push it to remote device
						await receiveCommandAsync(websocket,controlToken,sendingHandler,2048,cancellationToken);
					}
					//Time overdue
					await stopConnectionAsync("Lost Control", socketFinishedTcs, cancellationToken, websocket, controlToken);
				}
				else {
					//Send no match
					ArraySegment<byte> foundBytes = new ArraySegment<byte>(Encoding.UTF8.GetBytes("no match"));
					await websocket.SendAsync(foundBytes, WebSocketMessageType.Text, true, cancellationToken);
					//End socket exchange
					await stopConnectionAsync("Could not find device", socketFinishedTcs, cancellationToken, websocket);
					return;
				}
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
		private async Task<(bool, ClientInformation, RemoteDevice<T, U>)> getDeviceInfoAsync(WebSocket websocket, CancellationToken token) {
			//Send start signal
			byte[] startBytes = Encoding.UTF8.GetBytes("start");
			ArraySegment<byte> startSegment = new ArraySegment<byte>(startBytes);
			await websocket.SendAsync(startSegment, WebSocketMessageType.Text, true, token);

			byte[] nameBuffer = new byte[1024];
			byte[] locationBuffer = new byte[1024];
			byte[] typeBuffer = new byte[1024];
			byte[] subnameBuffer = new byte[1024];

			//Get name of wanted device
			ArraySegment<byte> nameSegment = new ArraySegment<byte>(nameBuffer);
			await websocket.ReceiveAsync(nameSegment, token);
			string name = Encoding.UTF8.GetString(nameSegment).TrimEnd('\0');

			//Get location of wanted device
			ArraySegment<byte> locationSegment = new ArraySegment<byte>(locationBuffer);
			await websocket.ReceiveAsync(locationSegment, token);
			string location = Encoding.UTF8.GetString(locationSegment).TrimEnd('\0');

			//Get type of device
			ArraySegment<byte> typeSegment = new ArraySegment<byte>(typeBuffer);
			await websocket.ReceiveAsync(typeSegment, token);
			string type = Encoding.UTF8.GetString(typeSegment).TrimEnd('\0');

			//Get subname representing what part of the device u want
			ArraySegment<byte> subnameSegment = new ArraySegment<byte>(subnameBuffer);
			await websocket.ReceiveAsync(subnameSegment, token);
			string subname = Encoding.UTF8.GetString(subnameSegment).TrimEnd('\0');

			//Check if device exists
			bool found = false;
			RemoteDevice<T, U> foundDevice = null;
			//Try to find the requested device
			if (remoteDeviceConnections.getRemoteDeviceWithNameLocationAndType(name, location, type, out RemoteDevice<T, U> outputDevice)) {
				//Device was found, check if the sub name exists on the device
				foundDevice = outputDevice;

				List<string> listOfSubNames = foundDevice.getSubNamesList();

				foreach (var obj in listOfSubNames) {
					if (obj.ToLower().Equals(subname.ToLower())) {
						found = true;
					}
				}
			}

			(bool, ClientInformation, RemoteDevice<T, U>) result = (found, new ClientInformation(name, location, type, subname), foundDevice);

			return result;
		}


#endregion

#region Enter queue


		/// <summary>
		/// Enter the queue for the Control handler that matches the client information
		/// </summary>
		/// <param name="device">Device with the controller u want to queue for</param>
		/// <param name="clientInformation">Information with the subname of the correct sending connection</param>
		/// <returns></returns>
		private (bool, SendingControlHandler<U>, ControlToken) enterQueue(RemoteDevice<T, U> device, ClientInformation clientInformation) {
			//Check if device is found
			if (device.getSendingControlHandlerWithSubname(clientInformation.SubName, out SendingControlHandler<U> output)) {
				//enter the queue and get control token
				ControlToken controlToken = output.enterQueue();
				return (true, output, controlToken);
			}
			else {
				return (false, default, default);
			}
		}

#endregion

#region Handle commands received

		/// <summary>
		/// Receive a command from the websocket
		/// </summary>
		/// <param name="websocket"></param>
		/// <param name="controlToken"></param>
		/// <param name="outputConnection"></param>
		/// <param name="bufferSize">Array size reserved for incoming message</param>
		/// <param name="token"></param>
		private async Task receiveCommandAsync(WebSocket websocket, ControlToken controlToken, SendingControlHandler<U> outputConnection,int bufferSize, CancellationToken token) {
			//Receive a command from the socket
			ArraySegment<byte> receivedArraySegment = new ArraySegment<byte>(new byte[bufferSize]);
			await websocket.ReceiveAsync(receivedArraySegment, token);
			string receivedJson = Encoding.UTF8.GetString(receivedArraySegment).TrimEnd('\0');

			try {
				//Deserialize into U and queue for sending back to the connection
				U newObject = JsonSerializer.Deserialize<U>(receivedJson);
				//attempt to send
				outputConnection.trySend(newObject, controlToken);
			}
			catch (Exception e) {
				logger.LogWarning(e, "Error happened in Json Serializing for crestronWebsocket");
				throw;
			}
		}

#endregion

#region Websocket actions

		/// <summary>
		/// Stops the connection, and signals the connecting websocket
		/// </summary>
		/// <param name="statusDescription"></param>
		/// <param name="socketFinishedTcs"></param>
		/// <param name="cancellationToken"></param>
		/// <param name="webSocket"></param>
		/// <param name="controlToken"></param>
		private async Task stopConnectionAsync(string statusDescription, TaskCompletionSource<object> socketFinishedTcs, CancellationToken cancellationToken, WebSocket webSocket, ControlToken controlToken = null) {
			//If control token is passed, set as inactive to move the queue along
			if (controlToken is not null) {
				controlToken.isInactive = true;
			}
			//Close websocket
			await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, statusDescription, cancellationToken);
			//signal connection is over
			socketFinishedTcs.TrySetResult(new object());
		}

		private async Task sendStringWithWebsocketAsync(string msg, WebSocket websocket, CancellationToken token) {
			//Send
			ArraySegment<byte> sendingBytes = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
			await websocket.SendAsync(sendingBytes, WebSocketMessageType.Text, true, token);
		}

#endregion
	}
}