using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

/// <summary>
/// Server to be run for listening to incoming device connections and sending commands and data to them
/// <author>Mikael Nilssen</author>
/// </summary>

namespace InstrumentCommunicator {

    public class InstrumentServer {
        private int maxConnections; //Maximum number of connections for the Pool
        private int maxPendingConnections;  //Backlog size of Listening socket
        private int numConnections = 0; //Connected Sockets
        public bool isServerRunning { get; private set; } //Should the server listen for more connection
        private Socket listenSocket;    //Socket for accepting incoming connections
        private IPEndPoint ipEndPoint { get; set; }     //Host Info
        private List<ClientConnection> clientConnectionList;   //All connected clients

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
            clientConnectionList = new List<ClientConnection>();  //Initialize empty list for tracking client connections
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

            //Accepts Connections
            while (isServerRunning) {
                //Accept an incoming connection
                Console.WriteLine("Main Thread {0} Says: Waiting For new Socket Connection...", Thread.CurrentThread.ManagedThreadId);
                Socket newSocket = listenSocket.Accept();
                //Increment Current Connections
                incrementConnectionNumber();
                //Creates a new Thread to run a client communication on
                Thread newClientThread = new Thread(ThreadRunProtocols);
                newClientThread.IsBackground = true;

                //Create a client connection object representing the connection
                ClientConnection newClientConnection = new ClientConnection(newSocket, newClientThread);

                try {
                    //Pass in ClientConnection and start a new thread ThreadProtocol
                    newClientThread.Start(newClientConnection);
                } catch (Exception e) {
                    //Lower Connection number
                    decrementConnectionNumber();
                    newSocket.Disconnect(false);
                    newSocket.Dispose();
                    newSocket.Close();
                }
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
        /// Represents a communication thread that handles all communication with a single connected client
        /// </summary>
        /// <param name="obj"> represents a ClientConnection object. in order to be used as a parameraizedThread, it needs to be casted</param>
        public void ThreadRunProtocols(object obj) {
            ClientConnection clientConnection;
            try {
                //cast input object to Client Connection
                clientConnection = (ClientConnection)obj;
            } catch (InvalidCastException) {
                throw new InvalidCastException("Could not cast input object to ClientConnection in method ThreadPortocol, Class InstrumentServer");
            }
            Console.WriteLine("Client Connection Thread: {0}, a new client thread is running now", Thread.CurrentThread.ManagedThreadId);

            AddClientConnection(clientConnection);

            //Do authorization process
            serverProtocolAuthorization(clientConnection);

            //Check if connection is active
            bool isActive = clientConnection.isConnectionActive();

            ConcurrentQueue<Message> inputQueue = clientConnection.getInputQueue();   //Get reference to the queue of inputs intended to send to the client
            ConcurrentQueue<Message> outputQueue = clientConnection.getOutputQueue();     //Get reference to the queue of things received by the client

            //Setup stopwatch
            Stopwatch stopwatch = new Stopwatch();

            //Time to wait between Pings in millis
            int timeToWait = 10000;
            //Time to sleep before checking for new commands
            int timeTosleep = 1000;
            //Start stopwatch
            stopwatch.Start();

            while (isActive) {
                //Variable representing protocol to use;
                protocolOption currentMode;
                Message message;
                bool hasValue = inputQueue.TryPeek(out message);
                Message msg = message;
                //Check what action to take
                //If queue is empty and time since last ping is greater than timeToWait, ping
                if ((hasValue) && (stopwatch.ElapsedMilliseconds > timeToWait)) {
                    //restart stopwatch
                    stopwatch.Restart();
                    //Set mode to ping
                    currentMode = protocolOption.ping;
                }
                //If queue has message check what type it is, and parse protcol type
                else if (hasValue) {
                    // check first message in queue, set protocol to use to the protocol of that message
                    Console.WriteLine("Thread {0} says: " + "peek result was {1}", Thread.CurrentThread.ManagedThreadId, hasValue);

                    if (hasValue) {
                        protocolOption messageOption = msg.getProtocol();
                        currentMode = messageOption;
                    } else {
                        continue;
                    }
                }
                //if queue is empty and time since last ping isnt big, sleep for an amount of time
                else if (!hasValue) {
                    Thread.Sleep(timeTosleep);
                    //Was empty and didnt need ping, so restart loop
                    Console.WriteLine("Thread {0} says: " + "Queue is empty, sleeping", Thread.CurrentThread.ManagedThreadId);
                    continue;
                } else {
                    //none of the above counted, just continue
                    Console.WriteLine("Thread {0} says: " + "Do nothing has been chosen", Thread.CurrentThread.ManagedThreadId);
                    continue;
                }

                //preform protocol corresponding to the current mode variable
                switch (currentMode) {
                    case protocolOption.ping:
                        //Preform ping protocol
                        this.serverProtocolPing(clientConnection);
                        break;

                    case protocolOption.message:
                        //preform send protocol
                        this.serverProtocolMessage(clientConnection);
                        break;

                    case protocolOption.status:
                        this.serverProtocolStatus(clientConnection);
                        break;

                    case protocolOption.authorize:
                        this.serverProtocolAuthorization(clientConnection);
                        break;

                    default:
                        break;
                }
            }
            //Stop stopwatch if ending i guess
            stopwatch.Stop();
            //Remove client from list of clients
            RemoveClientConnection(clientConnection);
        }

        #region Helper Functions

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
            if (numConnections > 0) {
                numConnections--;
            }
        }

