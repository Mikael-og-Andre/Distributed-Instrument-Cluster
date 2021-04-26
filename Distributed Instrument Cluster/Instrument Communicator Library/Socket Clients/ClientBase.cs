using Networking_Library;
using Server_Library.Authorization;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Server_Library.Socket_Clients {

	/// <summary>
	/// Base class for communicator classes, intended to be on the remote side of the instrument cluster
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
		/// Information about hardware
		/// </summary>
		protected readonly ClientInformation information;

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

		/// <summary>
		/// Cancellation token used to stop loops
		/// </summary>
		protected CancellationToken isRunningCancellationToken;

		protected ClientBase(string ip, int port, ClientInformation informationAboutClient, AccessToken accessToken, CancellationToken isRunningCancellationToken) {
			Ip = ip;
			Port = port;
			information = informationAboutClient;
			this.accessToken = accessToken;
			this.isRunningCancellationToken = isRunningCancellationToken;
		}

		/// <summary>
		/// Starts the client and attempts to connect to the server
		/// </summary>
		public void run() {
			// Create new socket
			connectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			//Connect
			connectToServer(connectionSocket);
			//Create stream
			connectionNetworkStream = new NetworkStream(connectionSocket, true);
			//Setup
			setupConnection(connectionSocket);
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
		/// Sends needed information for a connection ot the server
		/// </summary>
		/// <param name="socket"></param>
		private void setupConnection(Socket socket) {
			//Get start signal
			NetworkingOperations.receiveStringWithSocket(socket);
			//send auth hash
			//TODO: add auth hash encryption
			NetworkingOperations.sendStringWithSocket(accessToken.getAccessString(), socket);

			//Send instrument info
			NetworkingOperations.sendJsonObjectWithSocket(information,socket);
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