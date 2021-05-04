using Blazor_Instrument_Cluster.Client.Code.Websocket;
using Blazor_Instrument_Cluster.Shared.DeviceSelection;
using Blazor_Instrument_Cluster.Shared.Websocket.Enum;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Packet_Classes;
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
	public class KeyboardMouse : ComponentBase, IUpdate {

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
		/// Url encoded string with location
		/// </summary>
		[Parameter]
		public string urlName { get; set; }                                        //Name of the wanted device
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

		/// <summary>
		/// The current device id selected in the UI
		/// used for requesting device from backend
		/// </summary>
		protected string currentGuid { get; set; }

		protected Task currentConnectionTask { get; set; }

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

		private async void sendData(string s) {
			StateHasChanged();
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

			//Crestron connection
			crestronWebsocket = new CrestronWebsocket(uriCrestron,this);
			SubConnectionModel subConnectionModel = default;

			foreach (var subConnection in controllerDeviceList) {
				if (currentGuid.Equals(subConnection.guid.ToString())) {
					subConnectionModel = subConnection;
					break;
				}
			}
			//Check if device from select is in list of devices
			if (subConnectionModel is null) {
				logger.LogDebug("connectToCrestron: Connection not found");
				return;
			}
			currentConnectionTask = crestronWebsocket.startProtocol(deviceModel, subConnectionModel);
			stateHasChanged();
		}

		protected async Task stopCurrentConnection() {
			//Close old connection
			if (crestronWebsocket is not null) {
				await crestronWebsocket.cancel();
				Console.WriteLine("Waiting for task to end");
				await currentConnectionTask.ContinueWith(task => {
					switch (task.Status) {
						case TaskStatus.Created:
							break;
						case TaskStatus.WaitingForActivation:
							break;
						case TaskStatus.WaitingToRun:
							break;
						case TaskStatus.Running:
							break;
						case TaskStatus.WaitingForChildrenToComplete:
							break;
						case TaskStatus.RanToCompletion:
							break;
						case TaskStatus.Canceled:
							break;
						case TaskStatus.Faulted:
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				});
				Console.WriteLine("Task ended");
				crestronWebsocket.Dispose();
			}
			stateHasChanged();
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

		/// <summary>
		/// Temp method to take key events from virtual keyboard.
		/// TODO: refactor.
		/// </summary>
		public void virtualKeyboardHandler(string s) {
			if (s.StartsWith("up")) {
				s = s[3..];
				sendData("break " + s);
			} else if(s.StartsWith("down")) {
				s = s[5..];
				sendData("make " + s);
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

		/// <summary>
		/// IUpdate implementation that allows other classes to update the state of this class
		/// </summary>
		public void stateHasChanged() {
			StateHasChanged();
		}
	}
}