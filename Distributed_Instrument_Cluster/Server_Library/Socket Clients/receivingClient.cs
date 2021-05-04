using Networking_Library;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using Server_Library.Authorization;

namespace Server_Library.Socket_Clients {

	/// <summary>
	/// Client for Receiving objects from Sending Listener
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class ReceivingClient : ClientBase {

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="port"></param>
		/// <param name="informationAboutClient"></param>
		/// <param name="accessToken"></param>
		/// <param name="isRunningCancellationToken"></param>
		public ReceivingClient(string ip, int port, ClientInformation informationAboutClient, AccessToken accessToken, CancellationToken isRunningCancellationToken) : 
			base(ip, port, informationAboutClient, accessToken, isRunningCancellationToken) { }

		/// <summary>
		/// Check if socket has new data and returns it.
		/// </summary>
		/// <param name="output">New data from socket.</param>
		/// <returns>True if socket has received new data.</returns>
		public bool receiveBytes(out byte[] output) {

			if (isDataAvailable()) {
				output = NetworkingOperations.receiveBytes(connectionNetworkStream);
				return true;
			}

			output = new byte[] { };
			return false;
		}
	}
}