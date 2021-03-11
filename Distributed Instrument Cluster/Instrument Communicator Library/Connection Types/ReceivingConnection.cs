using System.Collections.Concurrent;
using Instrument_Communicator_Library.Connection_Classes;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading;
using Networking_Library;

namespace Instrument_Communicator_Library.Connection_Types {

	/// <summary>
	/// Connection for receiving objects
	/// <author>Mikael Nilssen</author>
	/// </summary>
	/// <typeparam name="T">Object Type You want the connection to receive</typeparam>
	public class ReceivingConnection<T> : ConnectionBase where T: ISerializeObject{

		/// <summary>
		/// Queue containing incoming objects
		/// </summary>
		private ConcurrentQueue<T> receivedObjectsConcurrentQueue;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="homeThread"></param>
		/// <param name="socket"></param>
		public ReceivingConnection(Thread homeThread, Socket socket) : base(homeThread, socket) {
			receivedObjectsConcurrentQueue = new ConcurrentQueue<T>();
		}

		/// <summary>
		/// Get an object from the incoming objects
		/// </summary>
		/// <param name="output"></param>
		/// <returns>True if and object was found</returns>
		public bool receive(out T output) {
			//Try to dequeue
			if (receivedObjectsConcurrentQueue.TryDequeue(out T obj)) {
				//Return with true
				output = obj;
				return true;
			}
			//Return with false
			output = default(T);
			return false;
		}

		/// <summary>
		/// Puts and object into the queue of received objects
		/// </summary>
		/// <param name="obj">Object of type T</param>
		public void enqueueObject(T obj) {
			receivedObjectsConcurrentQueue.Enqueue(obj);
		}

		/// <summary>
		/// Get the amount of objects in the queue
		/// </summary>
		/// <returns>int</returns>
		public int getItemCount() {
			return receivedObjectsConcurrentQueue.Count;
		}

		/// <summary>
		/// Removes all objects from the queue and makes a new empty queue
		/// </summary>
		public void resetQueue() {
			this.receivedObjectsConcurrentQueue = new ConcurrentQueue<T>();
		}
	}
}