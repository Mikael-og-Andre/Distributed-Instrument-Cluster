using System.Net.Sockets;
using System.Threading;
using Server_Library.Connection_Classes;

namespace Server_Library.Connection_Types {

	/// <summary>
	/// Connection for sending objects
	/// <author> Mikael Nilssen</author>
	/// </summary>
	/// <typeparam name="T">Object type you want to send</typeparam>
	public class SendingConnection<T> : ConnectionBase {

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="homeThread"></param>
		/// <param name="socket"></param>
		public SendingConnection(Thread homeThread, Socket socket) : base(homeThread, socket) { }
	}
}