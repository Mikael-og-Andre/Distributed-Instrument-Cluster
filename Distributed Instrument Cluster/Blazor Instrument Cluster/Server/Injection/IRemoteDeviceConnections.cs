using Instrument_Communicator_Library;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Blazor_Instrument_Cluster.Server.Injection {

	/// <summary>
	/// Interface for sharing video connection lists between classes
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IRemoteDeviceConnections<T> {

		public void SetCrestronConnectionList(List<CrestronConnection> listCrestronConnections);

		public void SetVideoConnectionList(List<VideoConnection<T>> listVideoConnections);

		public bool GetCrestronConnectionList(out List<CrestronConnection> listCrestronConnections);

		public bool GetVideoConnectionList(out List<VideoConnection<T>> listVideoConnections);

		public bool GetCrestronConcurrentQueueWithName(out ConcurrentQueue<Message> queue, string name);

		public bool GetVideoConcurrentQueueWithName(out ConcurrentQueue<T> queue, string name);
	}
}