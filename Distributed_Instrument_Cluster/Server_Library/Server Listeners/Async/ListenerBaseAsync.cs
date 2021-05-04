using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Networking_Library;
using Server_Library.Authorization;
using Server_Library.Connection_Classes;
using Server_Library.Connection_Types.Async;

namespace Server_Library.Server_Listeners.Async {
	public abstract class ListenerBaseAsync : IDisposable {
		/// <summary>
		/// The Ip EndPoint of the listener
		/// </summary>
		private readonly IPEndPoint ipEndPoint;

		/// <summary>
		/// Max number of connections waiting in the accepting socket
		/// </summary>
		private readonly int maxPendingConnections;

		/// <summary>
		/// Socket used for accepting incoming requests
		/// </summary>
		private Socket listeningSocket;

		/// <summary>
		/// Token for telling listener to stop
		/// </summary>
		protected readonly CancellationTokenSource cancellationTokenSource;

		/// <summary>
		/// Stack of incoming connections
		/// </summary>
		private ConcurrentStack<ConnectionBaseAsync> stackIncomingConnection;
		/// <summary>
		/// List of all started Tasks for handling connected devices
		/// </summary>
		public List<(Task,ConnectionBaseAsync)> connectionTasksList { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ipEndPoint"></param>
		/// <param name="maxPendingConnections"></param>
		protected ListenerBaseAsync(IPEndPoint ipEndPoint, int maxPendingConnections = 100) {
			this.ipEndPoint = ipEndPoint;
			this.maxPendingConnections = maxPendingConnections;
			cancellationTokenSource = new CancellationTokenSource();

			//Init data structs
			stackIncomingConnection = new ConcurrentStack<ConnectionBaseAsync>();
			connectionTasksList = new List<(Task,ConnectionBaseAsync)>();
		}

		/// <summary>
		/// Starts listening for remote connections
		/// </summary>
		public async Task run() {
			//Create socket for incoming connections
			listeningSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			listeningSocket.Bind(ipEndPoint);

			//Start listen with Backlog size of max connections
			listeningSocket.Listen(maxPendingConnections);

			//Accepts Connections
			while (!cancellationTokenSource.Token.IsCancellationRequested) {
				//Accept an incoming connection
				Console.WriteLine("SERVER - Main Thread {0} Says: Waiting For new Socket Connection...", Thread.CurrentThread.ManagedThreadId);
				Socket newSocket = await listeningSocket.AcceptAsync();

				//Authorize and setup connection
				ConnectionBaseAsync newClientConnection = await setupConnectionAsync(newSocket);

				//Creates a new Task to run a client communication on
				Task connectionTask = handleIncomingConnectionAsync(newClientConnection);
				
				//add connection as incoming connection on the stack
				stackIncomingConnection.Push(newClientConnection);

				//Add to list of running connection tasks
				lock (connectionTasksList) {
					connectionTasksList.Add((connectionTask,newClientConnection));
				}
			}
		}

		/// <summary>
		/// Trigger cancellation token and stop
		/// </summary>
		public void stop() {
			cancellationTokenSource.Cancel();
		}

		/// <summary>
		/// Function to handle the new incoming connection
		/// </summary>
		/// <param name="con">Subclass of ConnectionBase, Should be the corresponding type returned from createConnectionType</param>
		protected abstract Task handleIncomingConnectionAsync(ConnectionBaseAsync con);

		/// <summary>
		/// Function for so each childClass can create a connection of the type they want
		/// </summary>
		/// <param name="socket">Socket Of the incoming connection</param>
		/// <param name="accessToken">String used to identify the connector</param>
		/// <returns> a Connection of one of the child types of ConnectionBase</returns>
		protected abstract ConnectionBaseAsync createConnectionType(Socket socket, AccessToken accessToken);

		/// <summary>
		/// Get accessToken and creates a connection with the correct type
		/// </summary>
		/// <param name="socket"></param>
		/// <returns>Connection object</returns>
		private async Task<ConnectionBaseAsync> setupConnectionAsync(Socket socket) {
			//NetworkStream
			NetworkStream networkStream = new NetworkStream(socket);
			//Send start Auth signal
			await NetworkingOperations.sendStringAsync("auth",networkStream);
			//Get Authorization Token info
			//TODO: Add encryption for auth token
			string connectionHash = await NetworkingOperations.receiveStringAsync(networkStream);
			AccessToken accessToken = new AccessToken(connectionHash);

			//Create connection with overriden method from child
			return createConnectionType(socket, accessToken);
		}

		/// <summary>
		/// push a connection to the stack
		/// </summary>
		/// <param name="connection"></param>
		protected void addConnectionToStack(ConnectionBaseAsync connection) {
			stackIncomingConnection.Push(connection);
		}

		/// <summary>
		/// Get a connection from the stack of incoming accepted connections
		/// </summary>
		/// <param name="output">Connection of a child class of ConnectionBase</param>
		/// <returns>True if object was found</returns>
		public bool getIncomingConnection(out ConnectionBaseAsync output) {
			if (stackIncomingConnection.TryPop(out ConnectionBaseAsync result)) {
				output = result;
				return true;
			}

			output = default;
			return false;
		}

		/// <summary>
		/// Dispose sockets and Token
		/// </summary>
		public void Dispose() {
			listeningSocket?.Dispose();
			cancellationTokenSource?.Dispose();
		}
	}
}
