using Networking_Library;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using Server_Library.Authorization;
using Server_Library.Connection_Classes;

namespace Server_Library.Connection_Types {

	/// <summary>
	/// Connection for receiving objects
	/// <author>Mikael Nilssen</author>
	/// </summary>
	/// <typeparam name="T">Object Type You want the connection to receive</typeparam>
	public class ReceivingConnection<T> : ConnectionBase {

		/// <summary>
		/// Queue containing incoming objects
		/// </summary>
		private ConcurrentQueue<T> receivedObjectsConcurrentQueue;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="homeThread"></param>
		/// <param name="socket"></param>
		public ReceivingConnection(Thread homeThread, Socket socket, AccessToken accessToken, ClientInformation info, CancellationToken token) : base(homeThread, socket, accessToken, info, token) {
			receivedObjectsConcurrentQueue = new ConcurrentQueue<T>();
		}

		/// <summary>
		/// Get an object from the incoming objects
		/// </summary>
		/// <param name="output"></param>
		/// <returns>True if and object was found</returns>
		public bool getObjectFromConnection(out T output) {
			//Try to dequeue
			if (receivedObjectsConcurrentQueue.TryDequeue(out T obj)) {
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
		public void receive() {
			//Get json from Client
			string jsonObject = NetworkingOperations.receiveStringWithSocket(socket);
			//Convert to object
			T obj = JsonSerializer.Deserialize<T>(jsonObject);
			//Put object in queue
			enqueueObject(obj);
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

		/// <summary>
		/// Removes all objects from the queue and makes a new empty queue
		/// </summary>
		public void resetQueue() {
			receivedObjectsConcurrentQueue = new ConcurrentQueue<T>();
		}

		/// <summary>
		/// Puts and object into the queue of received objects
		/// </summary>
		/// <param name="obj">Object of type T</param>
		private void enqueueObject(T obj) {
			receivedObjectsConcurrentQueue.Enqueue(obj);
		}
	}
}