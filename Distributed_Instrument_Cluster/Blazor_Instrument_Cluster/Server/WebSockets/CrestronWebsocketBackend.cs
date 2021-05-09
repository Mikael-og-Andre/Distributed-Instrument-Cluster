using Blazor_Instrument_Cluster.Server.CrestronControl;
using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement;
using Blazor_Instrument_Cluster.Shared.Websocket;
using Blazor_Instrument_Cluster.Shared.Websocket.Enum;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server.WebSockets {

	/// <summary>
	/// Backend Implementation of the Crestron websocket communication protocol
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class CrestronWebsocketBackend {

		/// <summary>
		/// Websocket the communication runs over
		/// </summary>
		public WebSocket webSocket { get; set; }

		/// <summary>
		/// Signals that the task is complete
		/// </summary>
		public TaskCompletionSource<object> tskCompletionSource { get; set; }

		/// <summary>
		/// Remote Device manager, Containing info relating to the remote devices
		/// </summary>
		private RemoteDeviceManager remoteDeviceManager { get; set; }

		/// <summary>
		/// The state the protocol is in
		/// </summary>
		private CrestronWebsocketState state { get; set; }

		/// <summary>
		/// Cancellation
		/// </summary>
		private CancellationTokenSource cancellationTokenSource { get; set; }

		/// <summary>
		/// Should the socket close, if anything wants to close, set this to true and in the next loop the connection will close
		/// </summary>
		private bool isClosing { get; set; }

		/// <summary>
		/// An instance of a crestronUser, used to queue for control, and send commands
		/// </summary>
		private CrestronUser crestronUser { get; set; } = default;

		/// <summary>
		/// How long does the controller loop run without receiving commands before it closes
		/// </summary>
		private const int TimeOutConnectionMillis = 1000 * 60 * 2;

		public CrestronWebsocketBackend(WebSocket webSocket, RemoteDeviceManager remoteDeviceManager, TaskCompletionSource<object> tskCompletionSource) {
			this.webSocket = webSocket;
			this.remoteDeviceManager = remoteDeviceManager;
			this.tskCompletionSource = tskCompletionSource;
			this.state = CrestronWebsocketState.Requesting;
			this.cancellationTokenSource = new CancellationTokenSource();
			this.isClosing = false;
		}

		/// <summary>
		/// Start the protocol
		/// </summary>
		/// <returns></returns>
		public async Task start() {
			try {
				while (!cancellationTokenSource.IsCancellationRequested) {
					switch (state) {
						case CrestronWebsocketState.Requesting:
							state = await handleRequesting();
							break;

						case CrestronWebsocketState.InQueue:
							state = await handleInQueue();
							break;

						case CrestronWebsocketState.InControl:
							state = await handleInControl();
							break;

						case CrestronWebsocketState.Disconnecting:
							await handleDisconnecting();
							crestronUser?.delete();
							tskCompletionSource.SetResult(new object());
							return;

						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}
			catch (Exception e) {
				Console.WriteLine("Exception in CrestronWebsocketBackend: {0}", e.Message);
				
			}
			//remove device from list if it exists
			crestronUser?.delete();
			tskCompletionSource.SetResult(new object());
		}

		/// <summary>
		/// Receive information about the device they want to control and check if it is online, and check if it exists
		/// </summary>
		/// <returns></returns>
		private async Task<CrestronWebsocketState> handleRequesting() {
			//Send state
			await sendString(webSocket, CrestronWebsocketState.Requesting.ToString(), cancellationTokenSource.Token);

			//Get requested device info
			string deviceJson = await receiveString(webSocket, cancellationTokenSource.Token);
			RequestingDeviceModel requestedDevice;
			try {
				requestedDevice = JsonSerializer.Deserialize<RequestingDeviceModel>(deviceJson);
			}
			catch (Exception e) {
				return CrestronWebsocketState.Disconnecting;
			}

			//List of remote devices
			List<RemoteDevice> listOfRemoteDevices = remoteDeviceManager.getListOfRemoteDevices();
			RemoteDevice remoteDevice = default;
			//check if device exists
			lock (listOfRemoteDevices) {
				foreach (var dev in listOfRemoteDevices) {
					if (dev.name == requestedDevice.name &&
						dev.location == requestedDevice.location &&
						dev.type == requestedDevice.type) {
						remoteDevice = dev;
					}
				}
			}

			if (remoteDevice is null) {
				await sendString(webSocket, "Device not found", cancellationTokenSource.Token);
				return CrestronWebsocketState.Disconnecting;
			}
			else if (!remoteDevice.hasCrestron()) {
				await sendString(webSocket, "No Crestron", cancellationTokenSource.Token);
				return CrestronWebsocketState.Disconnecting;
			}

			this.crestronUser = remoteDevice.createCrestronUser();

			if (!crestronUser.checkConnectionAvailable()) {
				await sendString(webSocket, "Device is not online", cancellationTokenSource.Token);
				crestronUser.delete();
				return CrestronWebsocketState.Disconnecting;
			}

			await sendString(webSocket, "True", cancellationTokenSource.Token);
			return CrestronWebsocketState.InQueue;
		}

		/// <summary>
		/// Get a CrestronUser object and enter a queue for and wait until you have the control of the device,
		/// Also updates the position in queue to the client
		/// </summary>
		/// <returns></returns>
		private async Task<CrestronWebsocketState> handleInQueue() {
			//Send state
			await sendString(webSocket, CrestronWebsocketState.InQueue.ToString(), cancellationTokenSource.Token);

			//Send entering queue
			await sendString(webSocket, "Entering Queue", cancellationTokenSource.Token);

			while (!cancellationTokenSource.IsCancellationRequested) {
				//check if remote side closed
				if (webSocket.State != WebSocketState.Open) {
					return CrestronWebsocketState.Disconnecting;
				}

				int pos = crestronUser.getPosition();

				if (pos == 0) {
					await sendString(webSocket, "Complete", cancellationTokenSource.Token);
					return CrestronWebsocketState.InControl;
				}
				else if (pos < 0) {
					await sendString(webSocket, "closing", cancellationTokenSource.Token);
					return CrestronWebsocketState.Disconnecting;
				}
				else {
					QueueStatusModel queueStatus = new QueueStatusModel(pos);
					string queueJson = JsonSerializer.Serialize(queueStatus);
					await sendString(webSocket, queueJson, cancellationTokenSource.Token);
					await Task.Delay(1000, cancellationTokenSource.Token);
				}
			}

			return CrestronWebsocketState.Disconnecting;
		}

		/// <summary>
		/// Get commands from the client and send the to the remote device
		/// </summary>
		/// <returns></returns>
		private async Task<CrestronWebsocketState> handleInControl() {
			try {
				//Send state
				await sendString(webSocket, CrestronWebsocketState.InControl.ToString(), cancellationTokenSource.Token);
				//check if in control
				bool controlling = crestronUser.isControlling();
				if (!controlling) {
					await sendString(webSocket, "Not controlling", cancellationTokenSource.Token);
					//abandon controller instance
					crestronUser.delete();
					return CrestronWebsocketState.InQueue;
				}

				//In control
				await sendString(webSocket, "Controlling", cancellationTokenSource.Token);
				//Tack time between receives
				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start();
				bool hasExceeded = false;
				CancellationToken ct = cancellationTokenSource.Token;

				//If no command is received in 2 minutes close the connection
				Task timeTracker = Task.Run(async () => {
					while (stopwatch.ElapsedMilliseconds < TimeOutConnectionMillis) {
						await Task.Delay(100, ct);
					}
					//if canceled just leave the task
					if (ct.IsCancellationRequested) {
						return;
					}
					//Close connection due to time exceeded
					hasExceeded = true;
					cancellationTokenSource.Cancel();
				}, ct);

				//Receive commands
				while (!cancellationTokenSource.IsCancellationRequested) {
					if (hasExceeded) {
						crestronUser.delete();
						return CrestronWebsocketState.Disconnecting;
					}

					//check if remote endpoint closed
					if (webSocket.State != WebSocketState.Open) {
						crestronUser.delete();
						return CrestronWebsocketState.Disconnecting;
					}

					string receivedString = await receiveString(webSocket, cancellationTokenSource.Token);
					//Command received reset clock
					stopwatch.Restart();
					//Attempt to send disconnect if it fails
					if (!await crestronUser.send(receivedString, cancellationTokenSource.Token)) {
						//Sending failed, disconnect
						crestronUser.delete();
						return CrestronWebsocketState.Disconnecting;
					}
				}
				crestronUser.delete();
				//Disconnect
				return CrestronWebsocketState.Disconnecting;
			}
			catch (Exception e) {
				crestronUser?.delete();
				Console.WriteLine("Exception in handle in control");
				throw;
			}
		}

		/// <summary>
		/// Disconnect
		/// </summary>
		/// <returns></returns>
		private async Task handleDisconnecting() {
			//Send state
			await sendString(webSocket, CrestronWebsocketState.Disconnecting.ToString(), cancellationTokenSource.Token);
			await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,"disconnecting",cancellationTokenSource.Token);
		}

		/// <summary>
		/// Send a string on the websocket
		/// </summary>
		/// <param name="webSocket"></param>
		/// <param name="s"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task sendString(WebSocket webSocket, string s, CancellationToken token) {
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			//Get size
			byte[] size = BitConverter.GetBytes(bytes.Length);
			//Send size
			await webSocket.SendAsync(size, WebSocketMessageType.Binary, true, token);
			//Send msg
			await webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, token);
		}

		/// <summary>
		/// Receive a string with a websocket
		/// </summary>
		/// <param name="webSocket"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		private async Task<string> receiveString(WebSocket webSocket, CancellationToken token) {
			byte[] sizeBytes = new byte[sizeof(int)];
			await webSocket.ReceiveAsync(sizeBytes, token);
			int size = BitConverter.ToInt32(sizeBytes);
			byte[] stringBytes = new byte[size];
			await webSocket.ReceiveAsync(stringBytes, token);
			return Encoding.UTF8.GetString(stringBytes);
		}
	}
}