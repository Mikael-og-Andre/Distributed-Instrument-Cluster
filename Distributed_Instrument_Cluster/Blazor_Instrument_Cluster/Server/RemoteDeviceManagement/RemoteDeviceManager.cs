﻿using Blazor_Instrument_Cluster.Server.Stream;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Video_Library;

namespace Blazor_Instrument_Cluster.Server.RemoteDeviceManagement {

	/// <summary>
	/// Class for storing and managing remote devices
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
		/// Constructor, Injects logger and service provider
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		/// <param name="configuration"></param>
		public RemoteDeviceManager(ILogger<RemoteDeviceManager> logger, IServiceProvider services, IConfiguration configuration) {
			this.services = services;
			this.logger = logger;
			listRemoteDevices = new List<RemoteDevice>();

			//TODO: HARDCODED RemoteDevice
			addRemoteDevice(1,"192.168.1.164",6981,8080,1,"andre","Hardcoded location", "Hardcoded type");
			addRemoteDevice(2, "ooof.asuscomm.com", 6981, 8080, 1, "andre", "Hardcoded location", "Hardcoded type");
			addRemoteDevice(3, "ooof.asuscomm.com", 6981, 8080, 2, "andre", "Hardcoded location", "2 devices");
			addRemoteDevice(4, "192.168.1.172", 6981, 8080, 1, "andre", "Hardcoded location", "Hardcoded type");
			addRemoteDevice(5,"zretzy.asuscomm.com",6981,8080,2,"mikael laptop","Stua", "crestron");
			addRemoteDevice(6,"zretzy.asuscomm.com",7981,7080,1,"mikael desktop","rom", "crestron");
		}

		/// <summary>
		/// Add a new remote device to the list of devices
		/// This method is for a remote device that does not have a crestron
		/// </summary>
		/// <param name="id"></param>
		/// <param name="ip"></param>
		/// <param name="videoPort">The base port for video devices</param>
		/// <param name="videoDeviceNumber">The number of video servers on the device</param>
		/// <param name="name"></param>
		/// <param name="location"></param>
		/// <param name="type"></param>
		public void addRemoteDevice(int id,string ip,int videoPort,int videoDeviceNumber,string name,string location, string type) {
			RemoteDevice remoteDevice = new RemoteDevice(id,ip,videoPort,videoDeviceNumber,name,location,type);
			lock (listRemoteDevices) {
				listRemoteDevices.Add(remoteDevice);
			}
		}

		/// <summary>
		/// Add a new remote device to the list of devices
		/// This method is for a remote device with a crestron
		/// </summary>
		/// <param name="ip">IpAddress of the remote device</param>
		/// <param name="videoPort">The base port for video devices</param>
		/// <param name="videoDeviceNumber">The number of video servers on the device</param>
		/// <param name="name">Name of the remote device</param>
		/// <param name="location">Location of the remote device</param>
		/// <param name="type">type specification of the remote device</param>
		/// <param name="crestronPort"></param>
		public void addRemoteDevice(int id,string ip,int crestronPort,int videoPort,int videoDeviceNumber,string name,string location, string type) {
			RemoteDevice remoteDevice = new RemoteDevice(id,ip,crestronPort,videoPort,videoDeviceNumber,name,location,type);
			lock (listRemoteDevices) {
				listRemoteDevices.Add(remoteDevice);
			}
		}

		/// <summary>
		/// Get a list of remote devices
		/// </summary>
		/// <returns></returns>
		public List<RemoteDevice> getListOfRemoteDevices() {
			lock (listRemoteDevices) {
				return listRemoteDevices;
			}
		}
	}
}