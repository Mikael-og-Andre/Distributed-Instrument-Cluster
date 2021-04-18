using Blazor_Instrument_Cluster.Shared;
using Server_Library;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement;
using PackageClasses;

namespace Blazor_Instrument_Cluster.Server.Controllers {

	/// <summary>
	/// Api Controller for accessing data about connected devices
	/// https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-5.0
	/// </summary>
	[ApiController]
	[Route("/api/ConnectedDevices")]
	[Produces("application/json")]
	public class ConnectedDevicesController : ControllerBase {
		/// <summary>
		/// Remote Device connections
		/// </summary>
		private RemoteDeviceManager remoteDeviceManager;

		/// <summary>
		/// Constructor, Injects Service provider and get remote device connection
		/// </summary>
		/// <param name="services"></param>
		public ConnectedDevicesController(IServiceProvider services) {
			this.remoteDeviceManager = (RemoteDeviceManager)services.GetService<IRemoteDeviceManager>();
		}

		/// <summary>
		/// Get request for connected video Connections
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DeviceModel>))]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public IEnumerable<DeviceModel> getRemoteDevices() {
			//Get list of video connections
			List<RemoteDevice> listOfRemoteDevices = remoteDeviceManager.getListOfRemoteDevices();
			if (listOfRemoteDevices.Any()) {
				//Create an IEnumerable with device models
				IEnumerable<DeviceModel> enumerableDeviceModels = Array.Empty<DeviceModel>();
				//Lock unsafe list
				lock (listOfRemoteDevices) {
					foreach (var device in listOfRemoteDevices) {

						string deviceName = device.name;
						string deviceLocation = device.location;
						string deviceType = device.type;
						
						//Get sub devices
                        List<SubDevice> subDeviceInfo = device.getSubDeviceList();

						//Create a list of models
                        List<SubDeviceModel> modelList = new List<SubDeviceModel>();
						foreach(SubDevice subDevice in subDeviceInfo){
							modelList.Add(new SubDeviceModel(subDevice.videoDevice,subDevice.subname,subDevice.port,subDevice.streamType));
						}


						enumerableDeviceModels =
							enumerableDeviceModels.Append(new DeviceModel(deviceName,deviceLocation,deviceType,modelList));
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