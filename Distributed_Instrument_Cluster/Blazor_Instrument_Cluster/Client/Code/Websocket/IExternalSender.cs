using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Client.Code.Websocket {
	/// <summary>
	/// Interface for sending messages from an external class via the Crestron WebSocket
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public interface IExternalSender {
		public Task<bool> sendExternal(string msg);
	}
}
