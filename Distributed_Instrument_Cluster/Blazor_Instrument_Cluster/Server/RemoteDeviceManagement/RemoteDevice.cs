using Blazor_Instrument_Cluster.Server.CrestronControl;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Server_Library.Authorization;
using Server_Library.Connection_Types.Async;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement.Interface;
using Blazor_Instrument_Cluster.Shared.Websocket;
using Microsoft.AspNetCore.Mvc.Rendering;
using Video_Library;

namespace Blazor_Instrument_Cluster.Server.RemoteDeviceManagement {

	/// <summary>
	/// A remote device connected to the server
	/// Stores data about connections belonging to each device, and the providers
	/// </summary>
	public class RemoteDevice : IRemoteDeviceStatus{

		/// <summary>
		/// Device id
		/// </summary>
		public int id { get; set; }
		
		/// <summary>
		/// Internet address of the remote device
		/// </summary>
		private string ip { get; set; }

		private int crestronPort { get; set; }
		private int videoPort { get; set; }

		/// <summary>
		/// name of the device
		/// </summary>
		public string name { get; set; }

		/// <summary>
		/// location of the device
		/// </summary>
		public string location { get; set; }

		/// <summary>
		/// type of the device
		/// </summary>
		public string type { get; set; }

		/// <summary>
		/// Access token sent when establishing a connection
		/// </summary>
		public AccessToken accessToken { get; private set; }

		/// <summary>
		/// Handles one time control of the crestron device
		/// </summary>
		private ControlHandler controlHandler { get; set; }

		/// <summary>
		/// Crestron client
		/// </summary>
		private CrestronClient crestronClient { get; set; }

		/// <summary>
		/// Does the remote device have a crestron
		/// </summary>
		private bool _hasCrestron { get; set; }

		/// <summary>
		/// Constructor with crestron
		/// </summary>
		/// <param name="crestronPort"></param>
		/// <param name="videoPort"></param>
		/// <param name="name"></param>
		/// <param name="location"></param>
		/// <param name="type"></param>
		/// <param name="id"></param>
		/// <param name="ip"></param>
		public RemoteDevice(int id,string ip,int crestronPort,int videoPort,string name, string location, string type, AccessToken accessToken) {
			this.id = id;
			this.ip = ip;
			this.crestronPort = crestronPort;
			this.videoPort = videoPort;
			this.name = name;
			this.location = location;
			this.type = type;
			this.accessToken = accessToken;
			this._hasCrestron = true;
			this.crestronClient = new CrestronClient(ip,crestronPort,accessToken);
			this.controlHandler = new ControlHandler(crestronClient);
		}

		/// <summary>
		/// Constructor without crestron
		/// </summary>
		/// <param name="videoPort"></param>
		/// <param name="name"></param>
		/// <param name="location"></param>
		/// <param name="type"></param>
		/// <param name="id"></param>
		/// <param name="ip"></param>
		public RemoteDevice(int id,string ip,int videoPort,string name, string location, string type, AccessToken accessToken) {
			this.id = id;
			this.ip = ip;
			this.crestronPort = crestronPort;
			this.videoPort = videoPort;
			this.name = name;
			this.location = location;
			this.type = type;
			this.accessToken = accessToken;
			this._hasCrestron = false;
			this.crestronClient = default;
			this.controlHandler = default;
		}

		/// <summary>
		/// Does the remote device have a crestron connection
		/// </summary>
		/// <returns>True if crestron is set</returns>
		public bool hasCrestron() {
			return this._hasCrestron;
		}

		/// <summary>
		/// Create a controller instance for the crestronConnection
		/// </summary>
		/// <returns>ControllerInstance</returns>
		public ControllerInstance createControllerInstance() {
			lock (controlHandler) {
				return controlHandler.createControllerInstance();
			}
		}

		/// <summary>
		/// Get crestron client
		/// </summary>
		/// <returns></returns>
		public CrestronClient getCrestronClient() {
			return crestronClient;
		}

#region Interface impl

		public bool isConnected() {
			return crestronClient.isSocketConnected();
		}

		public void disconnect() {
			crestronClient.disconnect();
		}

		public async Task reconnect() {
			await crestronClient.connectAsync();
		}

		public bool ping(int pingTimeout) {
			Ping ping = new Ping();
			PingReply reply = ping.Send(ip,pingTimeout);
			if (reply.Status==IPStatus.Success) {
				return true;
			}

			return false;
		}

#endregion
	}
}