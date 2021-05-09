using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using Server_Library.Authorization;
using Server_Library.Connection_Classes;
using Socket_Library;

namespace Server_Library.Connection_Types {

	/// <summary>
	/// Connection for sending objects
	/// <author> Mikael Nilssen</author>
	/// </summary>
	public class SendingConnection : ConnectionBase {

		/// <summary>
		/// queue of objects to send
		/// </summary>
		private ConcurrentQueue<byte[]> sendingObjectsConcurrentQueue;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="accessToken"></param>
		/// <param name="info"></param>
		/// <param name="token"></param>
		public SendingConnection(Socket socket, AccessToken accessToken, ClientInformation info, CancellationToken token) : 
			base(socket, accessToken, info, token) {
			//init queue
			sendingObjectsConcurrentQueue = new ConcurrentQueue<byte[]>();
		}


		public bool send() {
			if (getByteArrayFromQueue(out byte[] output)) {
				//send object
				NetworkingOperations.sendBytes(connectionNetworkStream,output);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Get an object from the incoming objects
		/// </summary>
		/// <param name="output"></param>
		/// <returns>True if and object was found</returns>
		protected bool getByteArrayFromQueue(out byte[] output) {
			//Try to dequeue
			if (sendingObjectsConcurrentQueue.TryDequeue(out byte[] obj)) {
				//Return with true
				output = obj;
				return true;
			}
			//Return with false
			output = default;
			return false;
		}

		/// <summary>
		/// Puts and object into the queue of received objects
		/// </summary>
		/// <param name="obj">Object of type T</param>
		public void queueByteArrayForSending(byte[] obj) {
			sendingObjectsConcurrentQueue.Enqueue(obj);
		}

		/// <summary>
		/// Get the amount of objects in the queue
		/// </summary>
		/// <returns>int</returns>
		public int getItemCount() {
			return sendingObjectsConcurrentQueue.Count;
		}

		/// <summary>
		/// Removes all objects from the queue and makes a new empty queue
		/// </summary>
		public void resetQueue() {
			sendingObjectsConcurrentQueue = new ConcurrentQueue<byte[]>();
		}

		/// <summary>
		/// checks if there is data available on the socket
		/// </summary>
		/// <returns>Bool true if data is above 0</returns>
		protected bool isDataAvailable() {
			if (socket.Available > 0) {
				return true;
			}
			else {
				return false;
			}
		}
	}
}