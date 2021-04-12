using Networking_Library;
using Server_Library.Authorization;
using Server_Library.Connection_Classes;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;

namespace Server_Library.Connection_Types {

	/// <summary>
	/// Connection for receiving objects
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class ReceivingConnection : ConnectionBase {

		/// <summary>
		/// Queue containing incoming objects
		/// </summary>
		private ConcurrentQueue<byte[]> receivedObjectsConcurrentQueue;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="accessToken"></param>
		/// <param name="info"></param>
		/// <param name="token"></param>
		public ReceivingConnection(Socket socket, AccessToken accessToken, ClientInformation info, CancellationToken token) : base(socket, accessToken, info, token) {
			receivedObjectsConcurrentQueue = new ConcurrentQueue<byte[]>();
		}

		/// <summary>
		/// Get an object from the incoming objects
		/// </summary>
		/// <param name="output"></param>
		/// <returns>True if and object was found</returns>
		public bool getDataFromConnection(out byte[] output) {
			//Try to dequeue
			if (receivedObjectsConcurrentQueue.TryDequeue(out byte[] obj)) {
				//Return with true
				output = obj;
				return true;
			}
			//Return with false
			output = default;
			return false;
		}

		/// <summary>
		/// Get the amount of objects in the queue
		/// </summary>
		/// <returns>int</returns>
		public int getItemCount() {
			return receivedObjectsConcurrentQueue.Count;
		}

		/// <summary>
		/// Use socket to accept an incoming object and put it in the internal Queue
		/// </summary>
		public bool receive() {
			if (isDataAvailable()) {
				//Get data from stream
				byte[] incomingBytes=NetworkingOperations.receiveBytes(connectionNetworkStream,9000000);
				//Put object in queue
				enqueueBytes(incomingBytes);
				return true;
			}

			return false;
		}

		/// <summary>
		/// checks if there is data available on the socket
		/// </summary>
		/// <returns>Bool true if data is above 0</returns>
		private bool isDataAvailable() {
			if (socket.Available > 0) {
				return true;
			}
			else {
				return false;
			}
		}

		/// <summary>
		/// Removes all objects from the queue and makes a new empty queue
		/// </summary>
		public void resetQueue() {
			receivedObjectsConcurrentQueue = new ConcurrentQueue<byte[]>();
		}

		/// <summary>
		/// Puts and object into the queue of received objects
		/// </summary>
		private void enqueueBytes(byte[] bytes) {
			receivedObjectsConcurrentQueue.Enqueue(bytes);
		}
	}
}