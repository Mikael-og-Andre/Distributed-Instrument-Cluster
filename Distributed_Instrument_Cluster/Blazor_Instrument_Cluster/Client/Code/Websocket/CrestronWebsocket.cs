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
	/// Class for managing the websocket connection to the backend
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class CrestronWebsocket :IExternalSender, IDisposable {
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
		/// Interface for signaling updated state
		/// </summary>
		private IUpdate stateUpdater { get; set; }

		/// <summary>
		/// Token with id for controlling a crestron
		/// </summary>
		private ControlToken controlToken { get; set; }

		/// <summary>
		/// Position in queue
		/// </summary>
		private string positionInQueue { get;  set; }

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
		/// is the connection closed
		/// </summary>
		public bool isStopped { get; private set; }

		/// <summary>
		/// Status of the crestron
		/// </summary>
		public string statusMsg { get; private set; }

		/// <summary>
		/// boolean used to check if external senders can send when in the control loop
		/// </summary>
		private bool inControlLoop { get; set; }

		/// <summary>
		/// CrestronWebsocket
		/// </summary>
		/// <param name="connectionUri">Uri the websocket will connect to</param>
		/// <param name="stateUpdater">IStateHasChanged object used to update states</param>
		public CrestronWebsocket(Uri connectionUri,IUpdate stateUpdater = null) {
			this.connectionUri = connectionUri;
			this.controlToken = null;
			this.positionInQueue = "waiting";
			this.disconnectTime = DateTime.Now;
			this.isClosing = false;
			this.isStopped = false;
			this.statusMsg = "Setting up";
			this.stateUpdater = stateUpdater;
			this.inControlLoop = false;
		}

		/// <summary>
		/// Cancel the cancellation token
		/// </summary>
		public async Task cancel() {
			await closeWebsocketAsync(webSocket, "Canceled", CancellationToken.None);
			cancellationTokenSource.Cancel();

		}

		/// <summary>
		/// handle a connection to the Backend
		/// </summary>
		/// <param name="displayRemoteDeviceModel"></param>
		/// <returns></returns>
		public async Task startProtocol(DisplayRemoteDeviceModel displayRemoteDeviceModel) {
			cancellationTokenSource?.Cancel();
			cancellationTokenSource = new CancellationTokenSource();
			CancellationToken ct = cancellationTokenSource.Token;
			isClosing = false;
			webSocket = new ClientWebSocket();
			//Update state of the input IUpdate
			updateState();

			try {
				this.statusMsg = "Connecting";
				updateState();

				//Connect
				await webSocket.ConnectAsync(connectionUri, ct);

				while (!ct.IsCancellationRequested) {
					//Handle possible states, and check if websocket is viable
					checkIfClosing(webSocket, ct);

					//Connection is closing
					if (isClosing) {
						this.statusMsg = "Disconnected";
						updateState();
						Console.WriteLine("Closing Crestron Websocket");
						await closeWebsocketAsync(webSocket, "Closing",ct);
						cancellationTokenSource.Cancel();
						break;
					}

					//receive a state from the backend
					string receivedStateString = await receiveString(webSocket, ct);
					CrestronWebsocketState currentState = Enum.Parse<CrestronWebsocketState>(receivedStateString);
					state = currentState;

					switch (currentState) {
						case CrestronWebsocketState.Requesting:
							this.statusMsg = "Requesting Device";
							updateState();
							await handleRequestingAsync(displayRemoteDeviceModel, webSocket, ct);
							break;

						case CrestronWebsocketState.InQueue:
							this.statusMsg = "Entering Queue";
							updateState();
							await handleInQueueAsync(webSocket, ct);
							break;

						case CrestronWebsocketState.InControl:
							await handleInControlAsync(webSocket, ct);
							break;

						case CrestronWebsocketState.Disconnecting:
							this.statusMsg = "Disconnected";
							updateState();
							handleDisconnectingAsync(webSocket, ct);
							isClosing = true;
							break;

						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}
			catch (Exception e) {
				this.statusMsg = "Disconnected";
				isStopped = true;
				inControlLoop = false;
				updateState();
				Console.WriteLine("startAsync exception: " + e.Message);
				await closeWebsocketAsync(webSocket, "Error",ct);
			}
			isStopped = true;
			updateState();
		}

		/// <summary>
		/// Handle the requesting state
		/// Share id of the device wanted by the client
		/// Confirm if the device exists on the server
		/// </summary>
		/// <param name="clientWebSocket"></param>
		/// <param name="ct"></param>
		/// <param name="displayRemoteDeviceModel"></param>
		/// <returns>Should the connection close, True = yes</returns>
		private async Task handleRequestingAsync(DisplayRemoteDeviceModel displayRemoteDeviceModel, ClientWebSocket clientWebSocket, CancellationToken ct) {
			//Send Requesting Device model
			RequestingDeviceModel requestingDevice = new RequestingDeviceModel(displayRemoteDeviceModel.name, displayRemoteDeviceModel.location, displayRemoteDeviceModel.type);
			string requestingDeviceString = JsonSerializer.Serialize(requestingDevice);
			await sendString(clientWebSocket, requestingDeviceString, ct);

			//Confirm if the device is found or not
			string found = await receiveString(clientWebSocket, ct);
			//if the returned string isn't true, close connection
			if (!found.Equals("True")) {
				Console.WriteLine($"handleRequestingAsync: Device was not found. And the msg was: {found}");
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
					this.statusMsg = "Queue Complete";
					updateState();
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
					this.statusMsg = queuePos;
					updateState();
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

			Console.WriteLine("Entering control loop");
			this.statusMsg = "Controlling";
			inControlLoop = true;
			updateState();
			while (!ct.IsCancellationRequested) {
				//check if connection is still ok
				if (webSocket.State != WebSocketState.Open) {
					Console.WriteLine("handleInControl: Socket state not open in control loop");
					inControlLoop = false;
					isClosing = true;
					return;
				}
				else {
					await Task.Delay(250, cancellationTokenSource.Token);
				}
			}
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
		/// https://developer.mozilla.org/en-US/docs/Web/API/CloseEvent
		/// </summary>
		/// <param name="clientWebSocket"></param>
		/// <param name="msg"></param>
		/// <param name="ct"></param>
		/// <returns></returns>
		private async Task closeWebsocketAsync(ClientWebSocket clientWebSocket, string msg,CancellationToken ct) {
			try {
				switch (clientWebSocket.State) {
					case WebSocketState.None:
						break;

					case WebSocketState.Connecting:
						await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", ct);
						break;

					case WebSocketState.Open:
						await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, msg, ct);
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
				await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "error", ct);
			}
		}

		/// <summary>
		/// Send a string as utf32 bytes
		/// First send size, then the actual string
		/// </summary>
		/// <param name="clientWebSocket"></param>
		/// <param name="s"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task sendString(WebSocket clientWebSocket, string s, CancellationToken token) {
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			//Get size
			byte[] size = BitConverter.GetBytes(bytes.Length);
			//Send size
			await clientWebSocket.SendAsync(size, WebSocketMessageType.Binary, true, token);
			//Send msg
			await clientWebSocket.SendAsync(bytes, WebSocketMessageType.Text, true, token);
		}

		/// <summary>
		/// Receive string UTF#" bytes
		/// First receive int with size
		/// Then actual string
		/// </summary>
		/// <param name="clientWebSocket"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task<string> receiveString(WebSocket clientWebSocket, CancellationToken token) {
			byte[] sizeBytes = new byte[sizeof(int)];
			await clientWebSocket.ReceiveAsync(sizeBytes, token);
			int size = BitConverter.ToInt32(sizeBytes);
			byte[] stringBytes = new byte[size];
			await clientWebSocket.ReceiveAsync(stringBytes, token);
			return Encoding.UTF8.GetString(stringBytes);
		}

		/// <summary>
		/// Call update function on the passed in IUpdate if it is not null
		/// </summary>
		private void updateState() {
			stateUpdater?.updateState();
		}

		/// <summary>
		/// IDisposable to catch browser closure, so the server knows websocket is disconnected
		/// </summary>
		public void Dispose() {
			webSocket?.Dispose();
			cancellationTokenSource?.Dispose();
		}

		/// <summary>
		/// Send string on websocket, if in controlling state, and in the control loop
		/// </summary>
		/// <param name="msg"></param>
		/// <returns>True if message was sent, False if not</returns>
		public async Task<bool> sendExternal(string msg) {
			if (inControlLoop && state == CrestronWebsocketState.InControl) {
				await sendString(webSocket, msg, cancellationTokenSource.Token);
				return true;
			}

			return false;
		}
	}
}