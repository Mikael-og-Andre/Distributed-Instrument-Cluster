using Server_Library.Authorization;
using Server_Library.Connection_Classes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Server_Library.Connection_Types.Async;

namespace Server_Library.Server_Listeners.Async {

	/// <summary>
	/// Accepting listen for and handle incoming connections
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class DuplexListenerAsync : ListenerBaseAsync {

		/// <summary>
		/// List of connections
		/// </summary>
		private List<DuplexConnectionAsync> connectionList;

		public DuplexListenerAsync(IPEndPoint ipEndPoint, int maxPendingConnections = 100) : base(ipEndPoint,
			maxPendingConnections) {
			connectionList = new List<DuplexConnectionAsync>();
		}

		protected override async Task handleIncomingConnectionAsync(ConnectionBaseAsync obj) {
			try {
				//Cast
				DuplexConnectionAsync connection = (DuplexConnectionAsync) obj;
				//Add to list of connections
				lock (connectionList) {
					connectionList.Add(connection);
				}
			}
			catch (Exception e) {
				Console.WriteLine("DuplexListenerAsync:");
				Console.WriteLine(e);
				throw;
			}
		}

		protected override ConnectionBaseAsync createConnectionType(Socket socket) {
			return new DuplexConnectionAsync(socket,cancellationTokenSource.Token);
		}
	}
}