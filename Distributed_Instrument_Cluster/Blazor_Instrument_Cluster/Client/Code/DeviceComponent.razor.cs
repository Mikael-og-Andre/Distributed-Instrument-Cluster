using System;
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
	/// Get DisplayRemoteDeviceModel passed in an use it to display info
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class DeviceComponent : ComponentBase{
		
		[Inject]
		private NavigationManager navigationManager { get; set; }

		[Parameter]
		public DisplayRemoteDeviceModel displayRemoteDeviceInfo { get; set; }

		private string basePath = "VideoAndControl";

		protected void navigateToDevicePage() {

			string jsonObject = JsonSerializer.Serialize(displayRemoteDeviceInfo);


			string fullPath = basePath + "/" + HttpUtility.UrlEncode(jsonObject);

			navigationManager.NavigateTo(fullPath);
		}
	}
}
