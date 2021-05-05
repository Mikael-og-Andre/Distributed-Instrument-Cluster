using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server.RemoteDeviceManagement.Interface {
	/// <summary>
	/// Interface for preforming actions with remoteDevices and disconnecting, reconnecting or checking if the endpoint is available
	/// <Author>Mikael Nilssen</Author>
	/// </summary>
	public interface IRemoteDeviceStatus {

		public bool isConnected();

		public void disconnect();

		public Task reconnect();

		public bool ping(int timeout);

	}
}
