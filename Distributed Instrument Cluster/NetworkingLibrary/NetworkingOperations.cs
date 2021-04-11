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
			//Trim null bytes
			receivedObj = receivedObj.TrimEnd('\0');
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

		#region Serialize Objects to binary

		/// <summary>
		/// Send an object with the socket
		/// </summary>
		/// <param name="input">Object inheriting ISerializableObject</param>
		/// <param name="connectionSocket"></param>
		public static void sendObjectWithSocket<TU>(TU input, Socket connectionSocket) where TU : ISerializeObject {
			byte[] bytes = input.getBytes();
			NetworkStream networkStream = new NetworkStream(connectionSocket);
			networkStream.Write(bytes);
			networkStream.Flush();
		}

		/// <summary>
		/// Receive an object with socket
		/// </summary>
		/// <param name="connectionSocket">Socket used for receiving</param>
		/// <param name="receiveBufferSize">Size wanted for the buffer to receive the object</param>
		/// <returns>byte array representation of object</returns>
		public static byte[] receiveByteArrayWithSocket(Socket connectionSocket, int receiveBufferSize) {
			//Create stream
			NetworkStream networkStream = new NetworkStream(connectionSocket);
			networkStream.Flush();
			//read bytes to buffer
			byte[] bufferBytes = new byte[receiveBufferSize];
			networkStream.Read(bufferBytes);
			networkStream.Flush();
			int endInt = 0;
			//Check where the nullbytes are
			for (int i = bufferBytes.Length; i > 0; i--) {
				byte currentByte = bufferBytes[i - 1];
				if (currentByte != byte.MinValue) {
					endInt = i;
					break;
				}
			}

			//Copy non null bytes to array
			byte[] bytes = new byte[endInt];
			Buffer.BlockCopy(bufferBytes, 0, bytes, 0, endInt);
			//return arrray
			return bytes;
		}

		#endregion Serialize Objects to binary
	}
}