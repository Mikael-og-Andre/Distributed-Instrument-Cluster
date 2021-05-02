using Blazor_Instrument_Cluster.Shared.DeviceSelection;
using Blazor_Instrument_Cluster.Shared.Websocket;
using Blazor_Instrument_Cluster.Shared.Websocket.Enum;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Client.Code.Websocket {

	/// <summary>
	/// Class for managing the websocket connection ot the backend
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class CrestronWebsocket : IDisposable {

		/// <summary>
		/// The last state received from the backend
		/// </summary>
		public CrestronWebsocketState state { get; set; }

		/// <summary>
		/// Websocket for the connection
		/// </summary>
		protected ClientWebSocket webSocket { get; set; }

		/// <summary>
		/// Uri the websocket will connect to
		/// </summary>
		private Uri connectionUri { get; set; }

		/// <summary>
		/// Token with id for controlling a crestron
		/// </summary>
		private ControlToken controlToken { get; set; }

		/// <summary>
		/// Position in queue
		/// </summary>
		public string positionInQueue { get; set; }

		/// <summary>
		/// Queue for messages
		/// </summary>
		private ConcurrentQueue<string> commandConcurrentQueue { get; set; }

		/// <summary>
		/// Time the backend will abandon the connection
		/// </summary>
		private DateTime disconnectTime { get; set; }

		/// <summary>
		/// Source for canceling token source
		/// </summary>
		private CancellationTokenSource cancellationTokenSource { get; set; }

		/// <summary>
		/// Boolean used to determine if the connection should close for any reason
		/// </summary>
		private bool isClosing { get; set; }

		/// <summary>
		/// CrestronWebsocket
		/// </summary>
		/// <param name="connectionUri">Uri the websocket will connect to</param>
		public CrestronWebsocket(Uri connectionUri) {
			this.connectionUri = connectionUri;
			this.controlToken = null;
			this.positionInQueue = "waiting";
			this.commandConcurrentQueue = new ConcurrentQueue<string>();
			this.disconnectTime = DateTime.Now;
			isClosing = false;
		}

		public void cancel() {
			cancellationTokenSource.Cancel();
		}

		/// <summary>
		/// handle a connection to the Backend
		/// </summary>
		/// <param name="deviceModel"></param>
		/// <param name="subConnection"></param>
		/// <returns></returns>
		public async Task startProtocol(DeviceModel deviceModel, SubConnectionModel subConnection) {
			cancellationTokenSource?.Cancel();
			cancellationTokenSource = new CancellationTokenSource();
			CancellationToken ct = cancellationTokenSource.Token;
			isClosing = false;
			webSocket = new ClientWebSocket();

			try {
				//Connect
				await webSocket.ConnectAsync(connectionUri, ct);

				while (!ct.IsCancellationRequested) {
					//Handle possible states, and check if websocket is viable
					checkIfClosing(webSocket, ct);

					//Connection is closing
					if (isClosing) {
						Console.WriteLine("Closing Crestron Websocket");
						await closeWebsocketAsync(webSocket, "Closing", ct);
						cancellationTokenSource.Cancel();
						break;
					}

					//receive a state from the backend
					string receivedStateString = await receiveString(webSocket, ct);
					CrestronWebsocketState currentState = Enum.Parse<CrestronWebsocketState>(receivedStateString);
					state = currentState;

					switch (currentState) {
						case CrestronWebsocketState.Requesting:
							await handleRequestingAsync(deviceModel, subConnection, webSocket, ct);
							break;

						case CrestronWebsocketState.InQueue:
							await handleInQueueAsync(webSocket, ct);
							break;

						case CrestronWebsocketState.InControl:
							await handleInControlAsync(webSocket, ct);
							break;

						case CrestronWebsocketState.Disconnecting:
							handleDisconnectingAsync(webSocket, ct);
							break;

						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}
			catch (Exception e) {
				Console.WriteLine("startAsync exception: " + e.Message);
				await closeWebsocketAsync(webSocket, "Error", ct);
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
		private async Task handleRequestingAsync(DeviceModel deviceModel, SubConnectionModel subConnectionModel, ClientWebSocket clientWebSocket, CancellationToken ct) {
			//Send Requesting Device model
			RequestingDeviceModel requestingDevice = new RequestingDeviceModel(deviceModel.name, deviceModel.location, deviceModel.type, subConnectionModel.guid);
			string requestingDeviceString = JsonSerializer.Serialize(requestingDevice);
			await sendString(clientWebSocket, requestingDeviceString, ct);

			//Confirm if the device is found or not
			string found = await receiveString(clientWebSocket, ct);
			//if the returned string isn't true, close connection
			if (!found.Equals("True")) {
				Console.WriteLine("handleRequestingAsync: Device was not found");
				//Set to closing
				isClosing = true;
			}
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
		private async Task handleInQueueAsync(ClientWebSocket clientWebSocket, CancellationToken ct) {
			//Get signal that we are entering the queue
			string enteringQueue = await receiveString(clientWebSocket, ct);

			//Check for correct response
			if (!enteringQueue.Equals("Entering Queue")) {
				Console.WriteLine("handleInQueue: entering queue, protocol error");
				isClosing = true;
				return;
			}

			Console.WriteLine("Entering Queue");
			while (!ct.IsCancellationRequested) {
				//Check if anything happened to the connection
				if (webSocket.State != WebSocketState.Open) {
					Console.WriteLine("handleInQueue: Socket closed while in queue");
					isClosing = true;
					return;
				}

				//Get queue position
				string queuePos = await receiveString(clientWebSocket, ct);

				//Check if end signal
				if (queuePos.Equals("Complete")) {
					queuePos = "Position in queue: 0";
					return;
				}
				else if (queuePos.Equals("closing")) {
					Console.WriteLine("handleInQueue: While in queue, received close from the backend");
					isClosing = true;
					return;
				}

				try {
					QueueStatusModel queueStatus = JsonSerializer.Deserialize<QueueStatusModel>(queuePos);
					queuePos = "Position in queue: " + queueStatus.position;
					Console.WriteLine("Updating Queue: " + queuePos);
					await Task.Delay(250, cancellationTokenSource.Token);
				}
				catch (Exception) {
					Console.WriteLine("HandleInQueue: Failed to deserialize");
					isClosing = true;
					return;
				}
			}
		}

		/// <summary>
		/// handleInControl state
		/// Sends commands from the channel to the backend
		/// </summary>
		/// <param name="clientWebSocket"></param>
		/// <param name="ct"></param>
		/// <returns>True if the connection should close</returns>
		private async Task handleInControlAsync(ClientWebSocket clientWebSocket, CancellationToken ct) {
			Console.WriteLine("Entering In Control");
			//Receive confirmation if you have control or not
			string controlling = await receiveString(clientWebSocket, ct);

			//Check if correct signal
			if (controlling.Equals("Controlling")) {
				//do nothing
			}
			else if (controlling.Equals("Not controlling")) {
				//return normally and wait for next state
				Console.WriteLine("handleInControl: not in control");
				return;
			}
			else {
				//return and close the connection
				Console.WriteLine("handleInControl: controlling signal error, protocol error");
				isClosing = true;
				return;
			}

			//Queue for incoming messages
			ConcurrentQueue<string> commandQueue = commandConcurrentQueue;
			Console.WriteLine("Entering control loop");
			while (!ct.IsCancellationRequested) {
				//check if connection is still ok
				if (webSocket.State != WebSocketState.Open) {
					Console.WriteLine("handleInControl: Socket state not open in control loop");
					isClosing = true;
					return;
				}
				//get bytes from channel
				if (commandQueue.TryDequeue(out string toSend)) {
					await sendString(clientWebSocket, toSend, ct);
				}
				else {
					await Task.Delay(250, cancellationTokenSource.Token);
				}
			}
		}

		/// <summary>
		/// Add a message to the queue
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		public bool trySendingControlMessage(string msg) {
			//Check if in control
			if (state == CrestronWebsocketState.InControl) {
				commandConcurrentQueue.Enqueue(msg);
				return true;
			}
			//not in control
			return false;
		}

		/// <summary>
		/// Clears the command queue
		/// </summary>
		private void resetQueue() {
			commandConcurrentQueue.Clear();
		}

		/// <summary>
		/// Handles the Disconnecting state
		/// </summary>
		/// <param name="clientWebSocket"></param>
		/// <param name="ct"></param>
		/// <returns></returns>
		private void handleDisconnectingAsync(ClientWebSocket clientWebSocket, CancellationToken ct) {
			Console.WriteLine("handleDisconnecting");
			isClosing = true;
			return;
		}

		/// <summary>
		/// Handles the various states a websocket can be in
		/// If the connection is not open, it should be closed
		/// </summary>
		/// <param name="clientWebSocket"></param>
		/// <param name="ct"></param>
		/// <returns>Bool, True if connection is closing</returns>
		private void checkIfClosing(ClientWebSocket clientWebSocket, CancellationToken ct) {
			switch (clientWebSocket.State) {
				case WebSocketState.None:
					isClosing = true;
					return;

				case WebSocketState.Connecting:
					isClosing = true;
					return;

				case WebSocketState.Open:
					return;

				case WebSocketState.CloseSent:
					isClosing = true;
					return;

				case WebSocketState.CloseReceived:
					isClosing = true;
					return;

				case WebSocketState.Closed:
					isClosing = true;
					return;

				case WebSocketState.Aborted:
					isClosing = true;
					return;

				default:
					isClosing = true;
					return;
			}
		}

		/// <summary>
		/// Close the connection from any state
		/// If close has been received, close only output
		/// </summary>
		/// <param name="clientWebSocket"></param>
		/// <param name="msg"></param>
		/// <param name="ct"></param>
		/// <returns></returns>
		private async Task closeWebsocketAsync(ClientWebSocket clientWebSocket, string msg, CancellationToken ct) {
			try {
				switch (clientWebSocket.State) {
					case WebSocketState.None:
						break;

					case WebSocketState.Connecting:
						await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", ct);
						break;

					case WebSocketState.Open:
						await clientWebSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, msg, ct);
						break;

					case WebSocketState.CloseSent:
						break;

					case WebSocketState.CloseReceived:
						await clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "client Closing in response to closed socket", ct);
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
				Console.WriteLine("closeWebsocketAsync", e);
				await clientWebSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "error", ct);
				throw;
			}
		}

		private async Task sendString(WebSocket clientWebSocket, string s, CancellationToken token) {
			byte[] bytes = Encoding.UTF32.GetBytes(s);
			//Get size
			byte[] size = BitConverter.GetBytes(bytes.Length);
			//Send size
			await clientWebSocket.SendAsync(size, WebSocketMessageType.Binary, true, token);
			//Send msg
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
			cancellationTokenSource?.Dispose();
		}
	}
}