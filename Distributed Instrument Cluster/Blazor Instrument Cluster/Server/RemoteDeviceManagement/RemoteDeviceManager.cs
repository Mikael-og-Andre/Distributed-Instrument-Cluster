using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.Stream;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Server_Library;
using Server_Library.Authorization;
using Server_Library.Connection_Classes;
using Server_Library.Connection_Types;
using Server_Library.Connection_Types.Async;
using Video_Library;

namespace Blazor_Instrument_Cluster.Server.RemoteDeviceManagement {

	/// <summary>
	/// Class for storing connection lists
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class RemoteDeviceManager {

		/// <summary>
		/// Services
		/// </summary>
		private IServiceProvider services;

		/// <summary>
		/// Logger
		/// </summary>
		private ILogger<RemoteDeviceManager> logger;


		/// <summary>
		/// List of remote devices
		/// </summary>
		private List<RemoteDevice> listRemoteDevices;

		/// <summary>
		/// Streams
		/// </summary>
		private MJPEGStreamManager streamManager;

		/// <summary>
		/// Object used to lock when checking if a device exists, so that no duplicates get created
		/// </summary>
		private object existsCheckLock { get; set; }
	
		
		/// <summary>
		/// Constructor, Injects logger and service provider
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		public RemoteDeviceManager(ILogger<RemoteDeviceManager> logger, IServiceProvider services) {
			this.services = services;
			this.logger = logger;
			listRemoteDevices = new List<RemoteDevice>();
			streamManager = (MJPEGStreamManager) services.GetService(typeof(MJPEGStreamManager));
			existsCheckLock = new object();
		}

		/// <summary>
		/// Add a Remote device connection to the list of remote devices
		/// Multiple connections can exists on one device, so if a matching name, location and type are found, it will be added to that
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="isVideo"></param>
		/// <param name="name"></param>
		/// <param name="location"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public async Task addConnectionToRemoteDevices(ConnectionBaseAsync connection, bool isVideo, string name, string location, string type) {

			//Check if a device with the same accessToken is already in the system
			(bool found, RemoteDevice device) result;
			//Only one device checks at a time
			lock (existsCheckLock) {
				result = checkIfDeviceExists(name,location,type);	
			}

			//if device was found add the new connection to it
			if (result.found&&isVideo) {
				//Create stream
				MJPEG_Streamer stream = new MJPEG_Streamer(30,8080);
				lock (streamManager.streams) {
					streamManager.streams.Add(stream);
				}
				//Add device as a video connection
				await result.device.addVideoConnectionToDevice((DuplexConnectionAsync)connection,stream);
			}
			else if(result.found) {
				//add device as a control device
				result.device.addControlConnectionAsync((DuplexConnectionAsync) connection);
			}
			//If the device was not found create a new one and add the connection to it
			else {
				await handleDeviceNotFound(connection,isVideo,name,location,type);
			}
		}

		/// <summary>
		/// Handles creating a new remote device if one did not already exist
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="isVideo"></param>
		/// <param name="name"></param>
		/// <param name="location"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private async Task handleDeviceNotFound(ConnectionBaseAsync connection, bool isVideo, string name, string location, string type) {
			RemoteDevice newRemoteDevice = new RemoteDevice(name, location, type, connection.accessToken);
			lock (listRemoteDevices) {
				listRemoteDevices.Add(newRemoteDevice);
			}

			if (isVideo) {
				//Create stream
				MJPEG_Streamer stream = new MJPEG_Streamer(30,8080);
				lock (streamManager.streams) {
					streamManager.streams.Add(stream);
				}
				//add As a video connection
				await newRemoteDevice.addVideoConnectionToDevice((DuplexConnectionAsync) connection,stream);
			}
			else {
				//add as a control connection
				newRemoteDevice.addControlConnectionAsync((DuplexConnectionAsync) connection);
			}
		}
		
		/// <summary>
		/// Checks the list of RemoteDevices for a device with the matching name location and type
		/// </summary>
		/// <returns>
		/// Bool representing if the device was found
		/// if the device was found the device will also be returned
		/// </returns>
		private (bool found,RemoteDevice device) checkIfDeviceExists(string name, string location, string type) {
			//Lock list so devices are added in correct order and so on
			lock (listRemoteDevices) {
				foreach (var device in listRemoteDevices) {
					//Check if the devices exists
					if (device.name.Equals(name)&&device.location.Equals(location)&&device.type.Equals(type)) {
						return (true,device);
					}
				}
				return (false,default);
			}
		}

		/// <summary>
		/// Returns a list of all remote devices in the manager
		/// </summary>
		/// <returns>List containing RemoteDevice</returns>
		public List<RemoteDevice> getListOfRemoteDevices() {
			lock (listRemoteDevices) {
				return listRemoteDevices;
			}
		}
	}
}