using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server_Library.Server_Listener {

	public abstract class ListenerBase {

		/// <summary>
		/// The Ip EndPoint of the listener
		/// </summary>
		private readonly IPEndPoint ipEndPoint;

		/// <summary>
		/// Max number of connections to server
		/// </summary>
		private int maxConnections;

		/// <summary>
		/// Max number of connections waiting in the accepting socket
		/// </summary>
		private readonly int maxPendingConnections;

		/// <summary>
		/// Socket used for accepting incoming requests
		/// </summary>
		private Socket listeningSocket;

		/// <summary>
		/// Token for telling server to stop
		/// </summary>
		protected CancellationToken listenerCancellationToken;

		/// <summary>
		/// Amount of current Connections
		/// </summary>
		private int currentConnectionCount;

		public ListenerBase(IPEndPoint ipEndPoint, int _maxConnections = 30, int _maxPendingConnections = 30) {
			this.ipEndPoint = ipEndPoint;
			this.maxConnections = _maxConnections;
			this.maxPendingConnections = _maxPendingConnections;
			this.listenerCancellationToken = new CancellationToken();
			this.currentConnectionCount = 0;
		}

		/// <summary>
		/// Starts listening for remote connections
		/// </summary>
		public void start() {
			//Create socket for incoming connections
			listeningSocket = new Socket(this.ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			listeningSocket.Bind(this.ipEndPoint);

			//Start listen with Backlog size of max connections
			listeningSocket.Listen(this.maxPendingConnections);

			//Accepts Connections
			while (!listenerCancellationToken.IsCancellationRequested) {
				//Accept an incoming connection
				Console.WriteLine("SERVER - Main Thread {0} Says: Waiting For new Socket Connection...", Thread.CurrentThread.ManagedThreadId);
				Socket newSocket = listeningSocket.Accept();
				//Increment Current Connections
				this.currentConnectionCount += 1;
				//Creates a new Thread to run a client communication on
				Thread newThread = new Thread(handleIncomingConnection);
				newThread.IsBackground = true;

				//Create a client connection object representing the connection
				object newClientConnection = createConnectionType(newSocket, newThread);

				try {
					//Pass in ClientConnection and start a new thread ThreadProtocol
					newThread.Start(newClientConnection);
				}
				catch (Exception) {
					//Lower Connection number
					this.currentConnectionCount -= 1;
					newSocket.Disconnect(false);
					newSocket.Close();
					throw;
				}
			}
		}

		/// <summary>
		/// Function to handle the new incoming connection on a new thread
		/// </summary>
		/// <param name="obj">Subclass of ConnectionBase, Should be the corresponding type returned from createConnectionType</param>
		protected abstract void handleIncomingConnection(object obj);

		/// <summary>
		/// Function for specifying specific type of ConnectionBase child the class should be returning and handing over to the HandleIncomingConnection
		/// </summary>
		/// <param name="socket">Socket Of the incoming connection</param>
		/// <param name="thread">Thread the new connection will be running on</param>
		/// <returns> a Connection of one of the child types of ConnectionBase</returns>
		protected abstract object createConnectionType(Socket socket, Thread thread);
	}
}