using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Server_Library.Authorization;
using Server_Library.Connection_Types.Async;
using Server_Library.Server_Listeners.Async;

namespace Remote_Server.Crestron {
	/// <summary>
	/// Listen for incoming crestron connections and handle them
	/// <Author>Mikael Nilssen</Author>
	/// </summary>
	public class CrestronListener : ListenerBaseAsync{

		/// <summary>
		/// List of connections currently running
		/// Not threadsafe
		/// </summary>
		private List<CrestronConnection> crestronConnections { get; set; }

		/// <summary>
		/// Interface for sending commands to a crestron
		/// </summary>
		private ICrestronControl crestron { get; set; }
		/// <summary>
		/// CancellationTokenSource for async operations
		/// </summary>
		private CancellationTokenSource cts { get; set; }
		/// <summary>
		/// Cancellation token for async operations
		/// </summary>
		private CancellationToken globalCT { get; set; }

		public CrestronListener(IPEndPoint ipEndPoint, ICrestronControl crestronUnit, int maxPendingConnections = 100) :
			base(ipEndPoint, maxPendingConnections) {
			cts = new CancellationTokenSource();
			globalCT = cts.Token;
			crestronConnections = new List<CrestronConnection>();
			this.crestron = crestronUnit;

		}

		/// <summary>
		/// Receive data from the connection and send it to the crestron unit
		/// </summary>
		/// <param name="con"></param>
		/// <returns></returns>
		protected override async Task handleIncomingConnectionAsync(ConnectionBaseAsync con) {
			//Cast connection
			CrestronConnection connection;
			try { 
				connection = (CrestronConnection) con;
			}
			catch (InvalidCastException e) {
				Console.WriteLine("Error while handling crestron connection {0}",e.Message);
				return;
			}
			//Add connection to list of connections
			lock (crestronConnections) {
				crestronConnections.Add(connection);
			}

			try {
				//Handle connection
				while (!connection.isClosed()) {
					if (!connection.isSocketConnected()) {
						connection.close();
						break;
					}

					byte[] receivedBytes = await connection.receiveAsync();
					string decoded = Encoding.UTF32.GetString(receivedBytes);
					await crestron.sendCommandToCrestron(decoded);
				}
			}
			catch (Exception e) {
				Console.WriteLine("Error while handling crestron connection {0}",e.Message);
			}
			//Remove connection
			lock (crestronConnections) {
				crestronConnections.Remove(connection);
			}
		}

		/// <summary>
		/// Create a new connection with the type CrestronConnection
		/// </summary>
		/// <param name="socket">Socket for the connection</param>
		/// <param name="accessToken">AccessToken of the connection</param>
		/// <returns>CrestronConnection</returns>
		protected override ConnectionBaseAsync createConnectionType(Socket socket, AccessToken accessToken) {
			return new CrestronConnection(socket,accessToken,globalCT);
		}
	}
}
