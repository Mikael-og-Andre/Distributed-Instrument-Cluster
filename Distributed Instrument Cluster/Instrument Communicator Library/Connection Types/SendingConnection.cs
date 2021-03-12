using Networking_Library;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using Server_Library.Authorization;
using Server_Library.Connection_Classes;

namespace Server_Library.Connection_Types {

	/// <summary>
	/// Connection for sending objects
	/// <author> Mikael Nilssen</author>
	/// </summary>
	/// <typeparam name="T">Object type you want to send</typeparam>
	public class SendingConnection<T> : ConnectionBase {

		/// <summary>
		/// queue of objects to send
		/// </summary>
		private ConcurrentQueue<T> sendingObjectsConcurrentQueue;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="homeThread"></param>
		/// <param name="socket"></param>
		public SendingConnection(Thread homeThread, Socket socket, AccessToken accessToken, ClientInformation info,
			CancellationToken token) : base(homeThread, socket, accessToken, info, token) {
			//init queue
			sendingObjectsConcurrentQueue = new ConcurrentQueue<T>();
		}


		public void send() {
			if (getObjectFromQueue(out T output)) {
				//Serialize object
				string json = JsonSerializer.Serialize(output);
				//Send string
				NetworkingOperations.sendStringWithSocket(json,socket);
			}
		}

		/// <summary>
		/// Get an object from the incoming objects
		/// </summary>
		/// <param name="output"></param>
		/// <returns>True if and object was found</returns>
		protected bool getObjectFromQueue(out T output) {
			//Try to dequeue
			if (sendingObjectsConcurrentQueue.TryDequeue(out T obj)) {
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
		public void queueObjectForSending(T obj) {
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
			sendingObjectsConcurrentQueue = new ConcurrentQueue<T>();
		}

		/// <summary>
		/// checks if there is data available on the socket
		/// </summary>
		/// <returns>Bool true if data is above 0</returns>
		public bool isDataAvailable() {
			if (socket.Available > 0) {
				return true;
			}
			else {
				return false;
			}
		}
	}
}