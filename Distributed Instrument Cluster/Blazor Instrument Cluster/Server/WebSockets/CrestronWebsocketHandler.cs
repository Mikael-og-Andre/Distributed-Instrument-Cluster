using Blazor_Instrument_Cluster.Server.Injection;
using Instrument_Communicator_Library;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server.WebSockets {

	/// <summary>
	/// Websocket handler for crestron control connections
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class CrestronWebsocketHandler : ICrestronSocketHandler {
		private ILogger<CrestronWebsocketHandler> logger;       //Logger
		private IServiceProvider services;                      //Services
		private RemoteDeviceConnection remoteDeviceConnections; //Remote devices

		public CrestronWebsocketHandler(ILogger<CrestronWebsocketHandler> logger, IServiceProvider services) {
			this.logger = logger;
			remoteDeviceConnections = (RemoteDeviceConnection)services.GetService(typeof(IRemoteDeviceConnections));
		}

		/// <summary>
		/// Handles the incoming connection
		/// </summary>
		/// <param name="websocket"></param>
		/// <param name="socketFinishedTcs"></param>
		public async void StartCrestronWebsocketProtocol(WebSocket websocket, TaskCompletionSource<object> socketFinishedTcs) {
			//Create cancellation token
			CancellationToken token = new CancellationToken(false);
			//Send start signal
			byte[] startBytes = Encoding.ASCII.GetBytes("start");
			ArraySegment<byte> startSeg = new ArraySegment<byte>(startBytes);
			await websocket.SendAsync(startSeg, WebSocketMessageType.Text, true, token);

			////Get name of device
			//byte[] bufferBytes = new byte[2048];
			//ArraySegment<byte> nameBuffer = new ArraySegment<byte>(bufferBytes);
			//await websocket.ReceiveAsync(nameBuffer, token);
			//string name = Encoding.ASCII.GetString(nameBuffer.ToArray());
			////Trim Null btyes
			//name.Trim('\0');
			//TODO: Remove hardcoded names
			string name = "Radar1";

			//Check if device exists
			bool exists = remoteDeviceConnections.GetCrestronConnectionWithName(out CrestronConnection con, name);
			//If it does not exist close connection
			if (exists) {
				//TODO: add exclusive control
				//Do connection exclusive control actions

				////Tell websocket if they have the control
				//byte[] yesBytes = Encoding.ASCII.GetBytes("yes");
				//ArraySegment<byte> yesSeg = new ArraySegment<byte>(yesBytes);
				//await websocket.SendAsync(yesSeg, WebSocketMessageType.Text, true, token);

				ConcurrentQueue<Message> messageInputQueue = con.GetInputQueue();

				//Start command receive loop
				while (!token.IsCancellationRequested) {
					//Receive command
					byte[] cmdBufferBytes = new byte[2048];
					ArraySegment<byte> receiveBuffer = new ArraySegment<byte>(cmdBufferBytes);
					await websocket.ReceiveAsync(receiveBuffer,token);
					string receivedString = Encoding.ASCII.GetString(receiveBuffer.ToArray());
					//Trim nullbytes
					receivedString.Trim('\0');

					Message msg = new Message(protocolOption.message, receivedString);
					messageInputQueue.Enqueue(msg);
				}
				//Signal finished
				socketFinishedTcs.TrySetResult(new object());
			} else {
				////Send does not exist and close
				//byte[] noBytes = Encoding.ASCII.GetBytes("no");
				//ArraySegment<byte> noSeg = new ArraySegment<byte>(noBytes);
				//await websocket.SendAsync(noSeg, WebSocketMessageType.Text, true, token);

				socketFinishedTcs.TrySetResult(new object());
			}
		}
	}
}