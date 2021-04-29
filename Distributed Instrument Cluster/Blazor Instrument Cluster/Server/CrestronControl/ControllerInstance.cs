using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Shared.Websocket;

namespace Blazor_Instrument_Cluster.Server.CrestronControl {
	/// <summary>
	/// An instance of a controlling client
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class ControllerInstance {
		
		public Guid connectionId { get; set; }
		private ControlHandler controlHandler { get; set; }

		/// <summary>
		/// Token with specific id of the controller
		/// </summary>
		public ControlToken controlToken { get; set; }

		public ControllerInstance(Guid connectionId, ControlHandler controlHandler) {
			this.connectionId = connectionId;
			this.controlHandler = controlHandler;
			controlToken = new ControlToken();
		}

		/// <summary>
		/// Get position in queue
		/// </summary>
		/// <returns></returns>
		public int getPosition() {
			return controlHandler.checkPosition(this);
		}

		/// <summary>
		/// Check if the controller is able to send messages
		/// </summary>
		/// <returns></returns>
		public bool isControlling() {
			return controlHandler.checkIfControlling(this);
		}

		/// <summary>
		/// Send msg to connection
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		public async Task<bool> send(string msg, CancellationToken ct) {
			throw new NotImplementedException();
		}
	}
}
