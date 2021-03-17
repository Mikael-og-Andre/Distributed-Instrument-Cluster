using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;

namespace Instrument_Communicator_Library {

	/// <summary>
	/// Class holds information about a video connection that will be used to match it up with the pairing control socket
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class VideoConnection {

		/// <summary>
		/// Socket connection
		/// </summary>
		private Socket socketConnection;

		/// <summary>
		/// Thread the connection will run on
		/// </summary>
		private Thread myThread;

		/// <summary>
		/// Information about the device
		/// </summary>
		private InstrumentInformation info;

		/// <summary>
		/// Queue of items received by the connection
		/// </summary>
		private ConcurrentQueue<VideoFrame> outputQueue;

		public bool running { get; set; } = true;

		/// <summary>
		/// Has the instrument information been shared
		/// </summary>
		public bool hasInstrument { get; set; } = false;

		public VideoConnection(Socket socketConnection, Thread thread, InstrumentInformation info = null) {
			this.socketConnection = socketConnection;
			this.myThread = thread;
			this.outputQueue = new ConcurrentQueue<VideoFrame>();
			if (info != null) {
				hasInstrument = true;
			}
			this.info = info;
		}

		/// <summary>
		/// returns queue to store received objects in
		/// </summary>
		/// <returns>Concurrent queue</returns>
		public ConcurrentQueue<VideoFrame> GetOutputQueue() {
			return outputQueue;
		}

		/// <summary>
		/// Returns socket
		/// </summary>
		/// <returns>Socket</returns>
		public Socket GetSocket() {
			return socketConnection;
		}

		/// <summary>
		/// Set instrument information
		/// </summary>
		/// <param name="instrumentInformation">Instrument Information object</param>
		public void SetInstrumentInformation(InstrumentInformation instrumentInformation) {
			this.info = instrumentInformation;
			this.hasInstrument = true;
		}

		/// <summary>
		/// Get the instrument information object
		/// </summary>
		/// <returns></returns>
		public InstrumentInformation GetInstrumentInformation() {
			if (hasInstrument) {
				return this.info;
			}

			return null;
		}

		public bool isRunning() {
			return this.running;
		}
	}
}