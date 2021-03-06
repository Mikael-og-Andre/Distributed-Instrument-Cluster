﻿using Blazor_Instrument_Cluster.Client.Code.Websocket;
using Blazor_Instrument_Cluster.Shared.DeviceSelection;
using Blazor_Instrument_Cluster.Shared.Websocket.Enum;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Packet_Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
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

		[Inject]
		public HttpClient client { get; set; }

		/// <summary>
		/// Url encoded string with location
		/// </summary>
		[Parameter]
		public string urlDeviceJson { get; set; }                                        //Name of the wanted device


		private string name { get; set; }
		private string location { get; set; }
		private string type { get; set; }
		private bool hasCrestron { get; set; }
		private Dictionary<string, KeyDictionary.key> keyDictionary = new();

		/// <summary>
		/// Device name location type
		/// </summary>
		private DisplayRemoteDeviceModel displayRemoteDeviceModel { get; set; }

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
		/// Current task the connection is in
		/// </summary>
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
			var file = await client.GetByteArrayAsync("Json/KeyDictionary.json");
			var json = JsonSerializer.Deserialize<KeyDictionary>(file[3..]);	//BUG: JSON file starts with "." for some fucking reason!? Removed using [3..].
			foreach (var key in json.dictionary) {
				keyDictionary.Add(key.keyCode, key);
			}
			//updateQueueState();
		}

		/// <summary>
		/// Get base uri and path to crestron and create a new uri to connect to for the crestron
		/// </summary>
		/// <returns></returns>
		private Task setupUri() {
			string path = navigationManager.BaseUri.Replace("https://", "wss://").Replace("http://", "ws://");
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
				string deviceModelJson = HttpUtility.UrlDecode(urlDeviceJson,Encoding.UTF8);

				displayRemoteDeviceModel  = JsonSerializer.Deserialize<DisplayRemoteDeviceModel>(deviceModelJson);

				name = displayRemoteDeviceModel.name;
				location = displayRemoteDeviceModel.location;
				type = displayRemoteDeviceModel.type;
				hasCrestron = displayRemoteDeviceModel.hasCrestron;

				return Task.CompletedTask;
			}
			catch (Exception e) {
				Console.WriteLine("Error in url decoding");
				return Task.FromException(e);
			}
		}

		#endregion Lifecycle

		#region Sending commands

		private async void sendData(string s, bool overwrite=false) {
			//check if device is null
			if (crestronWebsocket is null) {
				return;
			}

			if (!(await JS.InvokeAsync<bool>("isLocked")) && !overwrite) {
				logger.LogDebug("sendData: Can not send data when not locked");
				return;
			}
			updateState();
			if (crestronWebsocket.state is not (CrestronWebsocketState.InControl)) {
				logger.LogDebug("sendData: CrestronWebsocket not in a state to receive data");
				return;
			}

			try {
				//Create object
				CrestronCommand sendingObject = new CrestronCommand(s);
				//Create json
				string json = JsonSerializer.Serialize(sendingObject);

				bool wasSent = await crestronWebsocket.sendExternal(json);
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

			//Crestron connection
			crestronWebsocket = new CrestronWebsocket(uriCrestron, this);

			currentConnectionTask = crestronWebsocket.startProtocol(displayRemoteDeviceModel);
			updateState();
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
			updateState();
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


		private readonly Dictionary<string, bool> downedKeys = new();

		protected void keyDown(KeyboardEventArgs e) {
			if (!keyDictionary.TryGetValue(e.Code, out var key)) return;
			if (key==null) return;
			if (downedKeys.TryGetValue(key.keyValue, out _)) return;
			if (key.breaks) return;

			sendData($"make {key.keyValue}");
			downedKeys.Add(key.keyValue, true);
		}

		protected void keyUp(KeyboardEventArgs e) {
			if (!keyDictionary.TryGetValue(e.Code, out var key)) return;
			if (key == null) return;
			if (key.breaks) return;

			sendData($"break {key.keyValue}");
			downedKeys.Remove(key.keyValue);
		}

		/// <summary>
		/// Temp method to take key events from virtual keyboard.
		/// TODO: refactor.
		/// </summary>
		public void virtualKeyboardHandler(string s) {
			
			if (s.StartsWith("up")) {
				s = s[3..];
				sendData("break " + s, true);
			}
			else if (s.StartsWith("down")) {
				s = s[5..];
				sendData("make " + s, true);
			}

		}

		//TODO: FIX Scroll
		protected void scroll() {
			Console.WriteLine("ok?");
			//Console.WriteLine(e.Detail);
		}

		private long sendTime = 0;
		private long sendDelay = 100000000;
		private int[] deltas = new int[2];

		protected async void move(MouseEventArgs e) {
			int[] deltas = await JS.InvokeAsync<int[]>("getPositionChange");

			//int x = deltas[0];
			//int y = deltas[1];

			this.deltas[0] += deltas[0];
			this.deltas[1] += deltas[1];

			if (sendTime >= Stopwatch.GetTimestamp()) return;
			sendTime = Stopwatch.GetTimestamp() + sendDelay;
			sendData("movecursor (" + this.deltas[0] + "," + this.deltas[1] + ")");
			this.deltas[0] = 0;
			this.deltas[1] = 0;
		}

		#endregion Events


		/// <summary>
		/// IUpdate implementation that allows other classes to update the state of this class
		/// </summary>
		public void updateState() {
			StateHasChanged();
		}
	}

	/// <summary>
	/// Json class for storing key translation from frontend web values to backend crestron e.g. KeyL => L, Digit3 => 3.
	/// </summary>
	public class KeyDictionary {
		public List<key> dictionary { get; set; }

		public class key {
			public string keyCode { get; set; }
			public string keyValue { get; set; }
			public bool breaks { get; set; }
		}
	}
}