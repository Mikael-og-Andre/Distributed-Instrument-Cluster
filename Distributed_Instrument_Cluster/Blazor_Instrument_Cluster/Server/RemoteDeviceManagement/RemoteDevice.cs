using Blazor_Instrument_Cluster.Server.CrestronControl;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement;
using Blazor_Instrument_Cluster.Shared.Websocket;
using Microsoft.AspNetCore.Mvc.Rendering;
using Video_Library;

namespace Blazor_Instrument_Cluster.Server.RemoteDeviceManagement {

	/// <summary>
	/// A remote device
	/// Stores data about the identity belonging to each device, and endpoints.
	/// </summary>
	public class RemoteDevice{

		/// <summary>
		/// Device id
		/// </summary>
		public int id { get; set; }
		
		/// <summary>
		/// Internet address of the remote device
		/// </summary>
		public string ip { get; private set; }
		/// <summary>
		/// Port where the crestron server is located
		/// </summary>
		private int crestronPort { get; set; }

		/// <summary>
		/// List of ports where mjpeg servers are located
		/// </summary>
		public int videoPort { get; private set; }

		/// <summary>
		/// The number of video devices
		/// </summary>
		public int videoDeviceNumber { get; private set; }

		/// <summary>
		/// name of the device
		/// </summary>
		public string name { get; private set; }

		/// <summary>
		/// location of the device
		/// </summary>
		public string location { get; private set; }

		/// <summary>
		/// type of the device
		/// </summary>
		public string type { get; private set; }

		/// <summary>
		/// Handles control of the crestronClient
		/// </summary>
		private CrestronUserHandler crestronUserHandler { get; set; }

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
		/// <param name="videoDeviceNumber">The number of video devices</param>
		/// <param name="name"></param>
		/// <param name="location"></param>
		/// <param name="type"></param>
		/// <param name="id"></param>
		/// <param name="ip"></param>
		public RemoteDevice(int id,string ip,int crestronPort,int videoPort,int videoDeviceNumber,string name, string location, string type) {
			this.id = id;
			this.ip = ip;
			this.crestronPort = crestronPort;
			this.videoPort = videoPort;
			this.videoDeviceNumber = videoDeviceNumber;
			this.name = name;
			this.location = location;
			this.type = type;
			this._hasCrestron = true;
			this.crestronClient = new CrestronClient(ip,crestronPort);
			this.crestronUserHandler = new CrestronUserHandler(crestronClient);
		}

		/// <summary>
		/// Constructor without crestron
		/// </summary>
		/// <param name="videoPort"></param>
		/// <param name="videoDeviceNumber">Number of video devices</param>
		/// <param name="name"></param>
		/// <param name="location"></param>
		/// <param name="type"></param>
		/// <param name="id"></param>
		/// <param name="ip"></param>
		public RemoteDevice(int id,string ip,int videoPort,int videoDeviceNumber,string name, string location, string type) {
			this.id = id;
			this.ip = ip;
			this.crestronPort = default;
			this.videoPort = videoPort;
			this.videoDeviceNumber = videoDeviceNumber;
			this.name = name;
			this.location = location;
			this.type = type;
			this._hasCrestron = false;
			this.crestronClient = default;
			this.crestronUserHandler = default;
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
		/// <returns>CrestronUser</returns>
		public CrestronUser createCrestronUser() {
			lock (crestronUserHandler) {
				return crestronUserHandler.createCrestronUser();
			}
		}

		/// <summary>
		/// Get crestron client
		/// </summary>
		/// <returns></returns>
		public CrestronClient getCrestronClient() {
			return crestronClient;
		}

		/// <summary>
		/// Ping server
		/// </summary>
		/// <returns></returns>
		public bool ping(int timeout) {
			try {

				IPHostEntry hostInfo = Dns.GetHostEntry(ip);
				IPAddress[] addresses = hostInfo.AddressList;

				foreach (var address in addresses) {
					Ping ping = new Ping();
					PingReply reply = ping.Send(address, timeout);
					if (reply.Status == IPStatus.Success) {
						return true;
					}
				}
				return false;
			}
			catch (Exception e) {
				//Console.WriteLine($"Exception in remoteDevice ping: {e.Message}");
				return false;
			}
		}
	}
}