using Blazor_Instrument_Cluster.Server.Injection;
using Blazor_Instrument_Cluster.Server.Worker;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.Events;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blazor_Instrument_Cluster {

	public class WebsocketConnection<T> : ISocketHandler {
		private RemoteDeviceConnection<T> remoteDeviceConnections;     //remote Device connections
		private ILogger<WebsocketConnection<T>> logger;

		public WebsocketConnection(ILogger<WebsocketConnection<T>> logger,IRemoteDeviceConnections<string> connections) {
			remoteDeviceConnections = (RemoteDeviceConnection<T>)connections;
			this.logger = logger;
		}

		public async Task AddSocket(WebSocket webSocket, TaskCompletionSource<object> socketFinishedTcs) {
			//Cancellation token
			CancellationToken token = new CancellationToken(false);

			//Get name of video device that they want the video from
			ArraySegment<byte> buffer = new ArraySegment<byte>();
			WebSocketReceiveResult receive = await webSocket.ReceiveAsync(buffer,token);

			string name = receive.ToString();
			logger.LogDebug("Websocket connection has asked for device with name: {0}",name);
			//Setup frame consumer to receive pushed frames from connection
			VideoConnectionFrameConsumer<T> consumer = new VideoConnectionFrameConsumer<T>(name);
			bool subbed = false;
			while (!subbed) {
				subbed = remoteDeviceConnections.SubscribeToVideoProviderWithName(name, consumer);
				logger.LogCritical("WebSocket tried to subscribe to {0} but i could not be found in the provider queue",name);
			}
			//Get consumer queue
			ConcurrentQueue<T> providerQueue =consumer.GetConcurrentQueue();

			//Do main loop
			while (!token.IsCancellationRequested) {
				//Check if something in queue
				if (!providerQueue.TryPeek(out _)) continue;
				//Dequeue and send
				providerQueue.TryDequeue(out T result);
				if (result is null) {
					continue;
				}
				ArraySegment<byte> bytes = SerializeObject(result);

				await webSocket.SendAsync(bytes,WebSocketMessageType.Binary,true,token);

			}
			socketFinishedTcs.TrySetResult(new object());
		}

		private ArraySegment<byte> SerializeObject(T obj) {

			byte[] jsonUtf8Bytes;
			var options = new JsonSerializerOptions {
				WriteIndented = true
			};
			//Serialize to json utf8bytes
			jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(obj, options);

			ArraySegment<byte> bytes = new ArraySegment<byte>(jsonUtf8Bytes);

			return bytes;
		}
	}
}