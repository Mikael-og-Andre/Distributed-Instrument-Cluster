using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Instrument_Communicator_Library.Authorization;
using Instrument_Communicator_Library.Remote_Device_side_Communicators;

namespace Instrument_Communicator_Library.Socket_Clients {
	/// <summary>
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class ReceivingClient : ClientBase{
		public ReceivingClient(string ip, int port, InstrumentInformation informationAboutClient, AccessToken accessToken, CancellationToken communicatorCancellationToken) : base(ip, port, informationAboutClient, accessToken, communicatorCancellationToken) { }
		protected override void handleConnected(Socket connectionSocket) {
			throw new NotImplementedException();
		}
	}
}
