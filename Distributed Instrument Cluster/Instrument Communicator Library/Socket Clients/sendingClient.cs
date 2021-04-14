using Networking_Library;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using Server_Library.Authorization;

namespace Server_Library.Socket_Clients {

	/// <summary>
	/// Client for sending objects to Receive Listener
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class SendingClient : ClientBase {

		/// <summary>
		/// queue for objects to send
		/// </summary>
		private ConcurrentQueue<byte[]> sendingByteArrayConcurrentQueue;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="port"></param>
		/// <param name="informationAboutClient"></param>
		/// <param name="accessToken"></param>
		/// <param name="isRunningCancellationToken"></param>
		public SendingClient(string ip, int port, ClientInformation informationAboutClient, AccessToken accessToken,
			CancellationToken isRunningCancellationToken) : base(ip, port, informationAboutClient, accessToken,
			isRunningCancellationToken) {
			//init queue
			sendingByteArrayConcurrentQueue = new ConcurrentQueue<byte[]>();
		}

		/// <summary>
		/// Send objects from the queue
		/// </summary>
		protected override void handleConnected() {

			isSetup = true;
			isSocketConnected = true;

			//Send objects
			while (!isRunningCancellationToken.IsCancellationRequested) {
				send();
			}
		}
		/// <summary>
		/// Put object into queue for sending
		/// </summary>
		/// <param name="obj"></param>
		public void queueBytesForSending(byte[] obj) {
			enqueueBytes(obj);
		}

		/// <summary>
		/// Overwrite the old queue with an empty new one
		/// </summary>
		public void resetQueue() {
			sendingByteArrayConcurrentQueue = new ConcurrentQueue<byte[]>();
		}

		/// <summary>
		/// Send an object from the queue
		/// </summary>
		private void send() {
			//If there is an object in the queue send it
			if (getObjectFromQueue(out byte[] output)) {
				NetworkingOperations.sendBytes(connectionNetworkStream,output);
			}
		}

		/// <summary>
		/// Get and object from the queue
		/// </summary>
		/// <param name="output"></param>
		/// <returns>True if object was found</returns>
		private bool getObjectFromQueue(out byte[] output) {
			if (sendingByteArrayConcurrentQueue.TryDequeue(out byte[] result)) {
				output = result;
				return true;
			}

			output = default;
			return false;
		}

		/// <summary>
		/// Add objects to queue for sending
		/// </summary>
		/// <param name="obj"></param>
		private void enqueueBytes(byte[] obj) {
			sendingByteArrayConcurrentQueue.Enqueue(obj);
		}

		/// <summary>
		/// Get the amount of items in the queue
		/// </summary>
		/// <returns>Number of items in queue</returns>
		private int queueCount() {
			return sendingByteArrayConcurrentQueue.Count;
		}
	}
}