using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Concurrent;


namespace Instrument_Communicator_Library {

    /// <summary>
    /// Class holds information about a video connection that will be used to match it up with the pairing control socket
    /// <author>Mikael Nilssen</author>
    /// </summary>

    public class VideoConnection<T> {

        private Socket socketConnection;    //Socket connection
        private Thread myThread;    //Thread the conenction will run on
        private InstrumentInformation info;     //Information about the device
        private ConcurrentQueue<T> outputQueue;     //Queue of items received by the connection

        public VideoConnection(Socket socketConnection, Thread thread, InstrumentInformation info = null) {
            this.socketConnection = socketConnection;
            this.myThread = thread;
            this.outputQueue = new ConcurrentQueue<T>();
            this.info = info;
        }

        /// <summary>
        /// returns queue to store received objects in
        /// </summary>
        /// <returns>Concurrent queue</returns>
        public ConcurrentQueue<T> GetOutputQueue() {
            return outputQueue;
        }

        /// <summary>
        /// Returns socket
        /// </summary>
        /// <returns>Socket</returns>
        public Socket GetSocket() {
            return socketConnection;
        }

        /// <summary>
        /// Set instrument information
        /// </summary>
        /// <param name="instrumentInformation">Instrument Information object</param>
        public void SetInstrumentInformation(InstrumentInformation instrumentInformation) {
            this.info = instrumentInformation;
        }
        /// <summary>
        /// Get Instrument Information
        /// </summary>
        /// <param name="instrumentInformation">Instrument Information Object</param>
        public InstrumentInformation GetInstrumentInformation(InstrumentInformation instrumentInformation) {
            return this.info;
        }
    }
}
