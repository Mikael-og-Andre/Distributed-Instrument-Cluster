using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Concurrent;
using Instrument_Communicator_Library.Information_Classes;


namespace Instrument_Communicator_Library {

    /// <summary>
    /// Class holds information about a video connection that will be used to match it up with the pairing control socket
    /// <author>Mikael Nilssen</author>
    /// </summary>

    public class VideoConnection {

        private Socket socketConnection;            //Socket connection
        private Thread myThread;                     //Thread the conenction will run on
        private InstrumentInformation info;          //Information about the device
        private ConcurrentQueue<VideoFrame> outputQueue;     //Queue of items received by the connection
        public bool hasInstrument { get; set; } = false;

        public VideoConnection(Socket socketConnection, Thread thread, InstrumentInformation info = null) {
            this.socketConnection = socketConnection;
            this.myThread = thread;
            this.outputQueue = new ConcurrentQueue<VideoFrame>();
            if (info!=null) {
                hasInstrument = true;
            }
            this.info = info;
        }

        /// <summary>
        /// returns queue to store received objects in
        /// </summary>
        /// <returns>Concurrent queue</returns>
        public ConcurrentQueue<VideoFrame> GetOutputQueue() {
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
            this.hasInstrument = true;
        }
        /// <summary>
        /// Get the instrument information object
        /// </summary>
        /// <returns></returns>
        public InstrumentInformation GetInstrumentInformation() {
            if (hasInstrument) {
                return this.info;
            }

            return null;
        }
    }
}
