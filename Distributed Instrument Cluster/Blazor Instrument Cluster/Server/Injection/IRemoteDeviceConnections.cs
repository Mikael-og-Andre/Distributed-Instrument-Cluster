using Blazor_Instrument_Cluster.Server.Events;
using Server_Library;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Blazor_Instrument_Cluster.Server.Object;
using Server_Library.Connection_Classes;
using Server_Library.Connection_Types;
using Server_Library.Connection_Types.deprecated;

namespace Blazor_Instrument_Cluster.Server.Injection {

	/// <summary>
	/// Interface for sharing video connection lists between classes
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public interface IRemoteDeviceConnections<T,U> {


		/// <summary>
		/// Adds a connection to the list of remote devices, If a remote device with the correct location and type don't exists create a new one
		/// </summary>
		public void addConnectionToRemoteDevices(ConnectionBase connection);

		/// <summary>
		/// get a remote device with the parameters matching location and type
		/// </summary>
		/// <param name="Location"></param>
		/// <param name="Type"></param>
		/// <returns>Remote Device</returns>
		public RemoteDevice<T,U> getRemoteDeviceWithNameLocationAndType<T,U>(string name,string location,string type);

		/// <summary>
		/// Subscribes the Consumer to a video provider with the name inputted
		/// </summary>
		/// <param name="name">Name of the device of the wanted Video Stream</param>
		/// <param name="consumer">Consumer for incoming objects</param>
		/// <returns>If provider with name was found or not</returns>
		public bool subscribeToObjectProviderWithName(string name,string location, string type, string subname, VideoObjectConsumer<T> consumer);
	}
}