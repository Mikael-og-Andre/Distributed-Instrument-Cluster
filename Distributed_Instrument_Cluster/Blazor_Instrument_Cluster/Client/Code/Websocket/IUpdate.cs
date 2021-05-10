using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Client.Code.Websocket {
	/// <summary>
	/// Interface for signaling state changes or updates need to occur in a ui
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public interface IUpdate {
		public void updateState();
	}
}
