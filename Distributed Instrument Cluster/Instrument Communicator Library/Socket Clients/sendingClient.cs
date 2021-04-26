using Networking_Library;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using Server_Library.Authorization;

namespace Server_Library.Socket_Clients {

	/// <summary>
	/// Client for sending objects to Receive Listener
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class SendingClient : ClientBase {

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="port"></param>
		/// <param name="informationAboutClient"></param>
		/// <param name="accessToken"></param>
		/// <param name="isRunningCancellationToken"></param>
		public SendingClient(string ip, int port, ClientInformation informationAboutClient, AccessToken accessToken,
			CancellationToken isRunningCancellationToken) : base(ip, port, informationAboutClient, accessToken,
			isRunningCancellationToken) { }

		/// <summary>
		/// Sends bytes to socket.
		/// </summary>
		/// <param name="b">Bytes to send.</param>
		public void sendBytes(byte[] b) {
			NetworkingOperations.sendBytes(connectionNetworkStream, b);
		}
	}
}