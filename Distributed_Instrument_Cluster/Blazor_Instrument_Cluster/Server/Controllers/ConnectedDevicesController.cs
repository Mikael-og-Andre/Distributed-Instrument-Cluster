using Blazor_Instrument_Cluster.Shared;
using Server_Library;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement;
using Blazor_Instrument_Cluster.Shared.DeviceSelection;
using Packet_Classes;

namespace Blazor_Instrument_Cluster.Server.Controllers {

	/// <summary>
	/// Api Controller for accessing data about connected devices
	/// <author>Mikael Nilssen</author>
	/// https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-5.0
	/// </summary>
	[ApiController]
	[Route("/api/ConnectedDevices")]
	[Produces("application/json")]
	public class ConnectedDevicesController : ControllerBase {
		/// <summary>
		/// Remote Device connections
		/// <author>Mikael Nilssen</author>
		/// </summary>
		private RemoteDeviceManager remoteDeviceManager;

		/// <summary>
		/// Constructor, Injects Service provider and get remote device connection
		/// </summary>
		/// <param name="services">Service injection</param>
		public ConnectedDevicesController(IServiceProvider services) {
			this.remoteDeviceManager = (RemoteDeviceManager)services.GetService<RemoteDeviceManager>();
		}

		/// <summary>
		/// Get request for connected video Connections
		/// </summary>
		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DisplayRemoteDeviceModel>))]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public IEnumerable<DisplayRemoteDeviceModel> getRemoteDevices() {

			//Get list of video connections
			List<RemoteDevice> listOfRemoteDevices = remoteDeviceManager.getListOfRemoteDevices();
			if (listOfRemoteDevices.Any()) {
				//Create an IEnumerable with device models
				IEnumerable<DisplayRemoteDeviceModel> enumerableDeviceModels = Array.Empty<DisplayRemoteDeviceModel>();
				//Lock unsafe list
				lock (listOfRemoteDevices) {
					foreach (var device in listOfRemoteDevices) {

						string deviceIp = device.ip;
						string deviceName = device.name;
						string deviceLocation = device.location;
						string deviceType = device.type;

						//Create ports list, each server is incremented by 1 from the base port
						int basePort = device.videoPort;
						int numDevices = device.videoDeviceNumber;
						List<int> videoPorts = new List<int>();
						for (int i = 0; i < numDevices; i++) {
							videoPorts.Add(basePort+i);
						}

						//check if it has a crestron
						bool hasCrestron=device.hasCrestron();
						bool pingResult = device.ping(2000);

						enumerableDeviceModels =
							enumerableDeviceModels.Append(new DisplayRemoteDeviceModel(deviceIp,deviceName,deviceLocation,deviceType,videoPorts,hasCrestron,pingResult));
					}
				}

				//Return items and status code 200 for success
				Response.StatusCode = 200;
				return enumerableDeviceModels;
			}
			else {
				//Return empty and set status code to 204 for no items
				Response.StatusCode = 204;
				return null;
			}
		}
	}
}