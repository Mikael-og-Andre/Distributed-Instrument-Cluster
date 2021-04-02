using System;
using Blazor_Instrument_Cluster.Server.CommandHandler;
using Server_Library;
using Server_Library.Connection_Types;

namespace Blazor_Instrument_Cluster.Server.SendingHandler {

	/// <summary>
	/// Queue for dealing with one at a time control of a remote device
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class SendingControlHandler<U> {

		/// <summary>
		/// Time the controller is allowed to be inactive before control is relinquished
		/// </summary>
		private double allowedInactiveTimeMinutes;

		/// <summary>
		/// Queue of controllers
		/// </summary>
		private TrackingQueue<ControlToken> queueControllers;

		/// <summary>
		/// The current token with control of the system
		/// </summary>
		private ControlToken currentController;

		/// <summary>
		/// Connection that sends items to the remote device
		/// </summary>
		private SendingConnection<U> sendingConnection;

		private bool allowMultiControl { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="allowedInactiveTimeMinutes"></param>
		/// <param name="sender"></param>
		/// <param name="allowMultiControl">True if you want to disable the check for current controllers</param>
		public SendingControlHandler(double allowedInactiveTimeMinutes, SendingConnection<U> sender, bool allowMultiControl) {
			this.queueControllers = new TrackingQueue<ControlToken>();
			this.currentController = null;
			this.allowedInactiveTimeMinutes = allowedInactiveTimeMinutes;
			this.sendingConnection = sender;
			this.allowMultiControl = allowMultiControl;
		}

		/// <summary>
		/// Generates a control token and gives it to the caller
		/// </summary>
		/// <returns></returns>
		public ControlToken enterQueue() {
			//Create a new control token
			ControlToken token = new ControlToken();
			//if multi user check has control
			if (allowMultiControl) {
				token.hasControl = true;
				return token;
			}

			//Add token to queue
			queueControllers.enqueue(token);
			//Return
			return token;
		}

		/// <summary>
		/// Updates the handler and relinquishes control of the controller if the specified inactive time has been surpassed or if there is no current controller
		/// </summary>
		public void updateController() {
			//if there is no current controller get the next one from the queue
			if (currentController is null) {
				setNextControllerFromQueue();
			}
			//If the current controller is inactive set the next controller
			else if (currentController.isInactive) {
				setNextControllerFromQueue();
			}
			//Check if the current controller has not preformed an action in a while, relinquish control if nothing has been updated in a while
			else if (currentController.timeLastAction.AddMinutes(allowedInactiveTimeMinutes) > DateTime.UtcNow) {
				//If passed inactive time and there are people in queue, go next
				if (!queueControllers.isEmpty()) {
					setNextControllerFromQueue();
				}
			}
		}

		/// <summary>
		/// Get a controller from queue if possible, set it to controlling, otherwise set a null as current controller
		/// </summary>
		private void setNextControllerFromQueue() {
			//If there is a current controller set it to no longer have control
			if (currentController is not null) {
				currentController.hasControl = false;
			}

			//Try to get a new controller
			if (queueControllers.tryDequeue(out ControlToken output)) {
				currentController = output;
				currentController.hasControl = true;
			}
			//Else set to null for no controller
			else {
				currentController = null;
			}
		}

		/// <summary>
		/// Try to send a command on the connection
		/// </summary>
		/// <param name="jsonObject">Object that u want to send</param>
		/// <param name="token">Token for control, Leave as null if multi control is on</param>
		/// <returns>True if object was queued for sending</returns>
		public bool trySend(U jsonObject, ControlToken token) {
			//If multi user is on, just send
			if (allowMultiControl) {
				sendingConnection.queueObjectForSending(jsonObject);
				return true;
			}
			//Check if null when not multi control
			if (token is null) {
				return false;
			}

			//Check if the controlToken Matches the current controller
			if (currentController.id.Equals(token.id)) {
				//Send and return true
				sendingConnection.queueObjectForSending(jsonObject);
				return true;
			}
			//Input token does not have control return
			else {
				return false;
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
		/// Gets the position of the device in the queue, 0 if the token has control, return -1 if it was not found in the queue, else it returns the position
		/// </summary>
		/// <param name="controlToken"></param>
		/// <returns>-1 if not found, 0 if in control, or int</returns>
		public int getQueuePosition(ControlToken controlToken) {
			//If multi user support return 0
			if (allowMultiControl) {
				return 0; 
			}
			//If is current controller return 0
			if (controlToken.id.Equals(currentController.id)) {
				return 0;
			}

			//Search for position in Tracking queue
			return queueControllers.getPosition(controlToken);
		}
	}
}