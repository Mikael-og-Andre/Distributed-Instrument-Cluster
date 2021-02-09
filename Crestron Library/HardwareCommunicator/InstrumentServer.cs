using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.Immutable;
using System.Collections.Concurrent;


/// <summary>
/// Server to be run for listening to incoming device connections and sending commands and data to them
/// <author>Mikael Nilssen</author>
/// </summary>

namespace HardwareCommunicator {
    public class InstrumentServer {

        private int maxConnections; //Maximum number of connections for the Pool
        private int maxPendingConnections;  //Backlog size of Listening socket
        private int numConnections = 0; //Connected Sockets
        public bool isServerRunning { get; private set; } //Should the server listen for more connection
        private Socket listenSocket;    //Socket for accepting incoming connections
        private IPEndPoint ipEndPoint { get; set; }     //Host Info
        private ImmutableList<ClientConnection> clientConnections;   //All connected clients

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
            clientConnections = ImmutableList<ClientConnection>.Empty;  //Initialize empty list for tracking client connections
            
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
        }

        /// <summary>
        /// Continuously accepts new incoming clients and ThreadProtocol on a new thread to handle communication.
        /// isServerRunning determines if the server will accept new clients
        /// </summary>
        /// <param name="listeningSocket"> Socket currently listenning for incoming connections</param>
        private void StartAccepting(Socket listeningSocket) {

            while (isServerRunning) {

                //Accept an incoming connection
                Console.WriteLine("Thread {0} Says: Waiting For new Connection...", Thread.CurrentThread.ManagedThreadId);
                Socket newSocket = listenSocket.Accept();
                //Increment Current Connections
                incrementConnectionNumber();
                //Creates a new Thread to run a client communication on
                Thread newClientThread = new Thread(ThreadRunProtocols);
                newClientThread.IsBackground = true;

                //Create a client connection object representing the connection
                ClientConnection newClientConnection = new ClientConnection(listenSocket, newClientThread);
                //Add connection to active connections
                AddClientConnection(newClientConnection);

                try {
                    //Pass in ClientConnection and start ThreadProtocol
                    newClientThread.Start(newClientConnection);
                } catch (Exception e) {
                    //Remove client from list of clients if failed
                    RemoveClientConnection(newClientConnection);
                    //Lower Connection number
                    decrementConnectionNumber();
                    newSocket.Disconnect(false);
                    newSocket.Dispose();
                    newSocket.Close();
                }
            }

        }

        /// <summary>
        /// Represents a communication thread that handles all communication with a single connected client
        /// </summary>
        /// <param name="obj"> represents a ClientConnection object. in order to be used as a parameraizedThread, it needs to be casted</param>
        public static void ThreadRunProtocols(object obj) {

            ClientConnection clientConnection;
            try {
                //cast input object to Client Connection
                clientConnection = (ClientConnection) obj;
            } catch (InvalidCastException) {
                throw new InvalidCastException("Could not cast input object to ClientConnection in method ThreadPortocol, Class InstrumentServer");
            }
            Console.WriteLine("Thread: {0}, is running now", Thread.CurrentThread.ManagedThreadId);

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
        /// <summary>
        /// Adds a client connection to the list of connections
        /// </summary>
        /// <param name="connection"> the ClientConnection to be added</param>
        /// <returns> Boolean value representing wheter the adding was succesful</returns>
        private bool AddClientConnection(ClientConnection connection) {
            bool success = true;
            try {
                this.clientConnections.Add(connection);
                return success;
            } catch (Exception e) {
                success = false;
                return success;
            }
        }

        /// <summary>
        /// Removes a client connection from the list of connections
        /// </summary>
        /// <param name="connection">Connection to be removed</param>
        /// <returns>Boolean representing succcessful removal</returns>
        private bool RemoveClientConnection(ClientConnection connection) {
            bool success = true;
            try {
                this.clientConnections.Remove(connection);
                return success;
            } catch (Exception e) {
                //TODO: add logging remove client connection
                success = false;
                return success;
            }
        }


        //TODO: handle authorization Protocol server
        private void serverProtocolAuthorization(ClientConnection clientConnection) {
            //get socket
            Socket connectionSocket = clientConnection.getSocket();
            //Convert string to bytes
            char[] chars = new string("authorize").ToCharArray();
            byte[] bytesToSend = new byte[chars.Length];
            for (int i = 0; i<chars.Length; i++) {
                bytesToSend[i] = (byte) chars[i];
            }
            //Send protocol type to client
            connectionSocket.Send(bytesToSend);
            //receive token
            byte[] receiveBuffer = new byte[32];
            connectionSocket.Receive(receiveBuffer);
            string receivedToken = receiveBuffer.ToString();

            //TODO: Add Encryption to accessTokens

            //Create Token
            AccessToken token = new AccessToken(receivedToken);
            //Validate token
            bool validationResult = validateAccessToken(token);
            //Send success/failure to client
            if (validationResult) {
                //Send char y for success
                bytesToSend = new byte[] { (byte)'y' };
                connectionSocket.Send(bytesToSend);
            } else {
                //Send char n for negative
                bytesToSend = new byte[] { (byte)'n' };
                connectionSocket.Send(bytesToSend);
                //authorization failed, return
                return;
            }
            // if success request more info
            //TODO: Add extended profiling to authorization process
        }

        //TODO: handle ping protocol server
        private void serverProtocolPingServer(ClientConnection connection) {

        }

        //TODO: handle status protocol server
        private void serverProtocolStatus(ClientConnection connection) {

        }

        //TODO: handle sending commmand protocol
        private void serverProtocolSendCommand(ClientConnection connection) {

        }

        //TODO: Validate AccessToken
        private bool validateAccessToken(AccessToken token) {
            return true;
        }

    }
}
