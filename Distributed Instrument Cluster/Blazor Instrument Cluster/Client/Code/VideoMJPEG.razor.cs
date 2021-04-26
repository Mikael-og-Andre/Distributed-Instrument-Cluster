using Blazor_Instrument_Cluster.Client.Code.UrlObjects;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Blazor_Instrument_Cluster.Client.Code {

	public class VideoMJPEG : ComponentBase, IDisposable {
		private CancellationTokenSource disposalTokenSource = new CancellationTokenSource();

		[Inject]
		protected NavigationManager navigationManager { get; set; }

		/// <summary>
		/// URL encoded PortsList Object
		/// </summary>
		[Parameter]
		public string urlPortObject { set; get; }

		/// <summary>
		/// List of all urls for video
		/// </summary>
		private LinkedList<string> listOfUrls;

		/// <summary>
		/// Uri to get the image from
		/// </summary>
		protected string uri = default;

		private LinkedListNode<string> currentUriNode = null;

		protected override void OnInitialized() {
			//Create http version of url
			string httpBase = navigationManager.BaseUri.Replace("https://", "http://");
			Uri olduriHttp = new Uri(httpBase);
			try {
				//Convert incoming url Json to object
				string portObjectJson = HttpUtility.UrlDecode(urlPortObject).TrimStart('\0').TrimEnd('\0');
				PortsList deserializedPortsList = JsonSerializer.Deserialize<PortsList>(portObjectJson);

				//Deserialize
				List<int> ports = deserializedPortsList.portsList;
				listOfUrls = new LinkedList<string>();
				foreach (var port in ports) {

					UriBuilder newUri = new UriBuilder(olduriHttp.AbsoluteUri);
					newUri.Port = port;
					listOfUrls.AddLast(new LinkedListNode<string>(newUri.ToString()));
					Console.WriteLine(newUri.ToString());
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
		protected bool nextUri() {
			LinkedListNode<string> nextNode = currentUriNode.Next;
			if (nextNode is null) {
				return false;
			}
			else {
				//set new currentNode and new uri
				currentUriNode = nextNode;
				uri = currentUriNode.Value;
				return true;
			}
		}

		/// <summary>
		/// Set uri to the previous node in the list
		/// </summary>
		/// <returns></returns>
		protected bool prevUri() {
			LinkedListNode<string> prevNode = currentUriNode.Previous;
			if (prevNode is null) {
				return false;
			}
			else {
				//set new currentNode and new uri
				currentUriNode = prevNode;
				uri = currentUriNode.Value;
				return true;
			}
		}

		public void Dispose() {
			disposalTokenSource.Cancel();
		}
	}
}