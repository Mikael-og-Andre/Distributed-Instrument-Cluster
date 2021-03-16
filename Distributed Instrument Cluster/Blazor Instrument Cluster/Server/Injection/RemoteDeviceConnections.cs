using Blazor_Instrument_Cluster.Server.Events;
using Blazor_Instrument_Cluster.Server.Object;
using Microsoft.Extensions.Logging;
using Server_Library;
using Server_Library.Connection_Classes;
using Server_Library.Connection_Types;
using System;
using System.Collections.Generic;

namespace Blazor_Instrument_Cluster.Server.Injection {

	/// <summary>
	/// Class for storing connection lists
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class RemoteDeviceConnections<T, U> : IRemoteDeviceConnections<T, U> {

		/// <summary>
		/// Services
		/// </summary>
		private IServiceProvider services;

		/// <summary>
		/// Logger
		/// </summary>
		private ILogger<RemoteDeviceConnections<T, U>> logger;

		/// <summary>
		/// Frame Providers
		/// </summary>
		private List<VideoObjectProvider<T>> listFrameProviders;

		/// <summary>
		/// List of remote devices
		/// </summary>
		private List<RemoteDevice<T, U>> listRemoteDevices;

		/// <summary>
		/// Constructor, Injects logger and service provider
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		public RemoteDeviceConnections(ILogger<RemoteDeviceConnections<T, U>> logger, IServiceProvider services) {
			this.services = services;
			this.logger = logger;
			listRemoteDevices = new List<RemoteDevice<T, U>>();
			listFrameProviders = new List<VideoObjectProvider<T>>();
		}

		public void addConnectionToRemoteDevices(ConnectionBase connection) {
			ClientInformation newInformation = connection.getInstrumentInformation();

			//Track if the device was found or a new one was added
			bool deviceAlreadyExisted = false;
			lock (listRemoteDevices) {
				foreach (var device in listRemoteDevices) {
					string deviceName = device.name;
					string deviceLocation = device.location;
					string deviceType = device.type;
					//Check if type name and location are all the same
					if (newInformation.Name.Equals(deviceName) && newInformation.Location.Equals(deviceLocation) && newInformation.Type.Equals(deviceType)) {
						//Set already existed to true
						deviceAlreadyExisted = true;

						//If found add to the remote device in the correct category, and if it is a receiver create a provider
						var receivingInstance = typeof(ReceivingConnection<T>);
						if (receivingInstance.IsInstanceOfType(connection)) {
							ReceivingConnection<T> receivingConnection = (ReceivingConnection<T>)connection;
							device.addReceivingConnection(receivingConnection);
						}
						//If it was not receiving it is sending
						else {
							SendingConnection<U> sendingConnection = (SendingConnection<U>)connection;
							device.addSendingConnection(sendingConnection);
						}
						//Stop looking
						break;
					}
				}
			}
			
			//If device did not exist create a new one
			if (!deviceAlreadyExisted) {
				RemoteDevice<T, U> newDevice =
					new RemoteDevice<T, U>(newInformation.Name, newInformation.Location, newInformation.Type);
				
				var receivingInstance = typeof(ReceivingConnection<T>);
				if (receivingInstance.IsInstanceOfType(connection)) {
					ReceivingConnection<T> receivingConnection = (ReceivingConnection<T>)connection;
					newDevice.addReceivingConnection(receivingConnection);
				}
				//If it was not receiving it is sending
				else {
					SendingConnection<U> sendingConnection = (SendingConnection<U>)connection;
					newDevice.addSendingConnection(sendingConnection);
				}
				//Add to list of remote devices
				lock (listRemoteDevices) {
					listRemoteDevices.Add(newDevice);
				}
			}

		}

		public RemoteDevice<T, U> getRemoteDeviceWithNameLocationAndType<T, U>(string name, string location, string type) {
			throw new NotImplementedException();
		}

		public bool subscribeToObjectProviderWithName(string name, string location, string type, string subname, VideoObjectConsumer<T> consumer) {
			throw new NotImplementedException();
		}
	}
}