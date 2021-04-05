using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using Networking_Library;
using Server_Library.Authorization;
using Server_Library.Enums;

namespace Server_Library.Socket_Clients {

	/// <summary>
	/// Client for connecting and receiving commands from server unit to control a crestron Device
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class CrestronClient : ClientBaseOld {

		/// <summary>
		/// Queue representing commands received by receive protocol
		/// </summary>
		private ConcurrentQueue<string> commandOutputQueue;

		//Values for state control
		/// <summary>
		/// Boolean for wheter the authorization process is complete
		/// </summary>
		private bool isAuthorized;

		//TODO: add buffer size limits to queue

		public CrestronClient(string ip, int port, ClientInformation informationAboutClient, AccessToken accessToken, CancellationToken cancellationToken) : base(ip, port, informationAboutClient, accessToken, cancellationToken) {
			this.commandOutputQueue = new ConcurrentQueue<string>();    //Init queue
		}

		/// <summary>
		/// Handles the connected device
		/// </summary>
		/// <param name="connectionSocket"></param>
		protected override void handleConnected(Socket connectionSocket) {
			try {
				//check if the client is authorized,
				if (!isAuthorized) {
					//receive the first authorization call from server
					NetworkingOperations.receiveStringWithSocket(connectionSocket);
					//Start Authorization
					isAuthorized = protocolAuthorize(connectionSocket);
				}
				//Check if successfully authorized
				if (isAuthorized) {
					Console.WriteLine("Thread {0} Client Authorization complete", Thread.CurrentThread.ManagedThreadId);
					//Run main protocol Loop
					while (!isRunningCancellationToken.IsCancellationRequested) {
						//Read a protocol choice from the buffer and execute it
						startAProtocol(connectionSocket);
					}
				}
			}
			catch (Exception) {
				throw;
			}
		}

		/// <summary>
		/// Listens for selected protocol sent by server and preforms correct response protocol
		/// </summary>
		/// <param name="connectionSocket"> Socket Connection to server</param>
		private void startAProtocol(Socket connectionSocket) {
			//Receive protocol type from server
			string extractedString = NetworkingOperations.receiveStringWithSocket(connectionSocket);
			//Parse Enum
			ProtocolOption option = (ProtocolOption)Enum.Parse(typeof(ProtocolOption), extractedString, true);
			Console.WriteLine("thread {0} Client says: " + "Received protocol request: {1} ", Thread.CurrentThread.ManagedThreadId, option);
			//Select Protocol
			switch (option) {
				case ProtocolOption.ping:
					protocolPing(connectionSocket);
					break;

				case ProtocolOption.message:
					protocolMessage(connectionSocket);
					break;

				case ProtocolOption.authorize:
					protocolAuthorize(connectionSocket);
					break;

				default:
					break;
			}
		}

		#region Protocols

		/// <summary>
		/// Activates predetermined sequence of socket operations for authorizing the client device as trusted
		/// </summary>
		/// <param name="connectionSocket"></param>
		/// <returns>Boolean representing if the authorization was successful or not</returns>
		private bool protocolAuthorize(Socket connectionSocket) {
			try {
				//Create accessToken
				AccessToken accessToken = this.accessToken;
				string accessTokenHash = accessToken.getAccessString();
				//Send token
				NetworkingOperations.sendStringWithSocket(accessTokenHash, connectionSocket);

				//Receive Result
				string result = NetworkingOperations.receiveStringWithSocket(connectionSocket);
				//Check result
				if (result.ToLower().Equals("y")) {
					Console.WriteLine("Thread {0} Authorization Successful", Thread.CurrentThread.ManagedThreadId);
					//If returned y then do instrument detailing
				}
				else {
					Console.WriteLine("Thread {0} Authorization Failed", Thread.CurrentThread.ManagedThreadId);
					// return false, representing a failed authorization
					return false;
				}

				//Receive Y for started instrument detailing
				string response = NetworkingOperations.receiveStringWithSocket(connectionSocket);
				if (!response.ToLower().Equals("y")) {
					return false;
				}
				NetworkingOperations.sendStringWithSocket(information.Name, connectionSocket);
				NetworkingOperations.sendStringWithSocket(information.Location, connectionSocket);
				NetworkingOperations.sendStringWithSocket(information.Type, connectionSocket);

				//Receive Y for finished
				string complete = NetworkingOperations.receiveStringWithSocket(connectionSocket);

				return true;
			}
			catch (Exception) {
				return false;
			}
		}

		/// <summary>
		/// Activates predetermined sequence of socket operations for a ping, to confirm both locations
		/// </summary>
		/// <param name="connectionSocket"> Authorized connection socket</param>
		private void protocolPing(Socket connectionSocket) {
			//Send simple byte to server
			NetworkingOperations.sendStringWithSocket("y", connectionSocket);
		}

		/// <summary>
		/// Activates predetermined sequence of socket operation for receiving an array of string from the server
		/// </summary>
		/// <param name="connectionSocket">Connected and authorized socket</param>
		private void protocolMessage(Socket connectionSocket) {
			//Loop boolean
			bool isAccepting = true;
			//Loop until end signal received by server
			while (isAccepting) {
				string received = NetworkingOperations.receiveStringWithSocket(connectionSocket);
				//Check if end in messages
				if (received.ToLower().Equals("end")) {
					//Set protocol to be over
					isAccepting = false;
					break;
				}
				Console.WriteLine("Thread {0} message received " + received, Thread.CurrentThread.ManagedThreadId);
				//Add Command To Concurrent queue
				commandOutputQueue.Enqueue(received);
			}
		}

#endregion Protocols

		/// <summary>
		/// Returns a reference to queue of commands received by receive protocol in string format
		/// </summary>
		/// <returns>reference to Concurrent queue of type string</returns>
		public ConcurrentQueue<string> getCommandOutputQueue() {
			return commandOutputQueue;
		}
	}
}