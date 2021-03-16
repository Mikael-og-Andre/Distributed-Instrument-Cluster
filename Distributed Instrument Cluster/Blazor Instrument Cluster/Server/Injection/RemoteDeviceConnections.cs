using Blazor_Instrument_Cluster.Server.Events;
using Server_Library;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Blazor_Instrument_Cluster.Server.Object;
using Server_Library.Connection_Classes;
using Server_Library.Connection_Types;
using Server_Library.Connection_Types.deprecated;

namespace Blazor_Instrument_Cluster.Server.Injection {

	/// <summary>
	/// Class for storing connection lists
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class RemoteDeviceConnections<T,U> : IRemoteDeviceConnections<T,U> {

		/// <summary>
		/// Services
		/// </summary>
		private IServiceProvider services;

		/// <summary>
		/// Logger
		/// </summary>
		private ILogger<RemoteDeviceConnections<T,U>> logger;

		/// <summary>
		/// Frame Providers
		/// </summary>
		private List<ReceivingObjectProvider<T>> listFrameProviders;

		/// <summary>
		/// List of remote devices
		/// </summary>
		private List<RemoteDevice> listRemoteDevices;

		/// <summary>
		/// Constructor, Injects logger and service provider
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		public RemoteDeviceConnections(ILogger<RemoteDeviceConnections<T,U>> logger, IServiceProvider services) {
			this.services = services;
			this.logger = logger;
			listFrameProviders = new List<ReceivingObjectProvider<T>>();
			
		}

		public void addConnectionToRemoteDevices(ConnectionBase connection) {
			throw new NotImplementedException();
		}

		public RemoteDevice getRemoteDeviceWithLocationAndType(string location, string type) {
			throw new NotImplementedException();
		}

		public bool subscribeToObjectProviderWithName(string name, ReceivingObjectConsumer<T> consumer) {
			throw new NotImplementedException();
		}
	}
}