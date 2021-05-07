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
	public class CrestronUser {

		private CrestronUserHandler crestronUserHandler { get; set; }

		/// <summary>
		/// Token with specific id of the controller
		/// </summary>
		public ControlToken controlToken { get; set; }

		public CrestronUser(CrestronUserHandler crestronUserHandler) {
			this.crestronUserHandler = crestronUserHandler;
			controlToken = new ControlToken();
		}

		/// <summary>
		/// Get position in queue
		/// </summary>
		/// <returns></returns>
		public int getPosition() {
			return crestronUserHandler.checkPosition(this);
		}

		/// <summary>
		/// Check if the controller is able to send messages
		/// </summary>
		/// <returns></returns>
		public bool isControlling() {
			return crestronUserHandler.checkIfControlling(this);
		}

		/// <summary>
		/// Attempt to send msg to remote device
		/// </summary>
		/// <param name="msg"></param>
		/// <returns>True if msg was sent</returns>
		public async Task<bool> send(string msg, CancellationToken ct) {
			return await crestronUserHandler.sendAsync(msg,this);
		}

		/// <summary>
		/// Check if the connection to the remtoe device is okay, and if not checks if it can reconnect
		/// </summary>
		/// <returns></returns>
		public bool checkConnectionAvailable() {
			return crestronUserHandler.checkConnectionAvailable();
		}

		/// <summary>
		/// Remove this object from its controlHandlers list of controllers
		/// </summary>
		public void delete() {
			crestronUserHandler.deleteCrestronUser(this);
		}
	}
}
