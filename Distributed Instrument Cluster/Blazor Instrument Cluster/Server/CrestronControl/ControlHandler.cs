using System.Threading;
using Server_Library;
using Server_Library.Connection_Types;

namespace Blazor_Instrument_Cluster.Server.CrestronControl {

	/// <summary>
	/// Queue for dealing with one at a time control of a remote device
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class ControlHandler {

		/// <summary>
		/// Time the controller is allowed to be inactive before control is relinquished
		/// </summary>
		private double allowedInactiveTimeMinutes;
		
		/// <summary>
		/// The current token with control of the system
		/// </summary>
		private ControlToken currentController;

		/// <summary>
		/// Connection that sends items to the remote device
		/// </summary>
		private SendingConnection sendingConnection;

		/// <summary>
		/// Should the control allow multiple senders
		/// </summary>
		private bool allowMultiControl { get; set; }

		/// <summary>
		/// Cancellation source
		/// </summary>
		private CancellationTokenSource cancellationTokenSource;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="allowedInactiveTimeMinutes"></param>
		/// <param name="sender"></param>
		/// <param name="allowMultiControl">True if you want to disable the check for current controllers</param>
		public ControlHandler(double allowedInactiveTimeMinutes, SendingConnection sender, bool allowMultiControl) {
			//create an abandoned token
			this.currentController = new ControlToken(this);
			this.currentController.abandon();

			this.allowedInactiveTimeMinutes = allowedInactiveTimeMinutes;
			this.sendingConnection = sender;
			this.allowMultiControl = allowMultiControl;
			this.cancellationTokenSource = new CancellationTokenSource();
		}

		/// <summary>
		/// Generates a control token and gives it to the caller
		/// </summary>
		/// <returns></returns>
		public ControlToken generateToken() {
			//Create a new control token
			ControlToken token = new ControlToken(this);
			//Return
			return token;
		}

		/// <summary>
		/// Try to send a command on the connection
		/// </summary>
		/// <param name="jsonObject">Object that u want to send</param>
		/// <param name="token">Token for control, Leave as null if multi control is on</param>
		/// <returns>True if object was queued for sending</returns>
		public bool trySend(byte[] bytes, ControlToken token) {
			//If multi user is on, just send
			if (allowMultiControl) {
				sendingConnection.queueByteArrayForSending(bytes);
				return true;
			}
			else {
				//Check if the token is the current controller
				if (currentController.id.Equals(token.id)) {
					sendingConnection.queueByteArrayForSending(bytes);
					return true;
				}
				else {
					return false;
				}
			}
		}

		/// <summary>
		/// Request to become the controller of the current
		/// </summary>
		/// <param name="controlToken"></param>
		/// <returns></returns>
		public bool requestControl(ControlToken controlToken) {
			//update time since last action
			controlToken.updateTime();
			lock (currentController) {
				//check if current controller has been abandoned
				if (currentController.isInactive) {
					setCurrentController(controlToken);
					return true;
				}
				//check if current controller has not sent anything for a long time
				else if (currentController.timeLastAction>currentController.timeLastAction.AddMinutes(allowedInactiveTimeMinutes)) {
					setCurrentController(controlToken);
					return true;
				}
				else {
					return false;
				}
			}
		}

		/// <summary>
		/// Get the information
		/// </summary>
		/// <returns>ClientInformation</returns>
		public ClientInformation getClientInformation() {
			return sendingConnection.getClientInformation();
		}

		/// <summary>
		/// Set new current controller and update its has control value
		/// </summary>
		/// <param name="controlToken"></param>
		private void setCurrentController(ControlToken controlToken) {
			//Update currentControl
			currentController = controlToken;
			currentController.hasControl = true;
		}
		
	}
}