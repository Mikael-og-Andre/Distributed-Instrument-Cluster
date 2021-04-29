using Blazor_Instrument_Cluster.Shared;
using Blazor_Instrument_Cluster.Shared.Websocket;
using Blazor_Instrument_Cluster.Shared.Websocket.Enum;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Client.Code.Websocket {

	/// <summary>
	/// Class for managing the websocket connection ot the backend
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class CrestronWebsocket : IDisposable {

		/// <summary>
		/// Injected logger class
		/// </summary>
		[Inject] public ILogger<CrestronWebsocket> logger { get; set; }

		/// <summary>
		/// The last state received from the backend
		/// </summary>
		public CrestronWebsocketState lastState { get; set; }

		/// <summary>
		/// Websocket for the connection
		/// </summary>
		public ClientWebSocket webSocket { get; set; }

		/// <summary>
		/// Uri the websocket will connect to
		/// </summary>
		private Uri connectionPath { get; set; }

		/// <summary>
		/// Token with id for controlling a crestron
		/// </summary>
		private ControlToken controlToken { get; set; }

		/// <summary>
		/// Position in queue
		/// </summary>
		public string positionInQueue { get; set; }

		/// <summary>
		/// channel for sending messages
		/// </summary>
		private Channel<string> commandChannel { get; set; }

		private DateTime disconnectTime { get; set; }

		/// <summary>
		/// CrestronWebsocket
		/// </summary>
		/// <param name="connectionPath">Uri the websocket will connect to</param>
		public CrestronWebsocket(Uri connectionPath) {
			this.connectionPath = connectionPath;
			webSocket = new ClientWebSocket();
			this.controlToken = null;
			this.positionInQueue = "Position:";
			commandChannel = Channel.CreateUnbounded<string>();
			this.disconnectTime = DateTime.Now;
		}

		/// <summary>
		/// Connect to the Uri
		/// </summary>
		/// <returns>Task</returns>
		public async Task connectAsync(CancellationToken ct) {
			await webSocket.ConnectAsync(connectionPath, ct);
		}

		public async Task startAsync(DeviceModel deviceModel, SubConnectionModel subdeviceModel, CancellationToken ct) {
			try {
				bool closing = false;

				while (!ct.IsCancellationRequested) {
					//Handle possible states
					await handleWebsocketStatesAsync(webSocket, ct);
					//receive a state from the backend
					string receivedStateString = await receiveString(webSocket, ct);
					CrestronWebsocketState currentState = Enum.Parse<CrestronWebsocketState>(receivedStateString);
					lastState = currentState;

					switch (currentState) {
						case CrestronWebsocketState.Requesting:
							closing |= await handleRequestingAsync(deviceModel, subdeviceModel, webSocket, ct);
							break;

						case CrestronWebsocketState.InQueue:
							closing |= await handleInQueueAsync(webSocket, ct);
							break;

						case CrestronWebsocketState.InControl:
							closing |= await handleInControlAsync(webSocket, ct);
							break;

						case CrestronWebsocketState.TimeLimitExceeded:
							(bool disconnect, DateTime time) handelTimeLimitResult = await handleTimeLimitExceededAsync(webSocket, ct);
							disconnectTime = handelTimeLimitResult.time;
							closing |= handelTimeLimitResult.disconnect;
							break;

						case CrestronWebsocketState.WaitingForInput:
							closing |= await handleWaitingForInputAsync(webSocket, ct);
							break;

						case CrestronWebsocketState.Disconnecting:
							closing |= await handleDisconnectingAsync(webSocket, ct);
							break;

						default:
							throw new ArgumentOutOfRangeException();
					}

					if (closing) {
						logger.LogDebug("Closing the connection");
						await closeWebsocketAsync(webSocket, "Protocol not followed", ct);
					}
				}
			}
			catch (Exception e) {
				logger.LogDebug("startAsync", e);

				throw;
			}
		}

		/// <summary>
		/// Handle the requesting state
		/// Share id of the device wanted by the client
		/// Confirm if the device exists on the server
		/// </summary>
		/// <param name="subConnectionModel"></param>
		/// <param name="clientWebSocket"></param>
		/// <param name="ct"></param>
		/// <param name="deviceModel"></param>
		/// <returns>Should the connection close, True = yes</returns>
		private async Task<bool> handleRequestingAsync(DeviceModel deviceModel, SubConnectionModel subConnectionModel, ClientWebSocket clientWebSocket, CancellationToken ct) {
			//Confirm Requesting
			await sendString(clientWebSocket, CrestronWebsocketState.Requesting.ToString(), ct);
			//Get request for id
			string requestId = await receiveString(clientWebSocket, ct);
			//If req does not match expected string, close
			if (!requestId.Equals("id")) {
				logger.LogDebug("handleRequestingAsync: requestId, protocol error");
				return true;
			}
			//Send Device
			string wantedDevice = JsonSerializer.Serialize(deviceModel);
			string wantedSubDevice = JsonSerializer.Serialize(subConnectionModel);
			await sendString(clientWebSocket, wantedDevice, ct);
			await sendString(clientWebSocket, wantedSubDevice, ct);

			//Confirm if the device is found or not
			string found = await receiveString(clientWebSocket, ct);
			//if the returned string isn't true, close connection
			if (!found.Equals("True")) {
				logger.LogDebug("handleRequestingAsync: found, protocol error");
				return true;
			}

			//no issues, return false for not closing connection
			return false;
		}

		/// <summary>
		/// Handle the in Queue state
		/// Get a control token representing the connection
		/// Or send one if you had one from a previous step
		/// Get updated queue position from the server
		/// </summary>
		/// <param name="clientWebSocket"></param>
		/// <param name="ct"></param>
		/// <returns></returns>
		private async Task<bool> handleInQueueAsync(ClientWebSocket clientWebSocket, CancellationToken ct) {
			//confirm state
			await sendString(clientWebSocket, CrestronWebsocketState.InQueue.ToString(), ct);

			//Receive request for token
			string token = await receiveString(clientWebSocket, ct);

			//check for correct response
			if (!token.Equals("Token")) {
				logger.LogDebug("handleInQueue: token, protocol error");
				return true;
			}

			//send true if you have a token, false if not
			if (controlToken is null) {
				//Get a token
				await sendString(clientWebSocket, "False", ct);
				string controlTokenJson = await receiveString(clientWebSocket, ct);
				//check if error occurred
				if (controlTokenJson.Equals("closing")) {
					logger.LogDebug("handleInQueue: token, protocol error");
					return true;
				}
				//set new control token
				controlToken = JsonSerializer.Deserialize<ControlToken>(controlTokenJson);
			}
			else {
				//Send token
				await sendString(clientWebSocket, "True", ct);
				string controlTokenJson = JsonSerializer.Serialize(controlToken);
				await sendString(clientWebSocket, controlTokenJson, ct);
			}

			//receive confirmation of token
			string confToken = await receiveString(clientWebSocket, ct);

			//Check for correct response
			if (!confToken.Equals("Confirmed")) {
				logger.LogDebug("handleInQueue: confirm token, protocol error");
				return true;
			}

			//Get signal that we are entering the queue
			string enteringQueue = await receiveString(clientWebSocket, ct);

			//Check for correct response
			if (!enteringQueue.Equals("Entering queue")) {
				logger.LogDebug("handleInQueue: entering queue, protocol error");
				return true;
			}

			//Send queue confirmation
			await sendString(clientWebSocket, "True", ct);

			bool inQueue = true;

			while (inQueue) {
				if (webSocket.State != WebSocketState.Open) {
					logger.LogDebug("handleInQueue: Socket closed while in queue");
					return true;
				}

				//Get queue position or end signal
				string queuePos = await receiveString(clientWebSocket, ct);
				//Check if end signal
				if (queuePos.Equals("Complete")) {
					inQueue = false;
					continue;
				}
				//Update position
				positionInQueue = "Position: " + queuePos;
			}

			return false;
		}

		/// <summary>
		/// handleInControl state
		/// Sends commands from the channel to the backend
		/// </summary>
		/// <param name="clientWebSocket"></param>
		/// <param name="ct"></param>
		/// <returns></returns>
		private async Task<bool> handleInControlAsync(ClientWebSocket clientWebSocket, CancellationToken ct) {
			//Send InControl state confirmation
			await sendString(clientWebSocket, CrestronWebsocketState.InControl.ToString(), ct);

			//Receive start signal
			string startSignal = await receiveString(clientWebSocket, ct);

			//Check if correct signal
			if (startSignal.Equals("Start")) {
				//do nothing
			}
			else if (startSignal.Equals("not controlling")) {
				//return normally and wait for next state
				logger.LogDebug("handleInControl: not in control");
				return false;
			}
			else {
				logger.LogDebug("handleInControl: startSignal, protocol error");
				return true;
			}

			//InControl
			bool inControl = true;

			//wait for a stop command in separate task
			Task stopListener = Task.Run(async () => {
				string receivedString = await receiveString(clientWebSocket, ct);
				inControl = false;
			});

			var reader = commandChannel.Reader;

			while (inControl) {
				//check if connection is still ok
				if (webSocket.State != WebSocketState.Open) {
					logger.LogDebug("handleInControl: Socket state not open in control loop");
					return true;
				}
				//get bytes from channel
				string toSend = await reader.ReadAsync(ct);
				await sendString(clientWebSocket, toSend, ct);
			}

			return false;
		}

		/// <summary>
		/// Send message to the channel, which is read and sent to the server
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		public async Task<bool> trySendingControlMessage(string msg) {
			//Check if in control
			if (lastState == CrestronWebsocketState.InControl) {
				await commandChannel.Writer.WriteAsync(msg);
				return true;
			}
			//not in control
			return false;
		}

		/// <summary>
		/// handleTimeLimitExceeded
		/// Receives time until backend abandons the connection
		/// </summary>
		/// <param name="clientWebSocket"></param>
		/// <param name="ct"></param>
		/// <returns></returns>
		private async Task<(bool, DateTime)> handleTimeLimitExceededAsync(ClientWebSocket clientWebSocket, CancellationToken ct) {
			//Send time limit state
			await sendString(clientWebSocket, CrestronWebsocketState.TimeLimitExceeded.ToString(), ct);

			string time = await receiveString(clientWebSocket, ct);
			try {
				DateTime timeUntilDisconnect = DateTime.Parse(time);
				return (false, timeUntilDisconnect);
			}
			catch (Exception e) {
				logger.LogDebug("handleTimeLimitExceeded", e);
				return (true, DateTime.Now);
			}
		}

		private async Task<bool> handleWaitingForInputAsync(ClientWebSocket clientWebSocket, CancellationToken ct) {
			//confirm state
			await sendString(clientWebSocket, CrestronWebsocketState.WaitingForInput.ToString(), ct);

			//Clear Channel
			resetChannel();

			//Cancellation token for waiting Task
			CancellationTokenSource waitForInputCanceler = new CancellationTokenSource();
			CancellationToken waitForInputToken = waitForInputCanceler.Token;

			Task waitForInput = Task.Run(async () => {
				await commandChannel.Reader.ReadAsync(waitForInputToken);
				return Task.CompletedTask;
			});

			//Cancellation token for waiting Task
			CancellationTokenSource checkTimeCanceler = new CancellationTokenSource();
			CancellationToken checkTimeToken = checkTimeCanceler.Token;

			Task checkTime = Task.Run(async () => {
				bool dateNotPassed = true;
				while (dateNotPassed) {
					if (disconnectTime < DateTime.Now) {
						dateNotPassed = false;
					}
					//wait
					await Task.Delay(1000, checkTimeToken);
				}

				return Task.CompletedTask;
			});
			//wait for any task to complete
			await Task.WhenAny(waitForInput, checkTime);

			if (checkTime.IsCompleted) {
				//Stop tasks
				waitForInputCanceler.Cancel();
				checkTimeCanceler.Cancel();
				//Disconnect
				logger.LogDebug("handleWaitingForInputAsync: ran out of time");
				return true;
			}
			else if (waitForInput.IsCompleted) {
				//Stop tasks
				waitForInputCanceler.Cancel();
				checkTimeCanceler.Cancel();
				//Send confirmation
				await sendString(clientWebSocket, "Confirm", ct);
				return false;
			}
			else {
				//stop tasks
				waitForInputCanceler.Cancel();
				checkTimeCanceler.Cancel();
				//Disconnect
				logger.LogDebug("handleWaitingForInputAsync: neither task completed");
				return true;
			}
		}

		/// <summary>
		/// Creates a new empty channel
		/// </summary>
		private void resetChannel() {
			commandChannel = Channel.CreateUnbounded<string>();
		}

		/// <summary>
		/// Handles the Disconnecting state
		/// </summary>
		/// <param name="clientWebSocket"></param>
		/// <param name="ct"></param>
		/// <returns></returns>
		private Task<bool> handleDisconnectingAsync(ClientWebSocket clientWebSocket, CancellationToken ct) {
			logger.LogDebug("handleDisconnecting");
			return Task.FromResult(true);
		}

		/// <summary>
		/// Handles the various states a websocket can be in, and checks if the connection needs to be closed
		/// </summary>
		/// <param name="clientWebSocket"></param>
		/// <param name="ct"></param>
		/// <returns>Bool, True if connection is closing</returns>
		private async Task<bool> handleWebsocketStatesAsync(ClientWebSocket clientWebSocket, CancellationToken ct) {
			switch (clientWebSocket.State) {
				case WebSocketState.None:
					return true;

				case WebSocketState.Connecting:
					return false;

				case WebSocketState.Open:
					return false;

				case WebSocketState.CloseSent:
					return true;

				case WebSocketState.CloseReceived:
					await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
						"Received Close from Server", ct);
					return true;

				case WebSocketState.Closed:
					return true;

				case WebSocketState.Aborted:
					return true;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private async Task closeWebsocketAsync(ClientWebSocket clientWebSocket, string msg, CancellationToken ct) {
			try {
				switch (clientWebSocket.State) {
					case WebSocketState.None:
						break;

					case WebSocketState.Connecting:
						await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", ct);
						break;

					case WebSocketState.Open:
						await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, msg, ct);
						break;

					case WebSocketState.CloseSent:
						break;

					case WebSocketState.CloseReceived:
						await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "client Closing in response to closed socket", ct);
						break;

					case WebSocketState.Closed:
						break;

					case WebSocketState.Aborted:
						break;

					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			catch (Exception e) {
				logger.LogDebug("closeWebsocketAsync", e);
				await clientWebSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "error", ct);
				throw;
			}
		}

		private async Task sendString(WebSocket clientWebSocket, string s, CancellationToken token) {
			byte[] bytes = Encoding.UTF32.GetBytes(s);
			await clientWebSocket.SendAsync(bytes, WebSocketMessageType.Text, true, token);
		}

		private async Task<string> receiveString(WebSocket clientWebSocket, CancellationToken token) {
			byte[] sizeBytes = new byte[sizeof(int)];
			await clientWebSocket.ReceiveAsync(sizeBytes, token);
			int size = BitConverter.ToInt32(sizeBytes);
			byte[] stringBytes = new byte[size];
			await clientWebSocket.ReceiveAsync(stringBytes, token);
			return Encoding.UTF32.GetString(stringBytes);
		}

		public void Dispose() {
			webSocket?.Dispose();
		}
	}
}