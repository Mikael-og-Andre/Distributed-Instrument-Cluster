using System.Net.Sockets;
using System.Threading;
using Server_Library.Authorization;

namespace Server_Library.Connection_Classes {

	/// <summary>
	/// Base Class for a connection made to the listener
	/// </summary>
	public abstract class ConnectionBase {

		/// <summary>
		/// The thread the connection is running on
		/// </summary>
		private Thread homeThread;

		/// <summary>
		/// Token representing a valid connection to the server
		/// </summary>
		protected AccessToken accessToken { get; set; }

		/// <summary>
		/// has the connection been Authorized
		/// </summary>
		public bool isAuthorized { get; set; } = false;

		/// <summary>
		/// Information about remote device
		/// </summary>
		protected ClientInformation info { get; set; }

		/// <summary>
		/// Socket of the client Connection
		/// </summary>
		protected Socket socket { get; private set; }

		/// <summary>
		/// Token for cancelling operations
		/// </summary>
		protected CancellationToken cancellation { get; private set; }

		/// <summary>
		/// has all setup been completed
		/// </summary>
		public bool isSetupCompleted { get; set; } = false;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="homeThread"> Thread Connection is running on</param>
		/// <param name="socket">Socket</param>
		/// <param name="accessToken">Token for authorization</param>
		/// <param name="info">Information About Client</param>
		/// <param name="cancellation">Token for cancelling</param>
		protected ConnectionBase(Thread homeThread, Socket socket, AccessToken accessToken, ClientInformation info, CancellationToken cancellation) {
			this.homeThread = homeThread;
			this.socket = socket;
			this.accessToken = accessToken;
			this.info = info;
			this.cancellation = cancellation;
		}

		/// <summary>
		/// Get the Instrument information from Crestron connection
		/// </summary>
		/// <returns>Instrument Information</returns>
		public ClientInformation getInstrumentInformation() {
			return info;
		}


	}
}