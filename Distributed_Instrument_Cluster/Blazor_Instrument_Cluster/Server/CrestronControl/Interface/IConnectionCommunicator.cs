using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server.CrestronControl.Interface {
	/// <summary>
	/// Interface for controlling a connection
	/// <Author>Mikael Nilssen</Author>
	/// </summary>
	public interface IConnectionCommunicator {

		public bool ping();

		public Task<bool> send(string msg);

		public Task<string> receive();

		public bool isReady();

		public bool ensureUP();
	}
}
