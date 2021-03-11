using System;
using System.Net.Sockets;
using System.Threading;

namespace Instrument_Communicator_Library.Remote_Device_side_Communicators {

	/// <summary>
	/// Base class for communicator classes, intended to be on the remote side of the instrument cluster
	/// <Author>Mikael Nilssen</Author>
	/// </summary>
	public abstract class CommunicatorBase {

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
		public bool isSocketConnected { get; private set; } = false;

		/// <summary>
		/// Cancellation token used to stop loops
		/// </summary>
		protected CancellationToken communicatorCancellationToken;

		protected CommunicatorBase(string ip, int port, InstrumentInformation informationAboutClient, AccessToken accessToken, CancellationToken communicatorCancellationToken) {
			this.Ip = ip;
			this.Port = port;
			this.information = informationAboutClient;
			this.accessToken = accessToken;
			this.communicatorCancellationToken = communicatorCancellationToken;
		}

		/// <summary>
		/// Starts the client and attempts to connect to the server
		/// </summary>
		public void Start() {
			try {
				// Create new socket
				connectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			}
			catch (SocketException) {
				throw;
			}
			//connection state
			isSocketConnected = false;

			// Loop whilst the client is supposed to run
			while (!communicatorCancellationToken.IsCancellationRequested) {
				//check if client is connected, if not connect
				if (!isSocketConnected) {
					// Try to connect
					isSocketConnected = attemptConnection(connectionSocket);
				}
				//check if client is connected, if it is handle the connection
				if (isSocketConnected) {
					//handle the connection
					handleConnected(connectionSocket);
				}
				else {
					Console.WriteLine("Thread {0} says: " + "Connection failed", Thread.CurrentThread.ManagedThreadId);
					Thread.Sleep(100);
				}
			}
		}

		/// <summary>
		/// Attempts to connect to the given host and ip
		/// </summary>
		/// <param name="socket"> unconnected Socket</param>
		/// <returns> boolean representing successful connection</returns>
		private bool attemptConnection(Socket socket) {
			try {
				if (socket.Connected) {
					return true;
				}
				//Try Connecting to server
				socket.Connect(Ip, Port);
				return true;
			}
			catch (SocketException ex) {
				//TODO: FIX IT MIKAEL!!!!!
				//throw new SocketException(ex.ErrorCode);
				//return false to represent failed connection
				return false;
			}
		}

		/// <summary>
		/// The main function of a communicator that gets called after you are connected and preforms actions with the socket
		/// </summary>
		/// <param name="connectionSocket"></param>
		protected abstract void handleConnected(Socket connectionSocket);

		/// <summary>
		/// returns the cancellation token
		/// </summary>
		/// <returns>Returns cancellation token</returns>
		public CancellationToken getCancellationToken() {
			return this.communicatorCancellationToken;
		}
	}
}