using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Instrument_Communicator_Library.Authorization;
using Instrument_Communicator_Library.Connection_Types;

namespace Instrument_Communicator_Library.Server_Listeners {
	/// <summary>
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class SendingListener<T> : ListenerBase {

		/// <summary>
		/// List of sending connections, for sending to receiving Clients
		/// </summary>
		private List<SendingConnection<T>> listSendingConnections;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ipEndPoint"></param>
		/// <param name="maxConnections"></param>
		/// <param name="maxPendingConnections"></param>
		public SendingListener(IPEndPoint ipEndPoint, int maxConnections = 30, int maxPendingConnections = 30) : base(
			ipEndPoint, maxConnections, maxPendingConnections) {
			//init list
			listSendingConnections = new List<SendingConnection<T>>();
		}

		/// <summary>
		/// handles the incoming connection, and pushes objects from the internal queue to the connected client
		/// </summary>
		/// <param name="obj">SendingConnection object</param>
		protected override void handleIncomingConnection(object obj) {
			//Cast incoming connections
			SendingConnection<T> connection = (SendingConnection<T>)obj;

			//Add Connection to list of connections
			lock (listSendingConnections) {
				listSendingConnections.Add(connection);
			}
			//switch bool of setup
			connection.isSetupCompleted = true;
			
			//Stopwatch stopwatch = new Stopwatch();
			//stopwatch.Start();

			//Receive objects
			while (cancellationTokenSource.Token.IsCancellationRequested) {
				if (connection.isDataAvailable()) {
					//Sends an objects from the internal queue
					connection.send();
				}
				else {
					Thread.Sleep(50);
				}
			}
		}

		/// <summary>
		/// Creates The correct connection type for this inheritance
		/// </summary>
		/// <param name="socket">Socket the connection is on</param>
		/// <param name="thread">Thread the connection is sending on</param>
		/// <param name="authToken">authorization token</param>
		/// <param name="info">Information about remote device</param>
		/// <returns>SendingConnection object</returns>
		protected override object createConnectionType(Socket socket, Thread thread, AccessToken authToken, InstrumentInformation info) {
			return new SendingConnection<T>(thread,socket,authToken,info,cancellationTokenSource.Token);
		}
	}
}
