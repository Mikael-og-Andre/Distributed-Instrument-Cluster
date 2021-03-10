using System;
using System.Net.Sockets;
using System.Threading;
using Instrument_Communicator_Library.Authorization;

namespace Instrument_Communicator_Library.Connection_Classes {

	/// <summary>
	/// Base Class for a connection made to the listener
	/// </summary>
	public abstract class ConnectionBase {

		/// <summary>
		/// The thread the connection is running on
		/// </summary>
		private Thread homeThread;

		/// <summary>
		/// Token representing a valid connection to the server
		/// </summary>
		protected AccessToken accessToken { get; set; } = null;

		/// <summary>
		///	Has the Access token been received
		/// </summary>
		protected bool hasAccessToken { get; set; } = false;

		/// <summary>
		/// has the connection been Authorized
		/// </summary>
		public bool isAuthorized { get; set; } = false;

		/// <summary>
		/// Is the connection running
		/// </summary>
		public bool isActive { get; set; } = true;

		/// <summary>
		/// Information about remote device
		/// </summary>
		protected InstrumentInformation info { get; set; } = null;

		/// <summary>
		/// Has the instrument Information been received
		/// </summary>
		public bool hasInstrument { get; private set; } = false;

		/// <summary>
		/// Socket of the client Connection
		/// </summary>
		protected Socket socket { get; private set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="homeThread"> Thread Connection is running on</param>
		/// <param name="socket"></param>
		protected ConnectionBase(Thread homeThread, Socket socket) {
			this.homeThread = homeThread;
			this.socket = socket;
		}

		/// <summary>
		/// Returns socket
		/// </summary>
		/// <returns>Socket</returns>
		public Socket getSocket() {
			return socket;
		}

		/// <summary>
		/// Set the instrument information on connection
		/// </summary>
		/// <param name="instrumentInformation">IInstrument Information</param>
		public void setInstrumentInformation(InstrumentInformation instrumentInformation) {
			this.info = instrumentInformation;
			hasInstrument = true;
		}

		/// <summary>
		/// Get the Instrument information from Crestron connection
		/// </summary>
		/// <returns>Instrument Information</returns>
		public InstrumentInformation getInstrumentInformation() {
			if (hasInstrument) {
				return info;
			}
			throw new NullReferenceException("Instrument information has not been set yet");
		}

		public void setAccessToken(AccessToken accessToken) {
			this.accessToken = accessToken;
			hasAccessToken = true;
		}
	}
}