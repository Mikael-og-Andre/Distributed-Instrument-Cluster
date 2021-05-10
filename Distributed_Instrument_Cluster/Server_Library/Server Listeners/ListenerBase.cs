using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Server_Library.Connection_Types;
using Socket_Library;

namespace Server_Library.Server_Listeners {

	/// <summary>
	/// Base class for a server listening for incoming connections
	/// Inherit and implement how to handle the connection
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
		/// Constructor
		/// </summary>
		/// <param name="ipEndPoint"></param>
		/// <param name="maxConnections"></param>
		/// <param name="maxPendingConnections"></param>
		protected ListenerBase(IPEndPoint ipEndPoint, int maxConnections = 100, int maxPendingConnections = 100) {
			this.ipEndPoint = ipEndPoint;
			this.maxConnections = maxConnections;
			this.maxPendingConnections = maxPendingConnections;
			cancellationTokenSource = new CancellationTokenSource();
			currentConnectionCount = 0;
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
		/// <returns> a Connection of one of the child types of ConnectionBase</returns>
		protected abstract object createConnectionType(Socket socket);

		/// <summary>
		/// Gets authorization instrument information from connecting client
		/// </summary>
		/// <param name="socket"></param>
		/// <returns>Connection object</returns>
		private object setupConnection(Socket socket) {
			//Create connection and return
			return createConnectionType(socket);
		}
	}
}