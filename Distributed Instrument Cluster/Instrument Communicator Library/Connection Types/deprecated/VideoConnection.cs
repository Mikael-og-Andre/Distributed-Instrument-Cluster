﻿using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using Server_Library.Connection_Classes;
using Server_Library.Authorization;

namespace Server_Library.Connection_Types.deprecated {

	/// <summary>
	/// Class holds information about a video connection that will be used to match it up with the pairing control socket
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class VideoConnection : ConnectionBaseOld {

		/// <summary>
		/// Queue of items received by the connection
		/// </summary>
		private readonly ConcurrentQueue<VideoFrame> outputQueue;

		public VideoConnection(Thread homeThread, Socket socket, AccessToken accessToken, ClientInformation info, CancellationToken cancellation) : base(homeThread, socket,accessToken,info, cancellation) {
			this.outputQueue = new ConcurrentQueue<VideoFrame>();
		}

		/// <summary>
		/// returns queue to store received objects in
		/// </summary>
		/// <returns>Concurrent queue</returns>
		public ConcurrentQueue<VideoFrame> getOutputQueue() {
			return outputQueue;
		}
	}
}