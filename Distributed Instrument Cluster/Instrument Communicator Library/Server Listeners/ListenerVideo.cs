using Instrument_Communicator_Library.Helper_Class;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Instrument_Communicator_Library.Server_Listener {

	/// <summary>
	/// Listener for incoming video connections
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class ListenerVideo : ListenerBase {

		/// <summary>
		/// list of connected video streams
		/// </summary>
		private List<VideoConnection> listVideoConnections;

		/// <summary>
		/// queue of all incoming connections
		/// </summary>
		private ConcurrentQueue<VideoConnection> incomingConnectionsQueue;

		public ListenerVideo(IPEndPoint ipEndPoint, int maxConnections = 30, int maxPendingConnections = 30) : base(ipEndPoint, maxConnections, maxPendingConnections) {
			listVideoConnections = new List<VideoConnection>();
			incomingConnectionsQueue = new ConcurrentQueue<VideoConnection>();
		}

		/// <summary>
		/// Create connection of the appropriate type, is used in the base class.
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="thread"></param>
		/// <returns>VideoConnection</returns>
		protected override object createConnectionType(Socket socket, Thread thread) {
			return new VideoConnection(socket, thread);
		}

		/// <summary>
		/// Runs when a new connection is accepted by the listener
		/// </summary>
		/// <param name="obj">VideoConnection object</param>
		protected override void handleIncomingConnection(object obj) {
			//Cast to video-connection
			VideoConnection videoConnection = (VideoConnection)obj;
			//add connection to list
			addVideoConnection(videoConnection);
			//Add connection to incoming connectionQueue

			incomingConnectionsQueue.Enqueue(videoConnection);

			//Get socket
			Socket connectionSocket = videoConnection.getSocket();

			//Send signal to start instrumentCommunication
			NetworkingOperations.sendStringWithSocket("y", connectionSocket);

			string name = NetworkingOperations.receiveStringWithSocket(connectionSocket);
			string location = NetworkingOperations.receiveStringWithSocket(connectionSocket);
			string type = NetworkingOperations.receiveStringWithSocket(connectionSocket);

			videoConnection.setInstrumentInformation(new InstrumentInformation(name, location, type));

			//Get outputQueue
			ConcurrentQueue<VideoFrame> outputQueue = videoConnection.getOutputQueue();

			//Do main loop
			while (!listenerCancellationToken.IsCancellationRequested) {
				//Get Incoming object
				VideoFrame newObj = NetworkingOperations.receiveObjectWithSocket(connectionSocket);

				outputQueue.Enqueue(newObj);
			}
			//remove connection
			removeVideoConnection(videoConnection);
		}

		/// <summary>
		/// Add connection to list of connections
		/// </summary>
		/// <param name="connection">VideoConnection</param>
		private void addVideoConnection(VideoConnection connection) {
			lock (listVideoConnections) {
				listVideoConnections.Add(connection);
			}
		}

		/// <summary>
		/// Remove the client connection from the list
		/// </summary>
		/// <param name="connection"> Video Connection</param>
		/// <returns>Boolean representing successful removal</returns>
		private bool removeVideoConnection(VideoConnection connection) {
			//Lock list and remove the connection
			bool result = false;
			lock (listVideoConnections) {
				//Try to remove connection
				result = listVideoConnections.Remove(connection);
			}
			//return bool
			return result;
		}

		/// <summary>
		/// Get the list of video connection objects
		/// </summary>
		/// <returns>List of video-connection objects of type T</returns>
		public List<VideoConnection> getVideoConnectionList() {
			lock (listVideoConnections) {
				return listVideoConnections;
			}
		}

		/// <summary>
		/// Returns the queue containing each incoming connection
		/// </summary>
		/// <returns></returns>
		public ConcurrentQueue<VideoConnection> getIncomingConnectionQueue() {
			return incomingConnectionsQueue;
		}
	}
}