using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using Instrument_Communicator_Library.Connection_Classes;


namespace Instrument_Communicator_Library {
    /// <summary>
    /// Class that contains All information about a Crestron connection
    /// <author>Mikael Nilssen</author>
    /// </summary>
    public class CrestronConnection : ConnectionBase {
	    /// <summary>
		/// Concurrent queue for Messages to send to the client
		/// </summary>
        private ConcurrentQueue<Message> concurrentQueueInput;
		/// <summary>
		/// Concurrent queue for Messages received by the client
		/// </summary>
        private ConcurrentQueue<Message> concurrentQueueOutput;
		
		public CrestronConnection(Thread homeThread, Socket socket) : base(homeThread, socket) {
            
            //Init queues
            concurrentQueueInput = new ConcurrentQueue<Message>();
            concurrentQueueOutput = new ConcurrentQueue<Message>();
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
        
    }
}