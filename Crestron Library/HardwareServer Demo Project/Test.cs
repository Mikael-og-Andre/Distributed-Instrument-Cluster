using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using InstrumentCommunicator;

/// <summary>
/// Class for testing server/client communication
/// <author>Mikael Nilssen</author>
/// </summary>

namespace Server_And_Demo_Project {

    internal class Test {

        public static void Main(string[] args) {

            Console.WriteLine(protocolOption.authorize.ToString());


            int port = 5050;
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            InstrumentServer instumentServer = new InstrumentServer(endPoint);
            Thread thread = new Thread(() => instumentServer.StartListening());
            thread.IsBackground = false;
            thread.Start();
            Thread.Sleep(1000);
            //instumentServer.StopServer();
            string ip = "127.0.0.1";
            InstrumentClient client = new InstrumentClient(ip,port);
            client.start();

            //Console.ReadLine();

        }
    }
}