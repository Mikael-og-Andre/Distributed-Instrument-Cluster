﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using InstrumentCommunicator;
using System.Collections.Immutable;
using System.Collections.Concurrent;
/// <summary>
/// Class for testing server/client communication
/// <author>Mikael Nilssen</author>
/// </summary>

namespace Server_And_Demo_Project {

    internal class Test {

        public static void Main(string[] args) {


            int port = 5050;
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            InstrumentServer instumentServer = new InstrumentServer(endPoint);
            Thread serverThread = new Thread(() => instumentServer.StartListening());
            serverThread.IsBackground = false;
            serverThread.Start();
            Thread.Sleep(10000);
            //instumentServer.StopServer();
            string ip = "127.0.0.1";
            InstrumentClient client = new InstrumentClient(ip,port);
            Thread clientThread = new Thread(() => client.start());
            clientThread.Start();
            InstrumentClient client2 = new InstrumentClient(ip, port);
            Thread clientThread2 = new Thread(() => client2.start());
            clientThread2.Start();
            InstrumentClient client3 = new InstrumentClient(ip, port);
            Thread clientThread3 = new Thread(() => client3.start());
            clientThread3.Start();
            Thread.Sleep(20000);
            List<ClientConnection> connections = instumentServer.getClientConnections();
            Console.WriteLine("populating messages");
            for (int i = 0; i<connections.Count;i++) {
                ClientConnection connection = connections[i];
                ConcurrentQueue<Message> queue = connection.getInputQueue();
                string[] strings = new string[] { "Hello","this", "is","a","test"};
                Message newMessage = new Message(protocolOption.message,strings);

                queue.Enqueue(newMessage);
            }
            Thread.Sleep(1000);
            Console.WriteLine("populating messages");
            for (int i = 0; i < connections.Count; i++) {
                ClientConnection connection = connections[i];
                ConcurrentQueue<Message> queue = connection.getInputQueue();
                string[] strings = new string[] { "Hello", "this", "is", "a", "test" };
                Message newMessage = new Message(protocolOption.message, strings);

                queue.Enqueue(newMessage);
            }
            Console.WriteLine("populating messages");
            for (int i = 0; i < connections.Count; i++) {
                ClientConnection connection = connections[i];
                ConcurrentQueue<Message> queue = connection.getInputQueue();
                string[] strings = new string[] { "Hello", "this", "is", "a", "test" };
                Message newMessage = new Message(protocolOption.message, strings);

                queue.Enqueue(newMessage);
            }

            Console.ReadLine();

        }
    }
}