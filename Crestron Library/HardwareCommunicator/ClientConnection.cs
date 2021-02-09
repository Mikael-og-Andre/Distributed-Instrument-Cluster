using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;

/// <summary>
/// Class that represents a client connection to the server unit.
/// <author>Mikael Nilssen</author>
/// </summary>

namespace HardwareCommunicator {
    public class ClientConnection {

        private Socket socket { get; set; }     //Socket of the client Connection
        ConcurrentQueue<string> concurrentQueueSendToClient;  // Concurrent queue for commands to send to the client
        ConcurrentQueue<string> concurrentQueueRecieveFromClient;  //Concurrent queue for commands recieved by the client
        private Thread myThread; // The thread the connection is running on
        private AccessToken accessToken = null; // Token representing a valid connection to the server

        public ClientConnection(Socket socket, Thread thread) {
            this.socket = socket;
            this.myThread = thread;
        }

        /// <summary>
        /// get the socket representing the client connection
        /// </summary>
        /// <returns>A socket</returns>
        public Socket getSocket() {
            return socket;
        }

        /// <summary>
        /// Sets the accessToken For the conncetion
        /// </summary>
        /// <param name="token"></param>
        public void setAccessToken(AccessToken token) {
            this.accessToken = token;
        }

        /// <summary>
        /// Get the accessToken set by the authorization process
        /// </summary>
        /// <returns>Returns accessToken</returns>
        public AccessToken getAccessToken() {
            if (this.accessToken != null) {
                return this.accessToken;
            } else {
                throw new NullReferenceException("AccessToken as not been set yet");
            }
        }
    }
}
