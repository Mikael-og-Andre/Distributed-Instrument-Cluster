using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net;

namespace Instrument_Communicator_Library.Server_Listener {
    public class ListenerVideo : ListenerBase {

        public ListenerVideo(IPEndPoint ipEndPoint, int maxConnections = 30, int maxPendingConnections = 30) : base(ipEndPoint, maxConnections, maxPendingConnections) {

        }

        protected override object createConnectionType(Socket socket, Thread thread) {
            throw new NotImplementedException();
        }

        protected override void handleIncomingConnection(object obj) {
            throw new NotImplementedException();
        }
    }
}
