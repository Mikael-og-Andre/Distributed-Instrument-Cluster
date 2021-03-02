using Blazor_Instrument_Cluster.Server.Events;
using Instrument_Communicator_Library;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Instrument_Communicator_Library.Information_Classes;

namespace Blazor_Instrument_Cluster.Server.Injection {

	/// <summary>
	/// Interface for sharing video connection lists between classes
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IRemoteDeviceConnections {

		public void SetCrestronConnectionList(List<CrestronConnection> listCrestronConnections);

		public void SetVideoConnectionList(List<VideoConnection> listVideoConnections);

		public bool GetCrestronConnectionList(out List<CrestronConnection> listCrestronConnections);

		public bool GetVideoConnectionList(out List<VideoConnection> listVideoConnections);

		public bool GetCrestronConnectionWithName(out CrestronConnection connection, string name);

		public bool GetVideoConcurrentQueueWithName(out ConcurrentQueue<VideoFrame> queue, string name);

		public bool SubscribeToVideoProviderWithName(string name, VideoConnectionFrameConsumer consumer);
	}
}