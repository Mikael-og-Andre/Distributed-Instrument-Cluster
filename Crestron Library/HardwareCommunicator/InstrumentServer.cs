using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.Concurrent;


/// <summary>
/// Server to be run for lsitnening to incoming device connections and sending commadns and data to them
/// @Author Mikael Nilssen
/// </summary>

namespace HardwareCommunicator {
    public class InstrumentServer {

        private int maxConnections; //Maximum number of connections for the Pool
        private int maxPendingConnections;  //Backlog size of Listening socket
        private int numConnections = 0; //Connected Sockets
        public bool isServerRunning { get; private set; } //Should the server listen for more connection

        private Socket listenSocket;    //Socket for accepting incoming connections
        private IPEndPoint ipEndPoint { get; set; }     //Host Info

        // TODO: Add Concurrent list for Client Connections

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ipEndPoint"> ip specification used by socket</param>
        /// <param name="maxConnections"> maximum number of allowed connections by the server</param>
        /// <param name="maxPendingConnections"> maximum number of pending connections for the lsitenning socket</param>
        public InstrumentServer(IPEndPoint ipEndPoint, int maxConnections = 30, int maxPendingConnections = 30) {

            this.maxConnections = maxConnections;
            this.ipEndPoint = ipEndPoint;
            this.maxPendingConnections = maxPendingConnections;
            isServerRunning = true;
            
        }

        
        /// <summary>
        /// Sets up listening socket, and calls StartAccepting to continously accept new clients
        /// </summary>
        public void StartListening() {

            //Create socket for incoming connections
            listenSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(ipEndPoint);

            //Start listen with Backlog size of max connections
            listenSocket.Listen(maxPendingConnections);
            //Accepts Connections async
            StartAccepting(listenSocket);
            Console.WriteLine("Thread {0} Says: Socket Setup Complete, Started Accepting", Thread.CurrentThread.ManagedThreadId);
            
        }

        /// <summary>
        /// Continuously accepts new incoming clients and ThreadProtocol on a new thread to handle communication.
        /// isServerRunning determines if the server will accept new clients
        /// </summary>
        /// <param name="listeningSocket"> Socket currently listenning for incoming connections</param>
        private async void StartAccepting(Socket listeningSocket) {

            while (isServerRunning) {

                //Accept an incoming connection
                for (int i = 0; i<100;i++) {
                    Console.WriteLine("Thread {0} Says: Waiting For new Connection...", Thread.CurrentThread.ManagedThreadId);
                    Thread.Sleep(1000);
                }
                Console.WriteLine("Thread {0} Says: Waiting For new Connection...", Thread.CurrentThread.ManagedThreadId);
                Socket newSocket = await listenSocket.AcceptAsync();
                //Increment Current Connections
                incrementConnectionNumber();
                //Creates a new Thread to run a client communication on
                Thread newClientThread = new Thread(ThreadProtocol);
                newClientThread.IsBackground = true;

                //Create a client connection object representing the connection
                ClientConnection newClientConnection = new ClientConnection(listenSocket, newClientThread);

                //Add connection to active connections
                //AddClientConnection(newClientConnection);

                try {
                    //Pass in ClientConnection and start ThreadProtocol
                    newClientThread.Start(newClientConnection);
                } catch (Exception e) {

                    RemoveClientConnection(newClientConnection);
                    decrementConnectionNumber();
                    newSocket.Dispose();
                    newSocket.Close();
                }
            }

        }

        /// <summary>
        /// Represents a communication thread that handles all communication with a single connected client
        /// </summary>
        /// <param name="obj"> represents a ClientConnection object. in order to be used as a parameraizedThread, it needs to be casted</param>
        public static void ThreadProtocol(object obj) {

            ClientConnection clientConnection;
            try {
                clientConnection = (ClientConnection) obj;
            } catch (InvalidCastException) {
                throw new Exception("Could not cast input object to ClientConnection in method ThreadPortocol");
            }
            Console.WriteLine("Thread: {0}, is running now");

        }
        /// <summary>
        /// Adds 1 to numConnections var
        /// Represents the number of connected clients to the server
        /// </summary>
        private void incrementConnectionNumber() {
            numConnections++;
        }
        /// <summary>
        /// Removes 1 from numConnections var
        /// Represents the number of connected clients to the server
        /// </summary>
        private void decrementConnectionNumber() {
            if (numConnections>0) {
                numConnections--;
            }

        }

        /// <summary>
        /// Stops server from looping and clears socket
        /// </summary>
        public void StopServer() {
            isServerRunning = false;
            listenSocket.Dispose();
        }


        // TODO: Add Concurrent adding
        private void AddClientConnection(ClientConnection connection) {
            throw new NotImplementedException();
        }

        //TODO: Add Concurrent removing
        private bool RemoveClientConnection(ClientConnection connection) {
            throw new NotImplementedException();
        }

    }
}
