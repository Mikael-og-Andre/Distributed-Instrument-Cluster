﻿using Blazor_Instrument_Cluster.Server.Injection;
using Server_Library;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Server_Library.Connection_Types;
using Server_Library.Connection_Types.deprecated;
using Server_Library.Enums;

namespace Blazor_Instrument_Cluster.Server.WebSockets {

	/// <summary>
	/// Websocket handler for crestron control connections
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class CrestronWebsocketHandler<T,U> : ICrestronSocketHandler {

		/// <summary>
		/// Logger
		/// </summary>
		private ILogger<CrestronWebsocketHandler<T,U>> logger;

		/// <summary>
		///Services
		/// </summary>
		private IServiceProvider services;

		/// <summary>
		/// Remote devices
		/// </summary>
		private RemoteDeviceConnections<T,U> remoteDeviceConnectionses;

		/// <summary>
		/// Constructor, Injects Logger and service provider and gets Remote device connection Singleton
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		public CrestronWebsocketHandler(ILogger<CrestronWebsocketHandler<T,U>> logger, IServiceProvider services) {
			this.logger = logger;
			remoteDeviceConnectionses = (RemoteDeviceConnections<T,U>)services.GetService(typeof(IRemoteDeviceConnections<T,U>));
		}

		/// <summary>
		/// Handles the incoming connection
		/// </summary>
		/// <param name="websocket"></param>
		/// <param name="socketFinishedTcs"></param>
		public async void StartCrestronWebsocketProtocol(WebSocket websocket, TaskCompletionSource<object> socketFinishedTcs) {
			//Create cancellation token
			CancellationToken token = new CancellationToken(false);
			try {
				//Send start signal
				byte[] startBytes = Encoding.ASCII.GetBytes("start");
				ArraySegment<byte> startSeg = new ArraySegment<byte>(startBytes);
				await websocket.SendAsync(startSeg, WebSocketMessageType.Text, true, token);

				//Get name of device'
				byte[] nameBufferBytes = new byte[100];
				ArraySegment<byte> nameBuffer = new ArraySegment<byte>(nameBufferBytes);
				await websocket.ReceiveAsync(nameBuffer, token);
				string name = Encoding.ASCII.GetString(nameBufferBytes).TrimEnd('\0');

				//Check if connection with he name exists and is available
				bool exists = false;
				int maxLoops = 20;
				int looped = 0;
				CrestronConnection con = null;
				while (!exists && (looped < maxLoops)) {
					exists = remoteDeviceConnectionses.getCrestronConnectionWithName(out con, name);
					logger.LogCritical("WebSocket tried to Find {0} but Crestron connection queue was not found", name);
					looped++;
					await Task.Delay(100, token);
				}

				//If it does not exist close connection
				if (exists) {
					logger.LogDebug("Crestron device with name {0} was found", name);
					//TODO: add exclusive control
					//Do connection exclusive control actions

					////Tell device found
					byte[] yesBytes = Encoding.ASCII.GetBytes("found");
					ArraySegment<byte> yesSeg = new ArraySegment<byte>(yesBytes);
					await websocket.SendAsync(yesSeg, WebSocketMessageType.Text, true, token);

					ConcurrentQueue<Message> messageInputQueue = con.getSendingQueue();

					//Start command receive loop
					while (!token.IsCancellationRequested) {
						//Receive command
						byte[] cmdBufferBytes = new byte[2048];
						ArraySegment<byte> receiveBuffer = new ArraySegment<byte>(cmdBufferBytes);
						await websocket.ReceiveAsync(receiveBuffer, token);
						string receivedString = Encoding.ASCII.GetString(receiveBuffer.ToArray());
						//Trim nullbytes
						receivedString.Trim('\0');

						Message msg = new Message(ProtocolOption.message, receivedString);
						messageInputQueue.Enqueue(msg);
					}

					//Signal finished
					socketFinishedTcs.TrySetResult(new object());
				}
				else {
					logger.LogDebug("Crestron Websocket requested a device: {0} that did not exist", name);
					////Send does not exist and close
					byte[] noBytes = Encoding.ASCII.GetBytes("failed");
					ArraySegment<byte> noSeg = new ArraySegment<byte>(noBytes);
					await websocket.SendAsync(noSeg, WebSocketMessageType.Text, true, token);

					socketFinishedTcs.TrySetResult(new object());
				}
			}
			catch (Exception ex) {
				//if websocket is running send close, and close socket pipeline
				if (websocket.State != WebSocketState.Closed) {
					await websocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Closing socket", token);
				}
				logger.LogError(ex, "Exception Thrown in CrestronWebSocketHandler");
				socketFinishedTcs.TrySetResult(new object());
			}
		}
	}
}