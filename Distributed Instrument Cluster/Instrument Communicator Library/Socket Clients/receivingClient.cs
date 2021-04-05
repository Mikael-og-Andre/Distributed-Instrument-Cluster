using Networking_Library;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using Server_Library.Authorization;

namespace Server_Library.Socket_Clients {

	/// <summary>
	/// Client for Receiving objects from Sending Listener
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class ReceivingClient<T> : ClientBase {

		/// <summary>
		/// Queue for received objects
		/// </summary>
		private ConcurrentQueue<T> receivedObjectsQueue;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="port"></param>
		/// <param name="informationAboutClient"></param>
		/// <param name="accessToken"></param>
		/// <param name="isRunningCancellationToken"></param>
		public ReceivingClient(string ip, int port, ClientInformation informationAboutClient, AccessToken accessToken, CancellationToken isRunningCancellationToken) : 
			base(ip, port, informationAboutClient, accessToken, isRunningCancellationToken) {
			//Init queue
			receivedObjectsQueue = new ConcurrentQueue<T>();
		}

		/// <summary>
		/// receive objects
		/// </summary>
		protected override void handleConnected(int delay) {

			isSetup = true;
			isSocketConnected = true;

			//Receive Objects
			while (!isRunningCancellationToken.IsCancellationRequested) {
				receive();
				Thread.Sleep(delay);
			}
		}

		/// <summary>
		/// Get an object from the queue of received objects
		/// </summary>
		/// <param name="output"></param>
		/// <returns>True if object was found</returns>
		public bool getObjectFromClient(out T output) {
			if (receivedObjectsQueue.TryDequeue(out T result)) {
				output = result;
				return true;
			}

			output = default;
			return false;
		}
		
		/// <summary>
		/// Overwrites the old Queue with a new empty one
		/// </summary>
		public void resetQueue() {
			receivedObjectsQueue = new ConcurrentQueue<T>();
		}

		/// <summary>
		/// Receive an object from the socket and put it in the internal queue
		/// </summary>
		private void receive() {
			if (isDataAvailable()) {

				T obj = NetworkingOperations.receiveJsonObjectWithSocket<T>(connectionSocket);

				//Put object in queue
				enqueueObject(obj);
			}
		}

		/// <summary>
		/// Enqueue an object
		/// </summary>
		/// <param name="obj">Object to enqueue</param>
		private void enqueueObject(T obj) {
			receivedObjectsQueue.Enqueue(obj);
		}
		
	}
}