using Blazor_Instrument_Cluster.Server.CrestronControl.Interface;
using Networking_Library;
using Server_Library.Authorization;
using Server_Library.Socket_Clients.Async;
using System;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server.CrestronControl {

	/// <summary>
	/// Class for connecting to a server for controlling a crestron
	/// <Author>Mikael Nilssen</Author>
	/// </summary>
	public class CrestronClient : ClientBaseAsync, IConnectionCommunicator {

		/// <summary>
		/// Mutex
		/// </summary>
		private Mutex controlMutex { get; set; }

		/// <summary>
		/// Time the ping of a server will wait before timing out
		/// </summary>
		private const int PingTimeout = 5000;

		/// <summary>
		/// Is the crestron connected
		/// </summary>
		public bool isConnected { get; set; }

		public CrestronClient(string ip, int port, AccessToken accessToken) : base(ip, port) {
			controlMutex = new Mutex();
			isConnected = false;
		}

		/// <summary>
		/// Connect socket to ip and port
		/// </summary>
		/// <returns></returns>
		public async Task connect() {
			if (isSocketConnected()) {
				Console.WriteLine("CrestronClient: Socket that was already connected tried to connect");
				return;
			}
			await connectToServer();
			isConnected = true;
		}

		/// <summary>
		/// Disconnect socket with reuse
		/// </summary>
		/// <returns></returns>
		public void disconnect() {
			if (isSocketConnected()) {
				//Disconnect with reuse
				connectionSocket.Disconnect(true);
				isConnected = connectionSocket.Connected;
			}
		}

		/// <summary>
		/// Send bytes over the socket
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		private async Task sendAsync(byte[] bytes) {
			await NetworkingOperations.sendBytesAsync(connectionNetworkStream, bytes);
		}

		/// <summary>
		/// Receive bytes from the socket
		/// </summary>
		/// <returns>Task byte array</returns>
		private async Task<byte[]> receiveAsync() {
			byte[] receivedBytes = await NetworkingOperations.receiveBytesAsync(connectionNetworkStream);
			return receivedBytes;
		}

		/// <summary>
		/// is the socket ready to send data
		/// </summary>
		/// <returns></returns>
		private bool ready() {
			if (isSocketConnected() && isConnected) {
				return true;
			}

			return false;
		}

		#region Interface impl

		/// <summary>
		/// Ping the ip address and check if it was a success
		/// </summary>
		/// <returns>True if success</returns>
		public bool ping() {
			Ping ping = new Ping();
			PingReply rply = ping.Send(Ip, PingTimeout);
			//If the ping was a success return true
			if (rply.Status == IPStatus.Success) {
				return false;
			}
			return false;
		}

		/// <summary>
		/// Send string
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		public async Task<bool> send(string msg) {
			if (!isSocketConnected()) {
				Console.WriteLine("CrestronClient: tried to send on closed socket");
				return false;
			}
			await sendAsync(Encoding.UTF32.GetBytes(msg));
			return true;
		}

		/// <summary>
		/// receive string
		/// </summary>
		/// <returns></returns>
		public async Task<string> receive() {
			if (!isSocketConnected()) {
				Console.WriteLine("CrestronClient: tried to receive on closed socket");
				return String.Empty;
			}
			byte[] bytesReceived = await receiveAsync();
			return Encoding.UTF32.GetString(bytesReceived);
		}

		/// <summary>
		/// Check if connection is ready to send
		/// </summary>
		/// <returns></returns>
		public bool isReady() {
			return ready();
		}

		/// <summary>
		/// Make sure the connection is up, if possible
		/// </summary>
		/// <returns></returns>
		public async Task<bool> ensureUP() {
			if (ready()) {
				return true;
			}

			Console.WriteLine("Crestron Client is reconnecting");
			bool connected = await reconnect();
			isConnected = connected;
			if (connected) {
				return true;
			}

			return false;
		}

		#endregion Interface impl
	}
}