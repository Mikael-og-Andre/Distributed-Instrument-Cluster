using Networking_Library;
using Server_Library.Authorization;
using System.Threading;
using System.Threading.Tasks;

namespace Server_Library.Socket_Clients.Async {

	/// <summary>
	/// Client that sends and receives
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class DuplexClientAsync : ClientBaseAsync {

		public DuplexClientAsync(string ip, int port, AccessToken accessToken) : base(ip, port, accessToken) {
		}

		/// <summary>
		/// Sends a byte array async
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public async Task sendBytesAsync(byte[] bytes) {
			await NetworkingOperations.sendBytesAsync(connectionNetworkStream, bytes);
		}

		/// <summary>
		/// Receive a byte array async
		/// </summary>
		/// <returns>Task byte array</returns>
		public async Task<byte[]> receiveBytesAsync() {
			return await NetworkingOperations.receiveBytesAsync(connectionNetworkStream);
		}
	}
}