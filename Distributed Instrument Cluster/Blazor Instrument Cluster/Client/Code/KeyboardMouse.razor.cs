using Blazor_Instrument_Cluster.Client.Code.UrlObjects;
using Blazor_Instrument_Cluster.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using PackageClasses;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Blazor_Instrument_Cluster.Client.Code {

	/// <summary>
	/// Handles capturing pointer lock data, key events and sends it to a web socket.
	/// <author>Mikael Nilssen, Andre Helland</author>
	/// </summary>
	public class KeyboardMouse : ComponentBase {

		[Inject]
		private IJSRuntime JS { get; set; }
		[Inject]
		private NavigationManager navigationManager { get; set; }

		private const string PathToCrestronWebsocket = "crestronControl";
		private CancellationTokenSource disposalTokenSource = new CancellationTokenSource();    //Disposal token used in websocket communication
		private ClientWebSocket crestronWebSocket = null;                                       //Websocket client


		[Parameter]
		public string urlName { get; set; }                                        //Name of the wanted device
		[Parameter]
		public string urlLocation { get; set; }
		[Parameter]
		public string urlType { get; set; }
		[Parameter]
		public string urlSubnames { get; set; }

		public string name { get; set; }
		public string location { get; set; }
		public string type { get; set; }

		protected List<Guid> controllerDeviceIdList = default;

		protected bool connected = false;
		protected bool deviceAndControllerFound = false;                                               //Bool representing if the control of the device has been granted
		protected bool controlling = false;

		#region Lifecycle

		//Run setup when DOM is loaded.
		protected override async Task OnAfterRenderAsync(bool firstRender) {
			if (firstRender) {
				await JS.InvokeVoidAsync("setup");
			}
		}

		/// <summary>
		/// When the application component initializes do this
		/// </summary>
		/// <returns></returns>
		protected override async Task OnInitializedAsync() {
			await decodeUrlText();
			//updateQueueState();
		}

		private async Task decodeUrlText() {
			try {

				name = HttpUtility.UrlDecode(urlName);
				location = HttpUtility.UrlDecode(urlLocation);
				type = HttpUtility.UrlDecode(urlType);
				//convert url object to object
				string controlSubdevicesJson = HttpUtility.UrlDecode(urlSubnames).TrimStart('\0').TrimEnd('\0');
				ControlConnections controlConnections = JsonSerializer.Deserialize<ControlConnections>(controlSubdevicesJson);
				//Set list
				controllerDeviceIdList = controlConnections.controllerIdList;
				
			}
			catch (Exception e) {
				Console.WriteLine("Error in url decoding");
			}
		}

		#endregion Lifecycle

		#region UI Updating

		/// <summary>
		/// If anything happens to the socket like closing or receiving a close request update the bool values
		/// </summary>
		/// <returns></returns>
		protected async Task updateQueueState() {
			
		}

		/// <summary>
		/// Sets all booleans to false
		/// </summary>
		private void resetStates() {
			//reset bools
			connected = false;
			deviceAndControllerFound = false;
			controlling = false;
			StateHasChanged();
		}

		#endregion UI Updating

		#region Sending commands

		protected async void sendData(string s) {
			StateHasChanged();
			if (!(await JS.InvokeAsync<bool>("isLocked"))) return;
			if (!deviceAndControllerFound) {
				Console.WriteLine("Device not found");
				return;
			}
			if (!controlling) {
				Console.WriteLine("Not controlling");
				return;
			}

			try {
				//Create object
				CrestronCommand sendingObject = new CrestronCommand(s);
				//Create json
				string json = JsonSerializer.Serialize(sendingObject);
				//Convert to bytes
				ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));

				//Send data to socket.
				await crestronWebSocket.SendAsync(bytesToSend, WebSocketMessageType.Text, true, disposalTokenSource.Token);
			}
			catch (Exception e) {
				Console.WriteLine(e.Message);
			}
		}

		#endregion Sending commands

		#region Websokcet Communication

		/// <summary>
		/// Connect to the backend device
		/// </summary>
		/// <returns></returns>
		protected async Task connectToCrestronControl() {
			
		}

		/// <summary>
		/// Does setup with backend websocket, sends name to server, and returns if it was found or not
		/// </summary>
		/// <returns></returns>
		private async Task<bool> setupSocket() {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Handle being in the queue and update the position
		/// </summary>
		/// <returns></returns>
		private async Task<bool> enterQueue() {
			throw new NotImplementedException();
		}

		#endregion socket

		#region Events

		protected async void click(MouseEventArgs e) {
			await JS.InvokeVoidAsync("click");
		}

		protected void mouseDown(MouseEventArgs e) {
			Console.WriteLine(e.Button);
			sendData("mouseClick (" + e.Button + ",1)");    // 1=make/down.
		}

		protected void mouseUp(MouseEventArgs e) {
			Console.WriteLine(e.Button);
			sendData("mouseClick (" + e.Button + ",0)");    // 0=break/up.
		}

		protected void keyDown(KeyboardEventArgs e) {
			Console.WriteLine(e.Code);
			Console.WriteLine(e.Key);
			switch (e.Code) {
				case "Space":
					if (downedKeys.ContainsKey(e.Code)) break;
					sendData("make space");
					downedKeys.Add(e.Code, true);
					return;

				case "Tab":
					return;

				default:
					if (downedKeys.ContainsKey(e.Code)) break;
					sendData("make " + e.Key);
					downedKeys.Add(e.Code, true);
					break;
			}
		}

		private Dictionary<string, bool> downedKeys = new();

		protected void keyUp(KeyboardEventArgs e) {
			Console.WriteLine(e.Code);
			switch (e.Code) {
				case "Space":
					sendData("break space");
					downedKeys.Remove(e.Code);
					return;

				case "Tab":
					return;

				default:
					sendData("break " + e.Key);
					downedKeys.Remove(e.Code);
					break;
			}
		}

		//TODO: FIX Scroll
		protected void scroll() {
			Console.WriteLine("ok?");
			//Console.WriteLine(e.Detail);
		}

		protected async void move(MouseEventArgs e) {
			int[] deltas = await JS.InvokeAsync<int[]>("getPositionChange");

			int x = deltas[0];
			int y = deltas[1];

			sendData("movecursor (" + x + "," + y + ")");
		}

		#endregion Events
	}
}