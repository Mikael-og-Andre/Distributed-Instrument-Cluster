using Instrument_Communicator_Library;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Instrument_Communicator_Library.Authorization;
using Instrument_Communicator_Library.Connection_Types;
using Instrument_Communicator_Library.Connection_Types.deprecated;
using Instrument_Communicator_Library.Enums;
using Instrument_Communicator_Library.Server_Listeners;
using Instrument_Communicator_Library.Server_Listeners.deprecated;
using Instrument_Communicator_Library.Socket_Clients;

namespace Server_And_Demo_Project {

    /// <summary>
    /// Class for testing server/client communication
    /// <author>Mikael Nilssen</author>
    /// </summary>
    internal class CrestronTest {

        public static void Main(string[] args) {
            int portCrestron = 5050;
            int portVideo = 5051;
            IPEndPoint endpointCrestron = new IPEndPoint(IPAddress.Parse("127.0.0.1"), portCrestron);
            IPEndPoint endpointVideo = new IPEndPoint(IPAddress.Parse("127.0.0.1"), portVideo);

            ListenerCrestron listenerCrestron = new ListenerCrestron(endpointCrestron);
            Thread serverThread = new Thread(() => listenerCrestron.start()) { IsBackground = false };

            serverThread.Start();
            Thread.Sleep(10000);
            //instumentServer.StopServer();
            string ip = "127.0.0.1";
            AccessToken accessToken = new AccessToken("access");
            InstrumentInformation info = new InstrumentInformation("Device 1", "Location 1", "sample type");
            CancellationToken cancellationToken = new CancellationToken(false);

            CrestronClient client = new CrestronClient(ip, portCrestron, info, accessToken, cancellationToken);
            Thread clientThread = new Thread(() => client.run());
            clientThread.Start();

            AccessToken accessToken2 = new AccessToken("access");
            InstrumentInformation info2 = new InstrumentInformation("Device 2", "Location 2", "sample type 2");
            CancellationToken cancellationToken2 = new CancellationToken(false);

            CrestronClient client2 = new CrestronClient(ip, portCrestron, info2, accessToken2, cancellationToken2);
            Thread clientThread2 = new Thread(() => client2.run());
            clientThread2.Start();

            AccessToken accessToken3 = new AccessToken("acess");
            InstrumentInformation info3 = new InstrumentInformation("Device 3", "Location 3", "sample type 3");
            CancellationToken cancellationToken3 = new CancellationToken(false);

            CrestronClient client3 = new CrestronClient(ip, portCrestron, info3, accessToken3, cancellationToken3);
            Thread clientThread3 = new Thread(() => client3.run());
            clientThread3.Start();

            Thread.Sleep(2000);
            List<CrestronConnection> crestronConnection = listenerCrestron.getCrestronConnectionList();

            Console.WriteLine("populating messages");

            for (int i = 0; i < crestronConnection.Count; i++) {
                CrestronConnection connection = crestronConnection[i];
                ConcurrentQueue<Message> queue = connection.getSendingQueue();
                string stringy = "Hello this is a test";
                Message newMessage = new Message(ProtocolOption.message, stringy);

                queue.Enqueue(newMessage);
            }
            Console.WriteLine("populating messages");

            foreach (CrestronConnection connection in crestronConnection) {
                ConcurrentQueue<Message> queue = connection.getSendingQueue();
                string stringy = "Wow i dont like greens";
                Message newMessage = new Message(ProtocolOption.message, stringy);

                queue.Enqueue(newMessage);
            }

            Console.ReadLine();
        }
    }
}