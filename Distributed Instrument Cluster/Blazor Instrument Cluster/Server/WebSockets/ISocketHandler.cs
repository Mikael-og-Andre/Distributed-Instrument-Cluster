



using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Blazor_Instrument_Cluster.Server.Worker {

	public interface ISocketHandler {

		public Task addSocket(WebSocket webSocket, TaskCompletionSource<object> socketFinishedTcs);

	}
}