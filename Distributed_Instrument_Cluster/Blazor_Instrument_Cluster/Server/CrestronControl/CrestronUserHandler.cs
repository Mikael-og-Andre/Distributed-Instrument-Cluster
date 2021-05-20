using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.CrestronControl.Interface;

namespace Blazor_Instrument_Cluster.Server.CrestronControl {

	/// <summary>
	/// Control Handler for a sub connection
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class CrestronUserHandler {
		/// <summary>
		/// Identifier for the user
		/// </summary>
		public Guid id { get; set; }

		/// <summary>
		/// Interface for communicating with a crestron
		/// </summary>
		private IConnectionCommunicator connection { get; set; }

		/// <summary>
		/// List of users
		/// </summary>
		private List<CrestronUser> crestronUsers { get; set; }

		public CrestronUserHandler(IConnectionCommunicator connection) {
			this.connection = connection;
			this.crestronUsers = new List<CrestronUser>();
		}

		/// <summary>
		/// Create a new controller instance
		/// </summary>
		/// <returns></returns>
		public CrestronUser createCrestronUser() {
			CrestronUser newCrestronUser = new CrestronUser( this);
			lock (crestronUsers) {
				crestronUsers.Add(newCrestronUser);
			}

			return newCrestronUser;
		}

		/// <summary>
		/// Remove Controller instance from list
		/// </summary>
		/// <param name="instance"></param>
		public void deleteCrestronUser(CrestronUser instance) {
			lock (crestronUsers) {
				crestronUsers.Remove(instance);
			}
		}

		/// <summary>
		/// Finds the position of a controller instance in the list
		/// </summary>
		/// <param name="crestronUser"></param>
		/// <returns></returns>
		public int checkPosition(CrestronUser crestronUser) {
			lock (crestronUsers) {
				for (int i = 0; i < crestronUsers.Count; i++) {
					//Check if same
					if (crestronUser.Equals(crestronUsers[i])) {
						return i;
					}
				}
				return -1;
			}
		}

		/// <summary>
		/// Check if the input controller is at index 0
		/// </summary>
		/// <param name="crestronUser"></param>
		/// <returns></returns>
		public bool checkIfControlling(CrestronUser crestronUser) {
			try {
				CrestronUser first = crestronUsers[0];
				if (first is null) {
					return false;
				}
				if (first.Equals(crestronUser)) {
					return true;
				}
				return false;
			}
			catch (Exception) {
				return false;
			}
		}

		/// <summary>
		/// Attempt to send a msg with the position of the user in the queue
		/// </summary>
		/// <param name="bytes">Bytes sent to the remote connection</param>
		/// <param name="crestronUser"></param>
		/// <returns>True if msg was sent</returns>
		public async Task<bool> sendAsync(string bytes, CrestronUser crestronUser) {
			if (!connection.isReady()) {
				Console.WriteLine("CrestronUserHandler: Connection is not ready, ensuringUP");
				bool isUp = connection.ensureUP();
				if (!isUp) {
					Console.WriteLine("CrestronUserHandler: Connection could not be made");
					return false;
				}
			}

			bool isControlling=checkIfControlling(crestronUser);
			if (isControlling) {
				return await this.connection.send(bytes);
			}
			else {
				return false;
			}
		}

		/// <summary>
		/// Check if the connection is up, if not check if it can be restarted
		/// </summary>
		/// <returns></returns>
		public bool checkConnectionAvailable() {
			return connection.ensureUP();
		}
	}
}