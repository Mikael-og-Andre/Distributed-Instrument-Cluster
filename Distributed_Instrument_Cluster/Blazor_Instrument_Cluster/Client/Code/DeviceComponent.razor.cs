﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Blazor_Instrument_Cluster.Shared;
using Blazor_Instrument_Cluster.Shared.DeviceSelection;
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
			List<SubConnectionModel> videoConnectionModels = new List<SubConnectionModel>();
			List<SubConnectionModel> controllerConnections = new List<SubConnectionModel>();
			foreach (var subConnectionModel in deviceInfo.subDevice) {
				if (subConnectionModel.isVideoDevice) {
					videoConnectionModels.Add(subConnectionModel);
				}
				else {
					controllerConnections.Add(subConnectionModel);
				}
			}
			//json for portsList
			string videoJson = JsonSerializer.Serialize(videoConnectionModels);

			//Json control devices
			string controllerJson = JsonSerializer.Serialize(controllerConnections);


			string fullPath = basePath + "/" + HttpUtility.UrlEncode(deviceInfo.name) + "/" + HttpUtility.UrlEncode(deviceInfo.location) + "/" + HttpUtility.UrlEncode(deviceInfo.type) + "/" + HttpUtility.UrlEncode(controllerJson) + "/" + HttpUtility.UrlEncode(videoJson);

			navigationManager.NavigateTo(fullPath);
		}
	}
}