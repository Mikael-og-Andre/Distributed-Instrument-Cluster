using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;



namespace Instrument_Communicator_Library {
    /// <summary>
    /// Class that represents a Crestron connection to the server unit.
    /// <author>Mikael Nilssen</author>
    /// </summary>
    public class CrestronConnection {
        private Socket socket { get; set; }     //Socket of the client Connection
        private ConcurrentQueue<Message> concurrentQueueInput;  // Concurrent queue for Messages to send to the client
        private ConcurrentQueue<Message> concurrentQueueOutput;  //Concurrent queue for Messages recieved by the client
        private Thread homeThread;                                // The thread the connection is running on
        private AccessToken accessToken = null;                 // Token representing a valid connection to the server
        private bool isActive = true;                           //Is the connection running
        private InstrumentInformation info;                     //Information about remote device
        public bool hasInstrument { get; private set; } = false;

        public CrestronConnection(Socket socket, Thread thread) {
            this.socket = socket;
            this.homeThread = thread;
            //Init queues
            concurrentQueueInput = new ConcurrentQueue<Message>();
            concurrentQueueOutput = new ConcurrentQueue<Message>();
        }

        /// <summary>
        /// get the socket representing the client connection
        /// </summary>
        /// <returns>A socket</returns>
        public Socket GetSocket() {
            return socket;
        }

        /// <summary>
        /// Sets the accessToken For the conncetion
        /// </summary>
        /// <param name="token"></param>
        public void SetAccessToken(AccessToken token) {
            this.accessToken = token;
        }

        /// <summary>
        /// Get the accessToken set by the authorization process
        /// </summary>
        /// <returns>Returns accessToken</returns>
        public AccessToken GetAccessToken() {
            if (this.accessToken != null) {
                return this.accessToken;
            } else {
                throw new NullReferenceException("AccessToken as not been set yet");
            }
        }

        /// <summary>
        /// Gets the connection status, representing wheter connection has closed
        /// </summary>
        /// <returns>Boolean true if still active</returns>
        public bool isConnectionActive() {
            return isActive;
        }

        /// <summary>
        /// Sets the is active value
        /// </summary>
        /// <param name="value">Boolean, True for still running</param>
        public void SetIsConnectionActive(bool value) {
            this.isActive = value;
        }

        /// <summary>
        /// Get the queue used to send commands to the client
        /// </summary>
        /// <returns></returns>
        public ConcurrentQueue<Message> GetInputQueue() {
            return this.concurrentQueueInput;
        }

        /// <summary>
        /// Get the queue used for receiving from the client
        /// </summary>
        /// <returns></returns>
        public ConcurrentQueue<Message> GetOutputQueue() {
            return this.concurrentQueueOutput;
        }
        /// <summary>
        /// Set the instrument information on connection
        /// </summary>
        /// <param name="instrumentInformation">IInstrument Information</param>
        public void SetInstrumentInformation(InstrumentInformation instrumentInformation) {
            this.info = instrumentInformation;
            hasInstrument = true;
        }

        public InstrumentInformation GetInstrumentInformation() {
            if (hasInstrument) {
                return info;
            }

            throw new NullReferenceException();
        }
    }
}