using Blazor_Instrument_Cluster.Server.Events;
using Blazor_Instrument_Cluster.Server.Injection;
using Blazor_Instrument_Cluster.Server.Worker;
using Instrument_Communicator_Library;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Networking_Library;

namespace Blazor_Instrument_Cluster {

	/// <summary>
	/// Class that handles incoming video websocket connections
	/// <author></author>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class VideoWebsocketHandler<T> : IVideoSocketHandler where T : ISerializeObject {
		/// <summary>
		/// remote Device connections
		/// </summary>
		private RemoteDeviceConnection remoteDeviceConnections;
		/// <summary>
		/// Logger
		/// </summary>
		private ILogger<VideoWebsocketHandler<T>> logger;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		public VideoWebsocketHandler(ILogger<VideoWebsocketHandler<T>> logger, IServiceProvider services) {
			remoteDeviceConnections = (RemoteDeviceConnection)services.GetService(typeof(IRemoteDeviceConnections));
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
				byte[] startBytes = Encoding.ASCII.GetBytes("start");
				ArraySegment<byte> startSeg = new ArraySegment<byte>(startBytes);
				await websocket.SendAsync(startSeg, WebSocketMessageType.Text, true, token);

				//Get name of video device that they want the video from
				byte[] bufferBytes = new byte[100];
				ArraySegment<byte> buffer = new ArraySegment<byte>(bufferBytes);
				await websocket.ReceiveAsync(buffer, token);
				byte[] nameBytes = buffer.ToArray();
				string name = Encoding.ASCII.GetString(nameBytes).TrimEnd('\0');

				logger.LogDebug("Websocket Video connection has asked for device with name: {0} ", name);
				//Setup frame consumer to receive pushed frames from connection
				VideoConnectionFrameConsumer consumer = new VideoConnectionFrameConsumer(name);
				//Check for name
				bool subbed = false;
				int maxLoops = 20;
				int looped = 0;
				while (!subbed && (looped < maxLoops)) {
					subbed = remoteDeviceConnections.subscribeToVideoProviderWithName(name, consumer);
					logger.LogDebug("WebSocket tried to subscribe to {0} but i could not be found in the provider queue", name);
					looped++;
					await Task.Delay(100, token);
				}

				//if the device was found send found, and continue
				if (subbed) {
					logger.LogDebug("Video Websocket requested a device: {0} And the device was found",name);
					ArraySegment<byte> foundBytes = Encoding.ASCII.GetBytes("found");
					await websocket.SendAsync(foundBytes, WebSocketMessageType.Text, true, token);

					//Get consumer queue
					ConcurrentQueue<VideoFrame> providerQueue = consumer.GetConcurrentQueue();

					//Do main loop
					while (!token.IsCancellationRequested) {
						//Check if something in queue
						if (!providerQueue.TryPeek(out _)) continue;
						//Dequeue and send
						providerQueue.TryDequeue(out VideoFrame result);
						ArraySegment<byte> bytesSegment = new ArraySegment<byte>(result.getBytes());
						await websocket.SendAsync(bytesSegment, WebSocketMessageType.Binary, true, token);
					}
					//After loop end websokcet connection
					socketFinishedTcs.TrySetResult(new object());
					return;
				}
				else {
					logger.LogCritical("Video Websocket requested a device: {0} that did not exist",name);
					//Not subbed, send fail and close
					ArraySegment<byte> failedBytes = new ArraySegment<byte>(Encoding.ASCII.GetBytes("failed"));
					await websocket.SendAsync(failedBytes, WebSocketMessageType.Text, true, token);

					//end websokcet connection
					socketFinishedTcs.TrySetResult(new object());
				}

			}
			catch (Exception ex) {
				//if websocket is running send close, and close socket pipeline
				if (websocket.State != WebSocketState.Closed) {
					await websocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Closing socket", token);
				}
				logger.LogError(ex, "Exception Thrown in VideoWebsocketHandler");
				socketFinishedTcs.TrySetResult(new object());
			}
		}
	}
}