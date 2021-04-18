﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Blazor_Instrument_Cluster.Client.Code.UrlObjects;
using Blazor_Instrument_Cluster.Shared;
using Microsoft.AspNetCore.Components;

namespace Blazor_Instrument_Cluster.Client.Code {
	/// <summary>
	/// Get DeviceModel passed in an use it to display info
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class DeviceComponent : ComponentBase{
		
		[Inject]
		private NavigationManager navigationManager { get; set; }

		[Parameter]
		public DeviceModel deviceInfo { get; set; }

		private string basePath = "VideoAndControl";

		protected void navigateToDevicePage() {

			//Loop devices
			List<int> portslist = new List<int>();
			List<string> controlNames = new List<string>();
			foreach (var subdevice in deviceInfo.subDevice) {
				if (subdevice.isVideoDevice) {
					portslist.Add(subdevice.port);
				}
				else {
					controlNames.Add(subdevice.subname);
				}
			}
			//json for portsList
			PortsList portsList = new PortsList();
			portsList.portsList = portslist;
			string portJson = JsonSerializer.Serialize(portsList);
			string urlPortsListJson = HttpUtility.UrlEncodeUnicode(portJson);

			//Json control devices
			ControlSubdevices controlDevices = new ControlSubdevices();
			controlDevices.subnameList = controlNames;
			string subnamesJson = JsonSerializer.Serialize(controlDevices);
			string urlSubnamesJson = HttpUtility.UrlEncodeUnicode(subnamesJson);


			string fullPath = basePath + "/" + deviceInfo.name + "/" + deviceInfo.location + "/" + deviceInfo.type + "/" + urlSubnamesJson + "/" + urlPortsListJson;

			navigationManager.NavigateTo(fullPath);
		}

	}
}
