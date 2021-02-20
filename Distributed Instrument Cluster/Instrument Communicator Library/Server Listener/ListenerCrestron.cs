using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Instrument_Communicator_Library.Server_Listener {

    public class ListenerCrestron : ListenerBase {
        private int timeToWait;     //Time to wait between pings
        private int timeToSleep;      // Time to sleep after nothing happens
        private List<CrestronConnection> listCrestronConnections;   //List of crestron Connections

        public ListenerCrestron(IPEndPoint ipEndPoint, int pingWaitTime = 1000 * 60, int sleepTime = 100, int maxConnections = 30, int maxPendingConnections = 30) : base(ipEndPoint, maxConnections, maxPendingConnections) {
            this.timeToWait = pingWaitTime;
            this.timeToSleep = sleepTime;
            this.listCrestronConnections = new List<CrestronConnection>();
        }

        /// <summary>
        /// Function to handle the new incoming connection on a new thread
        /// <param name="socket"></param>
        /// <param name="thread"></param>
        /// </summary>
        protected override object CreateConnectionType(Socket socket, Thread thread) {
            return new CrestronConnection(socket, thread);
        }

        /// <summary>
        /// Function for specifying specific type of ConnectionBase child the class should be returning and handing over to the HandleIncomingConnection
        /// </summary>
        protected override void HandleIncomingConnection(object obj) {
            CrestronConnection clientConnection;
            try {
                //cast input object to Client Connection
                clientConnection = (CrestronConnection)obj;
            } catch (InvalidCastException) {
                throw;
            }
            Console.WriteLine("SERVER - a Client Has Connected to Thread: {0}, thread {0} is running now", Thread.CurrentThread.ManagedThreadId);

            //add a connection to the list of connections
            AddClientConnection(clientConnection);

            //Do authorization process
            ServerProtocolAuthorization(clientConnection);

            ConcurrentQueue<Message> inputQueue = clientConnection.getInputQueue();   //Get reference to the queue of inputs intended to send to the client
            ConcurrentQueue<Message> outputQueue = clientConnection.getOutputQueue();     //Get reference to the queue of things received by the client

            //Setup stopwatch
            Stopwatch stopwatch = new Stopwatch();
            //Start stopwatch
            stopwatch.Start();

            while (!listenerCancellationToken.IsCancellationRequested) {
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
                //If queue has message check what type it is, and parse protocol type
                else if (hasValue) {
                    // check first message in queue, set protocol to use to the protocol of that message
                    protocolOption messageOption = msg.getProtocol();
                    currentMode = messageOption;
                }
                //if queue is empty and time since last ping isn't big, sleep for an amount of time
                else
                {
                    //Was empty and didn't need ping, so restart loop after short sleep
                    Thread.Sleep(timeToSleep);
                    continue;
                }

                //preform protocol corresponding to the current mode variable
                switch (currentMode) {
                    case protocolOption.ping:
                        //Preform ping protocol
                        this.ServerProtocolPing(clientConnection);
                        break;

                    case protocolOption.message:
                        //preform send protocol
                        this.ServerProtocolMessage(clientConnection);
                        break;

                    case protocolOption.status:
                        this.ServerProtocolStatus(clientConnection);
                        break;

                    case protocolOption.authorize:
                        this.ServerProtocolAuthorization(clientConnection);
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

        /// <summary>
        /// Adds a client connection to the list of connections
        /// </summary>
        /// <param name="connection"> the ClientConnection to be added</param>
        /// <returns> Boolean value representing whether the adding was successful</returns>
        private void AddClientConnection(CrestronConnection connection) {
            try
            {
                //Lock the non thread-safe list, and then add object
                lock (listCrestronConnections) {
                    this.listCrestronConnections.Add(connection);
                }

                return;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// Removes a client connection from the list of connections
        /// </summary>
        /// <param name="connection">Connection to be removed</param>
        /// <returns>Boolean representing successful removal</returns>
        private bool RemoveClientConnection(CrestronConnection connection)
        {
            //lock the non thread-safe list and then remove object
            lock (listCrestronConnections) {
                return this.listCrestronConnections.Remove(connection);
            }
        }

        /// <summary>
        /// Starts predetermined sequence eof socket operations used to authorize a remote device
        /// </summary>
        /// <param name="clientConnection">Client Connection representing The current Connection</param>
        private void ServerProtocolAuthorization(CrestronConnection clientConnection) {
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
            bool validationResult = ValidateAccessToken(token);
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
            }
        }

        /// <summary>
        /// Send protocol type PING to client and receives awnser
        /// </summary>
        /// <param name="clientConnection">Connected and authorized socket</param>
        private void ServerProtocolPing(CrestronConnection clientConnection) {
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
            if (receiveChars.Length > 0) {
                if ('y' == (char)receiveChars[0]) {
                    //Successful ping
                    Console.WriteLine("SERVER - Client Thread {0} says: Ping successful", Thread.CurrentThread.ManagedThreadId);
                } else {
                    //failed ping, stop connection
                    Console.WriteLine("SERVER - Client Thread {0} says: Ping failed, received wrong response", Thread.CurrentThread.ManagedThreadId);
                    clientConnection.setIsConnectionActive(false);
                }
            } else {
                //failed ping, stop connection
                Console.WriteLine("SERVER - Client Thread {0} says: Ping failed, received empty response", Thread.CurrentThread.ManagedThreadId);
                clientConnection.setIsConnectionActive(false);
            }
        }

        //TODO: handle status protocol server
        private void ServerProtocolStatus(CrestronConnection clientConnection) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sends an array of strings from the input queue in the client connection
        /// </summary>
        /// <param name="clientConnection">Client Connection Object</param>
        private void ServerProtocolMessage(CrestronConnection clientConnection) {
            int bufferSize = 128;
            //Get refrence to the queue
            ConcurrentQueue<Message> inputQueue = clientConnection.getInputQueue();
            //Get Socket
            Socket connectionSocket = clientConnection.getSocket();
            //Byte buffer decleration
            byte[] bytesToSend = new byte[bufferSize];
            try {
                //extract message from queue
                bool isSuccess = inputQueue.TryDequeue(out var messageToSend);
                Message msg = (Message) messageToSend;
                //Check if success and start sending messages
                if (isSuccess) {
                    //Say protocol type to client
                    //Convert string to bytes
                    string encodingTarget = protocolOption.message.ToString();
                    Encoding.ASCII.GetBytes(encodingTarget, 0, encodingTarget.Length, bytesToSend, 0);
                    //Send message string to client
                    connectionSocket.Send(bytesToSend, 32, SocketFlags.None);

                    //Get string array from message object
                    string[] stringArray = msg.getMessageArray();

                    //Send all strings
                    foreach (string s in stringArray) {
                        //If s is "end" skip to avoid premature ending
                        if (s.Equals("end")) {
                            continue;
                        }
                        Console.WriteLine("SERVER - Thread {0} is sending " + s + " to the client", Thread.CurrentThread.ManagedThreadId);
                        //clear byte buffer
                        encodingTarget = s;
                        bytesToSend = new byte[bufferSize];
                        //Write bytes to the bytes to send buffer
                        Encoding.ASCII.GetBytes(encodingTarget, 0, encodingTarget.Length, bytesToSend, 0);
                        //Send the 32 bytes in the bytesToSend buffer to the client
                        connectionSocket.Send(bytesToSend, bufferSize, SocketFlags.None);
                    }
                    //Send end signal to client, singling no more strings are coming
                    encodingTarget = "end";
                    bytesToSend = new byte[bufferSize];
                    //Write bytes to the bytes to send buffer
                    Encoding.ASCII.GetBytes(encodingTarget, 0, encodingTarget.Length, bytesToSend, 0);
                    //Send 32 bytes to client
                    connectionSocket.Send(bytesToSend, bufferSize, SocketFlags.None);
                } else {
                    Console.WriteLine("SERVER - Crestron Listener Message queue was empty when trying to  ");
                }
            } catch (Exception ex) {
                throw ex;
                //return;
            }
        }

        /// <summary>
        /// Validates an accessToken
        /// </summary>
        /// <param name="token">Access token</param>
        /// <returns>boolean representing a valid access token if true</returns>
        private bool ValidateAccessToken(AccessToken token) {
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

        /// <summary>
        /// Get the list of connected clients
        /// </summary>
        /// <returns>List of Client Connections</returns>
        public List<CrestronConnection> GetCrestronConnectionList()
        {
            lock (listCrestronConnections)
            {
                return this.listCrestronConnections;
            }
        }
    }
}