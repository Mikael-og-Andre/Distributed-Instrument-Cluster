using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Server_Library.Connection_Types;
using Socket_Library;

namespace Remote_Server.Crestron {

	/// <summary>
	/// A connection for controlling a crestron
	/// <Author>Mikael Nilssen</Author>
	/// </summary>
	public class CrestronConnection : ConnectionBaseAsync {
		/// <summary>
		/// Is the connection closed
		/// </summary>
		public bool closed { get; set; }

		public CrestronConnection(Socket socket, CancellationToken ct) : base(socket, ct) {
			closed = false;
		}

		/// <summary>
		/// Send bytes with NetworkStream Asynchronously
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns>Task</returns>
		public async Task sendAsync(byte[] bytes) {
			await SocketOperation.sendBytesAsync(connectionNetworkStream, bytes);
		}

		/// <summary>
		/// Receive bytes from NetworkStream Asynchronously
		/// </summary>
		/// <returns>Byte array task</returns>
		public async Task<byte[]> receiveAsync() {
			byte[] receivedBytes = await SocketOperation.receiveBytesAsync(connectionNetworkStream);
			return receivedBytes;
		}

		/// <summary>
		/// Set closed boolean to true
		/// </summary>
		public void close() {
			closed = true;
		}

		/// <summary>
		/// Check the value of the closed field
		/// </summary>
		/// <returns></returns>
		public bool isClosed() {
			return closed;
		}
	}
}