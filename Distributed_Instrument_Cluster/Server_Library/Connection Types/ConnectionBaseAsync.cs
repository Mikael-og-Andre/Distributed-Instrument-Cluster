using System.Net.Sockets;
using System.Threading;

namespace Server_Library.Connection_Types {
	/// <summary>
	/// Base class for an asynchronous connection
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public abstract class ConnectionBaseAsync {
		
		/// <summary>
		/// Socket of the client Connection
		/// </summary>
		protected Socket socket { get; set; }

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
		/// <param name="accessToken">Token for authorization</param>
		/// <param name="cancellation">Token for cancelling</param>
		protected ConnectionBaseAsync(Socket socket, CancellationToken cancellation) {
			this.socket = socket;
			this.cancellation = cancellation;
			this.connectionNetworkStream = new NetworkStream(socket, true);
		}

		/// <summary>
		/// Check if the socket is connected
		/// https://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c
		/// </summary>
		/// <returns></returns>
		public bool isSocketConnected() {
			return !((socket.Poll(1000, SelectMode.SelectRead) && (socket.Available == 0)) || !socket.Connected);
		}
		
	}
}
