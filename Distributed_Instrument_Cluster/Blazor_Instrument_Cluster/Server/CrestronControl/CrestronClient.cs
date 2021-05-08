using Blazor_Instrument_Cluster.Server.CrestronControl.Interface;
using Networking_Library;
using Server_Library.Authorization;
using Server_Library.Socket_Clients.Async;
using System;
using System.Linq;
using System.Net;
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
			if (ready()) {
				Console.WriteLine("CrestronClient: Connect was called but socket was ready");
				return;
			}
			connectToServer();
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
				isConnected = false;
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
			try {
				IPHostEntry hostInfo = Dns.GetHostEntry(Ip);
				IPAddress[] addresses = hostInfo.AddressList;

				if (addresses.Any()) {
					Ping ping = new Ping();
					PingReply reply = ping.Send(IPAddress.Parse(addresses.First().ToString()), PingTimeout);
					if (reply.Status == IPStatus.Success) {
						return true;
					}
					else {
						return false;
					}
				}

				return false;
			}
			catch (Exception e) {
				Console.WriteLine("Ping failed deu to exception: {0}",e);
				return false;
			}
		}

		/// <summary>
		/// Send string
		/// </summary>
		/// <param name="msg"></param>
		/// <returns>Was sent</returns>
		public async Task<bool> send(string msg) {
			try {
				if (!isSocketConnected()) {
					isConnected = false;
					Console.WriteLine("CrestronClient: tried to send on closed socket");
					return false;
				}
				await sendAsync(Encoding.UTF8.GetBytes(msg));
				return true;
			}
			catch (Exception e) {
				Console.WriteLine($"Exception in CrestronClient Send: {e.Message}");
				isConnected = false;
				return false;
			}
		}

		/// <summary>
		/// receive string
		/// </summary>
		/// <returns></returns>
		public async Task<string> receive() {
			try {
				if (!isSocketConnected()) {
					isConnected = false;
					Console.WriteLine("CrestronClient: tried to receive on closed socket");
					return String.Empty;
				}
				byte[] bytesReceived = await receiveAsync();
				return Encoding.UTF8.GetString(bytesReceived);
			}
			catch (Exception e) {
				Console.WriteLine($"Exception in CrestronClient Send: {e.Message}");
				isConnected = false;
				return String.Empty;
			}
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
		public bool ensureUP() {
			try {
				//Check if connection is ok
				if (ready()) {
					return true;
				}
				//if not reconnect
				Console.WriteLine("Crestron Client is reconnecting");
				bool connected = reconnect();
				isConnected = connected;
				return connected;
			}
			catch (Exception e) {
				Console.WriteLine("CrestronClient: Exception in ensureUP");
				Console.WriteLine($"CrestronClient Exception: {e.Message}");
				isConnected = false;
				return false;
			}
		}

		#endregion Interface impl
	}
}