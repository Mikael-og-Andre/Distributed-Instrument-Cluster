﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Blazor_Instrument_Cluster.Client.Code.UrlObjects;
using Microsoft.AspNetCore.Components;

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

		protected override async Task OnInitializedAsync() {
			//Create http version of url
			string httpBase = navigationManager.BaseUri.Replace("https://", "http://");
			string[] httpSplit = httpBase.Split(":");
			string httpReconstruction = httpSplit[0]+":"+ httpSplit[1];
			try {
				//Convert incoming url Json to object
				string portObjectJson = HttpUtility.UrlDecode(urlPortObject, Encoding.Unicode);
				PortsList deserializedPortsList = JsonSerializer.Deserialize<PortsList>(portObjectJson);
				if (deserializedPortsList != null) {
					//Deserialize
					List<int> ports = deserializedPortsList.portsList;
					listOfUrls = new LinkedList<string>();
					foreach (var port in ports) {
						listOfUrls.AddLast(new LinkedListNode<string>(httpReconstruction+":"+port));
						Console.WriteLine(httpReconstruction+":"+port);
					}
					//Set first uri;
					currentUriNode = listOfUrls.First;
					uri = currentUriNode?.Value;

				}
				else {
					return;
				}
			}
			catch (Exception e) {
				return;
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
