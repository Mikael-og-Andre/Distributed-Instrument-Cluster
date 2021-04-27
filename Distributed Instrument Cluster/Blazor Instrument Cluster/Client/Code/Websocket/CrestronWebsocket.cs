using Blazor_Instrument_Cluster.Shared.Websocket.Enum;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Shared;

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
		/// CrestronWebsocket
		/// </summary>
		/// <param name="connectionPath">Uri the websocket will connect to</param>
		public CrestronWebsocket(Uri connectionPath) {
			this.connectionPath = connectionPath;
			webSocket = new ClientWebSocket();
		}

		/// <summary>
		/// Connect to the Uri
		/// </summary>
		/// <returns>Task</returns>
		public async Task connectAsync(CancellationToken ct) {
			await webSocket.ConnectAsync(connectionPath, ct);
		}

		public async Task startAsync(DeviceModel deviceModel,SubConnectionModel subdeviceModel,CancellationToken ct) {
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
							closing |= await handleRequestingAsync(deviceModel,subdeviceModel,webSocket, ct);
							break;

						case CrestronWebsocketState.InQueue:
							closing |= await handleInQueueAsync(webSocket, ct);
							break;

						case CrestronWebsocketState.InControl:
							closing |= await handleInControlAsync(webSocket, ct);
							break;

						case CrestronWebsocketState.TimeLimitExceeded:
							closing |= await handleTimeLimitExceededAsync(webSocket, ct);
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
		private async Task<bool> handleRequestingAsync(DeviceModel deviceModel,SubConnectionModel subConnectionModel,ClientWebSocket clientWebSocket, CancellationToken ct) {
			//Confirm Requesting
			await sendString(clientWebSocket, CrestronWebsocketState.Requesting.ToString(), ct);
			//Get request for id
			string requestId = await receiveString(clientWebSocket,ct);
			//If req does not match expected string, close
			if (!requestId.Equals("id")) {
				logger.LogDebug("handleRequestingAsync: requestId, protocol error");
				await closeWebsocketAsync(clientWebSocket,"Protocol not followed",ct);
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
				await closeWebsocketAsync(clientWebSocket, "Device not found", ct);
				return true;
			}

			//no issues, return false for not closing connection
			return false;
		}

		private async Task<bool> handleInQueueAsync(ClientWebSocket clientWebSocket, CancellationToken ct) {
		}

		private async Task<bool> handleInControlAsync(ClientWebSocket clientWebSocket, CancellationToken ct) {
		}

		private async Task<bool> handleTimeLimitExceededAsync(ClientWebSocket clientWebSocket, CancellationToken ct) {
		}

		private async Task<bool> handleWaitingForInputAsync(ClientWebSocket clientWebSocket, CancellationToken ct) {
		}

		private async Task<bool> handleDisconnectingAsync(ClientWebSocket clientWebSocket, CancellationToken ct) {
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

		private async Task closeWebsocketAsync(ClientWebSocket clientWebSocket,string msg, CancellationToken ct) {
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
				await clientWebSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable,"error",ct);
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