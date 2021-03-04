using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server.WebSockets {

	/// <summary>
	/// Interface for a crestron connection handler
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public interface ICrestronSocketHandler {

		public void StartCrestronWebsocketProtocol(WebSocket websocket, TaskCompletionSource<object> socketFinishedTcs);

	}
}