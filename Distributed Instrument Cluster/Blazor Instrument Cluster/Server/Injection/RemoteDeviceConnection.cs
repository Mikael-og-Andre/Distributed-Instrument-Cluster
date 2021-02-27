using Instrument_Communicator_Library;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Blazor_Instrument_Cluster.Server.Injection {

	/// <summary>
	/// Class for storing connection list
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class RemoteDeviceConnection<T> : IRemoteDeviceConnections<T> {

		//Injections
		private IServiceProvider services;                      //Services
		private ILogger<RemoteDeviceConnection<T>> logger;      //Logger

		//Connections
		private List<CrestronConnection> listCrestronConnections;   //Crestron connections

		private List<VideoConnection<T>> listVideoConnections;      //Video connections

		public RemoteDeviceConnection(ILogger<RemoteDeviceConnection<T>> logger, IServiceProvider services) {
			this.services = services;
			this.logger = logger;
		}

		public void SetCrestronConnectionList(List<CrestronConnection> listCrestronConnection) {
			//Set list

			listCrestronConnections = listCrestronConnection;
		}

		public void SetVideoConnectionList(List<VideoConnection<T>> listVideoConnection) {
			//Set list
			listVideoConnections = listVideoConnection;
		}

		public bool GetCrestronConnectionList(out List<CrestronConnection> listCrestronConnection) {
			//Lock list
			lock (listCrestronConnections) {
				//check if list is null
				if (listCrestronConnections == null) {
					//return crestron connections
					listCrestronConnection = null;
					return false;
				} else {
					listCrestronConnection = listCrestronConnections;
					return true;
				}
			}
		}

		public bool GetVideoConnectionList(out List<VideoConnection<T>> listVideoConnection) {
			//Lock list
			lock (listVideoConnections) {
				//check if list is null
				if (listCrestronConnections == null) {
					listVideoConnection = null;
					return false;
				} else {
					//if not null set as output
					listVideoConnection = listVideoConnections;
					return true;
				}
			}
		}

		public bool GetCrestronConcurrentQueueWithName(out ConcurrentQueue<Message> queue, string name) {
			//Lock list
			lock (listCrestronConnections) {
				//Loop connection
				foreach (var connection in listCrestronConnections) {
					//Check name of connection
					InstrumentInformation instrumentInformation = connection.GetInstrumentInformation();
					if (instrumentInformation == null) continue;
					string infoName = instrumentInformation.name;
					if (!infoName.ToLower().Equals(name.ToLower())) continue;
					//Get queue and return true since it matched
					queue = connection.GetOutputQueue();
					return true;
				}
				//set null for object and return null
				queue = null;
				return false;
			}
		}

		public bool GetVideoConcurrentQueueWithName(out ConcurrentQueue<T> queue, string name) {
			//Lock list
			lock (listVideoConnections) {
				//Loop connection
				foreach (var connection in listVideoConnections) {
					//Check name of connection
					InstrumentInformation instrumentInformation = connection.GetInstrumentInformation();
					if (instrumentInformation == null) continue;
					string infoName = instrumentInformation.name;
					if (!infoName.ToLower().Equals(name.ToLower())) continue;
					//Get queue and return true since it matched
					queue = connection.GetOutputQueue();
					return true;
				}
				//set null for object and return null
				queue = null;
				return false;
			}
		}
	}
}