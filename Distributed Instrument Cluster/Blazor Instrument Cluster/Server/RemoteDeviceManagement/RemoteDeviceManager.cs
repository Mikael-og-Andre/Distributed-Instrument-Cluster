using System;
using System.Collections.Generic;
using Blazor_Instrument_Cluster.Server.Events;
using Blazor_Instrument_Cluster.Server.Stream;
using Microsoft.Extensions.Logging;
using Server_Library;
using Server_Library.Connection_Classes;
using Server_Library.Connection_Types;
using Video_Library;

namespace Blazor_Instrument_Cluster.Server.RemoteDeviceManagement {

	/// <summary>
	/// Class for storing connection lists
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class RemoteDeviceManager : IRemoteDeviceManager {

		/// <summary>
		/// Services
		/// </summary>
		private IServiceProvider services;

		/// <summary>
		/// Logger
		/// </summary>
		private ILogger<RemoteDeviceManager> logger;

		/// <summary>
		/// Frame Providers
		/// </summary>
		private List<VideoObjectProvider> listFrameProviders;

		/// <summary>
		/// List of remote devices
		/// </summary>
		private List<RemoteDeviceManagement.RemoteDevice> listRemoteDevices;

		private MJPEGStreamManager streamManager;

		/// <summary>
		/// Constructor, Injects logger and service provider
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		public RemoteDeviceManager(ILogger<RemoteDeviceManager> logger, IServiceProvider services) {
			this.services = services;
			this.logger = logger;
			listRemoteDevices = new List<RemoteDeviceManagement.RemoteDevice>();
			listFrameProviders = new List<VideoObjectProvider>();
			streamManager = (MJPEGStreamManager) services.GetService(typeof(MJPEGStreamManager));
		}

		/// <summary>
		/// Adds a connection to the remoteDevice with the corresponding name location and type, if not found it creates a new one
		/// </summary>
		/// <param name="connection"></param>
		public void addConnectionToRemoteDevices(ConnectionBase connection) {
			ClientInformation newInformation = connection.getInstrumentInformation();

			//Track if the device was found or a new one was added
			bool deviceAlreadyExisted = false;
			//Lock list so devices are added in correct order and so on
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
						var receivingInstance = typeof(ReceivingConnection);
						if (receivingInstance.IsInstanceOfType(connection)) {
							ReceivingConnection receivingConnection = (ReceivingConnection)connection;

							//Create a new stream
							MJPEG_Streamer streamer = new MJPEG_Streamer(30,8080);
							streamManager.streams.Add(streamer);

							device.addReceivingConnection(receivingConnection,streamer);
						}
						//If it was not receiving it is sending
						else {
							SendingConnection sendingConnection = (SendingConnection)connection;
							device.addSendingConnection(sendingConnection);
						}
						//Stop looking
						break;
					}
				}

				//If device did not exist create a new one
				if (!deviceAlreadyExisted) {
					RemoteDeviceManagement.RemoteDevice newDevice =
						new RemoteDeviceManagement.RemoteDevice(newInformation.Name, newInformation.Location, newInformation.Type);

					var receivingInstance = typeof(ReceivingConnection);
					bool isReceivingConnection = receivingInstance.IsInstanceOfType(connection);
					if (isReceivingConnection) {
						ReceivingConnection receivingConnection = (ReceivingConnection)connection;

						//Create a new video stream
						MJPEG_Streamer streamer = new MJPEG_Streamer(30,8080);
						streamManager.streams.Add(streamer);

						newDevice.addReceivingConnection(receivingConnection,streamer);
					}
					//If it was not receiving it is sending
					else {
						SendingConnection sendingConnection = (SendingConnection)connection;
						newDevice.addSendingConnection(sendingConnection);
					}
					//Add to list of remote devices
					listRemoteDevices.Add(newDevice);
				}
			}
		}

		/// <summary>
		/// Get a remote device with its name location and type
		/// </summary>
		/// <param name="name"></param>
		/// <param name="location"></param>
		/// <param name="type"></param>
		/// <param name="outputDevice"></param>
		/// <returns>If it was found or not</returns>
		public bool getRemoteDeviceWithNameLocationAndType(string name, string location, string type, out RemoteDeviceManagement.RemoteDevice outputDevice) {
			lock (listRemoteDevices) {
				foreach (var device in listRemoteDevices) {
					//Check if device info is the same
					if (device.name.Equals(name) && device.location.Equals(location) && device.type.Equals(type)) {
						outputDevice = device;
						return true;
					}
				}

				outputDevice = default;
				return false;
			}
		}

		/// <summary>
		/// Get List Of RemoteDevices
		/// </summary>
		/// <returns>List with type RemoteDevice</returns>
		public List<RemoteDeviceManagement.RemoteDevice> getListOfRemoteDevices() {
			lock (listRemoteDevices) {
				return listRemoteDevices;
			}
		}
	}
}