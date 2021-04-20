using Microsoft.Extensions.Logging;
using Server_Library.Connection_Types;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement;

namespace Blazor_Instrument_Cluster.Server.WebSockets {

	/// <summary>
	/// Websocket handler for crestron control connections
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class CrestronWebsocketHandler : ICrestronSocketHandler {

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
			remoteDeviceManager = (RemoteDeviceManager)services.GetService(typeof(IRemoteDeviceManager));
		}

		/// <summary>
		/// Handles the incoming connection
		/// </summary>
		/// <param name="websocket"></param>
		/// <param name="socketFinishedTcs"></param>
		public async void StartCrestronWebsocketProtocol(WebSocket websocket, TaskCompletionSource<object> socketFinishedTcs) {
			//Create cancellation token
			CancellationToken token = new CancellationToken(false);
			Console.WriteLine("web socket made yes?");

			try {
				//Send start signal
				byte[] startBytes = Encoding.UTF32.GetBytes("start");
				ArraySegment<byte> startSegment = new ArraySegment<byte>(startBytes);
				await websocket.SendAsync(startSegment, WebSocketMessageType.Text, true, token);

				byte[] nameBuffer = new byte[1024];
				byte[] locationBuffer = new byte[1024];
				byte[] typeBuffer = new byte[1024];
				byte[] subnameBuffer = new byte[1024];

				//Get name of wanted device
				ArraySegment<byte> nameSegment = new ArraySegment<byte>(nameBuffer);
				await websocket.ReceiveAsync(nameSegment, token);
				string name = Encoding.UTF32.GetString(nameSegment).TrimEnd('\0');

				//Get location of wanted device
				ArraySegment<byte> locationSegment = new ArraySegment<byte>(locationBuffer);
				await websocket.ReceiveAsync(locationSegment, token);
				string location = Encoding.UTF32.GetString(locationSegment).TrimEnd('\0');

				//Get type of device
				ArraySegment<byte> typeSegment = new ArraySegment<byte>(typeBuffer);
				await websocket.ReceiveAsync(typeSegment, token);
				string type = Encoding.UTF32.GetString(typeSegment).TrimEnd('\0');

				//Get subname representing what part of the device u want
				ArraySegment<byte> subnameSegment = new ArraySegment<byte>(subnameBuffer);
				await websocket.ReceiveAsync(subnameSegment, token);
				string subname = Encoding.UTF32.GetString(subnameSegment).TrimEnd('\0');

				//Check if device exists
				bool found = false;
				RemoteDeviceManagement.RemoteDevice foundDevice = null;

				if (remoteDeviceManager.getRemoteDeviceWithNameLocationAndType(name, location, type, out RemoteDevice outputDevice)) {
					foundDevice = outputDevice;

					List<SubDevice> listOfSubDevices = foundDevice.getSubDeviceList();

					foreach (var obj in listOfSubDevices) {
						if (obj.subname.ToLower().Equals(subname.ToLower())) {
							found = true;
						}
					}
				}
				//Tell socket if the device was found or not

				if (found) {
					//Get the device
					if (foundDevice.getSendingConnectionWithSubname(subname, out SendingConnection outputConnection)) {
						var i = 0;
						while (!token.IsCancellationRequested) {

							//Receive a command from the socket
							ArraySegment<byte> receivedArraySegment = new ArraySegment<byte>(new byte[2048]);
							await websocket.ReceiveAsync(receivedArraySegment, token);
							outputConnection.queueByteArrayForSending(receivedArraySegment.ToArray());
							Console.WriteLine($"loop{i}");
							i++;
						}

						Console.WriteLine("canceled");
					}
					else {
						//Send found
						ArraySegment<byte> foundBytes = new ArraySegment<byte>(Encoding.UTF32.GetBytes("found"));
						await websocket.SendAsync(foundBytes, WebSocketMessageType.Text, true, token);
					}
				}
				else {
					//Send no match
					ArraySegment<byte> foundBytes = new ArraySegment<byte>(Encoding.UTF32.GetBytes("no match"));
					await websocket.SendAsync(foundBytes, WebSocketMessageType.Text, true, token);
					//End socket exchange
					socketFinishedTcs.TrySetResult(new object());
					return;
				}
			}
			catch (Exception ex) {
				logger.LogWarning(ex, "Exception occurred in websocket");
				Console.WriteLine("socket closed?");
			}

			Console.WriteLine("socket closed?");
			//Complete
			socketFinishedTcs.TrySetResult(new object());

		}
	}
}