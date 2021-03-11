using System.Collections.Concurrent;
using Instrument_Communicator_Library.Connection_Classes;
using System.Net.Sockets;
using System.Threading;
using Networking_Library;

namespace Instrument_Communicator_Library.Connection_Types {

	/// <summary>
	/// Connection for sending objects
	/// <author> Mikael Nilssen</author>
	/// </summary>
	/// <typeparam name="T">Object type you want to send</typeparam>
	public class SendingConnection<T> : ConnectionBase where T: ISerializeObject{

		/// <summary>
		/// queue of objects to send
		/// </summary>
		private ConcurrentQueue<T> sendingObjectsConcurrentQueue;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="homeThread"></param>
		/// <param name="socket"></param>
		public SendingConnection(Thread homeThread, Socket socket) : base(homeThread, socket) { }

		/// <summary>
		/// Get an object from the incoming objects
		/// </summary>
		/// <param name="output"></param>
		/// <returns>True if and object was found</returns>
		public bool getObjectFromConnection(out T output) {
			//Try to dequeue
			if (sendingObjectsConcurrentQueue.TryDequeue(out T obj)) {
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
		public void send(T obj) {
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
			this.sendingObjectsConcurrentQueue = new ConcurrentQueue<T>();
		}
	}
}