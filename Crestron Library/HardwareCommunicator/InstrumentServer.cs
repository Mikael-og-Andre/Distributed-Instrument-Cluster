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

namespace Instrument_Communicator_Library {

    public class InstrumentServer {

        private int maxConnections; //Maximum number of connections for the Pool
        private int maxPendingConnections;  //Backlog size of Listening socket
        private int numConnections = 0; //Connected Sockets
        public bool isServerRunning { get; private set; } //Should the server listen for more connection
        private Socket listenSocket;    //Socket for accepting incoming connections
        private IPEndPoint ipEndPoint { get; set; }     //Host Info
        private List<ClientConnection> clientConnectionList;   //All connected clients

        private int timeToWait = 1000*60;         //Time to wait between Pings in millis in the main loop
        private int timeTosleep = 1000;         //Time to sleep before checking for new commands in the main loop



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
                Console.WriteLine("SERVER - Main Thread {0} Says: Waiting For new Socket Connection...", Thread.CurrentThread.ManagedThreadId);
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
                    newSocket.Close();
                }
            }
        }

        /// <summary>
        /// Stops server from looping and clears socket
        /// </summary>
        public void StopServer() {
            isServerRunning = false;
            listenSocket.Disconnect(true);
        }

        /// <summary>
        /// Represents a communication thread that handles all communication with a single connected client
        /// </summary>
        /// <param name="obj"> represents a ClientConnection object. in order to be used as a parameraizedThread, it needs to be casted</param>
        private void ThreadRunProtocols(object obj) {
            ClientConnection clientConnection;
            try {
                //cast input object to Client Connection
                clientConnection = (ClientConnection)obj;
            } catch (InvalidCastException e) {
                
                throw new InvalidCastException("Could not cast input object to ClientConnection in method ThreadPortocol, Class InstrumentServer");
            }
            Console.WriteLine("SERVER - a Client Has Connected to Thread: {0}, thread {0} is running now", Thread.CurrentThread.ManagedThreadId);

            //add a connection to the list of connections
            AddClientConnection(clientConnection);

            //Do authorization process
            serverProtocolAuthorization(clientConnection);

            ConcurrentQueue<Message> inputQueue = clientConnection.getInputQueue();   //Get reference to the queue of inputs intended to send to the client
            ConcurrentQueue<Message> outputQueue = clientConnection.getOutputQueue();     //Get reference to the queue of things received by the client

            //Setup stopwatch
            Stopwatch stopwatch = new Stopwatch();
            //Start stopwatch
            stopwatch.Start();

            while (clientConnection.isConnectionActive()) {
                //Variable representing protocol to use;
                protocolOption currentMode;
                Message message;
                bool hasValue = inputQueue.TryPeek(out message);
                Message msg = message;
                //Check what action to take
                //If queue is empty and time since last ping is greater than timeToWait, ping
                if ((!hasValue) && (stopwatch.ElapsedMilliseconds > timeToWait)) {
                    //stop stopwatch
                    stopwatch.Stop();
                    //Set mode to ping
                    currentMode = protocolOption.ping;
                }
                //If queue has message check what type it is, and parse protcol type
                else if (hasValue) {
                    // check first message in queue, set protocol to use to the protocol of that message
                    protocolOption messageOption = msg.getProtocol();
                    currentMode = messageOption;
                }
                //if queue is empty and time since last ping isnt big, sleep for an amount of time
                else if (!hasValue) {
                    //Was empty and didnt need ping, so restart loop after short sleep
                    Thread.Sleep(timeTosleep);
                    continue;
                } else {
                    //none of the above counted, just continue
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
                //Reset stopwatch
                stopwatch.Reset();
                stopwatch.Start();
            }
            RemoveClientConnection(clientConnection);
            //Stop stopwatch if ending i guess
            stopwatch.Stop();
            //get socket and disconnect
            Socket socket = clientConnection.getSocket();
            socket.Disconnect(false);
            //remove client from connections
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
            try {
                //Lock the non threadsafe list, and then add object
                lock (clientConnectionList) {
                    this.clientConnectionList.Add(connection);
                }
                return true;
            } catch (Exception ex) {
                return false;
            }
        }

        /// <summary>
        /// Removes a client connection from the list of connections
        /// </summary>
        /// <param name="connection">Connection to be removed</param>
        /// <returns>Boolean representing succcessful removal</returns>
        private bool RemoveClientConnection(ClientConnection connection) {
            try {
                //lock the non threadsafe list and then remove object
                lock (clientConnectionList) {
                    return this.clientConnectionList.Remove(connection);
                }
            } catch (Exception ex) {
                //TODO: add logging remove client connection
                return false;
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
                //authorization failed, set not clientConnection to not active and return
                clientConnection.setIsConnectionActive(false);
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
            if (receiveChars.Length>0) {
                if ('y'==(char)receiveChars[0]) {
                    //Succesful ping
                    Console.WriteLine("SERVER - Client Thread {0} says: Ping successful", Thread.CurrentThread.ManagedThreadId);
                    return;
                } else {
                    //failed ping, stop connection
                    Console.WriteLine("SERVER - Client Thread {0} says: Ping failed, received wrong response", Thread.CurrentThread.ManagedThreadId);
                    clientConnection.setIsConnectionActive(false);
                    return;
                } 
            } else {
                //failed ping, stop connection
                Console.WriteLine("SERVER - Client Thread {0} says: Ping failed, received empty response", Thread.CurrentThread.ManagedThreadId);
                clientConnection.setIsConnectionActive(false);
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
                        //clear byte buffer
                        encodingTarget = s;
                        bytesToSend = new byte[32];
                        //Write bytes to the bytes to send buffer
                        writtenBytes = Encoding.ASCII.GetBytes(encodingTarget, 0, encodingTarget.Length, bytesToSend, 0);
                        //Send the 32 bytes in the bytesToSend buffer to the client
                        connectionSocket.Send(bytesToSend,32,SocketFlags.None);
                    }
                    //Send end signal to client, singling no more strings are coming
                    encodingTarget = "end";
                    bytesToSend = new byte[32];
                    //Write bytes to the bytes to send buffer
                    writtenBytes = Encoding.ASCII.GetBytes(encodingTarget, 0, encodingTarget.Length, bytesToSend, 0);
                    //Send 32 bytes to client
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
            Console.WriteLine("SERVER - Thread {0} is now checking hash. Hash: " + hash, Thread.CurrentThread.ManagedThreadId);
            if (hash.Equals("access")) {
                Console.WriteLine("SERVER - Thread {0} is now authorized", Thread.CurrentThread.ManagedThreadId);
                return true;
            }
            Console.WriteLine("SERVER - Thread {0} Authorization Failed ", Thread.CurrentThread.ManagedThreadId);
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