using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Instrument_Communicator_Library.Helper_Class;

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

        protected override object createConnectionType(Socket socket, Thread thread) {
            return new VideoConnection<T>(socket, thread);
        }

        protected override void handleIncomingConnection(object obj) {
            //Cast to video-connection
            VideoConnection<T> videoConnection = (VideoConnection<T>)obj;
            //add connection to list
            addVideoConnection(videoConnection);
            
            //Get socket
            Socket connectionSocket = videoConnection.GetSocket();

            //Send signal to start instrumentCommunication
            NetworkingOperations.SendStringWithSocket("y",connectionSocket);

            string name = NetworkingOperations.ReceiveStringWithSocket(connectionSocket);
            string location = NetworkingOperations.ReceiveStringWithSocket(connectionSocket);
            string type = NetworkingOperations.ReceiveStringWithSocket(connectionSocket);

            videoConnection.SetInstrumentInformation(new InstrumentInformation(name,location,type));

            //Get outputQueue
            ConcurrentQueue<T> outputQueue = videoConnection.GetOutputQueue();

            //Do main loop
            while (!listenerCancellationToken.IsCancellationRequested) {
                //Get Incoming object
                T newObj=NetworkingOperations.ReceiveObjectWithSocket<T>(connectionSocket);

                try {
                    //try to cast newObj
                    T newObject = (T)newObj;
                    //Put in output-queue
                    outputQueue.Enqueue(newObject);
                } catch (InvalidCastException) {
                    Console.WriteLine("could not cast T");
                    throw;
                }
            }
            //remove connection
            removeVideoConnection(videoConnection);
        }

        /// <summary>
        /// Add item to the list
        /// </summary>
        private void addVideoConnection(VideoConnection<T> connection) {
            lock (listVideoConnections) {
                listVideoConnections.Add(connection);
            }
        }

        /// <summary>
        /// Remove the client connection from the list
        /// </summary>
        /// <param name="connection"> Video Connection</param>
        /// <returns>Boolean representing successful removal</returns>
        private bool removeVideoConnection(VideoConnection<T> connection) {
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
        /// Get the list of video connection objects
        /// </summary>
        /// <returns>List of video-connection objects of type T</returns>
        public List<VideoConnection<T>> getVideoConnectionList() {
            lock (listVideoConnections) {
                return listVideoConnections;
            }
        }
    }
}