using System.Collections.Generic;
using Server_Library.Connection_Classes;

namespace Blazor_Instrument_Cluster.Server.RemoteDevice {

	/// <summary>
	/// Interface for sharing video connection lists between classes
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public interface IRemoteDeviceManager<U> {

		/// <summary>
		/// Adds a connection to the list of remote devices, If a remote device with the correct location and type don't exists create a new one
		/// </summary>
		public void addConnectionToRemoteDevices(ConnectionBase connection);

		/// <summary>
		/// get a remote device with the parameters matching location and type
		/// </summary>
		/// <param name="name"></param>
		/// <param name="location"></param>
		/// <param name="type"></param>
		/// <param name="outputDevice"> The device that was found</param>
		/// <returns>IF it was successfully found or not</returns>
		public bool getRemoteDeviceWithNameLocationAndType(string name, string location, string type,out RemoteDevice<U> outputDevice);

		public List<RemoteDevice<U>> getListOfRemoteDevices();

	}
}