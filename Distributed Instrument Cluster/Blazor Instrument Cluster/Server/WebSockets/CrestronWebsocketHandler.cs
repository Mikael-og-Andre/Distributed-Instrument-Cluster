using Blazor_Instrument_Cluster.Server.CrestronControl;
using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement;
using Blazor_Instrument_Cluster.Shared;
using Blazor_Instrument_Cluster.Shared.Websocket;
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

			//Variables
			RemoteDevice remoteDevice = default;
			SubConnection subConnection = default;
			ControllerInstance controllerInstance = default;
			DateTime disconnectTime = DateTime.Now;//gets updated
			const int inactiveMinutes = 2;
			const int timeToWaitBeforeDisconnectingMin = 10;

			try {
				while (runnning) {
					//Check state of the connection
					bool closing=await isSocketClosing(websocket, cancellationTokenSource.Token);

					//Close
					if (closing) {
						runnning = false;
						continue;
					}

					switch (currentState) {
						case CrestronWebsocketState.Requesting:
							//Handle requesting device
							(CrestronWebsocketState currentState, RemoteDevice remoteDevice, SubConnection subConnection) handleRequestResult
								= await handleRequesting(websocket, cancellationTokenSource.Token);

							currentState = handleRequestResult.currentState;
							//update connections
							remoteDevice = handleRequestResult.remoteDevice;
							subConnection = handleRequestResult.subConnection;
							break;

						case CrestronWebsocketState.InQueue:
							//Check that conditions for being in queue are met
							if (remoteDevice is null || subConnection is null) {
								currentState = CrestronWebsocketState.Requesting;
								logger.LogDebug("Switch InQueue: requirements for being in queue are not met, returning to Requesting");
								break;
							}
							//Handle InQueue device
							(CrestronWebsocketState state, ControllerInstance instance) handleInQueueResult = await handleInQueue(websocket, cancellationTokenSource.Token, remoteDevice, subConnection);
							currentState = handleInQueueResult.state;
							controllerInstance = handleInQueueResult.instance;
							break;

						case CrestronWebsocketState.InControl:
							//Check that conditions for being in control are met
							if (remoteDevice is null || subConnection is null) {
								currentState = CrestronWebsocketState.Requesting;
								logger.LogDebug("Switch InControl: requirements for being in control are not met, returning to Requesting");
								break;
							}
							else if (controllerInstance is null) {
								currentState = CrestronWebsocketState.InQueue;
								logger.LogDebug("Switch InControl: requirements for being in control are not met, returning to InQueue");
								break;
							}
							//Handle in control device
							currentState = await handleInControl(controllerInstance, inactiveMinutes, websocket, cancellationTokenSource.Token);
							break;

						case CrestronWebsocketState.TimeLimitExceeded:
							(CrestronWebsocketState state,DateTime disconnectTime) handleTimeLimitExceededResult = await handleTimeLimitExceeded(timeToWaitBeforeDisconnectingMin,websocket, cancellationTokenSource.Token);
							currentState = handleTimeLimitExceededResult.state;
							disconnectTime = handleTimeLimitExceededResult.disconnectTime;
							break;

						case CrestronWebsocketState.WaitingForInput:
							currentState = await handleWaitingForInput(disconnectTime,websocket, cancellationTokenSource.Token);
							break;

						case CrestronWebsocketState.Disconnecting:
							await handleDisconnecting(websocket, cancellationTokenSource.Token);
							runnning = false;
							break;

						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}
			catch (Exception ex) {
				logger.LogError("Error in crestron websocket backend", ex);
			}

			//Disconnect task
			socketFinishedTcs.SetResult(new object());

		}

		/// <summary>
		/// Check websocket states
		/// If not open or connecting, the websocket is closing
		/// </summary>
		/// <param name="websocket"></param>
		/// <param name="ct"></param>
		/// <returns>True if socket is closing </returns>
		private async Task<bool> isSocketClosing(WebSocket websocket, CancellationToken ct) {
			switch (websocket.State) {
				case WebSocketState.None:
					return true;

				case WebSocketState.Connecting:
					return false;

				case WebSocketState.Open:
					return false;

				case WebSocketState.CloseSent:
					return true;

				case WebSocketState.CloseReceived:
					await websocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "Received close", ct);
					logger.LogDebug("handleWebsocketStateAsync: " + websocket.CloseStatusDescription);
					return true;

				case WebSocketState.Closed:
					return true;

				case WebSocketState.Aborted:
					return true;

				default:
					return true;
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
		private async Task<(CrestronWebsocketState state, RemoteDevice remoteDevice, SubConnection subConnection)> handleRequesting(WebSocket webSocket, CancellationToken ct) {
			try {
				//Send requesting state
				await sendString(webSocket, CrestronWebsocketState.Requesting.ToString(), ct);

				//get confirmation state
				string confState = await receiveString(webSocket, ct);

				//if returned state is not current state disconnect
				if (!confState.Equals(CrestronWebsocketState.Requesting.ToString())) {
					await sendString(webSocket, "closing", ct);
					return (CrestronWebsocketState.Disconnecting, default, default);
				}
				//send id request
				await sendString(webSocket, "id", ct);
				//get device info
				string wantedDevice = await receiveString(webSocket, ct);
				string wantedSubDevice = await receiveString(webSocket, ct);
				DeviceModel device = JsonSerializer.Deserialize<DeviceModel>(wantedDevice);
				SubConnectionModel subConnection = JsonSerializer.Deserialize<SubConnectionModel>(wantedSubDevice);

				//check if device exists, and get the device
				(bool found, RemoteDevice remoteDevice, SubConnection subConnection) findDeviceResult
					= findDevice(device, subConnection);

				//If not found
				if (!findDeviceResult.found) {
					await sendString(webSocket, "closing", ct);
					return (CrestronWebsocketState.Disconnecting, default, default);
				}
				//Send found true
				await sendString(webSocket, "True", ct);

				return (CrestronWebsocketState.InQueue, findDeviceResult.remoteDevice, findDeviceResult.subConnection);
			}
			catch (Exception ex) {
				logger.LogDebug(ex, "Error in handleRequesting");
				return (CrestronWebsocketState.Disconnecting, default, default);
			}
		}

		/// <summary>
		/// handle In Queue state
		/// Get a controller instance for the connection
		/// Check if client already has a control token
		/// update the position in queue to the client
		/// </summary>
		/// <param name="webSocket"></param>
		/// <param name="ct"></param>
		/// <param name="remoteDevice"></param>
		/// <param name="subConnection"></param>
		/// <returns></returns>
		private async Task<(CrestronWebsocketState state, ControllerInstance controllerInstance)> handleInQueue(WebSocket webSocket, CancellationToken ct, RemoteDevice remoteDevice, SubConnection subConnection) {
			try {
				//confirm state
				string confState = await receiveString(webSocket, ct);

				//if returned state is not current state disconnect
				if (!confState.Equals(CrestronWebsocketState.InQueue.ToString())) {
					await sendString(webSocket, "closing", ct);
					return (CrestronWebsocketState.Disconnecting, default);
				}
				//Send Request for token
				await sendString(webSocket, "Token", ct);

				//Receive if the client ahs a token already or not
				string hasToken = await receiveString(webSocket, ct);

				ControllerInstance instance = default;

				if (hasToken.Equals("False")) {
					//Create new controller instance and token for client
					bool success = remoteDevice.createControllerInstance(subConnection, out ControllerInstance controllerInstance);

					if (!success) {
						logger.LogDebug("handleInQueue: subConnection not found, failed to create a new controller instance");
						await sendString(webSocket, "closing", ct);
						return (CrestronWebsocketState.Disconnecting, default);
					}

					instance = controllerInstance;
					var newControlToken = controllerInstance.controlToken;
					//Send new token
					string tokenJson = JsonSerializer.Serialize(newControlToken);
					await sendString(webSocket, tokenJson, ct);
					//Send confirm
					await sendString(webSocket, "Confirmed", ct);
				}
				else if (hasToken.Equals("True")) {
					//receive token
					string tokenJson = await receiveString(webSocket, ct);
					ControlToken controlToken = JsonSerializer.Deserialize<ControlToken>(tokenJson);
					//get existing controller
					bool success = remoteDevice.getControllerInstance(controlToken, out ControllerInstance controllerInstance);

					if (!success) {
						logger.LogDebug("handleInQueue: controlToken not found, failed to find controller instance");
						await sendString(webSocket, "closing", ct);
						return (CrestronWebsocketState.Disconnecting, default);
					}

					instance = controllerInstance;
					//send confirmed
					await sendString(webSocket, "Confirmed", ct);
				}
				else {
					await sendString(webSocket, "Closing", ct);
					return (CrestronWebsocketState.Disconnecting, default);
				}

				//send entering queue
				await sendString(webSocket, "Entering Queue", ct);

				//receive confirm
				string queueConfirm = await receiveString(webSocket, ct);

				if (!queueConfirm.Equals("True")) {
					logger.LogDebug("handleInQueue: queueConfirm, Protocol error");
					await sendString(webSocket, "closing", ct);
					return (CrestronWebsocketState.Disconnecting, default);
				}

				bool inQueue = true;

				while (inQueue) {
					//check if client left
					if (webSocket.State != WebSocketState.Open) {
						return (CrestronWebsocketState.Disconnecting, default);
					}
					//get position
					int position = instance.getPosition();

					if (position < 0) {
						logger.LogDebug("handleInQueue: position was not found, error in ControllerInstance");
						await sendString(webSocket, "closing", ct);
						return (CrestronWebsocketState.Disconnecting, default);
					}
					else if (position == 0) {
						await sendString(webSocket, "Complete", ct);
						inQueue = false;
					}
					else {
						await sendString(webSocket, position.ToString(), ct);
					}
				}

				return (CrestronWebsocketState.InControl, instance);
			}
			catch (Exception ex) {
				logger.LogDebug(ex, "Error in handleInQueue");
				return (CrestronWebsocketState.Disconnecting, default);
			}
		}

		/// <summary>
		/// Handle in control state
		/// Send commands from the client to the remote device
		/// </summary>
		/// <param name="controller"></param>
		/// <param name="inactiveTime"></param>
		/// <param name="webSocket"></param>
		/// <param name="ct"></param>
		/// <returns></returns>
		private async Task<CrestronWebsocketState> handleInControl(ControllerInstance controller, int inactiveTime, WebSocket webSocket, CancellationToken ct) {
			try {
				//Send state
				await sendString(webSocket, CrestronWebsocketState.InControl.ToString(), ct);

				//receive state confirmation
				string confState = await receiveString(webSocket, ct);

				//check confirmation
				if (!confState.Equals(CrestronWebsocketState.InControl.ToString())) {
					logger.LogDebug("handleInControl: state confirmation failed");
					return CrestronWebsocketState.Disconnecting;
				}

				bool controlling = controller.isControlling();

				if (controlling) {
					//send start Signal
					await sendString(webSocket, "Start", ct);
				}
				else {
					//send wrong and return to queue
					logger.LogDebug("handleInControl: not controlling");
					await sendString(webSocket, "not controlling", ct);
					return CrestronWebsocketState.InQueue;
				}

				CancellationTokenSource timeLimitCancellationTokenSource = new CancellationTokenSource();
				bool inControl = true;
				bool timeLimitExceeded = false;
				DateTime lastAction = DateTime.UtcNow;

				//Check that messages are being received
				Task checkForInactiveTime = Task.Run(async () => {
					while (!timeLimitExceeded) {
						//Check if timeLimit is exceeded
						if (lastAction < DateTime.Now.AddMinutes(inactiveTime)) {
							timeLimitExceeded = true;
							timeLimitCancellationTokenSource.Cancel();
						}
						await Task.Delay(1000, ct);
					}
				});

				//receive messages
				while (inControl) {
					//Check if remote client disconnected
					if (webSocket.State != WebSocketState.Open) {
						return CrestronWebsocketState.Disconnecting;
					}
					//Check if used to long since last action
					if (timeLimitExceeded) {
						await sendString(webSocket, "stopping", ct);
						break;
					}

					//Get msg from client
					string msg = await receiveString(webSocket, timeLimitCancellationTokenSource.Token);
					bool sent = await controller.send(msg, timeLimitCancellationTokenSource.Token);
					lastAction = DateTime.UtcNow;
				}

				return CrestronWebsocketState.TimeLimitExceeded;
			}
			catch (Exception ex) {
				logger.LogDebug(ex, "Error in handleInControl");
				return CrestronWebsocketState.Disconnecting;
			}
		}

		/// <summary>
		/// Handle the Time Limit exceeded state
		/// Sends time the websocket will be in the waiting for input state to the client
		/// </summary>
		/// <param name="waitingTimeMin"></param>
		/// <param name="webSocket"></param>
		/// <param name="ct"></param>
		/// <returns></returns>
		private async Task<(CrestronWebsocketState, DateTime disconnectTime)> handleTimeLimitExceeded(int waitingTimeMin, WebSocket webSocket, CancellationToken ct) {
			try {
				//Send state
				await sendString(webSocket, CrestronWebsocketState.TimeLimitExceeded.ToString(), ct);
				//confirm state
				string confState = await receiveString(webSocket, ct);
				//Check if correct state
				if (!confState.Equals(CrestronWebsocketState.TimeLimitExceeded.ToString())) {
					logger.LogDebug("handleTimeLimitExceeded: wrong state, protocol error");
					return (CrestronWebsocketState.Disconnecting, DateTime.Now);
				}
				DateTime disconnectTime = DateTime.Now.AddMinutes(waitingTimeMin);
				//Send time of disconnection
				await sendString(webSocket, disconnectTime.ToString(), ct);

				return (CrestronWebsocketState.WaitingForInput, disconnectTime);
			}
			catch (Exception ex) {
				logger.LogDebug(ex, "Error in handleTimeLimitExceeded");
				return (CrestronWebsocketState.Disconnecting, DateTime.Now);
			}
		}

		/// <summary>
		/// Handle waiting for input state
		/// If the time until disconnection is passed before a message is received Disconnect
		/// If a message is received before time runs out, go back to the InQueue State
		/// </summary>
		/// <param name="disconnectTime"></param>
		/// <param name="webSocket"></param>
		/// <param name="ct"></param>
		/// <returns></returns>
		private async Task<CrestronWebsocketState> handleWaitingForInput(DateTime disconnectTime, WebSocket webSocket, CancellationToken ct) {
			try {
				//Send state
				await sendString(webSocket, CrestronWebsocketState.WaitingForInput.ToString(), ct);
				//Confirm state
				string confState = await receiveString(webSocket, ct);

				if (!confState.Equals(CrestronWebsocketState.WaitingForInput.ToString())) {
					return CrestronWebsocketState.Disconnecting;
				}

				//Check if time has ran out
				Task ranOutOfTime = Task.Run(async () => {
					bool checkingForTime = true;
					while (checkingForTime) {
						//Check if passed disconnect time
						await Task.Delay(1000, ct);
						if (disconnectTime < DateTime.Now) {
							checkingForTime = false;
						}
					}
				});

				//Cancellation token for listener
				CancellationTokenSource sourceToken = new CancellationTokenSource();
				CancellationToken listenerCancellationToken = sourceToken.Token;

				//Wait for input from client
				Task listenForInput = Task.Run(async () => {
					string conf = await receiveString(webSocket, listenerCancellationToken);
				});
				//Wait for any of the tasks to complete
				await Task.WhenAny(ranOutOfTime, listenForInput);

				//If ran oput of time Disconnect
				if (ranOutOfTime.IsCompleted) {
					//stop listener task
					sourceToken.Cancel();
					//ran out of time
					return CrestronWebsocketState.Disconnecting;
				}
				//Received input, go back to queue
				else if (listenForInput.IsCompleted) {
					//return to queue
					return CrestronWebsocketState.InQueue;
				}
				else {
					//Stop listener task
					sourceToken.Cancel();
					//Disconnect
					return CrestronWebsocketState.Disconnecting;
				}
			}
			catch (Exception ex) {
				logger.LogDebug(ex, "Error in handleWaitingForInput");
				return CrestronWebsocketState.Disconnecting;
			}
		}

		/// <summary>
		/// Handle disconnecting the socket
		/// </summary>
		/// <param name="webSocket"></param>
		/// <param name="ct"></param>
		/// <returns></returns>
		private async Task handleDisconnecting(WebSocket webSocket, CancellationToken ct) {
			try {
				await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,"Disconnecting",ct);
			}
			catch (Exception ex) {
				logger.LogDebug(ex, "Error in handleDisconnected");
			}
		}

		/// <summary>
		/// Send a string via websocket
		/// </summary>
		/// <param name="webSocket"></param>
		/// <param name="s"></param>
		/// <param name="ct"></param>
		/// <returns></returns>
		private async Task sendString(WebSocket webSocket, string s, CancellationToken ct) {
			byte[] bytes = Encoding.UTF32.GetBytes(s);
			await webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
		}

		/// <summary>
		/// Receive a string via websocket
		/// </summary>
		/// <param name="webSocket"></param>
		/// <param name="ct"></param>
		/// <returns></returns>
		private async Task<string> receiveString(WebSocket webSocket, CancellationToken ct) {
			byte[] sizeBytes = new byte[sizeof(int)];
			await webSocket.ReceiveAsync(sizeBytes, ct);
			int size = BitConverter.ToInt32(sizeBytes);
			byte[] stringBytes = new byte[size];
			await webSocket.ReceiveAsync(stringBytes, ct);
			return Encoding.UTF32.GetString(stringBytes);
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
	}
}