using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Networking_Library;
using Server_Library.Authorization;

namespace Server_Library.Connection_Types.Async {
	/// <summary>
	/// Connection type for 2way Asynchronous communication
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class DuplexConnectionAsync : ConnectionBaseAsync {

		private bool abandoned = false;

		public DuplexConnectionAsync(Socket socket, AccessToken accessToken, CancellationToken cancellation) : base(
			socket, accessToken, cancellation) {
		}

		/// <summary>
		/// Send bytes with NetworkStream Asynchronously
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns>Task</returns>
		public async Task sendBytesAsync(byte[] bytes) {
			await NetworkingOperations.sendBytesAsync(connectionNetworkStream, bytes);
		}
		/// <summary>
		/// Receive bytes from NetworkStream Asynchronously
		/// </summary>
		/// <returns>Byte array task</returns>
		public async Task<byte[]> receiveBytesAsync() {
			byte[] receivedBytes = await NetworkingOperations.receiveBytesAsync(connectionNetworkStream);
			return receivedBytes;
		}

		public void abandon() {
			abandoned = true;
		}

		public bool isDisconnected() {
			return abandoned;
		}
	}
	
}
