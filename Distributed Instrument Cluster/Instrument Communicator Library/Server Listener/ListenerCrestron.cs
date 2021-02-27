using Instrument_Communicator_Library.Helper_Class;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Instrument_Communicator_Library.Server_Listener {

	public class ListenerCrestron : ListenerBase {
		private int timeToWait;     //Time to wait between pings
		private int timeToSleep;      // Time to sleep after nothing happens
		private List<CrestronConnection> listCrestronConnections;   //List of crestron Connections

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
		protected override object createConnectionType(Socket socket, Thread thread) {
			return new CrestronConnection(socket, thread);
		}

		/// <summary>
		/// Function for specifying specific type of ConnectionBase child the class should be returning and handing over to the HandleIncomingConnection
		/// </summary>
		protected override void handleIncomingConnection(object obj) {
			CrestronConnection clientConnection;
			try {
				//cast input object to Client Connection
				clientConnection = (CrestronConnection)obj;
			} catch (InvalidCastException) {
				throw;
			}
			Console.WriteLine("SERVER - a Client Has Connected to Thread: {0}, thread {0} is running now", Thread.CurrentThread.ManagedThreadId);

			//add a connection to the list of connections
			addClientConnection(clientConnection);

			//Do authorization process
			serverProtocolAuthorization(clientConnection);

			ConcurrentQueue<Message> inputQueue = clientConnection.GetInputQueue();   //Get reference to the queue of inputs intended to send to the client
			ConcurrentQueue<Message> outputQueue = clientConnection.GetOutputQueue();     //Get reference to the queue of things received by the client

			//Setup stopwatch
			Stopwatch stopwatch = new Stopwatch();
			//Start stopwatch
			stopwatch.Start();

			while (!listenerCancellationToken.IsCancellationRequested) {
				//Variable representing protocol to use;
				protocolOption currentMode;
				Message message;
				bool hasValue = inputQueue.TryPeek(out message);
				Message msg = message;
				//Check what action to take
				//If queue is empty and time since last ping is greater than timeToWait, ping
				if ((!hasValue) && (stopwatch.ElapsedMilliseconds > timeToWait)) {
					//stop stopwatch
					stopwatch.Stop();
					//Set mode to ping
					currentMode = protocolOption.ping;
				}
				//If queue has message check what type it is, and parse protocol type
				else if (hasValue) {
					// check first message in queue, set protocol to use to the protocol of that message
					protocolOption messageOption = msg.getProtocol();
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
					case protocolOption.ping:
						//Preform ping protocol
						serverProtocolPing(clientConnection);
						break;

					case protocolOption.message:
						//preform send protocol
						this.serverProtocolMessage(clientConnection);
						break;

					case protocolOption.status:
						this.serverProtocolStatus(clientConnection);
						break;

					case protocolOption.authorize:
						this.serverProtocolAuthorization(clientConnection);
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
			Socket socket = clientConnection.GetSocket();
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
			} catch (Exception) {
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

		/// <summary>
		/// Starts predetermined sequence eof socket operations used to authorize a remote device
		/// </summary>
		/// <param name="clientConnection">Client Connection representing The current Connection</param>
		private void serverProtocolAuthorization(CrestronConnection clientConnection) {
			//get socket
			Socket connectionSocket = clientConnection.GetSocket();
			//Send protocol type to client
			NetworkingOperations.SendStringWithSocket(protocolOption.authorize.ToString(), connectionSocket);
			//receive token
			string receivedToken = NetworkingOperations.ReceiveStringWithSocket(connectionSocket);

			//TODO: Add Encryption to accessTokens

			//Create Token
			AccessToken token = new AccessToken(receivedToken);
			//Validate token
			bool validationResult = validateAccessToken(token);
			//Send success/failure to client
			if (validationResult) {
				//Send char y for success
				NetworkingOperations.SendStringWithSocket("y", connectionSocket);
				//Add access Token to clientConnection
				clientConnection.SetAccessToken(token);
			} else {
				//Send char n for negative
				NetworkingOperations.SendStringWithSocket("n", connectionSocket);
				//authorization failed, set not clientConnection to not active and return
				clientConnection.SetIsConnectionActive(false);
				return;
			}

			//Get instrument Information
			//Send signal to start instrumentCommunication
			NetworkingOperations.SendStringWithSocket("y", connectionSocket);

			string name = NetworkingOperations.ReceiveStringWithSocket(connectionSocket);
			string location = NetworkingOperations.ReceiveStringWithSocket(connectionSocket);
			string type = NetworkingOperations.ReceiveStringWithSocket(connectionSocket);

			//Send signal to for successful finish instrumentCommunication
			NetworkingOperations.SendStringWithSocket("y", connectionSocket);

			clientConnection.SetInstrumentInformation(new InstrumentInformation(name, location, type));
		}

		/// <summary>
		/// Send protocol type PING to client and receives answer
		/// </summary>
		/// <param name="clientConnection">Connected and authorized socket</param>
		private static void serverProtocolPing(CrestronConnection clientConnection) {
			//Send protocol type "ping" to client
			//get socket
			Socket connectionSocket = clientConnection.GetSocket();
			NetworkingOperations.SendStringWithSocket(protocolOption.ping.ToString(), connectionSocket);
			//Receive answer
			string receiveString = NetworkingOperations.ReceiveStringWithSocket(connectionSocket);
			//Check if correct Response

			if (receiveString.ToLower().Equals("y")) {
				//Successful ping
				Console.WriteLine("SERVER - Client Thread {0} says: Ping successful", Thread.CurrentThread.ManagedThreadId);
			} else {
				//failed ping, stop connection
				Console.WriteLine("SERVER - Client Thread {0} says: Ping failed, received wrong response", Thread.CurrentThread.ManagedThreadId);
				clientConnection.SetIsConnectionActive(false);
			}
		}

		//TODO: handle status protocol server
		private void serverProtocolStatus(CrestronConnection clientConnection) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sends an array of strings from the input queue in the client connection
		/// </summary>
		/// <param name="clientConnection">Client Connection Object</param>
		private void serverProtocolMessage(CrestronConnection clientConnection) {
			//Get reference to the queue
			ConcurrentQueue<Message> inputQueue = clientConnection.GetInputQueue();
			//Get Socket
			Socket connectionSocket = clientConnection.GetSocket();
			try {
				//extract message from queue
				bool isSuccess = inputQueue.TryDequeue(out var messageToSend);
				Message msg = (Message)messageToSend;
				//Check if success and start sending messages
				if (isSuccess) {
					//Say protocol type to client
					NetworkingOperations.SendStringWithSocket(protocolOption.message.ToString(), connectionSocket);

					//Get string array from message object
					string[] stringArray = msg.getMessageArray();

					//Send all strings
					foreach (string s in stringArray) {
						//If s is "end" skip to avoid premature ending
						if (s.Equals("end")) {
							continue;
						}
						Console.WriteLine("SERVER - Thread {0} is sending " + s + " to the client", Thread.CurrentThread.ManagedThreadId);
						NetworkingOperations.SendStringWithSocket(s, connectionSocket);
					}
					//Send end signal to client, singling no more strings are coming
					NetworkingOperations.SendStringWithSocket("end", connectionSocket);
				} else {
					Console.WriteLine("SERVER - Crestron Listener Message queue was empty when trying to  ");
				}
			} catch (Exception ex) {
				throw ex;
				//return;
			}
		}

		/// <summary>
		/// Validates an accessToken
		/// </summary>
		/// <param name="token">Access token</param>
		/// <returns>boolean representing a valid access token if true</returns>
		private bool validateAccessToken(AccessToken token) {
			//TODO: add Database checking
			string hash = token.getAccessString();
			Console.WriteLine("SERVER - Thread {0} is now checking hash. Hash: " + hash, Thread.CurrentThread.ManagedThreadId);
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
					if (connection.hasInstrument) {
						InstrumentInformation info = connection.GetInstrumentInformation();
						if (info.name.ToLower().Equals(name.ToLower())) {
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