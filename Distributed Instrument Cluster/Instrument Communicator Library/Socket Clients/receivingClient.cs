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
	public class ReceivingClient : ClientBase {

		/// <summary>
		/// Queue for received objects
		/// </summary>
		private ConcurrentQueue<byte[]> receivedByteArraysQueue;

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
			receivedByteArraysQueue = new ConcurrentQueue<byte[]>();
		}

		/// <summary>
		/// receive objects
		/// </summary>
		protected override void handleConnected() {

			isSetup = true;
			isSocketConnected = true;

			//Receive Objects
			while (!isRunningCancellationToken.IsCancellationRequested) {
				receive();
			}
		}

		/// <summary>
		/// Get an object from the queue of received objects
		/// </summary>
		/// <param name="output"></param>
		/// <returns>True if object was found</returns>
		public bool getBytesFromClient(out byte[] output) {
			if (receivedByteArraysQueue.TryDequeue(out byte[] result)) {
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
			receivedByteArraysQueue = new ConcurrentQueue<byte[]>();
		}

		/// <summary>
		/// Receive an object from the socket and put it in the internal queue
		/// </summary>
		private void receive() {
			if (isDataAvailable()) {

				byte[] incomingByteArray = NetworkingOperations.receiveBytes(connectionNetworkStream,9000000);

				//Put object in queue
				enqueueBytes(incomingByteArray);
			}
		}

		/// <summary>
		/// Enqueue an object
		/// </summary>
		/// <param name="obj">Object to enqueue</param>
		private void enqueueBytes(byte[] obj) {
			receivedByteArraysQueue.Enqueue(obj);
		}
		
	}
}