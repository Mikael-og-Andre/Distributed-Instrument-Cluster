﻿using Blazor_Instrument_Cluster.Server.Events;
using Server_Library;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Server_Library.Connection_Types;
using Server_Library.Connection_Types.deprecated;

namespace Blazor_Instrument_Cluster.Server.Injection {

	/// <summary>
	/// Interface for sharing video connection lists between classes
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public interface IRemoteDeviceConnections {
		/// <summary>
		/// Set the Crestron Connection List
		/// </summary>
		/// <param name="listCrestronConnections">List containing all crestron connections</param>
		public void setCrestronConnectionList(List<CrestronConnection> listCrestronConnections);
		/// <summary>
		/// Set video Connection list
		/// </summary>
		/// <param name="listVideoConnections"> List Containing Video Connections</param>
		public void setVideoConnectionList(List<VideoConnection> listVideoConnections);
		/// <summary>
		/// Get list of Crestron Connections
		/// </summary>
		/// <param name="listCrestronConnections">List of type CrestronConnection</param>
		/// <returns>success or not</returns>
		public bool getCrestronConnectionList(out List<CrestronConnection> listCrestronConnections);
		/// <summary>
		/// Get list with videoConnections
		/// </summary>
		/// <param name="listVideoConnections">List of type VideoConnection</param>
		/// <returns>success or not</returns>
		public bool getVideoConnectionList(out List<VideoConnection> listVideoConnections);
		/// <summary>
		/// Get a specific crestron connection with the instrument name matching the input name
		/// </summary>
		/// <param name="connection">Output connection</param>
		/// <param name="name">Name of wanted device</param>
		/// <returns>If it was found or not</returns>
		public bool getCrestronConnectionWithName(out CrestronConnection connection, string name);
		/// <summary>
		/// Get a concurrent queue from a video connection with the specified name
		/// </summary>
		/// <param name="queue"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool getVideoConcurrentQueueWithName(out ConcurrentQueue<VideoFrame> queue, string name);
		/// <summary>
		/// Subscribes the Consumer to a video provider with the name inputted
		/// </summary>
		/// <param name="name">Name of the device of the wanted Video Stream</param>
		/// <param name="consumer">Consumer for video frames</param>
		/// <returns>If provider with name was found or not</returns>
		public bool subscribeToVideoProviderWithName(string name, VideoConnectionFrameConsumer consumer);
	}
}