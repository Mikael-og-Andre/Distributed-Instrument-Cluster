using System;

/// <summary>
/// Client for connecting and recieving commands from server unit
/// @Author Mikael Nilssen
/// </summary>

namespace HardwareCommunicator {

    public class InstrumentClient {
        public string ip { get; private set; } //Ip address of target server
        public int port { get; private set; } //Port of target server

        public InstrumentClient(string ip, int port) {
            this.ip = ip;
            this.port = port;
        }

        public void run() {

        }

    }
}
