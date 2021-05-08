using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
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
		/// List of all urls for video
		/// </summary>
		private LinkedList<Uri> listOfUrls;

		/// <summary>
		/// Uri to get the image from
		/// </summary>
		protected Uri uri = default;

		/// <summary>
		/// linked list of video uris
		/// </summary>
		private LinkedListNode<Uri> currentUriNode = null;

		protected override void OnInitialized() {
			//Create http version of url
			string httpBase = navigationManager.BaseUri.Replace("https://", "http://");
			Uri oldUriHttp = new Uri(httpBase);
			try {
				//Convert incoming url Json to object
				string deviceJson = HttpUtility.UrlDecode(urlDeviceJson,Encoding.Unicode).TrimStart('\0').TrimEnd('\0');
				displayRemoteDeviceModel = JsonSerializer.Deserialize<DisplayRemoteDeviceModel>(deviceJson);
				
				listOfUrls = new LinkedList<Uri>();

				var videoIp = displayRemoteDeviceModel.ip;
				var videoPorts = displayRemoteDeviceModel.videoPorts;

				foreach (var port in videoPorts) {
					UriBuilder builder = new UriBuilder(videoIp);
					builder.Port = port;
					Uri uri = builder.Uri;
					listOfUrls.AddLast(uri);
				}

				if (listOfUrls.Count>0) {
					//Set first uri;
					currentUriNode = listOfUrls.First;
					uri = currentUriNode.Value;
				}
			}
			catch (Exception e) {
				throw;
			}
		}

		/// <summary>
		/// Set uri to the next uri in the list
		/// </summary>
		/// <returns></returns>
		protected void nextUri() {
			LinkedListNode<Uri> nextNode = currentUriNode.Next;
			if (nextNode is null) {
			}
			else {
				//set new currentNode and new uri
				currentUriNode = nextNode;
				uri = currentUriNode.Value;
			}
		}

		/// <summary>
		/// Set uri to the previous node in the list
		/// </summary>
		/// <returns></returns>
		protected void prevUri() {
			LinkedListNode<Uri> prevNode = currentUriNode.Previous;
			if (prevNode is null) {
				
			}
			else {
				//set new currentNode and new uri
				currentUriNode = prevNode;
				uri = currentUriNode.Value;
			}
		}

		public void Dispose() {
			disposalTokenSource.Cancel();
		}
	}
}