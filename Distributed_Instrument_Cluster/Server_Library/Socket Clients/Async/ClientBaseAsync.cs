using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Networking_Library;
using Server_Library.Authorization;

namespace Server_Library.Socket_Clients.Async {

	/// <summary>
	/// Base class for communicator classes, intended to be on the remote side of the instrument cluster
	/// <Author>Mikael Nilssen</Author>
	/// </summary>
	public abstract class ClientBaseAsync {

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
		/// Authorization code to send to the server
		/// </summary>
		protected AccessToken accessToken;

		//State
		/// <summary>
		/// Is the socket connected to the server
		/// </summary>
		public bool isSocketConnected { get; set; } = false;

		/// <summary>
		/// Is the setup process Complete
		/// </summary>
		protected bool isSetup { get; set; } = false;

		protected ClientBaseAsync(string ip, int port, AccessToken accessToken) {
			Ip = ip;
			Port = port;
			this.accessToken = accessToken;
		}

		/// <summary>
		/// Starts the client and attempts to connect to the server
		/// </summary>
		public async Task setup() {
			// Create new socket
			connectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			//Connect
			await connectToServer(connectionSocket);
			//Create stream
			connectionNetworkStream = new NetworkStream(connectionSocket, true);
			//Setup
			await setupConnection(connectionSocket);
		}

		/// <summary>
		/// Attempts to connect to the given host and ip
		/// </summary>
		/// <param name="socket"> unconnected Socket</param>
		/// <returns> boolean representing successful connection</returns>
		private async Task connectToServer(Socket socket) {
			await socket.ConnectAsync(Ip,Port);
			isSocketConnected = true;
		}

		/// <summary>
		/// Sends needed information for a connection ot the server
		/// </summary>
		/// <param name="socket"></param>
		private async Task setupConnection(Socket socket) {
			//NetworkStream
			NetworkStream stream = new NetworkStream(socket);
			//Get start signal
			await NetworkingOperations.receiveStringAsync(stream);
			//send auth hash
			//TODO: add auth hash encryption
			await NetworkingOperations.sendStringAsync(accessToken.getAccessString(), stream);
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