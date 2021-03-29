﻿using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using Networking_Library;
using Server_Library.Authorization;

namespace Server_Library.Socket_Clients {

    /// <summary>
    /// Represents a socket line from a device to the server, intended to send video
    /// <author>Mikael Nilssen</author>
    /// </summary>
    public class VideoClient : ClientBaseOld{

		/// <summary>
		/// queue of inputs meant to be sent to server
		/// </summary>
        private ConcurrentQueue<VideoFrame> inputQueue;

        public VideoClient(string ip, int port, ClientInformation informationAboutClient, AccessToken accessToken, CancellationToken cancellationToken) : base(ip, port, informationAboutClient, accessToken, cancellationToken) {
            //initialize queue
            inputQueue = new ConcurrentQueue<VideoFrame>();
        }

        /// <summary>
        /// Handles the protocols after the socket has been connected
        /// </summary>
        /// <param name="connectionSocket"></param>
        protected override void handleConnected(Socket connectionSocket) {
            //wait for signal to start instrument detailing
            string response = NetworkingOperations.receiveStringWithSocket(connectionSocket);
            if (!response.ToLower().Equals("y")) {
                
            }
			//Send Information about device
            NetworkingOperations.sendStringWithSocket(information.Name, connectionSocket);
            NetworkingOperations.sendStringWithSocket(information.Location, connectionSocket);
            NetworkingOperations.sendStringWithSocket(information.Type, connectionSocket);

            //While not canceled push from queue to socket
            while (!isRunningCancellationToken.IsCancellationRequested) {
                //get input form queue
                bool hasInput = inputQueue.TryDequeue(out VideoFrame frame);
                if (!hasInput) continue;

                NetworkingOperations.sendObjectWithSocket<VideoFrame>(frame, connectionSocket);
            }
        }

        /// <summary>
        /// Get the concurrent Queue where you can enqueue frame to push them to the server
        /// </summary>
        /// <returns></returns>
        public ConcurrentQueue<VideoFrame> getInputQueue() {
            return inputQueue;
        }
    }
}