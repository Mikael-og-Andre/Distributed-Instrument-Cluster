using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Networking_Library;
using Server_Library.Authorization;

namespace Server_Library.Server_Listeners.deprecated {

	/// <summary>
	/// Base class for a server listening for incoming connections
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public abstract class ListenerBaseOld {

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
		protected readonly CancellationTokenSource cancellationTokenSource;

		/// <summary>
		/// Amount of current Connections
		/// </summary>
		private int currentConnectionCount;

		protected ListenerBaseOld(IPEndPoint ipEndPoint, int maxConnections = 30, int maxPendingConnections = 30) {
			this.ipEndPoint = ipEndPoint;
			this.maxConnections = maxConnections;
			this.maxPendingConnections = maxPendingConnections;
			this.cancellationTokenSource = new CancellationTokenSource();
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
			while (!cancellationTokenSource.Token.IsCancellationRequested) {
				//Accept an incoming connection
				Console.WriteLine("SERVER - Main Thread {0} Says: Waiting For new Socket Connection...", Thread.CurrentThread.ManagedThreadId);
				Socket newSocket = listeningSocket.Accept();
				//Increment Current Connections
				this.currentConnectionCount += 1;

				//Creates a new Thread to run a client communication on
				Thread newThread = new Thread(handleIncomingConnection);
				newThread.IsBackground = true;

				//Authorize and setup connection
				object newClientConnection = setupConnection(newSocket, newThread);

				//Pass connection type to thread and start
				newThread.Start(newClientConnection);
			}
		}

		/// <summary>
		/// Trigger cancellation token and stop
		/// </summary>
		public void stop() {
			cancellationTokenSource.Cancel();
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
		protected abstract object createConnectionType(Socket socket, Thread thread, AccessToken accessToken, ClientInformation info);




		/// <summary>
		/// Gets authorization client information from connecting client
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="thread"></param>
		/// <returns>Connection object</returns>
		private object setupConnection(Socket socket, Thread thread) {
			//Send start Auth signal
			NetworkingOperations.sendStringWithSocket("auth", socket);

			//Get Authorization Token info
			string connectionHash = NetworkingOperations.receiveStringWithSocket(socket);
			AccessToken accessToken = new AccessToken(connectionHash);

			//Get instrument information
			string name = NetworkingOperations.receiveStringWithSocket(socket);
			string location = NetworkingOperations.receiveStringWithSocket(socket);
			string type = NetworkingOperations.receiveStringWithSocket(socket);
			string subName = NetworkingOperations.receiveStringWithSocket(socket);
			ClientInformation info = new ClientInformation(name, location, type, subName);

			//Create connection and return
			return createConnectionType(socket, thread, accessToken, info);
		}
	}
}