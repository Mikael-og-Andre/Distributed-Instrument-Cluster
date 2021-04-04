using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server.CommandHandler {
	/// <summary>
	/// Queue implementation with find position and delete
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class TrackingQueue<T> {
		/// <summary>
		/// Linked list of items
		/// </summary>
		private LinkedList<T> linkedList;

		public TrackingQueue() {
			linkedList = new LinkedList<T>();
		}

		public void enqueue(T item) {
			lock (linkedList) {
				linkedList.AddFirst(item);
			}
		}

		public bool tryDequeue(out T output) {
			lock (linkedList) {
				//Check if list is empty
				if (linkedList.Count<1) {
					//Return default and false
					output = default;
					return false;
				}

				if (linkedList.Last is null) {
					//Return default and false
					output = default;
					return false;
				}

				//Set output to last value
				output = linkedList.Last.Value;
				//remove the last value
				linkedList.RemoveLast();
				return true;
			}
		}

		/// <summary>
		/// Returns position in queue of the wanted item, Returns -1 if not found
		/// the number is position away from being the next dequeue
		/// The smallest position value will be 1
		/// </summary>
		/// <param name="input">Item to search for in the queue</param>
		/// <returns>position as int, or -1 if not found</returns>
		public int getPosition(T input) {
			lock (linkedList) {
				//List is empty
				if (linkedList.Last is null) {
					return -1;
				}

				//Get last node
				LinkedListNode<T> currentNode = linkedList.Last;
				//Loop all nodes and check if matching input
				for (int i = 1; i <= linkedList.Count; i++) {
					//Check if matches input
					if (currentNode.Equals(input)) {
						return i;
					}

					//Else set to previous node
					currentNode = currentNode.Previous;
					//Not found
					if (currentNode is null) {
						return -1;
					}
				}
			}
			//Not found
			return -1;
		}

		/// <summary>
		/// Clears all items from queue
		/// </summary>
		public void clear() {
			lock (linkedList) {
				linkedList.Clear();
			}
		}

		/// <summary>
		/// Returns true if the list is empty
		/// </summary>
		/// <returns></returns>
		public bool isEmpty() {
			lock (linkedList) {
				if (linkedList.Count<1) {
					return true;
				}
				else {
					return false;
				}
			}
		}
	}
}
