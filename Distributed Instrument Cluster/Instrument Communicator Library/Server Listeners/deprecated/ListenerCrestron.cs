using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Server_Library.Connection_Types;
using Networking_Library;
using Server_Library.Authorization;
using Server_Library.Connection_Types.deprecated;
using Server_Library.Enums;

namespace Server_Library.Server_Listeners.deprecated {

	public class ListenerCrestron : ListenerBaseOld {

		/// <summary>
		/// Time to wait between pings
		/// </summary>
		private readonly int timeToWait;

		/// <summary>
		/// Time to sleep after nothing happens
		/// </summary>
		private readonly int timeToSleep;

		/// <summary>
		/// List of crestron Connections
		/// </summary>
		private List<CrestronConnection> listCrestronConnections;

		public ListenerCrestron(IPEndPoint ipEndPoint, int pingWaitTime = 1000 * 60, int sleepTime = 100, int maxConnections = 30, int maxPendingConnections = 30) : base(ipEndPoint, maxConnections, maxPendingConnections) {
			this.timeToWait = pingWaitTime;
			this.timeToSleep = sleepTime;
			this.listCrestronConnections = new List<CrestronConnection>();
		}

		/// <summary>
		/// Function to Create a new connection of the desired type
		/// <param name="socket">Socket</param>
		/// <param name="thread">Thread</param>
		/// </summary>
		protected override object createConnectionType(Socket socket, Thread thread, AccessToken accessToken, ClientInformation info) {
			return new CrestronConnection(thread, socket, accessToken,info, cancellationTokenSource.Token);
		}

		/// <summary>
		/// Function for specifying specific type of ConnectionBase child the class should be returning and handing over to the HandleIncomingConnection
		/// </summary>
		protected override void handleIncomingConnection(object obj) {
			CrestronConnection clientConnection;
			try {
				//cast input object to Client Connection
				clientConnection = (CrestronConnection)obj;
			}
			catch (InvalidCastException) {
				throw;
			}
			Console.WriteLine("SERVER - a Client Has Connected to Thread: {0}, thread {0} is running now", Thread.CurrentThread.ManagedThreadId);

			//add a connection to the list of connections
			addClientConnection(clientConnection);

			ConcurrentQueue<Message> inputQueue = clientConnection.getSendingQueue();   //Get reference to the queue of inputs intended to send to the client
			ConcurrentQueue<Message> outputQueue = clientConnection.getReceivingQueue();     //Get reference to the queue of things received by the client

			//Setup stopwatch
			Stopwatch stopwatch = new Stopwatch();
			//Start stopwatch
			stopwatch.Start();

			while (!cancellationTokenSource.Token.IsCancellationRequested) {
				//Variable representing protocol to use;
				ProtocolOption currentMode;
				Message message;
				bool hasValue = inputQueue.TryPeek(out message);
				Message msg = message;
				//Check what action to take
				//If queue is empty and time since last ping is greater than timeToWait, ping
				if ((!hasValue) && (stopwatch.ElapsedMilliseconds > timeToWait)) {
					//stop stopwatch
					stopwatch.Stop();
					//Set mode to ping
					currentMode = ProtocolOption.ping;
				}
				//If queue has message check what type it is, and parse protocol type
				else if (hasValue) {
					// check first message in queue, set protocol to use to the protocol of that message
					ProtocolOption messageOption = msg.getProtocol();
					currentMode = messageOption;
				}
				//if queue is empty and time since last ping isn't big, sleep for an amount of time
				else {
					//Was empty and didn't need ping, so restart loop after short sleep
					Thread.Sleep(timeToSleep);
					continue;
				}

				//preform protocol corresponding to the current mode variable
				switch (currentMode) {
					case ProtocolOption.ping:
						//Preform ping protocol
						serverProtocolPing(clientConnection);
						break;

					case ProtocolOption.message:
						//preform send protocol
						this.serverProtocolMessage(clientConnection);
						break;

					default:
						break;
				}
				//Reset stopwatch
				stopwatch.Reset();
				stopwatch.Start();
			}
			removeClientConnection(clientConnection);
			//Stop stopwatch if ending i guess
			stopwatch.Stop();
			//get socket and disconnect
			Socket socket = clientConnection.getSocket();
			socket.Disconnect(false);
			//remove client from connections
		}

