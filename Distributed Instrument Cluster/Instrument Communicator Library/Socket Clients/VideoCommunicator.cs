using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using Instrument_Communicator_Library.Authorization;
using Instrument_Communicator_Library.Helper_Class;

namespace Instrument_Communicator_Library.Remote_Device_side_Communicators {

    /// <summary>
    /// Represents a socket line from a device to the server, intended to send video
    /// <author>Mikael Nilssen</author>
    /// </summary>
    public class VideoCommunicator : CommunicatorBase{

		/// <summary>
		/// queue of inputs meant to be sent to server
		/// </summary>
        private ConcurrentQueue<VideoFrame> inputQueue;

        public VideoCommunicator(string ip, int port, InstrumentInformation informationAboutClient, AccessToken accessToken, CancellationToken cancellationToken) : base(ip, port, informationAboutClient, accessToken, cancellationToken) {
            //initialize queue
            inputQueue = new ConcurrentQueue<VideoFrame>();
        }

        /// <summary>
        /// Handles the protocols after the socket has been connected
        /// </summary>
        /// <param name="connectionSocket"></param>
        protected override void handleConnected(Socket connectionSocket) {
            //wait for signal to start instrument detailing
            string response = NetworkingOperations.receiveStringWithSocket(connectionSocket);
            if (!response.ToLower().Equals("y")) {
                
            }
			//Send Information about device
            NetworkingOperations.sendStringWithSocket(information.Name, connectionSocket);
            NetworkingOperations.sendStringWithSocket(information.Location, connectionSocket);
            NetworkingOperations.sendStringWithSocket(information.Type, connectionSocket);

            //While not canceled push from queue to socket
            while (!communicatorCancellationToken.IsCancellationRequested) {
                //get input form queue
                bool hasInput = inputQueue.TryDequeue(out VideoFrame frame);
                if (!hasInput) continue;

                NetworkingOperations.sendObjectWithSocket(frame, connectionSocket);
            }
        }

        /// <summary>
        /// Get the concurrent Queue where you can enqueue frame to push them to the server
        /// </summary>
        /// <returns></returns>
        public ConcurrentQueue<VideoFrame> GetInputQueue() {
            return inputQueue;
        }
    }
}