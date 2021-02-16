using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace Instrument_Communicator_Library {
    public abstract class Communicator {

        public string ip { get; private set; } //Ip address of target server
        public int port { get; private set; } //Port of target server
        protected Socket connectionSocket;    //Connection to server
        protected InstrumentInformation clientInfo;   //Information about hardware
        protected AccessToken accessToken;   // Authorization code to send to the server

        //State
        protected bool isSocketConnected = false; //Is the socket connected to the server
        protected CancellationToken serverRunningCancellationToken;    //Cancelation token used to stop loops


        public Communicator(string ip, int port, InstrumentInformation informationAboutClient, AccessToken accessToken) {
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
            while (!serverRunningCancellationToken.IsCancellationRequested) {
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
            } catch (SocketException ex) {
                //TODO: Add logging to instrument server
                //return false to represent failed connection
                return false;
            }
        }

        abstract protected void handleConnected(Socket connectionSocket);

        

    }
}