        /// <summary>
        /// Adds a client connection to the list of connections
        /// </summary>
        /// <param name="connection"> the ClientConnection to be added</param>
        /// <returns> Boolean value representing wheter the adding was succesful</returns>
        private bool AddClientConnection(ClientConnection connection) {
            bool success = true;
            try {
                this.clientConnectionList.Add(connection);
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
                this.clientConnectionList.Remove(connection);
                return success;
            } catch (Exception e) {
                //TODO: add logging remove client connection
                success = false;
                return success;
            }
        }

        #endregion Helper Functions

        #region Protocols

        /// <summary>
        /// Starts predermined sequenc eof socket operations used to authorize a remote device
        /// </summary>
        /// <param name="clientConnection">Client Connection representing The current Connection</param>
        private void serverProtocolAuthorization(ClientConnection clientConnection) {
            //get socket
            Socket connectionSocket = clientConnection.getSocket();
            //Convert string to bytes
            byte[] bytesToSend = Encoding.ASCII.GetBytes(protocolOption.authorize.ToString());
            //Send protocol type to client
            connectionSocket.Send(bytesToSend);
            //receive token
            byte[] receiveBuffer = new byte[32];
            int bytesReceived = connectionSocket.Receive(receiveBuffer);
            string receivedToken = Encoding.ASCII.GetString(receiveBuffer, 0, bytesReceived);

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
                //Add access Token to clientConnection
                clientConnection.setAccessToken(token);
            } else {
                //Send char n for negative
                bytesToSend = new byte[] { (byte)'n' };
                connectionSocket.Send(bytesToSend);
                //authorization failed, return
                return;
            }
            // If successful request more info
            //TODO: Add extended profiling to authorization process
        }

        /// <summary>
        /// Send protocol type PING to client and receives awnser
        /// </summary>
        /// <param name="clientConnection">Connected and authorized socket</param>
        private void serverProtocolPing(ClientConnection clientConnection) {
            //Send protocol type "ping" to client
            //get socket
            Socket connectionSocket = clientConnection.getSocket();
            //Convert string to bytes
            byte[] bytesToSend = Encoding.ASCII.GetBytes(protocolOption.ping.ToString());
            //Send protocol type to client
            connectionSocket.Send(bytesToSend);
            //Receive awnser byte, or cancel connection
            byte[] receiveBuffer = new byte[8];
            int bytesReceived = connectionSocket.Receive(receiveBuffer);
            char[] receiveChars = Encoding.ASCII.GetChars(receiveBuffer, 0, bytesReceived);
            //Check if correct Response
            if (receiveChars[0].Equals('y')) {
                //Succesful ping
                Console.WriteLine("Client Connection Thread {0} says: Ping successful", Thread.CurrentThread.ManagedThreadId);
                return;
            } else {
                //failed ping, maybe do something
                return;
            }
        }

        //TODO: handle status protocol server
        private void serverProtocolStatus(ClientConnection clientConnection) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sends an array of strings from the input queue in the client connection
        /// </summary>
        /// <param name="clientConnection">Client Connection Object</param>
        private void serverProtocolMessage(ClientConnection clientConnection) {
            //Get refrence to the queue
            ConcurrentQueue<Message> inputQueue = clientConnection.getInputQueue();
            //Get Socket
            Socket connectionSocket = clientConnection.getSocket();
            //Byte buffer decleration
            byte[] bytesToSend = new byte[32];
            try {
                //extract message from queue
                Message messageToSend;
                bool isSuccess = inputQueue.TryDequeue(out messageToSend);
                Message msg = messageToSend;
                //Check if success and start sending messages
                if (isSuccess) {
                    //Say protocol type to client
                    //Convert string to bytes
                    string encodingTarget = protocolOption.message.ToString();
                    int writtenBytes = Encoding.ASCII.GetBytes(encodingTarget,0,encodingTarget.Length, bytesToSend,0);
                    //Send message string to client
                    connectionSocket.Send(bytesToSend, 32,SocketFlags.None);

                    //Get string array from message object
                    string[] stringArray = msg.getMessageArray();

                    //Send all strings
                    foreach (string s in stringArray) {
                        //If s is "end" skip to avoid premature ending
                        if (s.Equals("end")) {
                            continue;
                        }
                        //Add bytes to byte buffer
                        encodingTarget = s;
                        bytesToSend = new byte[32];
                        writtenBytes = Encoding.ASCII.GetBytes(encodingTarget, 0, encodingTarget.Length, bytesToSend, 0);
                        //Send message string to client
                        connectionSocket.Send(bytesToSend,32,SocketFlags.None);
                    }

                    encodingTarget = "end";
                    bytesToSend = new byte[32];
                    writtenBytes = Encoding.ASCII.GetBytes(encodingTarget, 0, encodingTarget.Length, bytesToSend, 0);
                    //Send message string to client
                    connectionSocket.Send(bytesToSend, 32, SocketFlags.None);
                }
            } catch (Exception e) {
            }
        }

        #endregion Protocols

        #region Security and Access Token

        /// <summary>
        /// Validates an accessToken
        /// </summary>
        /// <param name="token">Access token</param>
        /// <returns>boolean representing a valid access token if true</returns>
        private bool validateAccessToken(AccessToken token) {
            //TODO: add Database checking
            string hash = token.getAccessString();
            Console.WriteLine("Checking Hash: " + hash);
            if (hash.Equals("access")) {
                return true;
            }
            return false;
        }

        #endregion Security and Access Token

        /// <summary>
        /// Get the list of connected clients
        /// </summary>
        /// <returns>List of Client Connections</returns>
        public List<ClientConnection> getClientConnections() {
            return this.clientConnectionList;
        }
    }
}