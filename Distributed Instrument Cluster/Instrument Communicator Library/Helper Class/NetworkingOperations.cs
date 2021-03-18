using Instrument_Communicator_Library.Interface;
using System;
using System.Net.Sockets;
using System.Text;

namespace Instrument_Communicator_Library.Helper_Class {

	/// <summary>
	/// Class with different Socket operations
	/// </summary>
	public static class NetworkingOperations {
		/// <summary>
		/// Send an object with the socket
		/// </summary>
		/// <param name="input">Object inheriting ISerializableObject</param>
		/// <param name="connectionSocket"></param>
		public static void sendObjectWithSocket<TU>(TU input, Socket connectionSocket) where TU : ISerializable {

			byte[] bytes = input.getBytes();
            NetworkStream networkStream = new NetworkStream(connectionSocket);
			networkStream.Write(bytes);
			networkStream.Flush();
        }

		/// <summary>
		/// Receive an object with socket
		/// </summary>
		/// <param name="connectionSocket"></param>
		/// <returns></returns>
		public static VideoFrame receiveVideoFrameWithSocket(Socket connectionSocket) {
			//First 4 bytes are size
			NetworkStream networkStream = new NetworkStream(connectionSocket);
            networkStream.Flush();
			
			byte[] bufferBytes = new byte[200000];
			networkStream.Read(bufferBytes);
			networkStream.Flush();
            int endInt = 0;
            for (int i = bufferBytes.Length; i>0;i--) {
                byte currentByte = bufferBytes[i-1];
                if (currentByte != byte.MinValue) {
                    endInt = i;
                    break;
                }
            }


            byte[] bytes = new Byte[endInt];
			Buffer.BlockCopy(bufferBytes, 0, bytes, 0, endInt);

			VideoFrame frame = new VideoFrame(new byte[] { });
			frame = (VideoFrame)frame.getObject(bytes);
			return frame;
		}

		/// <summary>
		/// Receive an string with the given socket
		/// </summary>
		/// <param name="connectionSocket">Connected socket</param>
		/// <returns>string</returns>
		public static string receiveStringWithSocket(Socket connectionSocket) {
			
			//receive main object
			byte[] incomingObjectBuffer = new byte[5000];
			connectionSocket.Receive(incomingObjectBuffer, 0, 2048, SocketFlags.None);
			//get string from object
			string receivedObj = Encoding.UTF8.GetString(incomingObjectBuffer);
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
			byte[] stringBuffer = Encoding.UTF8.GetBytes(encodingTarget);
			//Send message string to client
			connectionSocket.Send(stringBuffer, 5000, SocketFlags.None);
		}
		

		private static int getIntFromBytes(byte[] array) {
			return BitConverter.ToInt32(array);
		}
		
		

	}
}