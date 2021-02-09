using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;

/// <summary>
/// Client for connecting and recieving commands from server unit
/// <author>Mikael Nilssen</author>
/// </summary>

namespace HardwareCommunicator {

    public class InstrumentClient {
        public string ip { get; private set; } //Ip address of target server
        public int port { get; private set; } //Port of target server
        private Socket connectionSocket;    //Connection to server
        private bool isClientRunning = true;       //Should the client
        private AccessToken accessToken;   // Authorization code to send to the server
        private ConcurrentQueue<string> commandOutputQueue; //Queue representing commands received by receive protocol

        public InstrumentClient(string ip, int port) {
            this.ip = ip;
            this.port = port;
            this.accessToken = new AccessToken("access");
        }

        /// <summary>
        /// 
        /// </summary>
        public void start() {

            try {
                // Create new socket
                connectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            catch (Exception e) {
                //TODO: add logging create new socket
                throw e;
            }
            // Loop whilst the client is supposed to run
            while (isClientRunning) {

                // Try to connect
                bool isConneceted = attemptConnection(connectionSocket);

                if (isConneceted) {
                    //handle the connection
                    handleConnected(connectionSocket);

                }
            }
        }

        /// <summary>
        /// Attempts to connect to the given host and ip
        /// </summary>
        /// <param name="connectionSocket"> unconnected Soccket</param>
        /// <returns> boolean representing succesful conncetion</returns>
        private bool attemptConnection(Socket connectionSocket) {

            try {
                //Try Connecting to server
                connectionSocket.Connect(ip,port);
                Console.WriteLine("Client Connected");
                return connectionSocket.Connected;

            } catch (Exception e) {
                //TODO: add Logging attempt conncetion
                //return false to represent failed connection
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionSocket"></param>
        private void handleConnected(Socket connectionSocket) {
            //receive the first authorization call from server
            byte[] bufferReceive = new byte[32];
            connectionSocket.Receive(bufferReceive);
            //Start Authorization
            bool isAuthorized = protocolAuthorize(connectionSocket);

            //Check if successfully authorized
            if (isAuthorized) {
                Console.WriteLine("Client Authorization complete");
                //Run main protocol Loop
                while (isClientRunning) {
                    //Read a protocol choice from the buffer and exceute it
                    startAProtocol(connectionSocket);
                }
            } else {
                Console.WriteLine("Client Authorization failed");
                //TODO: Handle failed autorization event when connection
            }
        }

        /// <summary>
        /// Listens for selected protocol sent by server and preforms correct response protocol
        /// </summary>
        /// <param name="connectionSocket"> Socket Connection to server</param>
        /// <param name="bufferSize">Size of the receive buffer with deafult size 32 bytes. May need to be adjusted base on how big protocol names become</param>
        private void startAProtocol(Socket connectionSocket, int bufferSize = 32) {
            //Recieve buffer
            byte[] receiveBuffer = new byte[bufferSize];
            //Recieve from server
            connectionSocket.Receive(receiveBuffer);
            //Extract string from buffer
            string extractedString = receiveBuffer.ToString();
            Console.WriteLine("Extracted string from server: "+extractedString);

            //Select Protocol
            switch (extractedString) {

                case "ping":
                    protocolPing(connectionSocket);
                    break;
                case "receive":
                    protocolReceive(connectionSocket);
                    break;
                case "status":
                    protocolStatus(connectionSocket);
                    break;
                case "authorize":
                    protocolAuthorize(connectionSocket);
                    break;

                default:
                    break;
            }


        }

        /// <summary>
        /// Activates predetermined sequence of socket operations for authorizing the client device as trusted
        /// </summary>
        /// <param name="connectionSocket"></param>
        /// <returns>Boolean representing if the authorization was successful or not</returns>
        private bool protocolAuthorize(Socket connectionSocket) {
            //Create accessToken
            AccessToken accessToken = this.accessToken;
            string accessTokenHash = accessToken.getAccessString();
            //Create byte array
            char[] chars = accessTokenHash.ToCharArray();
            byte[] bytesToSend = new byte[chars.Length];
            for (int i = 0; i < chars.Length; i++) {
                bytesToSend[i] = (byte)chars[i];
            }

            //TODO: Add Encryption to accessToken

            //Send Token
            connectionSocket.Send(bytesToSend);
            //Receive Result
            byte[] byteBuffer = new byte[1];
            connectionSocket.Receive(byteBuffer);
            //Translate to char
            char result = (char) byteBuffer[0];
            //Check result
            if (result.Equals('y')) {
                Console.WriteLine("Authorization Successful");
                // return true, representing a successful authorization
                return true;
            } else {
                Console.WriteLine("Authorization Failed");
                // return false, representing a failed authorization
                return false;
            }

        }

        /// <summary>
        /// Activates predetermined sequence of socket operations for a ping, to confirm both locations
        /// </summary>
        /// <param name="connectionSocket"> Authorized connection socket</param>
        private void protocolPing(Socket connectionSocket) {
            //Send simple byte to server
            byte[] sendBuffer = new byte[] { (byte)'y'};
            connectionSocket.Send(sendBuffer);
        }

        /// <summary>
        /// Activates predetermined sequence of socket operations for receiving a command and then execute it.
        /// </summary>
        /// <param name="connectionSocket">Authorized connection socket</param>
        private void protocolReceive(Socket connectionSocket) {

            //Receive buffer
            byte[] bufferReceive = new byte[32];
            //Receive from socket
            connectionSocket.Receive(bufferReceive);
            //convert to string
            string command = bufferReceive.ToString();

            //Add Command To Concurrent queue
            commandOutputQueue.Enqueue(command);
        }

        /// <summary>
        /// Activates predetermined sequence of socket operations for sending the status to the server
        /// </summary>
        /// <param name="connectionSocket">Authorized connection socket</param>
        private void protocolStatus(Socket connectionSocket) {
            //TODO: Implement Status Protocol
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a reference to queue of commands received by receive protocol in string format
        /// </summary>
        /// <returns>refrence to Concurrent queue of type string</returns>
        public ref ConcurrentQueue<string> getCommandOutputQueue() {
            return ref commandOutputQueue;
        }

    }
}
