using Blazor_Instrument_Cluster.Server.Injection;
using Blazor_Instrument_Cluster.Server.RemoteDevice;
using Microsoft.Extensions.Logging;
using Server_Library.Connection_Types;
using System;
using System.Collections.Generic;
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
	public class CrestronWebsocketHandler<T, U> : ICrestronSocketHandler {

		/// <summary>
		/// Logger
		/// </summary>
		private ILogger<CrestronWebsocketHandler<T, U>> logger;

		/// <summary>
		///Services
		/// </summary>
		private IServiceProvider services;

		/// <summary>
		/// Remote devices
		/// </summary>
		private RemoteDeviceConnections<T, U> remoteDeviceConnections;

		/// <summary>
		/// Constructor, Injects Logger and service provider and gets Remote device connection Singleton
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		public CrestronWebsocketHandler(ILogger<CrestronWebsocketHandler<T, U>> logger, IServiceProvider services) {
			this.logger = logger;
			remoteDeviceConnections = (RemoteDeviceConnections<T, U>)services.GetService(typeof(IRemoteDeviceConnections<T, U>));
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
				byte[] startBytes = Encoding.UTF8.GetBytes("start");
				ArraySegment<byte> startSegment = new ArraySegment<byte>(startBytes);
				await websocket.SendAsync(startSegment, WebSocketMessageType.Text, true, token);

				byte[] nameBuffer = new byte[1024];
				byte[] locationBuffer = new byte[1024];
				byte[] typeBuffer = new byte[1024];
				byte[] subnameBuffer = new byte[1024];

				//Get name of wanted device
				ArraySegment<byte> nameSegment = new ArraySegment<byte>(nameBuffer);
				await websocket.ReceiveAsync(nameSegment, token);
				string name = Encoding.UTF8.GetString(nameSegment);

				//Get location of wanted device
				ArraySegment<byte> locationSegment = new ArraySegment<byte>(locationBuffer);
				await websocket.ReceiveAsync(locationSegment, token);
				string location = Encoding.UTF8.GetString(locationSegment);

				//Get type of device
				ArraySegment<byte> typeSegment = new ArraySegment<byte>(typeBuffer);
				await websocket.ReceiveAsync(typeSegment, token);
				string type = Encoding.UTF8.GetString(typeSegment);

				//Get subname representing what part of the device u want
				ArraySegment<byte> subnameSegment = new ArraySegment<byte>(subnameBuffer);
				await websocket.ReceiveAsync(subnameSegment, token);
				string subname = Encoding.UTF8.GetString(subnameSegment);

				//Check if device exists
				bool found = false;
				RemoteDevice<T, U> foundDevice = null;

				if (remoteDeviceConnections.getRemoteDeviceWithNameLocationAndType(name, location, type, out RemoteDevice<T, U> outputDevice)) {
					foundDevice = outputDevice;

					List<string> listOfSubNames = foundDevice.getSubNamesList();

					foreach (var obj in listOfSubNames) {
						if (obj.ToLower().Equals(subname.ToLower())) {
							found = true;
						}
					}
				}
				//Tell socket if the device was found or not

				if (found) {
					//Get the device
					if (foundDevice.getSendingConnectionWithSubname(subname, out SendingConnection<U> outputConnection)) {
						while (!token.IsCancellationRequested) {
							ArraySegment<byte> receivedArraySegment = new ArraySegment<byte>(new byte[2048]);
							await websocket.ReceiveAsync(receivedArraySegment, token);
							string receivedJson = Encoding.UTF8.GetString(receivedArraySegment);
							receivedJson.TrimEnd('\0');

							try {
								U newObject = JsonSerializer.Deserialize<U>(receivedJson);

								outputConnection.queueObjectForSending(newObject);
							}
							catch (Exception e) {
								Console.WriteLine(e);
								throw;
							}
						}
					}
					else {
						//Send found
						ArraySegment<byte> foundBytes = new ArraySegment<byte>(Encoding.UTF8.GetBytes("found"));
						await websocket.SendAsync(foundBytes, WebSocketMessageType.Text, true, token);
					}
				}
				else {
					//Send no match
					ArraySegment<byte> foundBytes = new ArraySegment<byte>(Encoding.UTF8.GetBytes("no match"));
					await websocket.SendAsync(foundBytes, WebSocketMessageType.Text, true, token);
					//End socket exchange
					socketFinishedTcs.TrySetResult(new object());
					return;
				}
			}
			catch (Exception ex) {
				logger.LogWarning(ex, "Exception occurred in websocket");
			}

			//Complete
			socketFinishedTcs.TrySetResult(new object());
		}
	}
}