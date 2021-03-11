using System.Net.Sockets;
using System.Threading;
using Instrument_Communicator_Library.Authorization;
using Networking_Library;

namespace Instrument_Communicator_Library.Socket_Clients {

	/// <summary>
	/// Base class for communicator classes, intended to be on the remote side of the instrument cluster
	/// <Author>Mikael Nilssen</Author>
	/// </summary>
	public abstract class ClientBaseOld {

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
		private Socket connectionSocket;

		/// <summary>
		/// Information about hardware
		/// </summary>
		protected readonly InstrumentInformation information;

		/// <summary>
		/// Authorization code to send to the server
		/// </summary>
		protected AccessToken accessToken;

		//State
		/// <summary>
		/// Is the socket connected to the server
		/// </summary>
		public bool isSocketConnected { get; set; } = false;

		private bool isSetup { get; set; } = false;

		/// <summary>
		/// Cancellation token used to stop loops
		/// </summary>
		protected CancellationToken isRunningCancellationToken;

		protected ClientBaseOld(string ip, int port, InstrumentInformation informationAboutClient, AccessToken accessToken, CancellationToken isRunningCancellationToken) {
			this.Ip = ip;
			this.Port = port;
			this.information = informationAboutClient;
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
			//Setup
			setupConnection(connectionSocket);
			//HandleConnection
			handleConnected(connectionSocket);
		}

		/// <summary>
		/// Attempts to connect to the given host and ip
		/// </summary>
		/// <param name="socket"> unconnected Socket</param>
		/// <returns> boolean representing successful connection</returns>
		private void connectToServer(Socket socket) {
			socket.Connect(Ip,Port);
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
			NetworkingOperations.sendStringWithSocket(accessToken.getAccessString(),socket);

			//Send instrument info
			NetworkingOperations.sendStringWithSocket(information.Name,socket);
			NetworkingOperations.sendStringWithSocket(information.Location,socket);
			NetworkingOperations.sendStringWithSocket(information.Type,socket);
		}

		/// <summary>
		/// The main function of a communicator that gets called after you are connected and preforms actions with the socket
		/// </summary>
		/// <param name="connectionSocket"></param>
		protected abstract void handleConnected(Socket connectionSocket);
	}
}