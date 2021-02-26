using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using Instrument_Communicator_Library.Helper_Class;

namespace Instrument_Communicator_Library.Remote_Device_side_Communicators {

    /// <summary>
    /// Represents a socket line from a device to the server, intended to send video
    /// <author>Mikael Nilssen</author>
    /// </summary>
    public class VideoCommunicator<T> : CommunicatorBase {
        private ConcurrentQueue<T> inputQueue; //queue of inputs meant to be sent to server

        public VideoCommunicator(string ip, int port, InstrumentInformation informationAboutClient, AccessToken accessToken, CancellationToken cancellationToken) : base(ip, port, informationAboutClient, accessToken, cancellationToken) {
            //initialize queue
            inputQueue = new ConcurrentQueue<T>();
        }

        /// <summary>
        /// Handles the protocols after the socket has been connected
        /// </summary>
        /// <param name="connectionSocket"></param>
        protected override void HandleConnected(Socket connectionSocket) {
            //wait for signal to start instrument detailing
            string response = NetworkingOperations.ReceiveStringWithSocket(connectionSocket);
            if (!response.ToLower().Equals("y")) {
                
            }
            NetworkingOperations.SendStringWithSocket(information.name, connectionSocket);
            NetworkingOperations.SendStringWithSocket(information.location, connectionSocket);
            NetworkingOperations.SendStringWithSocket(information.type, connectionSocket);

            //While not canceled push from queue to socket
            while (!communicatorCancellationToken.IsCancellationRequested) {
                //get input form queue
                bool hasInput = inputQueue.TryDequeue(out T objectFromQueue);
                if (!hasInput) continue;
                //Get the object
                T obj = objectFromQueue;

                NetworkingOperations.SendObjectWithSocket<T>(obj, connectionSocket);
            }
        }

        /// <summary>
        /// Get the concurrent Queue
        /// </summary>
        /// <returns></returns>
        public ConcurrentQueue<T> GetInputQueue() {
            return inputQueue;
        }
    }
}