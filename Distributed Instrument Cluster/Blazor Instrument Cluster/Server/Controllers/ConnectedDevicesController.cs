using Blazor_Instrument_Cluster.Server.Injection;
using Blazor_Instrument_Cluster.Shared;
using Server_Library;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Server_Library.Connection_Types.deprecated;

namespace Blazor_Instrument_Cluster.Server.Controllers {

	/// <summary>
	/// Api Controller for accessing data about connected devices
	/// </summary>
	[Route("api/ConnectedDevices")]
	[ApiController]
	public class ConnectedDevicesController<T,U> : ControllerBase {
		/// <summary>
		/// Remote Device connections
		/// </summary>
		private RemoteDeviceConnections<T,U> remoteDeviceConnections;

		/// <summary>
		/// Constructor, Injects Service provider and get remote device connection
		/// </summary>
		/// <param name="services"></param>
		public ConnectedDevicesController(IServiceProvider services) {
			this.remoteDeviceConnections = (RemoteDeviceConnections<T,U>)services.GetService<IRemoteDeviceConnections<T,U>>();
		}

		/// <summary>
		/// Get request for connected video Connections
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DeviceModel>))]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public IEnumerable<DeviceModel> getVideoConnections() {
			//Get list of video connections
			List<VideoConnection> listVideoConnections;
			if (remoteDeviceConnections.getVideoConnectionList(out listVideoConnections)) {
				//Create an IEnumerable with device models
				IEnumerable<DeviceModel> enumerableDeviceModels = new DeviceModel[] { };
				//Lock unsafe list
				lock (listVideoConnections) {
					foreach (var videoConnection in listVideoConnections) {
						ClientInformation info = videoConnection.getInstrumentInformation();
						//TODO: Add has crestron boolean to instrument information
						enumerableDeviceModels =
							enumerableDeviceModels.Append(new DeviceModel(info.Name, info.Location, info.Type, true));
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