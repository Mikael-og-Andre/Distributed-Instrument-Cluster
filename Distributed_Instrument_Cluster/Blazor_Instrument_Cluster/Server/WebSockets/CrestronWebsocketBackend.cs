using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.CrestronControl;
using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement;
using Blazor_Instrument_Cluster.Server.Services;
using Blazor_Instrument_Cluster.Shared.Websocket;
using Blazor_Instrument_Cluster.Shared.Websocket.Enum;

namespace Blazor_Instrument_Cluster.Server.WebSockets {
	/// <summary>
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class CrestronWebsocketBackend {

		public WebSocket webSocket { get; set; }
		public TaskCompletionSource<object> tskCompletionSource { get; set; }

		private RemoteDeviceManager remoteDeviceManager { get; set; }

		private CrestronWebsocketState state { get; set; }

		private CancellationTokenSource cancellationTokenSource { get; set; }

		private bool isClosing { get; set; }

		private ControllerInstance controllerInstance { get; set; } = default;

		public CrestronWebsocketBackend(WebSocket webSocket,RemoteDeviceManager remoteDeviceManager, TaskCompletionSource<object> tskCompletionSource) {
			this.webSocket = webSocket;
			this.remoteDeviceManager = remoteDeviceManager;
			this.tskCompletionSource = tskCompletionSource;
			this.state = CrestronWebsocketState.Requesting;
			this.cancellationTokenSource = new CancellationTokenSource();
			this.isClosing = false;
		}

		public async Task start() {

			try {
				while (!cancellationTokenSource.IsCancellationRequested) {
					switch (state) {
						case CrestronWebsocketState.Requesting:
							state=await handleRequesting();
							break;
						case CrestronWebsocketState.InQueue:
							state=await handleInQueue();
							break;
						case CrestronWebsocketState.InControl:
							state=await handleInControl();
							break;
						case CrestronWebsocketState.Disconnecting:
							await handleDisconnecting();
							controllerInstance?.delete();
							tskCompletionSource.SetResult(new object());
							return;

						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}
			catch (Exception e) {
				Console.WriteLine("Exception in CrestronWebsocketBackend: {0}",e.Message);
			}
			//remove device from list if it exists
			controllerInstance?.delete();
			tskCompletionSource.SetResult(new object());
		}

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
			SubConnection subConnection = default;
			//check if device exists
			lock (listOfRemoteDevices) {
				foreach (var dev in listOfRemoteDevices) {
					if (dev.name==requestedDevice.name&&
					    dev.location==requestedDevice.location&&
					    dev.type==requestedDevice.type) {

						foreach (var sub in dev.getListOfSubConnections()) {
							if (sub.id.Equals(requestedDevice.id)) {
								subConnection = sub;
								remoteDevice = dev;
								break;
							}
						}
					}
				}
			}

			if (subConnection is null || remoteDevice is null) {
				await sendString(webSocket, "not found",cancellationTokenSource.Token);
				return CrestronWebsocketState.Disconnecting;
			}

			if (remoteDevice.createControllerInstance(subConnection, out ControllerInstance instance)) {
				this.controllerInstance = instance;
				await sendString(webSocket,"True",cancellationTokenSource.Token);
				return CrestronWebsocketState.InQueue;
			}

			await sendString(webSocket, "Not found", cancellationTokenSource.Token);
			return CrestronWebsocketState.Disconnecting;
		}

		private async Task<CrestronWebsocketState> handleInQueue() {
			//Send state
			await sendString(webSocket, CrestronWebsocketState.InQueue.ToString(), cancellationTokenSource.Token);

			//Send entering queue
			await sendString(webSocket, "Entering Queue", cancellationTokenSource.Token);

			while (!cancellationTokenSource.IsCancellationRequested) {
				//check if remote side closed
				if (webSocket.State!=WebSocketState.Open) {
					return CrestronWebsocketState.Disconnecting;
				}

				int pos = controllerInstance.getPosition();

				if (pos==0) {
					await sendString(webSocket, "Complete", cancellationTokenSource.Token);
					return CrestronWebsocketState.InControl;
				}else if (pos<0) {
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

		private async Task<CrestronWebsocketState> handleInControl() {
			//Send state
			await sendString(webSocket, CrestronWebsocketState.InControl.ToString(), cancellationTokenSource.Token);
			//check if in control
			bool controlling = controllerInstance.isControlling();
			if (!controlling) {
				await sendString(webSocket, "Not controlling", cancellationTokenSource.Token);
				//abandon controller instance
				controllerInstance.delete();
				return CrestronWebsocketState.InQueue;
			}

			//In control
			await sendString(webSocket, "Controlling", cancellationTokenSource.Token);

			//Receive commands
			while (!cancellationTokenSource.IsCancellationRequested) {
				//check if remote endpoint closed
				if (webSocket.State!=WebSocketState.Open) {
					controllerInstance.delete();
					return CrestronWebsocketState.Disconnecting;
				}

				string receivedString = await receiveString(webSocket,cancellationTokenSource.Token);
				//Attempt to send
				if (!await controllerInstance.send(receivedString,cancellationTokenSource.Token)) {
					//Sending failed, disconnect
					controllerInstance.delete();
					return CrestronWebsocketState.Disconnecting;
				}
			}
			controllerInstance.delete();
			//Disconnect
			return CrestronWebsocketState.Disconnecting;
		}

		private async Task handleDisconnecting() {
			//Send state
			await sendString(webSocket, CrestronWebsocketState.Disconnecting.ToString(), cancellationTokenSource.Token);
		}


		private async Task sendString(WebSocket webSocket, string s, CancellationToken token) {
			byte[] bytes = Encoding.UTF32.GetBytes(s);
			//Get size
			byte[] size = BitConverter.GetBytes(bytes.Length);
			//Send size
			await webSocket.SendAsync(size, WebSocketMessageType.Binary, true, token);
			//Send msg
			await webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, token);
		}

		private async Task<string> receiveString(WebSocket webSocket, CancellationToken token) {
			byte[] sizeBytes = new byte[sizeof(int)];
			await webSocket.ReceiveAsync(sizeBytes, token);
			int size = BitConverter.ToInt32(sizeBytes);
			byte[] stringBytes = new byte[size];
			await webSocket.ReceiveAsync(stringBytes, token);
			return Encoding.UTF32.GetString(stringBytes);
		}
	}
}
