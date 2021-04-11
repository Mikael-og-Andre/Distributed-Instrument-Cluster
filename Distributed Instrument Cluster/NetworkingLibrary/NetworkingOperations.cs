using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

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

#endregion

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
			string json = receiveStringWithSocket(socket);
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
			//First send size of incoming objects
			byte[] size = BitConverter.GetBytes(bytesToSend.Length);
			stream.Write(size, 0, sizeof(int));
			//Write the data
			stream.Write(bytesToSend, 0, bytesToSend.Length);
			//Flush stream
			stream.Flush();
		}

		/// <summary>
		/// Receive a byte array from the stream
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static byte[] receiveBytes(NetworkStream stream) {
			//Get size of incoming bytes
			byte[] sizeBytes = new byte[sizeof(int)];
			stream.Read(sizeBytes, 0, sizeof(int));
			int size = BitConverter.ToInt32(sizeBytes);
			//Receive byte array
			byte[] incomingBytes = new byte[size];
			stream.Read(incomingBytes, 0, incomingBytes.Length);

			return incomingBytes;
		}

		#endregion Byte array
	}
}