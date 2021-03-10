using Instrument_Communicator_Library.Connection_Classes;
using System.Net.Sockets;
using System.Threading;

namespace Instrument_Communicator_Library.Connection_Types {

	/// <summary>
	/// Connection for receiving objects
	/// <author>Mikael Nilssen</author>
	/// </summary>
	/// <typeparam name="T">Object Type You want the connection to receive</typeparam>
	public class ReceivingConnection<T> : ConnectionBase {

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="homeThread"></param>
		/// <param name="socket"></param>
		public ReceivingConnection(Thread homeThread, Socket socket) : base(homeThread, socket) {
		}
	}
}