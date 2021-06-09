using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement;
using Blazor_Instrument_Cluster.Shared.AuthenticationModels;
using Blazor_Instrument_Cluster.Shared.DeviceSelection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server.Controllers {

	/// <summary>
	/// Api Controller for accessing data about connected devices
	/// https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-5.0
	/// </summary>
	[ApiController]
	[Route("/api/ConnectedDevices")]
	public class ConnectedDevicesController : ControllerBase {

		/// <summary>
		/// Remote Device connections
		/// <author>Mikael Nilssen</author>
		/// </summary>
		private RemoteDeviceManager remoteDeviceManager;

		/// <summary>
		/// Constructor, Injects Service provider and get remote device connection
		/// </summary>
		/// <param name="services"></param>
		public ConnectedDevicesController(IServiceProvider services) {
			this.remoteDeviceManager = (RemoteDeviceManager)services.GetService<RemoteDeviceManager>();
		}

		/// <summary>
		/// Get request for connected video Connections
		/// </summary>
		/// <returns></returns>
		[Route("Devices")]
		[HttpGet]
		[Produces("application/json")]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DisplayRemoteDeviceModel>))]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<ActionResult<List<DisplayRemoteDeviceModel>>> getRemoteDevices() {
			//Get list of video connections
			List<RemoteDevice> listOfRemoteDevices = await remoteDeviceManager.getListOfRemoteDevices();
			if (listOfRemoteDevices.Any()) {
				//Create an IEnumerable with device models
				List<DisplayRemoteDeviceModel> displayRemoteDeviceModels = new List<DisplayRemoteDeviceModel>();
				//Lock unsafe list
				lock (listOfRemoteDevices) {
					foreach (var device in listOfRemoteDevices) {
						string deviceIp = device.ip;
						string deviceName = device.name;
						string deviceLocation = device.location;
						string deviceType = device.type;
						int deviceCrestronPort = device.crestronPort;

						//Create ports list, each server is incremented by 1 from the base port
						int basePort = device.videoPort;
						int numDevices = device.videoDeviceNumber;
						List<int> videoPorts = new List<int>();
						for (int i = 0; i < numDevices; i++) {
							videoPorts.Add(basePort + i);
						}

						//check if it has a crestron
						bool hasCrestron = device.hasCrestron();
						bool pingResult = device.ping(2000);

						displayRemoteDeviceModels.Add(new DisplayRemoteDeviceModel(deviceIp, deviceName, deviceLocation, deviceType, videoPorts, hasCrestron, deviceCrestronPort, pingResult));
					}
				}

				return Ok(displayRemoteDeviceModels);
			} else {
				return NoContent();
			}
		}

		[Authorize(Roles = "Admin")]
		[Route("add")]
		[HttpPost]
		public async Task<IActionResult> addDevice([FromBody] RegisterRemoteDeviceModel rdm) {
			try {
				if (rdm.hasCrestron) {
					await remoteDeviceManager.addRemoteDevice(rdm.ip, rdm.crestronPort, rdm.videoBasePort,
						rdm.videoDeviceNumber, rdm.name, rdm.location, rdm.type);
				} else {
					await remoteDeviceManager.addRemoteDevice(rdm.ip, rdm.videoBasePort, rdm.videoDeviceNumber, rdm.name,
						rdm.location, rdm.type);
				}

				var result = new RegisterResult();
				result.Successful = true;

				return Ok(result);
			} catch (Exception e) {
				return BadRequest(new RegisterResult().Successful = false);
			}
		}

		[Authorize(Roles = "Admin")]
		[Route("remove")]
		[HttpPost]
		public async Task<IActionResult> removeDevice([FromBody] DisplayRemoteDeviceModel rdm) {
			try {
				await remoteDeviceManager.removeDevice(new RemoteDevice(rdm.ip, rdm.videoPorts[0], rdm.videoPorts.Count, rdm.name,
					rdm.location, rdm.type));

				return Ok();
			} catch (Exception e) {
				return BadRequest();
			}
		}
	}
}