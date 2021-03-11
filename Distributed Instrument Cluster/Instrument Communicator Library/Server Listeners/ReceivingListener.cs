using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Instrument_Communicator_Library.Server_Listeners {
	/// <summary>
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class ReceivingListener : ListenerBase{
		public ReceivingListener(IPEndPoint ipEndPoint, int maxConnections = 30, int maxPendingConnections = 30) : base(ipEndPoint, maxConnections, maxPendingConnections) { }
		protected override void handleIncomingConnection(object obj) {
			throw new NotImplementedException();
		}

		protected override object createConnectionType(Socket socket, Thread thread) {
			throw new NotImplementedException();
		}
	}
}
