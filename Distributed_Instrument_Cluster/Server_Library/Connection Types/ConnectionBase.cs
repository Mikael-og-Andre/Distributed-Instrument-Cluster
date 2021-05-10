using System.Net.Sockets;
using System.Threading;

namespace Server_Library.Connection_Types {

	/// <summary>
	/// Base Class for a connection made to the listener
	/// </summary>
	public abstract class ConnectionBase {


		/// <summary>
		/// Socket of the client Connection
		/// </summary>
		protected Socket socket { get; private set; }

		/// <summary>
		/// Network Stream for the connection
		/// </summary>
		protected NetworkStream connectionNetworkStream { get; set; }

		/// <summary>
		/// Token for cancelling operations
		/// </summary>
		protected CancellationToken cancellation { get; private set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="socket">Socket</param>
		/// <param name="cancellation">Token for cancelling</param>
		protected ConnectionBase(Socket socket, CancellationToken cancellation) {
			this.socket = socket;
			this.cancellation = cancellation;
			this.connectionNetworkStream = new NetworkStream(socket, true);
		}

	}
}