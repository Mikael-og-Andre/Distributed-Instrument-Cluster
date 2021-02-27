﻿using Blazor_Instrument_Cluster.Server.Events;
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

		//Providers
		private List<VideoConnectionFrameProvider<T>> listFrameProviders;     //Frame Providers

		//Connections
		private List<CrestronConnection> listCrestronConnections;   //Crestron connections
		private List<VideoConnection<T>> listVideoConnections;      //Video connections

		public RemoteDeviceConnection(ILogger<RemoteDeviceConnection<T>> logger, IServiceProvider services) {
			this.services = services;
			this.logger = logger;
			listFrameProviders = new List<VideoConnectionFrameProvider<T>>();
		}

		/// <summary>
		/// Set the crestron connection list
		/// </summary>
		/// <param name="listCrestronConnection"></param>
		public void SetCrestronConnectionList(List<CrestronConnection> listCrestronConnection) {
			//Set list
			listCrestronConnections = listCrestronConnection;
		}

		/// <summary>
		/// Set the video connection list
		/// </summary>
		/// <param name="listVideoConnection">List of video connections</param>
		public void SetVideoConnectionList(List<VideoConnection<T>> listVideoConnection) {
			//Set list
			listVideoConnections = listVideoConnection;
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

		/// <summary>
		/// Get a concurrent queue with the device name of the input name
		/// </summary>
		/// <param name="queue">Concurrent queue of messages coming from the crestron device</param>
		/// <param name="name">Name of the wanted device</param>
		/// <returns>Successfully found or not</returns>
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
		/// <summary>
		/// Gets the output queue of the the video connection device of the same name
		/// </summary>
		/// <param name="queue"> Concurrent queue</param>
		/// <param name="name"> name of the wanted device</param>
		/// <returns>found or not bool</returns>
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

		/// <summary>
		/// Subscribes to the appropriate video provider with the name passed in
		/// </summary>
		/// <param name="unsubscriber"> IDisposable to remove subscription</param>
		/// <param name="name"> Name of the wanted provider</param>
		/// <param name="consumer"> VideoConnectionFrameConsumer</param>
		/// <returns>Found the device or not</returns>
		public bool SubscribeToVideoProviderWithName(string name, VideoConnectionFrameConsumer<T> consumer) {
			//Lock provider list
			lock (listFrameProviders) {
				foreach (VideoConnectionFrameProvider<T> provider in listFrameProviders) {
					string providerName = provider.name;
					//Check if its the name we are looking for
					if (providerName.ToLower().Equals(name)) {
						//Subscribe the consumer to the provider
						consumer.Subscribe(provider);
						return true;
					}
				}
				
				return false;
			}
		}

		public void AddFrameProviderToListOfProviders(VideoConnectionFrameProvider<T> frameProvider) {
			lock (listFrameProviders) {
				listFrameProviders.Add(frameProvider);
			}
		}
	}
}