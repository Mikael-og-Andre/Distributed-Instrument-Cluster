using Blazor_Instrument_Cluster.Server.Injection;
using Blazor_Instrument_Cluster.Shared;
using Instrument_Communicator_Library;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Blazor_Instrument_Cluster.Server.Controllers {

	[Route("api/ConnectedDevices")]
	[ApiController]
	public class ConnectedDevicesController : ControllerBase {
		private RemoteDeviceConnection remoteDeviceConnection;     //Handler for connected devices

		public ConnectedDevicesController(IServiceProvider services) {
			this.remoteDeviceConnection = (RemoteDeviceConnection)services.GetService<IRemoteDeviceConnections>();
		}

		/// <summary>
		/// Get request for connected video Connections
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DeviceModel>))]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public IEnumerable<DeviceModel> GetVideoConnections() {
			//Get list of video connections
			List<VideoConnection> listVideoConnections;
			if (remoteDeviceConnection.GetVideoConnectionList(out listVideoConnections)) {
				//Create an IEnumerable with device models
				IEnumerable<DeviceModel> enumerableDeviceModels = new DeviceModel[] { };
				//Lock unsafe list
				lock (listVideoConnections) {
					foreach (var videoConnection in listVideoConnections) {
						InstrumentInformation info = videoConnection.GetInstrumentInformation();
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