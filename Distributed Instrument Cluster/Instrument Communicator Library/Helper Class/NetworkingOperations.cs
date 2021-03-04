using Instrument_Communicator_Library.Information_Classes;
using Instrument_Communicator_Library.Interface;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

namespace Instrument_Communicator_Library.Helper_Class {

	/// <summary>
	/// Class with different Socket operations
	/// </summary>
	public static class NetworkingOperations {

		/// <summary>
		/// Send an object with the socket
		/// </summary>
		/// <param name="inObj"></param>
		/// <param name="connectionSocket"></param>
		public static void SendObjectWithSocket<U>(U input, Socket connectionSocket) where U : ISerializeableObject {
			//Get bytes of object T
			byte[] bytes = input.getBytes();
			int size = bytes.Length;
			byte[] sizeBytes = BitConverter.GetBytes(size);
			//Create stream
			NetworkStream networkStream = new NetworkStream(connectionSocket, false);
			networkStream.Flush();
			foreach (var b in sizeBytes) {
				networkStream.WriteByte(b);
			}
			networkStream.Write(bytes);
			networkStream.Flush();
		}

		/// <summary>
		/// Receive an object with socket
		/// </summary>
		/// <param name="connectionSocket"></param>
		/// <returns></returns>
		public static VideoFrame ReceiveVideoFrameWithSocket(Socket connectionSocket) {
			//First 4 bytes are size
			NetworkStream networkStream = new NetworkStream(connectionSocket);
			byte[] sizeBytes = new byte[sizeof(int)];
			for (int i = 0;i<sizeof(int);i++) {
				sizeBytes[i] = (byte) networkStream.ReadByte();
			}
			Array.Reverse(sizeBytes);
			//Convert to size
			int size = BitConverter.ToInt32(sizeBytes);
			byte[] bufferBytes = new byte[size];
			networkStream.Read(bufferBytes);
			networkStream.Flush();

			VideoFrame frame = new VideoFrame(new byte[] { });
			frame = (VideoFrame)frame.getObject(bufferBytes);
			return frame;
		}

		/// <summary>
		/// Receive an string with the given socket
		/// </summary>
		/// <param name="connectionSocket">Connected socket</param>
		/// <returns>string</returns>
		public static string ReceiveStringWithSocket(Socket connectionSocket) {
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
			string receivedObj = Encoding.ASCII.GetString(incomingObjectBuffer);
			//Trim null bytes
			receivedObj = receivedObj.TrimEnd('\0');
			return receivedObj;
		}

		/// <summary>
		/// Send a string with socket
		/// </summary>
		/// <param name="str">String to send</param>
		/// <param name="connectionSocket">Connected socket</param>
		public static void SendStringWithSocket(string str, Socket connectionSocket) {
			//Send name
			string encodingTarget = str;
			byte[] stringBuffer = Encoding.ASCII.GetBytes(encodingTarget);
			byte[] sizeBuffer = new byte[sizeof(int)];
			sizeBuffer = BitConverter.GetBytes(stringBuffer.Length);
			connectionSocket.Blocking = true;
			connectionSocket.Send(sizeBuffer, sizeof(int), SocketFlags.None);

			//Send message string to client
			connectionSocket.Send(stringBuffer, stringBuffer.Length, SocketFlags.None);
		}
		

		private static int GetIntFromBytes(byte[] array) {
			return BitConverter.ToInt32(array);
		}
		
		

	}
}