using Blazor_Instrument_Cluster.Client.Code.Websocket;
using Blazor_Instrument_Cluster.Shared.DeviceSelection;
using Blazor_Instrument_Cluster.Shared.Websocket.Enum;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using PackageClasses;
using System;
using System.Collections.Generic;
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

		/// <summary>
		/// Logger for keyboard mouse
		/// </summary>
		[Inject]
		public ILogger<KeyboardMouse> logger { get; set; }

		/// <summary>
		/// Url encoded string with name
		/// </summary>
		[Parameter]
		public string urlName { get; set; }

		/// <summary>
		/// Url encoded string with location
		/// </summary>
		[Parameter]
		public string urlLocation { get; set; }

		/// <summary>
		/// Url encoded string with type
		/// </summary>
		[Parameter]
		public string urlType { get; set; }

		/// <summary>
		/// list of control connections, Url encoded and serialized to json
		/// </summary>
		[Parameter]
		public string urlListSubconnections { get; set; }

		public string name { get; set; }
		public string location { get; set; }
		public string type { get; set; }

		/// <summary>
		/// Device name location type
		/// </summary>
		private DeviceModel deviceModel { get; set; }

		/// <summary>
		/// Crestron websocket communication
		/// </summary>
		protected CrestronWebsocket crestronWebsocket { get; set; }

		/// <summary>
		/// Token for canceling the crestron websocket
		/// </summary>
		private CancellationTokenSource crestronCanceler = new CancellationTokenSource();    //Disposal token used in websocket communication

		/// <summary>
		/// Path to crestron
		/// </summary>
		private const string PathToCrestronWebsocket = "crestronControl";

		/// <summary>
		/// Uri to the backend
		/// </summary>
		private Uri uriCrestron { get; set; }

		/// <summary>
		/// List of control connections
		/// </summary>
		protected List<SubConnectionModel> controllerDeviceList = default;

		protected string currentGuid { get; set; }

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
			await setupUri();
			//updateQueueState();
		}

		/// <summary>
		/// Get base uri and path to crestron and create a new uri to connect to for the crestron
		/// </summary>
		/// <returns></returns>
		private Task setupUri() {
			string path = navigationManager.BaseUri.Replace("https://", "wss://").Replace("http://", "wss://");
			string pathCrestron = path + PathToCrestronWebsocket;
			UriBuilder uriBuilder = new UriBuilder(pathCrestron);
			uriCrestron = uriBuilder.Uri;
			return Task.CompletedTask;
		}

		/// <summary>
		/// Parse incoming url parameters from url and make them into objects
		/// </summary>
		/// <returns></returns>
		private Task decodeUrlText() {
			try {
				name = HttpUtility.UrlDecode(urlName);
				location = HttpUtility.UrlDecode(urlLocation);
				type = HttpUtility.UrlDecode(urlType);
				deviceModel = new DeviceModel(name, location, type, new List<SubConnectionModel>());
				//convert url object to object
				string controlSubConnectionsJson = HttpUtility.UrlDecode(urlListSubconnections).TrimStart('\0').TrimEnd('\0');
				List<SubConnectionModel> controlConnections = JsonSerializer.Deserialize<List<SubConnectionModel>>(controlSubConnectionsJson);
				//Set list
				controllerDeviceList = controlConnections;

				return Task.CompletedTask;
			}
			catch (Exception e) {
				Console.WriteLine("Error in url decoding");
				return Task.FromException(e);
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

		#endregion UI Updating

		#region Sending commands

		protected async void sendData(string s) {
			StateHasChanged();
			if (!(await JS.InvokeAsync<bool>("isLocked"))) return;
			if (crestronWebsocket.state is not (CrestronWebsocketState.InControl)) {
				logger.LogDebug("sendData: CrestronWebsocket not in a state to receive data");
				return;
			}

			try {
				//Create object
				CrestronCommand sendingObject = new CrestronCommand(s);
				//Create json
				string json = JsonSerializer.Serialize(sendingObject);

				//Send data to socket.
				crestronWebsocket.trySendingControlMessage(json);
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
			if (currentGuid is null) {
				logger.LogDebug("connectToCrestronControl: no current sub connection is selected");
				return;
			}
			else if (currentGuid.Equals(String.Empty)) {
				logger.LogDebug("connectToCrestronControl: no current sub connection is selected");
				return;
			}
			//stop any old connection
			crestronWebsocket?.cancel();
			crestronWebsocket?.Dispose();
			//Crestron connection
			crestronWebsocket = new CrestronWebsocket(uriCrestron);
			SubConnectionModel subConnectionModel = default;

			foreach (var subConnection in controllerDeviceList) {
				if (currentGuid.Equals(subConnection.guid.ToString())) {
					subConnectionModel = subConnection;
					break;
				}
			}

			if (subConnectionModel is null) {
				logger.LogDebug("connectToCrestron: Connection not found");
				return;
			}
			await crestronWebsocket.startProtocol(deviceModel, subConnectionModel);
		}

		protected async Task reconnectWithCrestron() {
			await connectToCrestronControl();
		}

		#endregion Websokcet Communication

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