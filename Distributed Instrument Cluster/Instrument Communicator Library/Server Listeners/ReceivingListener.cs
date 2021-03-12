using Instrument_Communicator_Library.Authorization;
using Instrument_Communicator_Library.Connection_Types;
using Networking_Library;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Instrument_Communicator_Library.Server_Listeners.deprecated;

namespace Instrument_Communicator_Library.Server_Listeners {

	/// <summary>
	/// Server Listener for receiving objects from SendingClient connections
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class ReceivingListener<T> : ListenerBase {

		/// <summary>
		/// list of receiving connections
		/// </summary>
		private readonly List<ReceivingConnection<T>> listReceivingConnections;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ipEndPoint"></param>
		/// <param name="maxConnections"></param>
		/// <param name="maxPendingConnections"></param>
		public ReceivingListener(IPEndPoint ipEndPoint, int maxConnections = 30, int maxPendingConnections = 30) : base(
			ipEndPoint, maxConnections, maxPendingConnections) {
			//Init List
			listReceivingConnections = new List<ReceivingConnection<T>>();
		}

		/// <summary>
		/// Receives incoming objects
		/// </summary>
		/// <param name="obj"></param>
		protected override void handleIncomingConnection(object obj) {
			//Cast incoming connections
			ReceivingConnection<T> connection = (ReceivingConnection<T>)obj;

			//Add Connection to list of connections
			lock (listReceivingConnections) {
				listReceivingConnections.Add(connection);
			}
			//switch bool of setup
			connection.isSetupCompleted = true;
			
			//Stopwatch stopwatch = new Stopwatch();
			//stopwatch.Start();

			//Receive objects
			while (cancellationTokenSource.Token.IsCancellationRequested) {
				if (connection.isDataAvailable()) {
					connection.receive();
				}
				else {
					Thread.Sleep(50);
				}
			}
		}

		/// <summary>
		/// Creates the connection of the correct type
		/// </summary>
		/// <param name="socket">Socket</param>
		/// <param name="thread">Thread</param>
		/// <returns></returns>
		protected override object createConnectionType(Socket socket, Thread thread, AccessToken accessToken, InstrumentInformation info) {
			return new ReceivingConnection<T>(thread, socket, accessToken, info, cancellationTokenSource.Token);
		}
	}
}