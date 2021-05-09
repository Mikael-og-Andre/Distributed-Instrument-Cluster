using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
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
		protected string Ip { get; set; }

		/// <summary>
		/// Port of target server
		/// </summary>
		protected int Port { get; set; }

		/// <summary>
		/// Connection to server
		/// </summary>
		protected Socket connectionSocket;

		/// <summary>
		/// Network stream for the connection
		/// </summary>
		protected NetworkStream connectionNetworkStream { get; set; }


		protected ClientBaseAsync(string ip, int port) {
			Ip = ip;
			Port = port;
			this.connectionSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
		}
		
		/// <summary>
		/// Attempts to connect to the given host and ip
		/// </summary>
		/// <returns></returns>
		protected  bool connectToServer() {
			try {
				if (connectionSocket.Connected) {
					connectionSocket.Disconnect(false);
					connectionSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
				}
				connectionSocket.Connect(Ip,Port);
				this.connectionNetworkStream = new NetworkStream(connectionSocket, false);
				return true;
			}
			catch (Exception e) {
				Console.WriteLine("Exception in ClientBaseAsync: {0}",e.Message);
				return false;
			}
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

		/// <summary>
		/// Check if the socket is connected
		/// https://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c
		/// </summary>
		/// <returns></returns>
		public bool isSocketConnected() {
			if (connectionSocket is null) {
				return false;
			}
			return !((connectionSocket.Poll(1000, SelectMode.SelectRead) && (connectionSocket.Available == 0)) || !connectionSocket.Connected);
		}

		/// <summary>
		/// Closes current connection if needed, Then tries to connect to the ip and port
		/// </summary>
		/// <returns>Returns true if connected</returns>
		protected  bool reconnect() {

			if (isSocketConnected()) {
				Console.WriteLine("ClientBaseAsync: Tried to reconnect when socket was connected");
				return true;
			}

			try {
				//Reconnect
				bool connected=connectToServer();
				return connected;
			}
			catch (Exception e) {
				Console.WriteLine("ClientBaseAsync: Could not connect to {0}, {1}",Ip,Port);
				Console.WriteLine("ClientBaseAsync: Exception: {0}",e.Message);
				return false;
			}
		}

	}
}