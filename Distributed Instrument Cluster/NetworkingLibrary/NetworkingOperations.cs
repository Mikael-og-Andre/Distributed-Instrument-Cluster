using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Networking_Library {

	/// <summary>
	/// Class with different Socket operations
	/// </summary>
	public static class NetworkingOperations {

		/// <summary>
		/// Receive an string with the given socket
		/// </summary>
		/// <param name="connectionSocket">Connected socket</param>
		/// <returns>string</returns>
		public static string receiveStringWithSocket(Socket connectionSocket) {
			//Get size of incoming object
			byte[] sizeOfIncomingBuffer = new byte[sizeof(int)];
			connectionSocket.Blocking = true;
			connectionSocket.Receive(sizeOfIncomingBuffer, 0, sizeof(int), SocketFlags.None);
			//extract int
			int sizeOfIncoming = BitConverter.ToInt32(sizeOfIncomingBuffer);
			//receive main object
			byte[] incomingObjectBuffer = new byte[sizeOfIncoming];
			connectionSocket.Receive(incomingObjectBuffer, 0, sizeOfIncoming, SocketFlags.None);
			//get string from object
			string receivedObj = Encoding.UTF32.GetString(incomingObjectBuffer);
			return receivedObj;
		}

		/// <summary>
		/// Send a string with socket
		/// </summary>
		/// <param name="str">String to send</param>
		/// <param name="connectionSocket">Connected socket</param>
		public static void sendStringWithSocket(string str, Socket connectionSocket) {
			//Send name
			string encodingTarget = str;
			byte[] stringBuffer = Encoding.UTF32.GetBytes(encodingTarget);
			byte[] sizeBuffer = BitConverter.GetBytes(stringBuffer.Length);
			connectionSocket.Blocking = true;
			connectionSocket.Send(sizeBuffer, sizeof(int), SocketFlags.None);

			//Send message string to client
			connectionSocket.Send(stringBuffer, stringBuffer.Length, SocketFlags.None);
		}

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
			stream.Write(size,0,sizeof(int));
			//Write the data
			stream.Write(bytesToSend,0,bytesToSend.Length);
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
			stream.Read(sizeBytes,0,sizeof(int));
			int size = BitConverter.ToInt32(sizeBytes);
			//Receive byte array
			byte[] incomingBytes = new byte[size];
			stream.Read(incomingBytes, 0, incomingBytes.Length);
			
			return incomingBytes;
		}

#endregion
	}
}