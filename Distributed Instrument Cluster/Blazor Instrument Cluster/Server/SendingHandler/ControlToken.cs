using System;

namespace Blazor_Instrument_Cluster.Server.SendingHandler {

	/// <summary>
	/// Token for representing who has the right to control
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class ControlToken {

		/// <summary>
		/// Id
		/// </summary>
		public Guid id { get; set; }

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
		public DateTime timeLastAction { get; set; }

		public ControlToken() {
			this.id = Guid.NewGuid();
			this.hasControl = false;
			this.isInactive = false;
			timeLastAction = DateTime.UtcNow;
		}
		/// <summary>
		/// Sets time since last action to current time
		/// </summary>
		public void updateTime() {
			timeLastAction = DateTime.UtcNow;
		}
	}
}