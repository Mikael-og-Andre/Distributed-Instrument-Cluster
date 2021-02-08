using System;
using System.Net.Sockets;
using System.Threading;

/// <summary>
/// Client for connecting and recieving commands from server unit
/// @Author Mikael Nilssen
/// </summary>

namespace HardwareCommunicator {

    public class InstrumentClient {
        public string ip { get; private set; } //Ip address of target server
        public int port { get; private set; } //Port of target server

        private Socket connectionSocket;    //Connection to server
        private bool isClientRunning = true;       //Should the client

        private string authorizationHash;   // Authorization code to send to the server

        public InstrumentClient(string ip, int port) {
            this.ip = ip;
            this.port = port;
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
        /// 
        /// </summary>
        /// <param name="connectionSocket"></param>
        /// <returns></returns>
        private bool attemptConnection(Socket connectionSocket) {

            try {
                //Try Connecting to server
                connectionSocket.Connect(ip,port);
                return connectionSocket.Connected;

            } catch (Exception e) {
                //return false to represent failed connection
                return false;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionSocket"></param>
        private void handleConnected(Socket connectionSocket) {

            //Do authorization process
            bool isAuthorized = protocolAuthorize(connectionSocket);

            if (isAuthorized) {
                //Run main protocol Loop
                while (isClientRunning) {
                    //Read a protocol choice from the buffer and exceute it
                    startAProtocol(connectionSocket, 32);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionSocket"></param>
        /// <param name="bufferSize"></param>
        private void startAProtocol(Socket connectionSocket, int bufferSize) {
            //Recieve buffer
            byte[] receiveBuffer = new byte[bufferSize];
            //Recieve from server
            connectionSocket.Receive(receiveBuffer);
            //Extract string from buffer
            string extractedString = receiveBuffer.ToString();
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
                case "reauthorize":
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
            throw new NotImplementedException();
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

            //Preform Command
            Command newCommand = new Command();
            newCommand.sendToCrestron(command);
        }

        /// <summary>
        /// Activates predetermined sequence of socket operations for sending the status to the server
        /// </summary>
        /// <param name="connectionSocket">Authorized connection socket</param>
        private void protocolStatus(Socket connectionSocket) {
            throw new NotImplementedException();
        }


    }
}
