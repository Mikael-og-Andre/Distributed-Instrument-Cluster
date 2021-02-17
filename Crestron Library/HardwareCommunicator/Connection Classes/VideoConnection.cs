using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Concurrent;


/// <summary>
/// Class holds information about a video connection that will be used to match it up with the pairing control socket
/// <author>Mikael Nilssen</author>
/// </summary>

namespace Instrument_Communicator_Library {
    public class VideoConnection<T> {

        private Socket socketConnection;    //Socket connection
        private Thread myThread;    //Thread the conenction will run on
        private InstrumentInformation info;     //Information about the device
        private ConcurrentQueue<T> outputQueue;     //Queue of items received by the connection

        public VideoConnection(Socket socketConnection, Thread thread) {
            this.socketConnection = socketConnection;
            this.myThread = thread;
        }

        public ConcurrentQueue<T> getOutputQueue() {
            return outputQueue;
        }

    }
}
