using Instrument_Communicator_Library;
using Instrument_Communicator_Library.Server_Listener;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;

/// <summary>
/// Class for testing server/client communication
/// <author>Mikael Nilssen</author>
/// </summary>

namespace Server_And_Demo_Project {

    internal class Test {

        public static void Main(string[] args) {
            int portCrestron = 5050;
            int portVideo = 5051;
            IPEndPoint endpointCrestron = new IPEndPoint(IPAddress.Parse("127.0.0.1"), portCrestron);
            IPEndPoint endpointVideo = new IPEndPoint(IPAddress.Parse("127.0.0.1"), portVideo);

            ListenerCrestron listenerCrestron = new ListenerCrestron(endpointCrestron);
            Thread serverThread = new Thread(() => listenerCrestron.Start());

            serverThread.IsBackground = false;
            serverThread.Start();
            Thread.Sleep(10000);
            //instumentServer.StopServer();
            string ip = "127.0.0.1";
            AccessToken accessToken = new AccessToken("access");
            InstrumentInformation info = new InstrumentInformation("Device 1", "Location 1", "sample type");
            CancellationToken cancellationToken = new CancellationToken(false);

            CrestronCommunicator client = new CrestronCommunicator(ip, portCrestron, info, accessToken, cancellationToken);
            Thread clientThread = new Thread(() => client.Start());
            clientThread.Start();

            AccessToken accessToken2 = new AccessToken("access");
            InstrumentInformation info2 = new InstrumentInformation("Device 2", "Location 2", "sample type 2");
            CancellationToken cancellationToken2 = new CancellationToken(false);

            CrestronCommunicator client2 = new CrestronCommunicator(ip, portCrestron, info2, accessToken2, cancellationToken2);
            Thread clientThread2 = new Thread(() => client2.Start());
            clientThread2.Start();

            AccessToken accessToken3 = new AccessToken("acess");
            InstrumentInformation info3 = new InstrumentInformation("Device 3", "Location 3", "sample type 3");
            CancellationToken cancellationToken3 = new CancellationToken(false);

            CrestronCommunicator client3 = new CrestronCommunicator(ip, portCrestron, info3, accessToken3, cancellationToken3);
            Thread clientThread3 = new Thread(() => client3.Start());
            clientThread3.Start();

            Thread.Sleep(2000);
            List<CrestronConnection> crestronConnection = listenerCrestron.GetCrestronConnectionList();

            Console.WriteLine("populating messages");
            lock (crestronConnection) {
                for (int i = 0; i < crestronConnection.Count; i++) {
                    lock (crestronConnection) {
                        CrestronConnection connection = crestronConnection[i];
                        ConcurrentQueue<Message> queue = connection.getInputQueue();
                        string[] strings = new string[] { "Hello", "this", "is", "a", "test" };
                        Message newMessage = new Message(protocolOption.message, strings);

                        queue.Enqueue(newMessage);
                    }
                }
            }
            Console.WriteLine("populating messages");

            lock (crestronConnection) {
                for (int i = 0; i < crestronConnection.Count; i++) {
                    CrestronConnection connection = crestronConnection[i];
                    ConcurrentQueue<Message> queue = connection.getInputQueue();
                    string[] strings = new string[] { "wow", "i", "dont", "like", "greens" };
                    Message newMessage = new Message(protocolOption.message, strings);

                    queue.Enqueue(newMessage);
                }
            }

            Console.ReadLine();
        }
    }
}