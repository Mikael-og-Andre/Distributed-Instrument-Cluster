using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using Server_Library.Connection_Classes;

namespace Server_Library.Connection_Types {
    /// <summary>
    /// Class that contains All information about a Crestron connection
    /// <author>Mikael Nilssen</author>
    /// </summary>
    public class CrestronConnection : ConnectionBase {
	    /// <summary>
		/// Concurrent queue for Messages to send to the client
		/// </summary>
        private readonly ConcurrentQueue<Message> sendingQueue;
		/// <summary>
		/// Concurrent queue for Messages received by the client
		/// </summary>
        private readonly ConcurrentQueue<Message> receiveQueue;
		
		/// <summary>
		/// Constructor for Crestron Connection
		/// </summary>
		/// <param name="homeThread"></param>
		/// <param name="socket"></param>
		public CrestronConnection(Thread homeThread, Socket socket) : base(homeThread, socket) {
            
            //Init queues
            sendingQueue = new ConcurrentQueue<Message>();
            receiveQueue = new ConcurrentQueue<Message>();
        }

        /// <summary>
        /// Get the queue used to send commands to the client
        /// </summary>
        /// <returns></returns>
        public ConcurrentQueue<Message> getSendingQueue() {
            return sendingQueue;
        }

        /// <summary>
        /// Get the queue used for receiving from the client
        /// </summary>
        /// <returns></returns>
        public ConcurrentQueue<Message> getReceivingQueue() {
            return receiveQueue;
        }

    }
}