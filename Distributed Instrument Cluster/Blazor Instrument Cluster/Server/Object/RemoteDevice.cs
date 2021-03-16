using Server_Library.Connection_Types;
using System.Collections.Generic;
using Blazor_Instrument_Cluster.Server.Events;

namespace Blazor_Instrument_Cluster.Server.Object {
	/// <summary>
	/// A remote device connected to the server
	/// Stores data about connections belonging to each device, and the providers
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="U"></typeparam>
	public class RemoteDevice<T, U> {

		/// <summary>
		/// Top level name of the device
		/// </summary>
		public string name { get; set; }

		/// <summary>
		/// location of the device
		/// </summary>
		public string location { get; set; }

		/// <summary>
		/// type of the device
		/// </summary>
		public string type { get; set; }

		/// <summary>
		/// List of sending connections for the device
		/// </summary>
		private List<SendingConnection<U>> listOfSendingConnections;

		/// <summary>
		/// List of Receiving connections for the device
		/// </summary>
		private List<ReceivingConnection<T>> listOfReceivingConnections;

		/// <summary>
		/// List of video object providers
		/// </summary>
		private List<VideoObjectProvider<T>> listOfReceivingConnectionProviders;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name"></param>
		/// <param name="location"></param>
		/// <param name="type"></param>
		public RemoteDevice(string name, string location, string type) {
			this.name = name;
			this.location = location;
			this.type = type;
			this.listOfSendingConnections = new List<SendingConnection<U>>();
			this.listOfReceivingConnections = new List<ReceivingConnection<T>>();
			this.listOfReceivingConnectionProviders = new List<VideoObjectProvider<T>>();
		}

		/// <summary>
		/// Adds a receiving connection to the list of receiving connections for this remote device
		/// </summary>
		/// <param name="receivingConnection"></param>
		public void addReceivingConnection(ReceivingConnection<T> receivingConnection) {
			lock (listOfReceivingConnections) {
				listOfReceivingConnections.Add(receivingConnection);
			}
		}

		/// <summary>
		/// Adds a sending connection tot he list of sending connections for this remote device
		/// </summary>
		/// <param name="sendingConnection"></param>
		public void addSendingConnection(SendingConnection<U> sendingConnection) {
			lock (listOfSendingConnections) {
				listOfSendingConnections.Add(sendingConnection);	
			}
		}
	}
}