using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Shared.Websocket.Enum {
	/// <summary>
	/// Enum for tracking what type of message will be received when communicating between server and webapp with the crestronWebsocket
	/// </summary>
	public enum CrestronWebsocketMessageType {
		GetToken,
		EnterQueue,
		QueuePositionUpdate,
		Command,
		Leave
	}
}
