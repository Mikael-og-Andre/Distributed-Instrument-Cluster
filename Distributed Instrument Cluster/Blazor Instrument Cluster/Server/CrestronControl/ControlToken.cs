using System;

namespace Blazor_Instrument_Cluster.Server.CrestronControl {

	/// <summary>
	/// Token for representing who has the right to control
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class ControlToken {

		/// <summary>
		/// The control handler for this token
		/// </summary>
		private ControlHandler controlHandler;

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

		public ControlToken(ControlHandler handler) {
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
		public bool send(byte[] bytes) {
			//Update the time since last action
			updateTime();
			//Attempt to send
			return controlHandler.trySend(bytes, this);
		}

		/// <summary>
		/// Sets time since last action to current time
		/// </summary>
		public void updateTime() {
			timeLastAction = DateTime.UtcNow;
		}

		/// <summary>
		/// Request control of the device, True if you are granted control
		/// </summary>
		/// <returns>True if you are now the controller</returns>
		public bool requestControl() {
			return controlHandler.requestControl(this);
		}

		/// <summary>
		/// Abandon the token
		/// </summary>
		public void abandon() {
			isInactive = true;
		}
	}
}