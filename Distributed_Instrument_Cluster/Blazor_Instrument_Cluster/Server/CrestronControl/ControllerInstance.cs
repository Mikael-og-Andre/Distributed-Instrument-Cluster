using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		/// Attempt to send msg to remote device
		/// </summary>
		/// <param name="msg"></param>
		/// <returns>True if msg was sent</returns>
		public async Task<bool> send(string msg, CancellationToken ct) {
			return await controlHandler.sendAsync(msg,this);
		}

		/// <summary>
		/// Remove this object from its controlHandlers list of controllers
		/// </summary>
		public void delete() {
			controlHandler.deleteControllerInstance(this);
		}
	}
}
