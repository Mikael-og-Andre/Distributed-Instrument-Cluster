using Blazor_Instrument_Cluster.Server.Events;
using Blazor_Instrument_Cluster.Server.Injection;
using Blazor_Instrument_Cluster.Server.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Instrument_Communicator_Library.Information_Classes;
using Instrument_Communicator_Library.Interface;

namespace Blazor_Instrument_Cluster {

	public class WebsocketConnection<T> : ISocketHandler where T : ISerializeableObject {
		private RemoteDeviceConnection remoteDeviceConnections;     //remote Device connections
		private ILogger<WebsocketConnection<T>> logger;

		public WebsocketConnection(ILogger<WebsocketConnection<T>> logger, IServiceProvider services) {
			remoteDeviceConnections = (RemoteDeviceConnection) services.GetService(typeof(IRemoteDeviceConnections));
			this.logger = logger;
		}

		public async Task AddSocketVideo(WebSocket webSocket, TaskCompletionSource<object> socketFinishedTcs) {
			//Cancellation token
			CancellationToken token = new CancellationToken(false);

			//Get name of video device that they want the video from
			//ArraySegment<byte> buffer = new ArraySegment<byte>();
			//WebSocketReceiveResult receive = await webSocket.ReceiveAsync(buffer, token);
			string name = "Radar1";

			logger.LogDebug("Websocket Video connection has asked for device with name: {0} ", name);
			//Setup frame consumer to receive pushed frames from connection
			VideoConnectionFrameConsumer consumer = new VideoConnectionFrameConsumer(name);
			bool subbed = false;
			while (!subbed) {
				subbed = remoteDeviceConnections.SubscribeToVideoProviderWithName("Radar1", consumer);
				logger.LogCritical("WebSocket tried to subscribe to {0} but i could not be found in the provider queue", name);
			}
			//Get consumer queue
			ConcurrentQueue<VideoFrame> providerQueue = consumer.GetConcurrentQueue();

			//Do main loop
			while (!token.IsCancellationRequested) {
				//Check if something in queue
				if (!providerQueue.TryPeek(out _)) continue;
				//Dequeue and send
				providerQueue.TryDequeue(out VideoFrame result);

				//TODO: Make serializer for other things
				ArraySegment<byte> bytesSegment = result.getBytes();

				await webSocket.SendAsync(bytesSegment, WebSocketMessageType.Binary, true, token);
			}
			socketFinishedTcs.TrySetResult(new object());

		}
		
	}
}