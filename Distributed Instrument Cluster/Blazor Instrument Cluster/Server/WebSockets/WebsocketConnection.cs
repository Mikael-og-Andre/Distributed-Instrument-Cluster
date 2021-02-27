using Blazor_Instrument_Cluster.Server.Injection;
using Blazor_Instrument_Cluster.Server.Worker;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster {

	public class WebsocketConnection : ISocketHandler {
		private RemoteDeviceConnection<string> remoteDeviceConnections;     //remote Device connections

		public WebsocketConnection(IRemoteDeviceConnections<string> connections) {
			remoteDeviceConnections = (RemoteDeviceConnection<string>)connections;
		}

		public async Task addSocket(WebSocket webSocket, TaskCompletionSource<object> socketFinishedTcs) {
			//Get name of video device that they want the video from

			//Do main loop
			while (true) {
				for (int i = 0; i < 1000000; i++) {
					byte[] buffer = Encoding.ASCII.GetBytes("Current i is " + i);
					await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
					await Task.Delay(1);
				}

				socketFinishedTcs.TrySetResult(new object());
			}
		}
	}
}