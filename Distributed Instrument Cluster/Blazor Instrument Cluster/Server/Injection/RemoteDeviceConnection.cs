using Blazor_Instrument_Cluster.Server.Events;
using Instrument_Communicator_Library;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Blazor_Instrument_Cluster.Client.Pages;

namespace Blazor_Instrument_Cluster.Server.Injection {

	/// <summary>
	/// Class for storing connection lists
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class RemoteDeviceConnection : IRemoteDeviceConnections {

		//Injections
		private IServiceProvider services;                      //Services

		private ILogger<RemoteDeviceConnection> logger;      //Logger

		//Providers
		private List<VideoConnectionFrameProvider> listFrameProviders;     //Frame Providers

		//Connections
		private List<CrestronConnection> listCrestronConnections;   //Crestron connections
		private List<VideoConnection> listVideoConnections;      //Video connections

		public RemoteDeviceConnection(ILogger<RemoteDeviceConnection> logger, IServiceProvider services) {
			this.services = services;
			this.logger = logger;
			listFrameProviders = new List<VideoConnectionFrameProvider>();
			listCrestronConnections = new List<CrestronConnection>();
			listVideoConnections = new List<VideoConnection>();
		}

		/// <summary>
		/// Set the crestron connection list
		/// </summary>
		/// <param name="listCrestronConnection"></param>
		public void SetCrestronConnectionList(List<CrestronConnection> listCrestronConnection) {
			//Set list
			lock (listCrestronConnections) {
				listCrestronConnections = listCrestronConnection;
			}
		}

		/// <summary>
		/// Set the video connection list
		/// </summary>
		/// <param name="listVideoConnection">List of video connections</param>
		public void SetVideoConnectionList(List<VideoConnection> listVideoConnection) {
			//Set list
			lock (listVideoConnections) {
				listVideoConnections = listVideoConnection;
			}
		}

		/// <summary>
		/// Get the crestron connection list
		/// </summary>
		/// <param name="listCrestronConnection"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Get the list of video connections
		/// </summary>
		/// <param name="listVideoConnection"></param>
		/// <returns></returns>
		public bool GetVideoConnectionList(out List<VideoConnection> listVideoConnection) {
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

		/// <summary>
		/// Get the connection with the matching name
		/// </summary>
		/// <param name="con">Crestron connection output</param>
		/// <param name="name">Name of the wanted device</param>
		/// <returns>Successfully found or not</returns>
		public bool GetCrestronConnectionWithName(out CrestronConnection con, string name) {
			//Lock list
			lock (listCrestronConnections) {
				//Loop connection
				foreach (var connection in listCrestronConnections) {
					//Check name of connection
					InstrumentInformation instrumentInformation = connection.GetInstrumentInformation();
					if (instrumentInformation == null) continue;
					string infoName = instrumentInformation.Name;
					if (!infoName.ToLower().Equals(name.ToLower())) continue;
					//Get queue and return true since it matched
					con = connection;
					return true;
				}
				//set null for object and return null
				con = null;
				return false;
			}
		}
		/// <summary>
		/// Gets the output queue of the the video connection device of the same name
		/// </summary>
		/// <param name="queue"> Concurrent queue</param>
		/// <param name="name"> name of the wanted device</param>
		/// <returns>found or not bool</returns>
		public bool GetVideoConcurrentQueueWithName(out ConcurrentQueue<VideoFrame> queue, string name) {
			//Lock list
			lock (listVideoConnections) {
				//Loop connection
				foreach (var connection in listVideoConnections) {
					//Check name of connection
					InstrumentInformation instrumentInformation = connection.GetInstrumentInformation();
					if (instrumentInformation == null) continue;
					string infoName = instrumentInformation.Name;
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

		/// <summary>
		/// Subscribes to the appropriate video provider with the name passed in
		/// </summary>
		/// <param name="unsubscriber"> IDisposable to remove subscription</param>
		/// <param name="name"> Name of the wanted provider</param>
		/// <param name="consumer"> VideoConnectionFrameConsumer</param>
		/// <returns>Found the device or not</returns>
		public bool SubscribeToVideoProviderWithName(string name, VideoConnectionFrameConsumer consumer) {
			//Lock provider list
			lock (listFrameProviders) {
				foreach (VideoConnectionFrameProvider provider in listFrameProviders) {
					string providerName = provider.name;
					//Check if its the name we are looking for
					if (providerName.ToLower().Equals(name.ToLower())) {
						//Subscribe the consumer to the provider
						consumer.Subscribe(provider);
						return true;
					}
				}
				
				return false;
			}
		}
		/// <summary>
		/// Adds a provider for a specific video connection to the list of connections
		/// </summary>
		/// <param name="frameProvider"></param>
		public void AddFrameProviderToListOfProviders(VideoConnectionFrameProvider frameProvider) {
			lock (listFrameProviders) {
				listFrameProviders.Add(frameProvider);
			}
		}
	}
}