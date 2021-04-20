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
	/// <author>Andre Helland, Mikael Nilssen</author>
	/// </summary>
	public class KeyboardMouse : ComponentBase {

		[Inject]
		private IJSRuntime JS { get; set; }
		[Inject]
		private NavigationManager navigationManager { get; set; }

		private const string pathToCrestronWebsocket = "crestronControl";
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

		protected List<string> listOfSubNames = default;
		protected string currentSubname = default;

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
			continuouslyCheckSocketStateAsync();
		}

		private async Task decodeUrlText() {
			try {

				name = HttpUtility.UrlDecode(urlName);
				location = HttpUtility.UrlDecode(urlLocation);
				type = HttpUtility.UrlDecode(urlType);
				//convert url object to object
				string controlSubdevicesJson = HttpUtility.UrlDecode(urlSubnames).TrimStart('\0').TrimEnd('\0');
				ControlSubdevices controlSubdevices = JsonSerializer.Deserialize<ControlSubdevices>(controlSubdevicesJson);
				//Set list
				listOfSubNames = controlSubdevices.subnameList;
				currentSubname = listOfSubNames[0];
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
		protected async Task continuouslyCheckSocketStateAsync() {
			await Task.Run(() => {
				while (!disposalTokenSource.IsCancellationRequested) {
					if (
						(crestronWebSocket.State == WebSocketState.CloseReceived) ||
						(crestronWebSocket.State == WebSocketState.CloseSent) ||
						(crestronWebSocket.State == WebSocketState.Closed) ||
						(crestronWebSocket.State == WebSocketState.Aborted)
					) {
						//reset bools
						resetStates();
						StateHasChanged();
					}
					Task.Delay(5000);
				}
			});
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

		#region socket

		/// <summary>
		/// Connect to the backend device
		/// </summary>
		/// <returns></returns>
		protected async Task connectToCrestronControl() {
			//if name is not in the list of subnames, dont try to connect
			if (!listOfSubNames.Contains(currentSubname)) {
				Console.WriteLine("Can not connect to this subname");
				return;
			}
			//abort websocket one already exists and reset website states
			crestronWebSocket?.Abort();
			resetStates();
			crestronWebSocket = new ClientWebSocket();

			//Get base uri and connect to that
			string basePath = navigationManager.BaseUri;
			basePath = basePath.Replace("https://", "wss://");

			await crestronWebSocket.ConnectAsync(new Uri(basePath + pathToCrestronWebsocket), disposalTokenSource.Token);
			//Check if the device requested exists
			try {
				StateHasChanged();
				deviceAndControllerFound = await setupSocket();
				StateHasChanged();
				//enter the queue
				if (deviceAndControllerFound) {
					controlling = await enterQueue();
					StateHasChanged();
				}
			}
			catch (Exception) {
				Console.WriteLine("Connection failed");
			}
		}

		/// <summary>
		/// Does setup with backend websocket, sends name to server, and returns if it was found or not
		/// </summary>
		/// <returns></returns>
		private async Task<bool> setupSocket() {
			try {
				//Receive start signal
				byte[] startBuffer = new byte[1024];
				ArraySegment<byte> startSignalBuffer = new ArraySegment<byte>(startBuffer);
				await crestronWebSocket.ReceiveAsync(startSignalBuffer, disposalTokenSource.Token);

				//Get json data for a RequestConnectionModel
				RequestConnectionModel requestModel = new RequestConnectionModel(name, location, type, currentSubname);

				string json = JsonSerializer.Serialize(requestModel);
				ArraySegment<byte> jsonArraySegment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));
				//Send device data
				await crestronWebSocket.SendAsync(jsonArraySegment, WebSocketMessageType.Text, true, disposalTokenSource.Token);

				//Get found or not
				byte[] foundBuffer = new byte[1024];
				ArraySegment<byte> foundBytes = new ArraySegment<byte>(foundBuffer);
				await crestronWebSocket.ReceiveAsync(foundBytes, disposalTokenSource.Token);
				string found = Encoding.UTF8.GetString(foundBytes).TrimEnd('\0');

				//Device was found
				return found.ToLower().Equals("Found Device".ToLower());
			}
			catch (Exception) {
				Console.WriteLine("Error when requesting a device");
				return false;
			}
		}

		/// <summary>
		/// Handle being in the queue and update the position
		/// </summary>
		/// <returns></returns>
		private async Task<bool> enterQueue() {
			try {
				//receive in queue
				byte[] startQueueBytes = new byte[1024];
				ArraySegment<byte> startQueueSegment = new ArraySegment<byte>(startQueueBytes);
				await crestronWebSocket.ReceiveAsync(startQueueSegment, disposalTokenSource.Token);

				while (!disposalTokenSource.IsCancellationRequested) {
					QueueStatusModel queueStatus = null;
					try {
						//Receive a message
						byte[] queueBytes = new byte[4098];
						ArraySegment<byte> qSegment = new ArraySegment<byte>(queueBytes);
						await crestronWebSocket.ReceiveAsync(qSegment, disposalTokenSource.Token);
						string receivedJson = Encoding.UTF8.GetString(queueBytes).TrimEnd('\0');

						queueStatus = JsonSerializer.Deserialize<QueueStatusModel>(receivedJson);

						//If you have control return true
						if (queueStatus != null && queueStatus.hasControl) {
							Console.WriteLine("Received Control");
							return true;
						}
					}
					catch (Exception) {
						Console.WriteLine("Failed deserialize in queue");
						return false;
					}
				}
				return false;
			}
			catch (Exception) {
				Console.WriteLine("Error occurred while in queue");
				return false;
			}
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