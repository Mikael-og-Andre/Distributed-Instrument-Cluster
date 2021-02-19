﻿using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading;

/// <summary>
/// Client for connecting and recieving commands from server unit to control a crestron Device
/// <author>Mikael Nilssen</author>
/// </summary>

namespace Instrument_Communicator_Library {

    public class CrestronCommunicator : CommunicatorBase {
        private ConcurrentQueue<string> commandOutputQueue; //Queue representing commands received by receive protocol

        //Values for state control
        private bool isAuthorized;  //Boolean for wheter the authorization process is complete

        private int failedAuthorizationAttempts;   //Count of failed authorization attempts

        public CrestronCommunicator(string ip, int port, InstrumentInformation informationAboutClient, AccessToken accessToken) : base(ip, port, informationAboutClient, accessToken) {
            this.commandOutputQueue = new ConcurrentQueue<string>();    //Init queue
        }

        /// <summary>
        /// Handles the conneceted device
        /// </summary>
        /// <param name="connectionSocket"></param>
        protected override void handleConnected(Socket connectionSocket) {
            try {
                //check if the client is authorized,
                if (!isAuthorized) {
                    //receive the first authorization call from server
                    byte[] bufferReceive = new byte[32];
                    connectionSocket.Receive(bufferReceive);
                    //Start Authorization
                    isAuthorized = protocolAuthorize(connectionSocket);
                }
                //Check if successfully authorized
                if (isAuthorized) {
                    Console.WriteLine("Thread {0} Client Authorization complete", Thread.CurrentThread.ManagedThreadId);
                    //Run main protocol Loop
                    while (!communicatorCancellationToken.IsCancellationRequested) {
                        //Read a protocol choice from the buffer and exceute it
                        startAProtocol(connectionSocket);
                    }
                }
            } catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// Listens for selected protocol sent by server and preforms correct response protocol
        /// </summary>
        /// <param name="connectionSocket"> Socket Connection to server</param>
        /// <param name="bufferSize">Size of the receive buffer with deafult size 32 bytes. May need to be adjusted base on how big protocol names become</param>
        private void startAProtocol(Socket connectionSocket) {
            //Recieve protocol type from server
            byte[] receiveBuffer = new byte[32];
            int bytesReceived = connectionSocket.Receive(receiveBuffer, 32, SocketFlags.None);
            string extractedString = Encoding.ASCII.GetString(receiveBuffer, 0, 32);
            extractedString = extractedString.Trim('\0');
            //Parse Enum
            protocolOption option = (protocolOption)Enum.Parse(typeof(protocolOption), extractedString, true);
            Console.WriteLine("thread {0} Client says: " + "Received option " + option, Thread.CurrentThread.ManagedThreadId);
            //Select Protocol
            switch (option) {
                case protocolOption.ping:
                    protocolPing(connectionSocket);
                    break;

                case protocolOption.message:
                    protocolMessage(connectionSocket);
                    break;

                case protocolOption.status:
                    protocolStatus(connectionSocket);
                    break;

                case protocolOption.authorize:
                    protocolAuthorize(connectionSocket);
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
        private bool protocolAuthorize(Socket connectionSocket) {
            try {
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
                byte[] byteBuffer = new byte[32];
                connectionSocket.Receive(byteBuffer, SocketFlags.None);
                //Translate to char
                char result = (char)byteBuffer[0];
                //Check result
                if ((char)'y' == result) {
                    Console.WriteLine("Thread {0} Authorization Successful", Thread.CurrentThread.ManagedThreadId);
                    // return true, representing a successful authorization
                    return true;
                } else {
                    Console.WriteLine("Thread {0} Authorization Failed", Thread.CurrentThread.ManagedThreadId);
                    // return false, representing a failed authorization
                    return false;
                }
            } catch (Exception ex) {
                
                return false;
            }
        }

        /// <summary>
        /// Activates predetermined sequence of socket operations for a ping, to confirm both locations
        /// </summary>
        /// <param name="connectionSocket"> Authorized connection socket</param>
        private void protocolPing(Socket connectionSocket) {
            //Send simple byte to server
            byte[] sendBuffer = new byte[] { (byte)'y' };
            connectionSocket.Send(sendBuffer);
        }

        /// <summary>
        /// Activates predetermined sequence of socket operation for receiving an array of string from the server
        /// </summary>
        /// <param name="connectionSocket">Connected and authorized socket</param>
        private void protocolMessage(Socket connectionSocket) {
            int bufferSize = 128;
            //Loop boolean
            bool isAccepting = true;
            //Socket buffer
            byte[] receiveBuffer;
            //Loop until end signal received by server
            while (isAccepting) {
                //receive buffer of bufferSize bytes
                receiveBuffer = new byte[bufferSize];
                //Receive into the receive buffer from slot 0 to bufferSize
                int bytesReceived = connectionSocket.Receive(receiveBuffer, 0, bufferSize, SocketFlags.None);
                string received = Encoding.ASCII.GetString(receiveBuffer, 0, bytesReceived);
                //trim null bytes that were sent by the socket
                received = received.Trim('\0');
                Console.WriteLine("Thread {0} message received " + received, Thread.CurrentThread.ManagedThreadId);
                //Check if end in messages
                if (received.Equals("end")) {
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
        private void protocolStatus(Socket connectionSocket) {
            //TODO: Implement Status Protocol
            throw new NotImplementedException();
        }

        #endregion Protocols

        /// <summary>
        /// Returns a reference to queue of commands received by receive protocol in string format
        /// </summary>
        /// <returns>refrence to Concurrent queue of type string</returns>
        public ConcurrentQueue<string> getCommandOutputQueue() {
            return commandOutputQueue;
        }
    }
}