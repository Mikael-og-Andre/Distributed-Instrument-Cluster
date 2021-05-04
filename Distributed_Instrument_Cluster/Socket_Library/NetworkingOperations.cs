using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Networking_Library {

	/// <summary>
	/// Class with different Socket operations
	/// </summary>
	public static class NetworkingOperations {

		#region String

		/// <summary>
		/// Receive an string with the given socket
		/// </summary>
		/// <param name="connectionSocket">Connected socket</param>
		/// <returns>string</returns>
		public static string receiveStringWithSocket(Socket connectionSocket) {
			NetworkStream networkStream = new NetworkStream(connectionSocket, true);
			//receive bytes with socket stream
			byte[] incomingBytes = receiveBytes(networkStream);
			//get string from bytes
			string receivedString = Encoding.UTF32.GetString(incomingBytes);
			return receivedString;
		}

		/// <summary>
		/// Send a string with socket
		/// </summary>
		/// <param name="inputString"></param>
		/// <param name="connectionSocket">Connected socket</param>
		public static void sendStringWithSocket(string inputString, Socket connectionSocket) {
			//Send name
			byte[] stringBuffer = Encoding.UTF32.GetBytes(inputString);
			//send data with stream
			NetworkStream networkStream = new NetworkStream(connectionSocket, true);
			sendBytes(networkStream, stringBuffer);
		}

		#endregion String

		#region Json

		/// <summary>
		/// Serialize object to json and send it
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="socket"></param>
		public static void sendJsonObjectWithSocket<T>(T obj, Socket socket) {
			string json = JsonSerializer.Serialize(obj);
			sendStringWithSocket(json, socket);
		}

		/// <summary>
		/// Receive string and deserialize to object from json
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="socket"></param>
		/// <returns></returns>
		public static T receiveJsonObjectWithSocket<T>(Socket socket) {
			string json = receiveStringWithSocket(socket).TrimEnd('\0').TrimStart('\0');
			T receivedObj = JsonSerializer.Deserialize<T>(json);
			return receivedObj;
		}

		#endregion Json

		#region Byte array

		/// <summary>
		/// Send a byte array with a stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="bytesToSend"></param>
		public static void sendBytes(NetworkStream stream, byte[] bytesToSend) {
			lock (stream) {
				//First send size of incoming objects
				byte[] size = BitConverter.GetBytes(bytesToSend.Length);
				stream.Write(size, 0, sizeof(int));
				//Write the data
				stream.Write(bytesToSend, 0, bytesToSend.Length);
				//Flush stream
				stream.Flush();
			}
		}

		/// <summary>
		/// Receive a byte array from the stream
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static byte[] receiveBytes(NetworkStream stream) {
			lock (stream) {
				stream.Flush();
				//Get size of incoming bytes
				byte[] sizeBytes = new byte[sizeof(int)];
				stream.Read(sizeBytes, 0, sizeBytes.Length);
				int size = BitConverter.ToInt32(sizeBytes);
				//Thread sleep to fix insane bug.
				Thread.Sleep(1);
				//Receive byte array
				byte[] incomingBytes = new byte[size];
				stream.Read(incomingBytes, 0, incomingBytes.Length);
				stream.Flush();

				return incomingBytes;
			}
		}

		#endregion Byte array

		#region Async opertaions

		/// <summary>
		/// Receive a byte array from the stream async
		/// </summary>
		/// <param name="stream">Network Stream</param>
		/// <returns>Task Byte array</returns>
		public static async Task<byte[]> receiveBytesAsync(NetworkStream stream) {
			try {
				//Get size of incoming bytes
				byte[] sizeBytes = new byte[sizeof(int)];
				await stream.ReadAsync(sizeBytes, 0, sizeBytes.Length);
				Thread.Sleep(20);
				int size = BitConverter.ToInt32(sizeBytes);

				//Receive byte array
				byte[] incomingBytes = new byte[size];
				int readBytes = await stream.ReadAsync(incomingBytes, 0, incomingBytes.Length);

				return incomingBytes;
			}
			catch (Exception) {
				throw;
			}
		}

		/// <summary>
		/// Send a byte array with a stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="bytesToSend"></param>
		public static async Task sendBytesAsync(NetworkStream stream, byte[] bytesToSend) {
			try {
				//First send size of incoming objects
				byte[] size = BitConverter.GetBytes(bytesToSend.Length);
				await stream.WriteAsync(size, 0, sizeof(int));
				//Write the data
				await stream.WriteAsync(bytesToSend, 0, bytesToSend.Length);
			}
			catch (Exception) {
				throw;
			}
		}

		/// <summary>
		/// Send a string with socket async
		/// Encoding utf32
		/// </summary>
		/// <param name="inputString">string you want sent</param>
		/// <param name="networkStream">Connected Network stream</param>
		public static async Task sendStringAsync(string inputString, NetworkStream networkStream) {
			try {
				//Send name
				byte[] stringBuffer = Encoding.UTF32.GetBytes(inputString);
				//send data with stream
				await sendBytesAsync(networkStream, stringBuffer);
			}
			catch (Exception) {
				throw;
			}
		}

		/// <summary>
		/// Receive an string with socket async
		/// UTF32
		/// </summary>
		/// <param name="networkStream"></param>
		/// <returns>task string</returns>
		public static async Task<string> receiveStringAsync(NetworkStream networkStream) {
			try {
				//receive bytes with socket stream
				byte[] incomingBytes = await receiveBytesAsync(networkStream);
				//get string from bytes
				string receivedString = Encoding.UTF32.GetString(incomingBytes);
				return receivedString;
			}
			catch (Exception) {
				throw;
			}
		}

		/// <summary>
		/// Send object of type T with NetworkStream
		/// </summary>
		/// <typeparam name="T">JsonSerializable Object</typeparam>
		/// <param name="networkStream">Connected NetworkStream</param>
		/// <param name="obj">object to send</param>
		/// <returns>Task</returns>
		public static async Task sendObjectAsJsonAsync<T>(NetworkStream networkStream, T obj) {
			try {
				//deserialize
				string json = JsonSerializer.Serialize(obj);
				//Send string
				await NetworkingOperations.sendStringAsync(json, networkStream);
			}
			catch (Exception) {
				throw;
			}
		}

		/// <summary>
		/// Receive object of type T with networks stream
		/// </summary>
		/// <typeparam name="T">JsonSerializable Object</typeparam>
		/// <param name="networkStream">Connected NetworkStream</param>
		/// <returns>Task Object of type T</returns>
		public static async Task<T> receiveObjectAsJson<T>(NetworkStream networkStream) {
			try {
				//Receive string
				string json = await NetworkingOperations.receiveStringAsync(networkStream);
				//deserialize
				T obj = JsonSerializer.Deserialize<T>(json);
				return obj;
			}
			catch (Exception) {
				throw;
			}
		}

		#endregion Async opertaions
	}
}