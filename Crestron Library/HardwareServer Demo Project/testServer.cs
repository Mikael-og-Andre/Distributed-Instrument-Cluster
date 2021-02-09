using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using HardwareCommunicator;



/// <summary>
/// Class for testing server/client communication
/// <author>Mikael Nilssen</author>
/// </summary>

namespace HardwareServer_Demo_Project {
    class testServer {

        public static void Main(string[] args) {

            int port = 5050;
            

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

            InstrumentServer instumentServer = new InstrumentServer(endPoint);
            Thread thread = new Thread(() => instumentServer.StartListening());
            thread.Start();
            //Thread.Sleep(10000);
            //instumentServer.StopServer();
            Console.ReadLine();
        }
    }
}
