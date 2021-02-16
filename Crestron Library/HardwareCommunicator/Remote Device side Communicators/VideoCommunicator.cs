using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;
using System.Text;

namespace Instrument_Communicator_Library {
    public class VideoCommunicator<T> : Communicator {

        private ConcurrentQueue<T> inputQueue; //queue of inputs ment to be sent to server

        public VideoCommunicator(string ip, int port, InstrumentInformation informationAboutClient, AccessToken accessToken) : base(ip, port, informationAboutClient, accessToken) {
            //initialize queue
            inputQueue = new ConcurrentQueue<T>();
        }

        /// <summary>
        /// Handles the protocols after the socket has been connected
        /// </summary>
        /// <param name="connectionSocket"></param>
        protected override void handleConnected(Socket connectionSocket) {
            throw new NotImplementedException();
        }
    }
}
