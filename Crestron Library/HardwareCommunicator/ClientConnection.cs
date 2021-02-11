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

namespace InstrumentCommunicator {
    public class ClientConnection {

        private Socket socket { get; set; }     //Socket of the client Connection
        ConcurrentQueue<string> concurrentQueueInput;  // Concurrent queue for commands to send to the client
        ConcurrentQueue<string> concurrentQueueOutput;  //Concurrent queue for commands recieved by the client
        private Thread myThread; // The thread the connection is running on
        private AccessToken accessToken = null; // Token representing a valid connection to the server
        private bool isActive = true; //Is the connection running

        public ClientConnection(Socket socket, Thread thread) {
            this.socket = socket;
            this.myThread = thread;
            //Init queues
            concurrentQueueInput = new ConcurrentQueue<string>();
            concurrentQueueOutput = new ConcurrentQueue<string>();

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

        /// <summary>
        /// Gets the connection status, representing wheter connection has closed
        /// </summary>
        /// <returns>Boolean true if still active</returns>
        public bool isConnectionActive() {
            return isActive;
        }

        /// <summary>
        /// Sets the is active value
        /// </summary>
        /// <param name="value">Boolean, True for still running</param>
        public void setIsConnectionActive(bool value) {
            this.isActive = value;
        }

        /// <summary>
        /// Get a refrence to the queue used to send commands to the client
        /// </summary>
        /// <returns></returns>
        public ref ConcurrentQueue<string> getRefInputQueue() {
            return ref this.concurrentQueueInput;
        }

        /// <summary>
        /// Get a refrence to the queue used for receiving from the client
        /// </summary>
        /// <returns></returns>
        public ref ConcurrentQueue<string> getRefOutputQueue() {
            return ref this.concurrentQueueOutput;
        }

    }
}
