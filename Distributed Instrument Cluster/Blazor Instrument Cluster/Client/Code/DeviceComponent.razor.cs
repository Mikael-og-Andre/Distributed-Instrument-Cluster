using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

			string crestronSubname = "crestronControl";
			string videoSubname = "videoStream";

			foreach (var subName in deviceInfo.subNames) {

				if (subName.Contains("crestron")) {
					crestronSubname = subName;
				}
				if (subName.Contains("video")) {
					videoSubname = subName;
				}

			}

			string fullPath = basePath + "/" + deviceInfo.name + "/" + deviceInfo.location + "/" + deviceInfo.type + "/" + crestronSubname + "/" + videoSubname;

			navigationManager.NavigateTo(fullPath);
		}
	}
}
