using System;
using Blazor_Instrument_Cluster.Server.ControlHandler;

namespace Blazor_Instrument_Cluster.Server.SendingHandler {

	/// <summary>
	/// Token for representing who has the right to control
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class ControlToken<U> {

		/// <summary>
		/// The control handler for this token
		/// </summary>
		private ControlHandler<U> controlHandler;

		/// <summary>
		/// Id
		/// </summary>
		public Guid id { get; private set; }

		/// <summary>
		/// Does this token have control
		/// </summary>
		public bool hasControl { get; set; }
		/// <summary>
		/// Bool representing if the token was abandoned
		/// </summary>
		public bool isInactive { get; set; }

		/// <summary>
		/// Time tracker that should be updated representing the last time an action from the controller was taken
		/// </summary>
		public DateTime timeLastAction { get; private set; }

		public ControlToken(ControlHandler<U> handler) {
			this.controlHandler = handler;
			this.id = Guid.NewGuid();
			this.hasControl = false;
			this.isInactive = false;
			timeLastAction = DateTime.UtcNow;
		}

		/// <summary>
		/// Tries to send the object
		/// </summary>
		/// <param name="json"></param>
		/// <returns>True if successfully sent</returns>
		public bool send(U json) {
			//Update the time since last action
			updateTime();
			//Attempt to send
			return controlHandler.trySend(json, this);
		}

		/// <summary>
		/// Get the position of the token in queue
		/// </summary>
		/// <returns>position, or -1 if not in the queue</returns>
		public int getPosition() {
			updateTime();
			return controlHandler.getQueuePosition(this);
		}

		/// <summary>
		/// Sets time since last action to current time
		/// </summary>
		private void updateTime() {
			timeLastAction = DateTime.UtcNow;
		}
		/// <summary>
		/// Abandon the token
		/// </summary>
		public void abandon() {
			isInactive = true;
		}
	}
}