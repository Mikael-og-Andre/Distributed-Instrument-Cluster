using System;
using System.Net.Sockets;
using System.Threading;

/// <summary>
/// Base class for communicator classes, intended to be on the remote side of the instrument cluster
/// <Author>Mikael Nilssen</Author>
/// </summary>

namespace Instrument_Communicator_Library {

    public abstract class CommunicatorBase {
        public string ip { get; private set; } //Ip address of target server
        public int port { get; private set; } //Port of target server
        protected Socket connectionSocket;    //Connection to server
        protected InstrumentInformation clientInfo;   //Information about hardware
        protected AccessToken accessToken;   // Authorization code to send to the server

        //State
        public bool isSocketConnected { get; private set; } = false; //Is the socket connected to the server

        protected CancellationToken communicatorCancellationToken;    //Cancelation token used to stop loops

        public CommunicatorBase(string ip, int port, InstrumentInformation informationAboutClient, AccessToken accessToken) {
            this.ip = ip;
            this.port = port;
            this.clientInfo = informationAboutClient;
            this.accessToken = accessToken;
        }

        /// <summary>
        /// Starts the client and attempts to connect to the server
        /// </summary>
        public void start() {
            try {
                // Create new socket
                connectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            } catch (Exception ex) {
                throw ex;
            }
            //connection state
            isSocketConnected = false;

            // Loop whilst the client is supposed to run
            while (!communicatorCancellationToken.IsCancellationRequested) {
                //check if client is connected, if not connect
                if (!isSocketConnected) {
                    // Try to connect
                    isSocketConnected = attemptConnection(connectionSocket);
                }
                //check if client is connected, if it is handle the connection
                if (isSocketConnected) {
                    //handle the connection
                    handleConnected(connectionSocket);
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
        protected bool attemptConnection(Socket connectionSocket) {
            try {
                if (connectionSocket.Connected) {
                    return true;
                }
                //Try Connecting to server
                connectionSocket.Connect(ip, port);
                return true;
            } catch (Exception ex) {
                throw new Exception(ex.ToString());
                //return false to represent failed connection
                //return false;
            }
        }

        /// <summary>
        /// The main function of a communicator that gets called after you are connected and preforms actions with the socket
        /// </summary>
        /// <param name="connectionSocket"></param>
        abstract protected void handleConnected(Socket connectionSocket);

        /// <summary>
        /// returns the cnacelation token
        /// </summary>
        /// <returns>Returns cancellation token</returns>
        public CancellationToken GetCancellationToken() {
            return this.communicatorCancellationToken;
        }
    }
}