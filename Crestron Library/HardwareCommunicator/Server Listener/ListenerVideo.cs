using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Instrument_Communicator_Library.Server_Listener {
    public class ListenerVideo<T> : ListenerBase {

        private List<VideoConnection<T>> listVideoConnections;     //list of connected video streams

        public ListenerVideo(IPEndPoint ipEndPoint, int maxConnections = 30, int maxPendingConnections = 30) : base(ipEndPoint, maxConnections, maxPendingConnections) {
            this.listVideoConnections = new List<VideoConnection<T>>();
        }

        protected override object createConnectionType(Socket socket, Thread thread) {
            return new VideoConnection<T>(socket,thread);
        }

        protected override void handleIncomingConnection(object obj) {
            VideoConnection<T> videoConnection;
            try {
                //Cast to videoconnection
                videoConnection = (VideoConnection<T>) obj;

            } catch (Exception ex) {
                throw ex;
            }

            //add connection to list
            AddVideoConnection(videoConnection);

            //Get outputQueue
            ConcurrentQueue<T> outputQueue = videoConnection.getOutputQueue();
            //Get socket
            Socket connectionSocket = videoConnection.GetSocket();

            //Delegate
            byte[] sizeOfIncomingBuffer;
            int sizeOfIncoming;
            byte[] incomingObjectBuffer;
            T newObject;

            //Do main loop
            while (!listenerCancellationToken.IsCancellationRequested) {

                //Get size of incoming object
                sizeOfIncomingBuffer = new byte[sizeof(int)];
                connectionSocket.Receive(sizeOfIncomingBuffer,0,sizeof(int),SocketFlags.None);
                //extract int
                sizeOfIncoming = BitConverter.ToInt32(sizeOfIncomingBuffer,0);
                //receive main object
                incomingObjectBuffer = new byte[sizeOfIncoming];
                connectionSocket.Receive(incomingObjectBuffer,0,sizeOfIncoming, SocketFlags.None);

                object newObj = ByteArrayToObject(incomingObjectBuffer);

                try {
                    //try to cast newObj
                    newObject = (T)newObj;
                    //Put in outputqueue
                    outputQueue.Enqueue(newObject);

                } catch (InvalidCastException ex) {
                    Console.WriteLine("could not cast T");
                    throw ex;
                }

            }
        }

        /// <summary>
        /// Add item to the list
        /// </summary>
        private void AddVideoConnection(VideoConnection<T> connection) {
            try {
                lock (this.listVideoConnections) {
                    this.listVideoConnections.Add(connection);
                }

            }catch (Exception ex) {
                throw ex;
            }
        }
        /// <summary>
        /// Remove the client connection from the list
        /// </summary>
        /// <param name="connection"> Video Connection</param>
        /// <returns>Boolean representing successful removal</returns>
        private bool RemoveVideoConnection(VideoConnection<T> connection) {
            bool result;
            try {
                //Lock list and remove the connection
                lock (listVideoConnections) {
                    //Try to remove connection
                    result = listVideoConnections.Remove(connection);
                }
                //return bool
                return result;

            } catch (Exception ex) {
                return false;
                throw ex;
            }
        }

        /// <summary>
        /// From stackoverflow
        /// https://stackoverflow.com/questions/1446547/how-to-convert-an-object-to-a-byte-array-in-c-sharp
        /// </summary>
        /// <param name="arrBytes"></param>
        /// <returns></returns>
        public static Object ByteArrayToObject(byte[] arrBytes) {
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
        /// <returns>List of videoconnection objects of type T</returns>
        public List<VideoConnection<T>> getVideoConnectionList() {
            return listVideoConnections;
        }
    }
}
