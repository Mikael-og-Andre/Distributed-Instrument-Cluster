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

    internal class testClient {

        public static void Main(string[] args) {
            int port = 5050;
            string ip = "127.0.0.1";
            InstrumentClient client = new InstrumentClient(ip,port);

        }
    }
}