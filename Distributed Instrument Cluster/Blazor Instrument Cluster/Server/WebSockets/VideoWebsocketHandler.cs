using Blazor_Instrument_Cluster.Server.Events;
using Blazor_Instrument_Cluster.Server.Injection;
using Blazor_Instrument_Cluster.Server.Worker;
using Server_Library;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.RemoteDevice;
using Networking_Library;

namespace Blazor_Instrument_Cluster {

	/// <summary>
	/// Class that handles incoming video websocket connections
	/// <author></author>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class VideoWebsocketHandler<T,U> : IVideoSocketHandler {
		/// <summary>
		/// remote Device connections
		/// </summary>
		private RemoteDeviceConnections<T,U> remoteDeviceConnections;
		/// <summary>
		/// Logger
		/// </summary>
		private ILogger<VideoWebsocketHandler<T,U>> logger;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		public VideoWebsocketHandler(ILogger<VideoWebsocketHandler<T,U>> logger, IServiceProvider services) {
			remoteDeviceConnections = (RemoteDeviceConnections<T,U>)services.GetService(typeof(IRemoteDeviceConnections<T,U>));
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
			
		}
	}
}