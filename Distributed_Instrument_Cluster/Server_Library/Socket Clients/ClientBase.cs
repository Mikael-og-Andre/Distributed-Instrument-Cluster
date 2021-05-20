using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Socket_Library;

namespace Server_Library.Socket_Clients {

	/// <summary>
	/// Base class for communicator classes
	/// <Author>Mikael Nilssen</Author>
	/// </summary>
	public abstract class ClientBase {

		/// <summary>
		/// Ip address of target server
		/// </summary>
		private string Ip { get; set; }

		/// <summary>
		/// Port of target server
		/// </summary>
		private int Port { get; set; }

		/// <summary>
		/// Connection to server
		/// </summary>
		protected Socket connectionSocket;

		/// <summary>
		/// Network stream for the connection
		/// </summary>
		protected NetworkStream connectionNetworkStream { get; set; }

		/// <summary>
		/// Cancellation token used to stop loops
		/// </summary>
		protected CancellationToken isRunningCancellationToken;

		protected ClientBase(string ip, int port, CancellationToken isRunningCancellationToken) {
			Ip = ip;
			Port = port;
			connectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.isRunningCancellationToken = isRunningCancellationToken;
		}

		/// <summary>
		/// Attempts to connect to the given host and ip
		/// </summary>
		/// <param name="socket"> unconnected Socket</param>
		/// <returns> boolean representing successful connection</returns>
		private void connectToServer(Socket socket) {
			socket.Connect(Ip, Port);
		}


		/// <summary>
		/// Returns true if data available in socket is larger than 0
		/// </summary>
		/// <returns></returns>
		protected bool isDataAvailable() {
			if (connectionSocket is null) {
				return false;
			}
			else if (connectionSocket.Available > 0) {
				return true;
			}
			else {
				return false;
			}
		}
	}
}