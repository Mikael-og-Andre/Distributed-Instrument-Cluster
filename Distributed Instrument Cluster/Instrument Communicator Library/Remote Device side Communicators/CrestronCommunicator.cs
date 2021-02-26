using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using Instrument_Communicator_Library.Helper_Class;

namespace Instrument_Communicator_Library.Remote_Device_side_Communicators {

    /// <summary>
    /// Client for connecting and receiving commands from server unit to control a crestron Device
    /// <author>Mikael Nilssen</author>
    /// </summary>
    public class CrestronCommunicator : CommunicatorBase {
        private ConcurrentQueue<string> commandOutputQueue; //Queue representing commands received by receive protocol

        //Values for state control
        private bool isAuthorized;  //Boolean for wheter the authorization process is complete

        //TODO: add buffer size limts to queue

        public CrestronCommunicator(string ip, int port, InstrumentInformation informationAboutClient, AccessToken accessToken, CancellationToken cancellationToken) : base(ip, port, informationAboutClient, accessToken, cancellationToken) {
            this.commandOutputQueue = new ConcurrentQueue<string>();    //Init queue
        }

        /// <summary>
        /// Handles the conneceted device
        /// </summary>
        /// <param name="connectionSocket"></param>
        protected override void HandleConnected(Socket connectionSocket) {
            try {
                //check if the client is authorized,
                if (!isAuthorized) {
                    //receive the first authorization call from server
                    NetworkingOperations.ReceiveStringWithSocket(connectionSocket);
                    //Start Authorization
                    isAuthorized = ProtocolAuthorize(connectionSocket);
                }
                //Check if successfully authorized
                if (isAuthorized) {
                    Console.WriteLine("Thread {0} Client Authorization complete", Thread.CurrentThread.ManagedThreadId);
                    //Run main protocol Loop
                    while (!communicatorCancellationToken.IsCancellationRequested) {
                        //Read a protocol choice from the buffer and execute it
                        StartAProtocol(connectionSocket);
                    }
                }
            } catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Listens for selected protocol sent by server and preforms correct response protocol
        /// </summary>
        /// <param name="connectionSocket"> Socket Connection to server</param>
        private void StartAProtocol(Socket connectionSocket) {
            //Receive protocol type from server
            string extractedString=NetworkingOperations.ReceiveStringWithSocket(connectionSocket);
            //Parse Enum
            protocolOption option = (protocolOption)Enum.Parse(typeof(protocolOption), extractedString, true);
            Console.WriteLine("thread {0} Client says: " + "Received option " + option, Thread.CurrentThread.ManagedThreadId);
            //Select Protocol
            switch (option) {
                case protocolOption.ping:
                    ProtocolPing(connectionSocket);
                    break;

                case protocolOption.message:
                    ProtocolMessage(connectionSocket);
                    break;

                case protocolOption.status:
                    ProtocolStatus(connectionSocket);
                    break;

                case protocolOption.authorize:
                    ProtocolAuthorize(connectionSocket);
                    break;

                default:
                    break;
            }
        }

        #region Protocols

        /// <summary>
        /// Activates predetermined sequence of socket operations for authorizing the client device as trusted
        /// </summary>
        /// <param name="connectionSocket"></param>
        /// <returns>Boolean representing if the authorization was successful or not</returns>
        private bool ProtocolAuthorize(Socket connectionSocket) {
            try {
                //Create accessToken
                AccessToken accessToken = this.accessToken;
                string accessTokenHash = accessToken.getAccessString();
                //Send token
                NetworkingOperations.SendStringWithSocket(accessTokenHash,connectionSocket);

                //Receive Result
                string result = NetworkingOperations.ReceiveStringWithSocket(connectionSocket);
                //Check result
                if (result.ToLower().Equals("y")) {
                    Console.WriteLine("Thread {0} Authorization Successful", Thread.CurrentThread.ManagedThreadId);
                    //If returned y then do instrument detailing
                    
                } else {
                    Console.WriteLine("Thread {0} Authorization Failed", Thread.CurrentThread.ManagedThreadId);
                    // return false, representing a failed authorization
                    return false;
                }

                //Receive Y for started instrument detailing
                string response = NetworkingOperations.ReceiveStringWithSocket(connectionSocket);
                Console.WriteLine("Response was " + response);
                if (!response.ToLower().Equals("y")) {
                    return false;
                }
                NetworkingOperations.SendStringWithSocket(information.name, connectionSocket);
                NetworkingOperations.SendStringWithSocket(information.location, connectionSocket);
                NetworkingOperations.SendStringWithSocket(information.type, connectionSocket);

                //Receive Y for started instrument detailing
                string complete = NetworkingOperations.ReceiveStringWithSocket(connectionSocket);

                return true;
            } catch (Exception ex) {
                return false;
            }
        }

        /// <summary>
        /// Activates predetermined sequence of socket operations for a ping, to confirm both locations
        /// </summary>
        /// <param name="connectionSocket"> Authorized connection socket</param>
        private void ProtocolPing(Socket connectionSocket) {
            //Send simple byte to server
            NetworkingOperations.SendStringWithSocket("y",connectionSocket);
        }

        /// <summary>
        /// Activates predetermined sequence of socket operation for receiving an array of string from the server
        /// </summary>
        /// <param name="connectionSocket">Connected and authorized socket</param>
        private void ProtocolMessage(Socket connectionSocket) {
            //Loop boolean
            bool isAccepting = true;
            //Loop until end signal received by server
            while (isAccepting) {
                string received = NetworkingOperations.ReceiveStringWithSocket(connectionSocket);
                Console.WriteLine("Thread {0} message received " + received, Thread.CurrentThread.ManagedThreadId);
                //Check if end in messages
                if (received.ToLower().Equals("end")) {
                    //Set protocol to be over
                    isAccepting = false;
                    break;
                }
                //Add Command To Concurrent queue
                commandOutputQueue.Enqueue(received);
            }
        }

        /// <summary>
        /// Activates predetermined sequence of socket operations for sending the status to the server
        /// </summary>
        /// <param name="connectionSocket">Authorized connection socket</param>
        private void ProtocolStatus(Socket connectionSocket) {
            //TODO: Implement Status Protocol
            throw new NotImplementedException();
        }

        #endregion Protocols

        /// <summary>
        /// Returns a reference to queue of commands received by receive protocol in string format
        /// </summary>
        /// <returns>reference to Concurrent queue of type string</returns>
        public ConcurrentQueue<string> GetCommandOutputQueue() {
            return commandOutputQueue;
        }
    }
}