using System;
using System.Net.Sockets;
using System.Threading;

namespace Instrument_Communicator_Library {

    /// <summary>
    /// Base class for communicator classes, intended to be on the remote side of the instrument cluster
    /// <Author>Mikael Nilssen</Author>
    /// </summary>
    public abstract class CommunicatorBase {
        private string ip { get; set; } //Ip address of target server
        private int port { get; set; } //Port of target server
        private Socket connectionSocket;    //Connection to server
        private InstrumentInformation clientInfo;   //Information about hardware
        protected AccessToken accessToken;   // Authorization code to send to the server

        //State
        public bool isSocketConnected { get; private set; } = false; //Is the socket connected to the server

        protected CancellationToken communicatorCancellationToken;    //Cancellation token used to stop loops

        protected CommunicatorBase(string ip, int port, InstrumentInformation informationAboutClient, AccessToken accessToken, CancellationToken communicatorCancellationToken) {
            this.ip = ip;
            this.port = port;
            this.clientInfo = informationAboutClient;
            this.accessToken = accessToken;
            this.communicatorCancellationToken = communicatorCancellationToken;
        }

        /// <summary>
        /// Starts the client and attempts to connect to the server
        /// </summary>
        public void Start() {
            try {
                // Create new socket
                connectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            } catch (SocketException ex) {
                throw ex;
            }
            //connection state
            isSocketConnected = false;

            // Loop whilst the client is supposed to run
            while (!communicatorCancellationToken.IsCancellationRequested) {
                //check if client is connected, if not connect
                if (!isSocketConnected) {
                    // Try to connect
                    isSocketConnected = AttemptConnection(connectionSocket);
                }
                //check if client is connected, if it is handle the connection
                if (isSocketConnected) {
                    //handle the connection
                    HandleConnected(connectionSocket);
                } else {
                    Console.WriteLine("Thread {0} says: " + "Connection failed", Thread.CurrentThread.ManagedThreadId);
                }
            }
        }

        /// <summary>
        /// Attempts to connect to the given host and ip
        /// </summary>
        /// <param name="connectionSocket"> unconnected Soccket</param>
        /// <returns> boolean representing succesful conncetion</returns>
        private bool AttemptConnection(Socket connectionSocket) {
            try {
                if (connectionSocket.Connected) {
                    return true;
                }
                //Try Connecting to server
                connectionSocket.Connect(ip, port);
                return true;
            } catch (SocketException ex) {
                throw new SocketException(ex.ErrorCode);
                //return false to represent failed connection
                //return false;
            }
        }

        /// <summary>
        /// The main function of a communicator that gets called after you are connected and preforms actions with the socket
        /// </summary>
        /// <param name="connectionSocket"></param>
        protected abstract void HandleConnected(Socket connectionSocket);

        /// <summary>
        /// returns the cnacelation token
        /// </summary>
        /// <returns>Returns cancellation token</returns>
        public CancellationToken GetCancellationToken() {
            return this.communicatorCancellationToken;
        }
    }
}