using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server_Library.Connection_Types.Async;

namespace Blazor_Instrument_Cluster.Server.CrestronControl {

	/// <summary>
	/// Control Handler for a sub connection
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class ControlHandler {
		public Guid id { get; set; }

		private SubConnection subConnection { get; set; }

		private List<ControllerInstance> controllerInstances { get; set; }

		public ControlHandler(SubConnection subConnection) {
			this.subConnection = subConnection;
			this.id = subConnection.id;
			this.controllerInstances = new List<ControllerInstance>();
		}

		/// <summary>
		/// Create a new controller instance
		/// </summary>
		/// <returns></returns>
		public ControllerInstance createControllerInstance() {
			ControllerInstance newControllerInstance = new ControllerInstance(id, this);
			lock (controllerInstances) {
				controllerInstances.Add(newControllerInstance);
			}

			return newControllerInstance;
		}

		/// <summary>
		/// Remove Controller instance from list
		/// </summary>
		/// <param name="instance"></param>
		public void deleteControllerInstance(ControllerInstance instance) {
			lock (controllerInstances) {
				controllerInstances.Remove(instance);
			}
		}

		/// <summary>
		/// Finds the position of a controller instance in the list
		/// </summary>
		/// <param name="controllerInstance"></param>
		/// <returns></returns>
		public int checkPosition(ControllerInstance controllerInstance) {
			lock (controllerInstances) {
				for (int i = 0; i < controllerInstances.Count; i++) {
					//Check if same
					if (controllerInstance.Equals(controllerInstances[i])) {
						return i;
					}
				}
				return -1;
			}
		}

		/// <summary>
		/// Check if the input controller is at index 0
		/// </summary>
		/// <param name="controllerInstance"></param>
		/// <returns></returns>
		public bool checkIfControlling(ControllerInstance controllerInstance) {
			try {
				ControllerInstance first = controllerInstances[0];
				if (first is null) {
					return false;
				}
				if (first.Equals(controllerInstance)) {
					return true;
				}
				return false;
			}
			catch (Exception) {
				return false;
			}
		}

		/// <summary>
		/// Attempt to send a msg with the authority of the input controller instance
		/// </summary>
		/// <param name="bytes">Bytes sent to the remote connection</param>
		/// <param name="controllerInstance"></param>
		/// <returns></returns>
		public async Task<bool> sendAsync(byte[] bytes,ControllerInstance controllerInstance) {
			bool isControlling=checkIfControlling(controllerInstance);
			if (isControlling) {
				DuplexConnectionAsync con=(DuplexConnectionAsync)subConnection.connection;
				await con.sendBytesAsync(bytes);
				return true;
			}
			else {
				return false;
			}
		}
	}
}