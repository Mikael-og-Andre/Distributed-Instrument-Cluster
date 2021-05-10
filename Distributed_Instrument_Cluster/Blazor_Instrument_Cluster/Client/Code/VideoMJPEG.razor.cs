using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Blazor_Instrument_Cluster.Shared;
using Blazor_Instrument_Cluster.Shared.DeviceSelection;
using Microsoft.Extensions.Logging;

namespace Blazor_Instrument_Cluster.Client.Code {

	public class VideoMJPEG : ComponentBase, IDisposable {
		private CancellationTokenSource disposalTokenSource = new CancellationTokenSource();

		[Inject]
		protected NavigationManager navigationManager { get; set; }

		[Inject]
		protected ILogger<VideoMJPEG> logger { get; set; }

		/// <summary>
		/// URL encoded DisplayRemoteDeviceModel
		/// </summary>
		[Parameter]
		public string urlDeviceJson { set; get; }

		/// <summary>
		/// Display device model
		/// </summary>
		protected DisplayRemoteDeviceModel displayRemoteDeviceModel { get; set; }

		/// <summary>
		/// Uri to get the image from
		/// </summary>
		protected Uri uri = default;

		/// <summary>
		/// List of uris
		/// </summary>
		protected List<Uri> uris = null;

		protected override void OnInitialized() {
			try {
				//Convert incoming url Json to object
				string deviceJson = HttpUtility.UrlDecode(urlDeviceJson,Encoding.UTF8).TrimStart('\0').TrimEnd('\0');
				displayRemoteDeviceModel = JsonSerializer.Deserialize<DisplayRemoteDeviceModel>(deviceJson);
				
				uris = new List<Uri>();

				var videoIp = displayRemoteDeviceModel.ip;
				var videoPorts = displayRemoteDeviceModel.videoPorts;

				foreach (var port in videoPorts) {
					UriBuilder builder = new UriBuilder(videoIp);
					builder.Port = port;
					Uri uri = builder.Uri;
					uris.Add(uri);
				}

				if (uris.Any()) {
					uri = uris[0];
				}

			}
			catch (Exception e) {
				Console.WriteLine("Error While initializing {0}",e.Message);
				
			}
		}
		public void Dispose() {
			disposalTokenSource.Cancel();
		}
	}
}