using Blazor_Instrument_Cluster.Server.Database;
using Blazor_Instrument_Cluster.Shared.DeviceSelection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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

		private readonly AppDbContext dbContext;

		/// <summary>
		/// Logger
		/// </summary>
		private ILogger<RemoteDeviceManager> logger;

		/// <summary>
		/// Constructor, Injects logger and service provider
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="services"></param>
		/// <param name="configuration"></param>
		public RemoteDeviceManager(ILogger<RemoteDeviceManager> logger, IServiceProvider services, IConfiguration configuration, AppDbContext dbContext) {
			this.services = services;
			this.dbContext = dbContext;
			this.logger = logger;
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
		public async Task addRemoteDevice(string ip, int videoPort, int videoDeviceNumber, string name, string location, string type) {
			RemoteDevice remoteDevice = new RemoteDevice(ip, videoPort, videoDeviceNumber, name, location, type);
			await dbContext.AddAsync(new RemoteDeviceDB() {
				crestronPort = remoteDevice.crestronPort,
				hasCrestron = remoteDevice._hasCrestron,
				ip = remoteDevice.ip,
				location = remoteDevice.location,
				name = remoteDevice.name,
				type = remoteDevice.type,
				videoBasePort = remoteDevice.videoPort,
				videoDeviceNumber = remoteDevice.videoDeviceNumber,
			});

			await dbContext.SaveChangesAsync();
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
		public async Task addRemoteDevice(string ip, int crestronPort, int videoPort, int videoDeviceNumber, string name, string location, string type) {
			RemoteDevice remoteDevice = new RemoteDevice(ip, crestronPort, videoPort, videoDeviceNumber, name, location, type);
			await dbContext.AddAsync(new RemoteDeviceDB() {
				crestronPort = remoteDevice.crestronPort,
				hasCrestron = remoteDevice._hasCrestron,
				ip = remoteDevice.ip,
				location = remoteDevice.location,
				name = remoteDevice.name,
				type = remoteDevice.type,
				videoBasePort = remoteDevice.videoPort,
				videoDeviceNumber = remoteDevice.videoDeviceNumber,
			});

			await dbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Get a list of remote devices
		/// </summary>
		/// <returns></returns>
		public async Task<List<RemoteDevice>> getListOfRemoteDevices() {
			var devicesDB = dbContext.devices.AsAsyncEnumerable();

			List<RemoteDevice> devices = new List<RemoteDevice>();

			await foreach (var d in devicesDB) {
				devices.Add(new RemoteDevice() {
					_hasCrestron = d.hasCrestron,
					crestronPort = d.crestronPort,
					ip = d.ip,
					location = d.location,
					name = d.name,
					type = d.type,
					videoDeviceNumber = d.videoDeviceNumber,
					videoPort = d.videoBasePort,
				});
			}

			return devices;
		}

		public async Task removeDevice(RemoteDevice device) {
			var removeDev = dbContext.devices.FirstOrDefault(r => r.ip == device.ip && r.name == device.name && r.type == device.type && r.location == device.location);
			if (removeDev is not null) {
				dbContext.Remove(removeDev);
			}

			await dbContext.SaveChangesAsync();
		}
	}
}