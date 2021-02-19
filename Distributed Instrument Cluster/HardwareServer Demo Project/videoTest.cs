﻿using Instrument_Communicator_Library;
using Instrument_Communicator_Library.Server_Listener;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;

/// <summary>
/// Used for testing video part of lib
/// </summary>

namespace Server_And_Demo_Project {

    internal class videoTest {

        public static void Main(string[] args) {
            int portVideo = 5051;
            IPEndPoint endpointVid = new IPEndPoint(IPAddress.Parse("127.0.0.1"), portVideo);

            ListenerVideo<string> vidListener = new ListenerVideo<string>(endpointVid);

            Thread videoListenerThread = new Thread(() => vidListener.Start());
            videoListenerThread.Start();

            List<VideoConnection<string>> listListenerConnections = vidListener.getVideoConnectionList();

            //Wait so server runs before connecting
            Thread.Sleep(1000);
            //Communicator
            InstrumentInformation info = new InstrumentInformation("name", "loc", "type");
            AccessToken accessToken = new AccessToken("access");

            VideoCommunicator<string> vidCom = new VideoCommunicator<string>("127.0.0.1", 5051, info, accessToken);
            Thread vidComThread = new Thread(() => vidCom.start());
            vidComThread.Start();

            ConcurrentQueue<string> inputQueue = vidCom.getInputQueue();

            Thread loaderThread = new Thread((object inputQueue) => loadQueue(inputQueue));
            loaderThread.Start(inputQueue);

            while (true) {
                lock (listListenerConnections) {
                    foreach (VideoConnection<string> con in listListenerConnections) {
                        ConcurrentQueue<string> outp = con.getOutputQueue();

                        string s;
                        bool hadValue = outp.TryDequeue(out s);
                        if (hadValue) {
                            Console.WriteLine("Output queue pushes " + s);
                        }
                    }
                }
            }
        }

        public static void loadQueue(object que) {
            ConcurrentQueue<string> queue = (ConcurrentQueue<string>)que;

            for (int i = 0; i < 100; i++) {
                queue.Enqueue("int is " + i);
                Thread.Sleep(1000);
            }
        }
    }
}