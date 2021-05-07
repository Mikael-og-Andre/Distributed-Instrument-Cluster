using Blazor_Instrument_Cluster.Server.Stream;
using Microsoft.Extensions.Logging;
using Server_Library.Connection_Types.Async;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Server_Library.Authorization;
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
		/// Access token used when connecting to remote devices
		/// </summary>
		private AccessToken accessToken { get; set; }

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
			accessToken = new AccessToken("access");

			//TODO: HARDOCDED RemoteDevice
			addRemoteDevice(1,"192.168.50.160",6981,6980,"Hardcoded Name","Hardcoded location", "Hardcoded type");

		}

		/// <summary>
		/// Add a new remote device to the list of devices
		/// This method is for a remote device that does not have a crestron
		/// </summary>
		/// <param name="id"></param>
		/// <param name="ip"></param>
		/// <param name="videoPort"></param>
		/// <param name="name"></param>
		/// <param name="location"></param>
		/// <param name="type"></param>
		public void addRemoteDevice(int id,string ip,int videoPort,string name,string location, string type) {
			RemoteDevice remoteDevice = new RemoteDevice(id,ip,videoPort,name,location,type,accessToken);
			lock (listRemoteDevices) {
				listRemoteDevices.Add(remoteDevice);
			}
		}

		/// <summary>
		/// Add a new remote device to the list of devices
		/// This method is for a remote device with a crestron
		/// </summary>
		/// <param name="ip">IpAddress of the remote device</param>
		/// <param name="videoPort"></param>
		/// <param name="name">Name of the remote device</param>
		/// <param name="location">Location of the remote device</param>
		/// <param name="type">type specification of the remote device</param>
		/// <param name="crestronPort"></param>
		public void addRemoteDevice(int id,string ip,int crestronPort,int videoPort,string name,string location, string type) {
			RemoteDevice remoteDevice = new RemoteDevice(id,ip,crestronPort,videoPort,name,location,type,accessToken);
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