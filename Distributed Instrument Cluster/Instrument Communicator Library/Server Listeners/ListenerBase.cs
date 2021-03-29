using Networking_Library;
using Server_Library.Authorization;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Server_Library.Connection_Classes;

namespace Server_Library.Server_Listeners {

	/// <summary>
	/// Base class for a server listening for incoming connections
	/// <author>Mikael Nilssen</author>
	/// </summary>
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
		protected readonly CancellationTokenSource cancellationTokenSource;

		/// <summary>
		/// Amount of current Connections
		/// </summary>
		private int currentConnectionCount;

		/// <summary>
		/// Queue containing incoming connections
		/// </summary>
		private ConcurrentQueue<ConnectionBase> queueOfIncomingConnections;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ipEndPoint"></param>
		/// <param name="maxConnections"></param>
		/// <param name="maxPendingConnections"></param>
		protected ListenerBase(IPEndPoint ipEndPoint, int maxConnections = 30, int maxPendingConnections = 30) {
			this.ipEndPoint = ipEndPoint;
			this.maxConnections = maxConnections;
			this.maxPendingConnections = maxPendingConnections;
			cancellationTokenSource = new CancellationTokenSource();
			currentConnectionCount = 0;

			//Init queue
			queueOfIncomingConnections = new ConcurrentQueue<ConnectionBase>();
		}

		/// <summary>
		/// Starts listening for remote connections
		/// </summary>
		public void start() {
			//Create socket for incoming connections
			listeningSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			listeningSocket.Bind(ipEndPoint);

			//Start listen with Backlog size of max connections
			listeningSocket.Listen(maxPendingConnections);

			//Accepts Connections
			while (!cancellationTokenSource.Token.IsCancellationRequested) {
				//Accept an incoming connection
				Console.WriteLine("SERVER - Main Thread {0} Says: Waiting For new Socket Connection...", Thread.CurrentThread.ManagedThreadId);
				Socket newSocket = listeningSocket.Accept();
				//Increment Current Connections
				currentConnectionCount += 1;

				

				//Authorize and setup connection
				object newClientConnection = setupConnection(newSocket);

				//Creates a new Thread to run a client communication on
				Task newTask = new Task(() => handleIncomingConnection(newClientConnection));

				//Start the task
				newTask.Start();
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
		protected abstract object createConnectionType(Socket socket, AccessToken accessToken, ClientInformation info);

		/// <summary>
		/// Gets authorization instrument information from connecting client
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="thread"></param>
		/// <returns>Connection object</returns>
		private object setupConnection(Socket socket) {
			//Send start Auth signal
			NetworkingOperations.sendStringWithSocket("auth", socket);

			//Get Authorization Token info
			//TODO: Add encryption for auth token
			string connectionHash = NetworkingOperations.receiveStringWithSocket(socket);
			AccessToken accessToken = new AccessToken(connectionHash);

			//Get Client information
			ClientInformation info = NetworkingOperations.receiveJsonObjectWithSocket<ClientInformation>(socket);

			//Create connection and return
			return createConnectionType(socket, accessToken, info);
		}

		/// <summary>
		/// Queue the incoming connection
		/// </summary>
		/// <param name="connection"></param>
		protected void addConnectionToQueueOfIncomingConnections(ConnectionBase connection) {
			queueOfIncomingConnections.Enqueue(connection);
		}

		/// <summary>
		/// Get a connection from the queue of incoming accepted connections
		/// </summary>
		/// <param name="output">Connection of a child class of ConnectionBase</param>
		/// <returns>True if object was found</returns>
		public bool getIncomingConnection(out ConnectionBase output) {
			if (queueOfIncomingConnections.TryDequeue(out ConnectionBase result)) {
				output = result;
				return true;
			}

			output = default;
			return false;
		}
	}
}