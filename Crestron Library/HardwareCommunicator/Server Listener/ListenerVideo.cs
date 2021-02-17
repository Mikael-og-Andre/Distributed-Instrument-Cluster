using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net;

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

            //Do main loop
            while (!listenerCancellationToken.IsCancellationRequested) {
                


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

    }
}
