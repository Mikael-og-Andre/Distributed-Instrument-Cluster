using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading;



namespace Instrument_Communicator_Library {
    /// <summary>
    /// Represents a socket line from a device to the server, intended to send video
    /// <author>Mikael Nilssen</author>
    /// </summary>
    public class VideoCommunicator<T> : CommunicatorBase {

        private ConcurrentQueue<T> inputQueue; //queue of inputs meant to be sent to server

        public VideoCommunicator(string ip, int port, InstrumentInformation informationAboutClient, AccessToken accessToken, CancellationToken cancellationToken) : base(ip, port, informationAboutClient, accessToken, cancellationToken) {
            //initialize queue
            inputQueue = new ConcurrentQueue<T>();
        }

        /// <summary>
        /// Handles the protocols after the socket has been connected
        /// </summary>
        /// <param name="connectionSocket"></param>
        protected override void HandleConnected(Socket connectionSocket) {

            //While not canceled push from queue to socket
            while (!communicatorCancellationToken.IsCancellationRequested) {
                //get input form queue
                T objectFromQueue;
                bool hasInput = inputQueue.TryDequeue(out objectFromQueue);
                if (hasInput) {
                    //Get the object
                    T obj = objectFromQueue;

                    //Get bytes of object T
                    byte[] objectBytes = ObjectToByteArray(obj);

                    //size byte array from sizeOfT
                    byte[] sizeByte = BitConverter.GetBytes(objectBytes.Length);
                    //Send int for amount of incoming bytes
                    connectionSocket.Send(sizeByte, sizeof(int), SocketFlags.None);

                    //Send queue object
                    connectionSocket.Send(objectBytes,objectBytes.Length,SocketFlags.None);
                }
                
            }
        }
        /// <summary>
        /// Get the concurrent Queue
        /// </summary>
        /// <returns></returns>
        public ConcurrentQueue<T> getInputQueue() {
            return inputQueue;
        }

        /// <summary>
        /// Truns object into byte array
        /// From stack overflow
        /// https://stackoverflow.com/questions/1446547/how-to-convert-an-object-to-a-byte-array-in-c-sharp
        /// </summary>
        /// <param name="obj">Any Object</param>
        /// <returns>Byte array</returns>
        public static byte[] ObjectToByteArray(Object obj) {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream()) {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}