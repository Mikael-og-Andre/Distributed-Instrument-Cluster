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
using Blazor_Instrument_Cluster.Server.ControlHandler;
using Blazor_Instrument_Cluster.Shared;

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
					
					//Get a control token
					if (remoteDevice.getControlTokenForDevice(info.SubName, out ControlToken<U> output)) {
						ControlToken<U> controlToken = output;
						//Update that the control token was found
						await sendStringWithWebsocketAsync("found controller", websocket, cancellationToken);

						while (!cancellationToken.IsCancellationRequested) {
							//Break loop if token is abandoned
							if (controlToken.isInactive) {
								break;
							}

							//if the token has control, start sending
							if (controlToken.hasControl) {
								//Send controlling statement
								await sendStringWithWebsocketAsync("controlling".ToLower(), websocket, cancellationToken);
								
								//Send while having control
								while (controlToken.hasControl) {

									byte[] receivedBytes = new byte[1024];
									ArraySegment<byte> receivedSegment = new ArraySegment<byte>(receivedBytes);
									await websocket.ReceiveAsync(receivedSegment, cancellationToken);
									string receivedString = Encoding.UTF8.GetString(receivedSegment).TrimEnd('\0');

									//Try to deserialize the incoming object
									try {
										//Deserialize
										U receivedObject = JsonSerializer.Deserialize<U>(receivedString);
										//Send
										bool sent = controlToken.send(receivedObject);
									}
									catch (Exception) {
										logger.LogWarning("Deserialization in CrestronWebsocket Failed");
									}

								}
								//End token
								controlToken.abandon();
							}

							//get position
							int pos = controlToken.getPosition();
							//check if -1, that means the device was not found in the queue or as a controller
							if (pos==-1) {
								await stopConnectionAsync("No longer in queue", socketFinishedTcs, cancellationToken,
									websocket, controlToken);
								break;
							}
							//Update position
							await sendStringWithWebsocketAsync("" + pos, websocket, cancellationToken);

						}
					}
					else {
						//Controller not found
						await sendStringWithWebsocketAsync("no controller",websocket,cancellationToken);
					}
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

			byte[] deviceBuffer = new byte[4098];
			
			//Get device info
			ArraySegment<byte> deviceSeg = new ArraySegment<byte>(deviceBuffer);
			await websocket.ReceiveAsync(deviceSeg, token);
			string deviceJson = Encoding.UTF8.GetString(deviceSeg).TrimEnd('\0');

			RequestConnectionModel deviceInfo = null;

			try {
				//Deserialize from json
				deviceInfo = JsonSerializer.Deserialize<RequestConnectionModel>(deviceJson);
			}
			catch (Exception) {
				//failed
				return (false, null, null);
			}

			//Check if device exists
			bool found = false;
			RemoteDevice<T, U> foundDevice = null;
			//Try to find the requested device
			if (remoteDeviceConnections.getRemoteDeviceWithNameLocationAndType(deviceInfo.name, deviceInfo.location, deviceInfo.type, out RemoteDevice<T, U> outputDevice)) {
				//Device was found, check if the sub name exists on the device
				foundDevice = outputDevice;

				List<string> listOfSubNames = foundDevice.getSubNamesList();

				foreach (var obj in listOfSubNames) {
					if (obj.ToLower().Equals(deviceInfo.subname.ToLower())) {
						found = true;
					}
				}
			}

			(bool, ClientInformation, RemoteDevice<T, U>) result = (found, new ClientInformation(deviceInfo.name, deviceInfo.location, deviceInfo.type, deviceInfo.subname), foundDevice);

			return result;
		}

		#endregion Startup information sharing

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