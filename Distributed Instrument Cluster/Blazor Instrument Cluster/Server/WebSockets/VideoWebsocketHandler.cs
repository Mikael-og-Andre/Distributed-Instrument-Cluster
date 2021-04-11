using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.ProviderAndConsumer;
using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement;
using Blazor_Instrument_Cluster.Server.Worker;
using Microsoft.Extensions.Logging;
using PackageClasses;

namespace Blazor_Instrument_Cluster.Server.WebSockets {

	/// <summary>
	/// Class that handles incoming video websocket connections
	/// <author> Mikael Nilssen</author>
	/// </summary>
	public class VideoWebsocketHandler : IVideoSocketHandler {
		/// <summary>
		/// remote Device connections
		/// </summary>
		private readonly RemoteDeviceManager remoteDeviceManager;
		/// <summary>
		/// Logger
		/// </summary>
		private readonly ILogger<VideoWebsocketHandler> logger;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		public VideoWebsocketHandler(ILogger<VideoWebsocketHandler> logger, IServiceProvider services) {
			remoteDeviceManager = (RemoteDeviceManager)services.GetService(typeof(IRemoteDeviceManager));
			this.logger = logger;
		}

		/// <summary>
		/// Gets the wanted video device from the websocket client and subscribes to that device, and pushes incoming sockets to web client
		/// </summary>
		/// <param name="websocket"></param>
		/// <param name="socketFinishedTcs"></param>
		/// <returns></returns>
		public async Task StartWebSocketVideoProtocol(WebSocket websocket, TaskCompletionSource<object> socketFinishedTcs) {
			//Cancellation token
			CancellationToken token = new CancellationToken(false);
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
				RemoteDevice foundDevice = null;

				if (remoteDeviceManager.getRemoteDeviceWithNameLocationAndType(name,location,type, out RemoteDevice outputDevice)) {
					foundDevice = outputDevice;

					List<SubDevice> listOfSubNames = foundDevice.getSubDeviceList();

					foreach (var obj in listOfSubNames) {
						if (obj.subname.ToLower().Equals(subname.ToLower())) {
							found = true;
						}
					}

				}
				//Tell socket if the device was found or not
				
				if (found) {
					//Send found
					ArraySegment<byte> foundBytes = new ArraySegment<byte>(Encoding.UTF32.GetBytes("found"));
					await websocket.SendAsync(foundBytes, WebSocketMessageType.Text, true, token);

					//subscribe to provider and push frames
					VideoObjectConsumer objectConsumer = new VideoObjectConsumer(name,location,type,subname);
					//Subscribe consumer to the correct provider, if not found cancel connection
					if (foundDevice.subscribeToProvider(objectConsumer)) {
						//Get queue for the objects pushed to the consumer
						ConcurrentQueue<Jpeg> consumerQueue = objectConsumer.GetConcurrentQueue();

						//Loop and send objects tot he connected websocket
						while (!token.IsCancellationRequested) {
							if (consumerQueue.TryDequeue(out Jpeg output)) {
								//Serialize object
								string json = JsonSerializer.Serialize(output);
								//Send
								ArraySegment<byte> jsonSegment = new ArraySegment<byte>(Encoding.UTF32.GetBytes(json));
								await websocket.SendAsync(jsonSegment, WebSocketMessageType.Text, true, token);

							}
							else {
								await Task.Delay(5);
							}
						}

					}
					else {
						//End socket exchange
						socketFinishedTcs.TrySetResult(new object());
						return;
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
			}

			//Complete
			socketFinishedTcs.TrySetResult(new object());
			
		}
	}
}