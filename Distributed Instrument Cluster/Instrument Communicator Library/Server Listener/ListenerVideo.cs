using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Instrument_Communicator_Library.Server_Listener {

    /// <summary>
    /// Base class for all listeners
    /// </summary>
    /// <typeparam name="T">The type of object that will be sent through the socket</typeparam>
    public class ListenerVideo<T> : ListenerBase {
        private List<VideoConnection<T>> listVideoConnections;     //list of connected video streams

        public ListenerVideo(IPEndPoint ipEndPoint, int maxConnections = 30, int maxPendingConnections = 30) : base(ipEndPoint, maxConnections, maxPendingConnections) {
            listVideoConnections = new List<VideoConnection<T>>();
        }

        protected override object CreateConnectionType(Socket socket, Thread thread) {
            return new VideoConnection<T>(socket, thread);
        }

        protected override void HandleIncomingConnection(object obj) {
            //Cast to video-connection
            var videoConnection = (VideoConnection<T>)obj;

            //add connection to list
            AddVideoConnection(videoConnection);

            //Get outputQueue
            ConcurrentQueue<T> outputQueue = videoConnection.getOutputQueue();
            //Get socket
            Socket connectionSocket = videoConnection.GetSocket();

            //Do main loop
            while (!listenerCancellationToken.IsCancellationRequested) {
                //Get size of incoming object
                var sizeOfIncomingBuffer = new byte[sizeof(int)];
                connectionSocket.Receive(sizeOfIncomingBuffer, 0, sizeof(int), SocketFlags.None);
                //extract int
                var sizeOfIncoming = BitConverter.ToInt32(sizeOfIncomingBuffer, 0);
                //receive main object
                var incomingObjectBuffer = new byte[sizeOfIncoming];
                connectionSocket.Receive(incomingObjectBuffer, 0, sizeOfIncoming, SocketFlags.None);

                var newObj = ByteArrayToObject(incomingObjectBuffer);

                try {
                    //try to cast newObj
                    var newObject = (T)newObj;
                    //Put in output-queue
                    outputQueue.Enqueue(newObject);
                } catch (InvalidCastException) {
                    Console.WriteLine("could not cast T");
                    throw;
                }
            }
            //remove connection
            RemoveVideoConnection(videoConnection);
        }

        /// <summary>
        /// Add item to the list
        /// </summary>
        private void AddVideoConnection(VideoConnection<T> connection) {
            lock (listVideoConnections) {
                listVideoConnections.Add(connection);
            }
        }

        /// <summary>
        /// Remove the client connection from the list
        /// </summary>
        /// <param name="connection"> Video Connection</param>
        /// <returns>Boolean representing successful removal</returns>
        private bool RemoveVideoConnection(VideoConnection<T> connection) {
            //Lock list and remove the connection
            bool result = false;
            lock (listVideoConnections) {
                //Try to remove connection
                result = listVideoConnections.Remove(connection);
            }
            //return bool
            return result;
        }

        /// <summary>
        /// From stack overflow
        /// https://stackoverflow.com/questions/1446547/how-to-convert-an-object-to-a-byte-array-in-c-sharp
        /// </summary>
        /// <param name="arrBytes"></param>
        /// <returns></returns>
        private static Object ByteArrayToObject(byte[] arrBytes) {
            using (var memStream = new MemoryStream()) {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }

        /// <summary>
        /// Get the list of video connection objects
        /// </summary>
        /// <returns>List of video-connection objects of type T</returns>
        public List<VideoConnection<T>> GetVideoConnectionList() {
            lock (listVideoConnections) {
                return listVideoConnections;
            }
        }
    }
}