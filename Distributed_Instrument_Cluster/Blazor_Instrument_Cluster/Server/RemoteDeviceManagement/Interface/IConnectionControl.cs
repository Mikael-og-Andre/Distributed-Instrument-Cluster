using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server.RemoteDeviceManagement.Interface {
	/// <summary>
	/// Interface for controlling a connection
	/// <Author>Mikael Nilssen</Author>
	/// </summary>
	public interface IConnectionControl {

		public bool ping();

		public Task send(string msg);

		public Task<string> receive();
	}
}
