using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;

/// <summary>
/// Class that represents a client connection to the server unit.
/// @Author Mikael Nilssen
/// </summary>

namespace HardwareCommunicator {
    public class ClientConnection {

        private Socket socket { get; set; }     //Socket of the client Connection
        ConcurrentQueue<Command> concurrentQueueSendToClient;  // Concurrent queue for commands to send to the client
        ConcurrentQueue<Command> concurrentQueueRecieveFromClient;  //Concurrent queue for commands recieved by the client
        private Thread myThread; // The thread the connection is running on

        public ClientConnection(Socket socket, Thread thread) {
            this.socket = socket;
            this.myThread = thread;
        }


    }
}
