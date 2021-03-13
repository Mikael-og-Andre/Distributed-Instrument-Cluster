using Server_Library.Authorization;
using Server_Library.Connection_Types;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server_Library.Server_Listeners {

	/// <summary>
	/// Server for listening and creating sending connections, This class is made to work with Receiving Clients
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

			//Send objects
			while (!cancellationTokenSource.Token.IsCancellationRequested) {
				if (connection.send()) {

				}
				else {
					Thread.Sleep(100);
				}
			}

			//remove Connection from list of connections
			lock (listSendingConnections) {
				listSendingConnections.Remove(connection);
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
		protected override object createConnectionType(Socket socket, Thread thread, AccessToken authToken, ClientInformation info) {
			return new SendingConnection<T>(thread, socket, authToken, info, cancellationTokenSource.Token);
		}

		/// <summary>
		/// Get list of connections
		/// </summary>
		/// <returns>List SendingConnection</returns>
		public List<SendingConnection<T>> getListOfSendingConnections() {
			lock (listSendingConnections) {
				return listSendingConnections;
			}
		}
	}
}