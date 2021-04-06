using Microsoft.AspNetCore.Components;
using PackageClasses;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


namespace Blazor_Instrument_Cluster.Client.Code {
	/// <summary>
	/// Video component implementation
	/// <Author> Mikael Nilssen, Andre Helland</Author>
	/// </summary>
	public class Video : ComponentBase,IDisposable {

		/// <summary>
		/// Navigation manger for traversing the blazor page
		/// </summary>
		[Inject]
		private NavigationManager navigationManager { get; set; }

		private readonly CancellationTokenSource disposalTokenSource = new();           //Disposal token for async communication

		private readonly ClientWebSocket videoWebSocket = new();                        //Client websocket
		private bool foundDevice = false;                                               //Was the requested device found or not

		[Parameter]
		public string name { get; set; }                                        //Name of the wanted device

		[Parameter]
		public string location { get; set; }

		[Parameter]
		public string type { get; set; }

		[Parameter]
		public string subname { get; set; }

		protected string imgsrc = "";                                             //img to show
		private int frameBufferSize = 900000;                                   //Buffer for incoming msg's

		private string pathToVideoWebsocket = "videoStream";

		protected override async Task OnInitializedAsync() {
			//Try to connect on launch
			await connectAsync();
		}

		/// <summary>
		/// Connect to the video websocket on the backend server
		/// </summary>
		/// <returns></returns>
		private async Task connectAsync() {
			try {
				//Get base uri and connect to that
				string basePath = navigationManager.BaseUri;
				basePath = basePath.Replace("https://", "wss://");

				await videoWebSocket.ConnectAsync(new Uri(basePath + pathToVideoWebsocket), disposalTokenSource.Token);

				//Sends name and setup socket
				foundDevice = await setupSocket();

				//Receive video
				_ = receiveVideo();
			}
			catch (Exception) {
				Console.WriteLine("Unable to connect due to unknown error");
			}
			return;
		}

		/// <summary>
		/// Sets up the socket, send name of wanted device, and receive if it was found or not
		/// </summary>
		/// <returns></returns>
		private async Task<bool> setupSocket() {
			try {
				//Receive start signal
				byte[] startBuffer = new byte[1024];
				ArraySegment<byte> startSignalBuffer = new ArraySegment<byte>(startBuffer);
				await videoWebSocket.ReceiveAsync(startSignalBuffer, disposalTokenSource.Token);

				//Send name
				ArraySegment<byte> nameBytes = new ArraySegment<byte>(Encoding.UTF8.GetBytes(name));
				await videoWebSocket.SendAsync(nameBytes, WebSocketMessageType.Text, true, disposalTokenSource.Token);

				//Send location
				ArraySegment<byte> locationBytes = new ArraySegment<byte>(Encoding.UTF8.GetBytes(location));
				await videoWebSocket.SendAsync(locationBytes, WebSocketMessageType.Text, true, disposalTokenSource.Token);

				//Send type
				ArraySegment<byte> typeBytes = new ArraySegment<byte>(Encoding.UTF8.GetBytes(type));
				await videoWebSocket.SendAsync(typeBytes, WebSocketMessageType.Text, true, disposalTokenSource.Token);

				//Send subanme
				ArraySegment<byte> subnameBytes = new ArraySegment<byte>(Encoding.UTF8.GetBytes(subname));
				await videoWebSocket.SendAsync(subnameBytes, WebSocketMessageType.Text, true, disposalTokenSource.Token);

				//Get found or not
				byte[] foundBuffer = new byte[1024];
				ArraySegment<byte> foundBytes = new ArraySegment<byte>(foundBuffer);
				await videoWebSocket.ReceiveAsync(foundBytes, disposalTokenSource.Token);
				string found = Encoding.UTF8.GetString(foundBytes).TrimEnd('\0');

				return found.ToLower().Equals("found".ToLower());
			}
			catch (Exception) {
				return false;
			}
		}

		/// <summary>
		/// Start Receiving video
		/// </summary>
		/// <returns></returns>
		private async Task receiveVideo() {
			while (!disposalTokenSource.IsCancellationRequested) {
				//If device is not found skip
				if (!foundDevice) continue;
				//get frames
				try {
					ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[frameBufferSize]);
					await videoWebSocket.ReceiveAsync(buffer, disposalTokenSource.Token);

					string incomingJson = Encoding.UTF8.GetString(buffer).TrimEnd('\0');

					ExampleVideoObject exampleVideoObject = null;
					try {
						exampleVideoObject = JsonSerializer.Deserialize<ExampleVideoObject>(incomingJson);
					}
					catch (Exception) {
						Console.WriteLine("Could not serialize incoming json");
						continue;
					}
					//Update image
					imgsrc = string.Format("data:image/jpg;base64,{0}", exampleVideoObject.imgbase64);

					//Console.WriteLine("Base64: {0}", exampleVideoObject.imgbase64);

					StateHasChanged();
				}
				catch (Exception e) {
					Console.WriteLine(e);
					throw;
				}
			}
		}
		/// <summary>
		/// Close connection
		/// </summary>
		public void Dispose() {
			disposalTokenSource.Cancel();
			_ = videoWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None);
		}
	}
}