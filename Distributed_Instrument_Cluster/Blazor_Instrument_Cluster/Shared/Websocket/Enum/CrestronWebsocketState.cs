using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Shared.Websocket.Enum {
	/// <summary>
	/// Enum for tracking Crestron WebSocketState
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public enum CrestronWebsocketState {
		Requesting,
		InQueue,
		InControl,
		Disconnecting
	}
}
