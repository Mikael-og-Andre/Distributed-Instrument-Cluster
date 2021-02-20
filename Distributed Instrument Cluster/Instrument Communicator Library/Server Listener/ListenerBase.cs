using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace Instrument_Communicator_Library.Server_Listener {

    public abstract class ListenerBase {

        protected IPEndPoint ipEndPoint; //Access location of listener
        protected int maxConnections;   //Max number of connections to server
        protected int maxPendingConnections;    //Max number of connections waiting in the accepting socket
        protected Socket listeningSocket;       //Socket used for accepting incoming requests
        protected CancellationToken listenerCancellationToken;  //Token for telling server to stop

        protected int currentConnectionCount;       //Amount of current Connections

        public ListenerBase(IPEndPoint ipEndPoint, int _maxConnections = 30, int _maxPendingConnections = 30) {
            this.ipEndPoint = ipEndPoint;
            this.maxConnections = _maxConnections;
            this.maxPendingConnections = _maxPendingConnections;
            this.listenerCancellationToken = new CancellationToken();
            this.currentConnectionCount = 0;
        }

        /// <summary>
        /// Starts listening for remote connections
        /// </summary>
        public void Start() {
            //Create socket for incoming connections
            listeningSocket = new Socket(this.ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listeningSocket.Bind(this.ipEndPoint);

            //Start listen with Backlog size of max connections
            listeningSocket.Listen(this.maxPendingConnections);

            //Accepts Connections
            while (!listenerCancellationToken.IsCancellationRequested) {
                //Accept an incoming connection
                Console.WriteLine("SERVER - Main Thread {0} Says: Waiting For new Socket Connection...", Thread.CurrentThread.ManagedThreadId);
                Socket newSocket = listeningSocket.Accept();
                //Increment Current Connections
                this.currentConnectionCount += 1;
                //Creates a new Thread to run a client communication on
                Thread newThread = new Thread(HandleIncomingConnection);
                newThread.IsBackground = true;

                //Create a client connection object representing the connection
                object newClientConnection = CreateConnectionType(newSocket, newThread);

                try {
                    //Pass in ClientConnection and start a new thread ThreadProtocol
                    newThread.Start(newClientConnection);
                } catch (Exception ex) {
                    //Lower Connection number
                    this.currentConnectionCount -= 1;
                    newSocket.Disconnect(false);
                    newSocket.Close();
                }
            }
        }

        /// <summary>
        /// Function to handle the new incoming connection on a new thread
        /// </summary>
        /// <param name="obj">Subclass of ConnectionBase, Should be the corresponsind type returned from createConnectionType</param>
        protected abstract void HandleIncomingConnection(object obj);

        /// <summary>
        /// Function for specifying specific type of ConnectionBase child the class should be returning and handing over to the HandleIncomingConnection
        /// </summary>
        /// <param name="socket">Socket Of the incoming connection</param>
        /// <param name="thread">Thread the new connection will be running on</param>
        /// <returns> a Connection of one of the child types of ConnectionBase</returns>
        protected abstract object CreateConnectionType(Socket socket, Thread thread);
    }
}