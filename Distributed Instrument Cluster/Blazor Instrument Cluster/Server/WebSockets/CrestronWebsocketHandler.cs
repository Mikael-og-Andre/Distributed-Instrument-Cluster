using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement;
using Blazor_Instrument_Cluster.Shared;
using Blazor_Instrument_Cluster.Shared.Websocket.Enum;
using Microsoft.Extensions.Logging;
using System;
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
			bool runnning = true;

			RemoteDevice remoteDevice = default;
			SubConnection subConnection = default;

			try {
				while (runnning) {
					//Check state of the connection
					await handleWebsocketStateAsync(websocket, cancellationTokenSource.Token);

					switch (currentState) {
						case CrestronWebsocketState.Requesting:
							(CrestronWebsocketState currentState, RemoteDevice remoteDevice, SubConnection subConnection) handleRequestResult 
								= await handleRequesting(websocket, cancellationTokenSource.Token);

							currentState = handleRequestResult.currentState;
							//update connections
							remoteDevice = handleRequestResult.remoteDevice;
							subConnection = handleRequestResult.subConnection;
							break;

						case CrestronWebsocketState.InQueue:
							currentState = await handleInQueue(websocket, cancellationTokenSource.Token);
							break;

						case CrestronWebsocketState.InControl:
							currentState = await handleInControl(websocket, cancellationTokenSource.Token);
							break;

						case CrestronWebsocketState.WaitingForInput:
							currentState = await handleWaitingForInput(websocket, cancellationTokenSource.Token);
							break;

						case CrestronWebsocketState.TimeLimitExceeded:
							currentState = await handleTimeLimitExceeded(websocket, cancellationTokenSource.Token);
							break;

						case CrestronWebsocketState.Disconnecting:
							await handleDisconnected(websocket, cancellationTokenSource.Token);
							break;

						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}
			catch (Exception ex) {
				logger.LogError("Error in crestron websocket backend", ex);
			}
		}

		private async Task handleWebsocketStateAsync(WebSocket websocket, CancellationToken ct) {
			switch (websocket.State) {
				case WebSocketState.None:
					break;

				case WebSocketState.Connecting:
					break;

				case WebSocketState.Open:
					break;

				case WebSocketState.CloseSent:
					break;

				case WebSocketState.CloseReceived:
					await websocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "Received close", ct);
					break;

				case WebSocketState.Closed:
					break;

				case WebSocketState.Aborted:
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		/// Handle the requesting state
		/// Share id of the device wanted by the client
		/// Confirm if the device exists on the server
		/// </summary>
		/// <param name="webSocket"></param>
		/// <param name="ct"></param>
		/// <returns>The next state, and the RemoteDevice and SubConnection</returns>
		private async Task<(CrestronWebsocketState state,RemoteDevice remoteDevice,SubConnection subConnection)> handleRequesting(WebSocket webSocket, CancellationToken ct) {
			try {
				//Send requesting state
				await sendString(webSocket, CrestronWebsocketState.Requesting.ToString(), ct);

				//get confirmation state
				string confState = await receiveString(webSocket, ct);

				//if returned state is not current state disconnect
				if (!confState.Equals(CrestronWebsocketState.Requesting.ToString())) {
					await sendString(webSocket, "closing", ct);
					return (CrestronWebsocketState.Disconnecting,default,default);
				}
				//send id request
				await sendString(webSocket, "id", ct);
				//get id
				string wantedDevice = await receiveString(webSocket, ct);
				string wantedSubDevice = await receiveString(webSocket, ct);

				DeviceModel device = JsonSerializer.Deserialize<DeviceModel>(wantedDevice);
				SubConnectionModel subConnection = JsonSerializer.Deserialize<SubConnectionModel>(wantedSubDevice);

				(bool found, RemoteDevice remoteDevice, SubConnection subConnection) findDeviceResult = findDevice(device, subConnection);

				//If not found
				if (!findDeviceResult.found) {
					await sendString(webSocket, "closing", ct);
					return (CrestronWebsocketState.Disconnecting,default,default);
				}
				//Send found true
				await sendString(webSocket,"True",ct);
				

				return (CrestronWebsocketState.InQueue,findDeviceResult.remoteDevice,findDeviceResult.subConnection);
			}
			catch (Exception ex) {
				logger.LogDebug(ex, "Error in handleRequesting");
				return (CrestronWebsocketState.Disconnecting,default,default);
			}
		}

		/// <summary>
		/// Check remote device manager for a matching device and subconnection
		/// </summary>
		/// <param name="device"></param>
		/// <param name="subConnection"></param>
		/// <returns>Found bool, RemoteDevice that matched, SubConnection that matched</returns>
		private (bool found, RemoteDevice remoteDevice, SubConnection subConnection) findDevice(DeviceModel device, SubConnectionModel subConnection) {
			//get list of devices from remote device manager
			var listOfRemoteDevices = remoteDeviceManager.getListOfRemoteDevices();
			//not found return
			(bool, RemoteDevice, SubConnection) result = (false, default, default);

			foreach (var remoteDevice in listOfRemoteDevices) {
				//if same device, check sub connections
				if (remoteDevice.name.Equals(device.name)
					&& remoteDevice.location.Equals(device.location)
					&& remoteDevice.type.Equals(device.type)) {
					var subConnections = remoteDevice.getListOfSubConnections();
					//check if wanted sub
					foreach (var cons in subConnections) {
						//check if same connection
						if (cons.id.Equals(subConnection.guid)) {
							//Connection found, return
							return (true, remoteDevice, cons);
						}
					}
					//Found device but not sub device, stop loop
					logger.LogDebug("findDevice: Remote device matched, but no sub connections matched");
					break;
				}
			}
			logger.LogDebug("findDevice: No matching Devices");
			return result;
		}

		private async Task<CrestronWebsocketState> handleInQueue(WebSocket webSocket, CancellationToken ct) {
			try {
				return CrestronWebsocketState.InControl;
			}
			catch (Exception ex) {
				logger.LogDebug(ex, "Error in handleInQueue");
				return CrestronWebsocketState.Disconnecting;
			}
		}

		private async Task<CrestronWebsocketState> handleInControl(WebSocket webSocket, CancellationToken ct) {
			try {
				return CrestronWebsocketState.TimeLimitExceeded;
			}
			catch (Exception ex) {
				logger.LogDebug(ex, "Error in handleInControl");
				return CrestronWebsocketState.Disconnecting;
			}
		}

		private async Task<CrestronWebsocketState> handleTimeLimitExceeded(WebSocket webSocket, CancellationToken ct) {
			try {
				return CrestronWebsocketState.WaitingForInput;
			}
			catch (Exception ex) {
				logger.LogDebug(ex, "Error in handleTimeLimitExceeded");
				return CrestronWebsocketState.Disconnecting;
			}
		}

		private async Task<CrestronWebsocketState> handleWaitingForInput(WebSocket webSocket, CancellationToken ct) {
			try {
				return CrestronWebsocketState.InQueue;
			}
			catch (Exception ex) {
				logger.LogDebug(ex, "Error in handleWaitingForInput");
				return CrestronWebsocketState.Disconnecting;
			}
		}

		private async Task handleDisconnected(WebSocket webSocket, CancellationToken ct) {
			try {
			}
			catch (Exception ex) {
				logger.LogDebug(ex, "Error in handleDisconnected");
			}
		}

		private async Task sendString(WebSocket webSocket, string s, CancellationToken ct) {
			byte[] bytes = Encoding.UTF32.GetBytes(s);
			await webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
		}

		private async Task<string> receiveString(WebSocket webSocket, CancellationToken ct) {
			byte[] sizeBytes = new byte[sizeof(int)];
			await webSocket.ReceiveAsync(sizeBytes, ct);
			int size = BitConverter.ToInt32(sizeBytes);
			byte[] stringBytes = new byte[size];
			await webSocket.ReceiveAsync(stringBytes, ct);
			return Encoding.UTF32.GetString(stringBytes);
		}
	}
}