using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using Instrument_Communicator_Library.Authorization;
using Instrument_Communicator_Library.Connection_Classes;

namespace Instrument_Communicator_Library.Connection_Types.deprecated {
    /// <summary>
    /// Class that contains All information about a Crestron connection
    /// <author>Mikael Nilssen</author>
    /// </summary>
    public class CrestronConnection : ConnectionBaseOld {
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
		public CrestronConnection(Thread homeThread, Socket socket, AccessToken accessToken, InstrumentInformation info, CancellationToken cancellation) : base(homeThread, socket,accessToken,info, cancellation) {
            
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