		/// <summary>
		/// Adds a client connection to the list of connections
		/// </summary>
		/// <param name="connection"> the ClientConnection to be added</param>
		/// <returns> Boolean value representing whether the adding was successful</returns>
		private void addClientConnection(CrestronConnection connection) {
			try {
				//Lock the non thread-safe list, and then add object
				lock (listCrestronConnections) {
					this.listCrestronConnections.Add(connection);
				}

				return;
			}
			catch (Exception) {
				// ignored
			}
		}

		/// <summary>
		/// Removes a client connection from the list of connections
		/// </summary>
		/// <param name="connection">Connection to be removed</param>
		/// <returns>Boolean representing successful removal</returns>
		private bool removeClientConnection(CrestronConnection connection) {
			//lock the non thread-safe list and then remove object
			lock (listCrestronConnections) {
				return this.listCrestronConnections.Remove(connection);
			}
		}

		#region Protocols

		/// <summary>
		/// Send protocol type PING to client and receives answer
		/// </summary>
		/// <param name="clientConnection">Connected and authorized socket</param>
		private static void serverProtocolPing(CrestronConnection clientConnection) {
			//Send protocol type "ping" to client
			//get socket
			Socket connectionSocket = clientConnection.getSocket();
			NetworkingOperations.sendStringWithSocket(ProtocolOption.ping.ToString(), connectionSocket);
			//Receive answer
			string receiveString = NetworkingOperations.receiveStringWithSocket(connectionSocket);
			//Check if correct Response

			if (receiveString.ToLower().Equals("y")) {
				//Successful ping
				Console.WriteLine("SERVER - Client Thread {0} says: Ping successful", Thread.CurrentThread.ManagedThreadId);
			}
			else {
				//failed ping, stop connection
				Console.WriteLine("SERVER - Client Thread {0} says: Ping failed, received wrong response", Thread.CurrentThread.ManagedThreadId);
				
			}
		}

		/// <summary>
		/// Sends an array of strings from the input queue in the client connection
		/// </summary>
		/// <param name="clientConnection">Client Connection Object</param>
		private void serverProtocolMessage(CrestronConnection clientConnection) {
			//Get reference to the queue
			ConcurrentQueue<Message> inputQueue = clientConnection.getSendingQueue();
			//Get Socket
			Socket connectionSocket = clientConnection.getSocket();
			try {
				//extract message from queue
				bool isSuccess = inputQueue.TryDequeue(out var messageToSend);
				Message msg = (Message)messageToSend;
				//Check if success and start sending messages
				if (isSuccess) {
					//Say protocol type to client
					NetworkingOperations.sendStringWithSocket(ProtocolOption.message.ToString(), connectionSocket);

					//Get string array from message object
					string messageString = msg.getMessage();

					//Send string
					NetworkingOperations.sendStringWithSocket(messageString, connectionSocket);
					//Send end signal to client, singling no more strings are coming
					NetworkingOperations.sendStringWithSocket("end", connectionSocket);
				}
				else {
					Console.WriteLine("SERVER - Crestron Listener Message queue was empty when trying to Send a message");
				}
			}
			catch (Exception ex) {
				throw ex;
				//return;
			}
		}

		#endregion Protocols

		/// <summary>
		/// Validates an accessToken
		/// </summary>
		/// <param name="token">Access token</param>
		/// <returns>boolean representing a valid access token if true</returns>
		private bool validateAccessToken(AccessToken token) {
			//TODO: add Database checking
			string hash = token.getAccessString();
			if (hash.Equals("access")) {
				Console.WriteLine("SERVER - Thread {0} is now authorized", Thread.CurrentThread.ManagedThreadId);
				return true;
			}
			Console.WriteLine("SERVER - Thread {0} Authorization Failed ", Thread.CurrentThread.ManagedThreadId);
			return false;
		}

		/// <summary>
		/// Get the list of connected clients
		/// </summary>
		/// <returns>List of Client Connections</returns>
		public List<CrestronConnection> getCrestronConnectionList() {
			lock (listCrestronConnections) {
				return this.listCrestronConnections;
			}
		}

		/// <summary>
		///	Search the connections for a connection with the name
		/// </summary>
		/// <returns>Boolean representing if it was found or not</returns>
		public bool getCrestronConnectionWithName(out CrestronConnection crestronConnection, string name) {
			lock (listCrestronConnections) {
				//Search for connection
				foreach (var connection in listCrestronConnections) {
					if (connection.isSetupCompleted) {
						ClientInformation info = connection.getInstrumentInformation();
						//Check if the name is the same
						if (info.Name.ToLower().Equals(name.ToLower())) {
							crestronConnection = connection;
							return true;
						}
					}
				}

				crestronConnection = null;
				return false;
			}
		}
	}
}