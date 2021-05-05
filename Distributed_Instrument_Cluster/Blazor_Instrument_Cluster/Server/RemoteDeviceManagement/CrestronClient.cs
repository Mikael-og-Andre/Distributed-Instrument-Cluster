using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement.Interface;
using Networking_Library;
using Server_Library.Authorization;
using Server_Library.Socket_Clients.Async;

namespace Blazor_Instrument_Cluster.Server.RemoteDeviceManagement {
	/// <summary>
	/// Class for connecting to a server for controlling a crestron
	/// <Author>Mikael Nilssen</Author>
	/// </summary>
	public class CrestronClient : ClientBaseAsync , IConnectionControl{

		private Mutex controlMutex { get; set; }

		private const int PingTimeout = 5000;

		public CrestronClient(string ip, int port, AccessToken accessToken) : base(ip, port, accessToken) {
			controlMutex = new Mutex();
		}

		/// <summary>
		/// Ping the ip address and check if it was a success
		/// </summary>
		/// <returns>True if success</returns>
		public bool ping() {
			Ping ping = new Ping();
			PingReply rply = ping.Send(Ip, PingTimeout);
			//If the ping was a success return true
			if (rply.Status==IPStatus.Success) {
				return false;
			}
			return false;
		}

		/// <summary>
		/// Send string
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		public async Task send(string msg) {
			await sendAsync(Encoding.UTF32.GetBytes(msg));
		}

		/// <summary>
		/// receive string
		/// </summary>
		/// <returns></returns>
		public async Task<string> receive() {
			byte[] bytesReceived = await receiveAsync();
			return Encoding.UTF32.GetString(bytesReceived);
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
		/// Disconnect socket with reuse
		/// </summary>
		/// <returns></returns>
		public void disconnect() {
			if (isSocketConnected()) {
				//Disconnect with reuse
				connectionSocket.Disconnect(true);
				isSetup = false;
			}
		}

		public async Task connectAsync() {
			//Check if already connected
			if (isSocketConnected()) {
				Console.WriteLine("CrestronClient: Socket tried to connect when already connected");
				return;
			}
			await connectToServer(connectionSocket);
			await setupConnection(connectionSocket);
		}
	}
}
