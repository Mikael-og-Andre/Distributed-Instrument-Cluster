



using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Blazor_Instrument_Cluster.Server.Worker {

	/// <summary>
	/// Interface for video websocket connection
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public interface IVideoSocketHandler {

		public Task StartWebSocketVideoProtocol(WebSocket websocket, TaskCompletionSource<object> socketFinishedTcs);

	}
}