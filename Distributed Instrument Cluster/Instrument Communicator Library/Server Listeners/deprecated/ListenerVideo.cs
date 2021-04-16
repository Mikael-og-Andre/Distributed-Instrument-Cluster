﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Networking_Library;
using Server_Library.Authorization;
using Server_Library.Connection_Types.deprecated;

namespace Server_Library.Server_Listeners.deprecated {

	/// <summary>
	/// Listener for incoming video connections
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class ListenerVideo : ListenerBaseOld {

		private const int IncomingByteArrayBufferSize = 200000;

		/// <summary>
		/// list of connected video streams
		/// </summary>
		private List<VideoConnection> listVideoConnections;

		/// <summary>
		/// queue of all incoming connections
		/// </summary>
		private ConcurrentQueue<VideoConnection> incomingConnectionsQueue;

		public ListenerVideo(IPEndPoint ipEndPoint, int maxConnections = 30, int maxPendingConnections = 30) : base(ipEndPoint, maxConnections, maxPendingConnections) {
			listVideoConnections = new List<VideoConnection>();
			incomingConnectionsQueue = new ConcurrentQueue<VideoConnection>();
		}

		/// <summary>
		/// Create connection of the appropriate type, is used in the base class.
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="thread"></param>
		/// <returns>VideoConnection</returns>
		protected override object createConnectionType(Socket socket, Thread thread,AccessToken accessToken, ClientInformation info) {
			return new VideoConnection(thread, socket, accessToken, info, cancellationTokenSource.Token);
		}

		/// <summary>
		/// Runs when a new connection is accepted by the listener
		/// </summary>
		/// <param name="obj">VideoConnection object</param>
		protected override void handleIncomingConnection(object obj) {
			//Cast to video-connection
			VideoConnection videoConnection = (VideoConnection)obj;
			//add connection to list
			addVideoConnection(videoConnection);
			//Add connection to incoming connectionQueue

			incomingConnectionsQueue.Enqueue(videoConnection);

			//Get socket
			Socket connectionSocket = videoConnection.getSocket();
			
			
			//Get outputQueue
			ConcurrentQueue<VideoFrame> outputQueue = videoConnection.getOutputQueue();

			//Do main loop
			while (!cancellationTokenSource.Token.IsCancellationRequested) {
				//Get Incoming object
				byte[] objectBytes = NetworkingOperations.receiveByteArrayWithSocket(connectionSocket,IncomingByteArrayBufferSize);
				//create empty frame
				VideoFrame temp = new VideoFrame(new byte[]{});
				//Convert array to new frame
				VideoFrame inFrame = (VideoFrame) temp.getObject(objectBytes);
				//Enqueue the frame
				outputQueue.Enqueue(inFrame);
			}
			//remove connection
			removeVideoConnection(videoConnection);
		}

		/// <summary>
		/// Add connection to list of connections
		/// </summary>
		/// <param name="connection">VideoConnection</param>
		private void addVideoConnection(VideoConnection connection) {
			lock (listVideoConnections) {
				listVideoConnections.Add(connection);
			}
		}

		/// <summary>
		/// Remove the client connection from the list
		/// </summary>
		/// <param name="connection"> Video Connection</param>
		/// <returns>Boolean representing successful removal</returns>
		private bool removeVideoConnection(VideoConnection connection) {
			//Lock list and remove the connection
			bool result = false;
			lock (listVideoConnections) {
				//Try to remove connection
				result = listVideoConnections.Remove(connection);
			}
			//return bool
			return result;
		}

		/// <summary>
		/// Get the list of video connection objects
		/// </summary>
		/// <returns>List of video-connection objects of type T</returns>
		public List<VideoConnection> getVideoConnectionList() {
			lock (listVideoConnections) {
				return listVideoConnections;
			}
		}

		/// <summary>
		/// Returns the queue containing each incoming connection
		/// </summary>
		/// <returns></returns>
		public ConcurrentQueue<VideoConnection> getIncomingConnectionQueue() {
			return incomingConnectionsQueue;
		}
	}
}