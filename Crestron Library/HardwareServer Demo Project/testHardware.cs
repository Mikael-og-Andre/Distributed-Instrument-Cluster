using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;

namespace HardwareCommunicator {
    class testHardware {

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
