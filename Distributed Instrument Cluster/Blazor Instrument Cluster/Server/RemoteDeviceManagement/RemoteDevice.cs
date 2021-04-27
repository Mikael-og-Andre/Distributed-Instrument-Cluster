using Blazor_Instrument_Cluster.Server.CrestronControl;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Server_Library.Authorization;
using Server_Library.Connection_Types.Async;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Video_Library;

namespace Blazor_Instrument_Cluster.Server.RemoteDeviceManagement {

	/// <summary>
	/// A remote device connected to the server
	/// Stores data about connections belonging to each device, and the providers
	/// </summary>
	public class RemoteDevice {
		[Inject] public ILogger<RemoteDevice> logger { get; set; }

		/// <summary>
		/// Top level name of the device
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
		/// Access token received when establishing a connection
		/// </summary>
		public AccessToken accessToken { get; private set; }

		/// <summary>
		/// List of sending connections for the device
		/// </summary>
		private List<DuplexConnectionAsync> listCrestronConnections;

		/// <summary>
		/// List of control handlers for the crestron connections
		/// </summary>
		private List<ControlHandler> listControlHandlers;

		/// <summary>
		/// List of Receiving connections for the device
		/// </summary>
		private List<DuplexConnectionAsync> listVideoConenctions;

		/// <summary>
		/// Tasks that reads bytes from the socket and send them to the mjpeg stream
		/// </summary>
		private List<Task> listVideoTasks;

		/// <summary>
		/// List of sub devices
		/// </summary>
		private List<SubConnection> listOfSubconnections;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name"></param>
		/// <param name="location"></param>
		/// <param name="type"></param>
		public RemoteDevice(string name, string location, string type, AccessToken accessToken) {
			this.name = name;
			this.location = location;
			this.type = type;
			this.accessToken = accessToken;
			this.listCrestronConnections = new List<DuplexConnectionAsync>();
			this.listControlHandlers = new List<ControlHandler>();
			this.listVideoConenctions = new List<DuplexConnectionAsync>();
			this.listVideoTasks = new List<Task>();
			this.listOfSubconnections = new List<SubConnection>();
		}

		/// <summary>
		/// Adds a video connection to the list of video connections for this remote device and start its corresponding stream
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="streamer"></param>
		public async Task addVideoConnectionToDevice(DuplexConnectionAsync connection, MJPEG_Streamer streamer) {
			lock (listVideoConenctions) {
				listVideoConenctions.Add(connection);
			}
			//Add sub device
			await addVideoConnectionAsync(connection, streamer);
			////Start a provider to run asynchronously pushing frames to the stream
			Task videoProviderTask = startVideoFrameProviderAsync(connection, streamer).ContinueWith(task => {
				switch (task.Status) {
					case TaskStatus.RanToCompletion:
						logger.LogWarning("Video provider task Ended with state RanToCompletion");
						break;

					case TaskStatus.Canceled:
						logger.LogWarning("Video provider task Ended with state cancel");
						break;

					case TaskStatus.Faulted:
						logger.LogWarning("Video provider task Ended with state Faulted");
						Exception exception = task.Exception?.Flatten();
						if (exception != null) throw exception;
						break;

					default:
						logger.LogWarning("Video provider task ended without the status canceled, faulted, or ran to completion");
						break;
				}

				//Do something when ended
			});
			lock (listVideoTasks) {
				listVideoTasks.Add(videoProviderTask);
			}
		}

		/// <summary>
		/// Adds a sending connection tot he list of sending connections for this remote device
		/// </summary>
		public void addControlConnectionAsync(DuplexConnectionAsync connection) {
			lock (listCrestronConnections) {
				listCrestronConnections.Add(connection);
			}
			//Add connection to connection
			SubConnection subConnection = addControlConnection(connection);
			//create handler
			createControlHandler(subConnection);
		}

		/// <summary>
		/// Add a Video connection to the remote device
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="streamer"></param>
		private async Task addVideoConnectionAsync(DuplexConnectionAsync connection, MJPEG_Streamer streamer) {
			string streamtype = "Mjpeg";
			//Wait for a port to be assigned in the streamer
			while (!streamer.isPortSet) {
				await Task.Delay(100);
			}

			//Add a sub device
			lock (listOfSubconnections) {
				listOfSubconnections.Add(new SubConnection(connection, true,
					streamer.portNumber, streamtype));
			}
		}

		/// <summary>
		/// Add a Control connection to the remote device
		/// </summary>
		/// <param name="connection"></param>
		private SubConnection addControlConnection(DuplexConnectionAsync connection) {
			SubConnection subConnection = new SubConnection(connection);
			lock (listOfSubconnections) {
				listOfSubconnections.Add(subConnection);
			}

			return subConnection;
		}

		/// <summary>
		/// Adds a control handler for a crestron connection
		/// </summary>
		/// <param name="subConnection"></param>
		private void createControlHandler(SubConnection subConnection) {
			lock (listControlHandlers) {
				ControlHandler controlHandler = new ControlHandler(subConnection);
				listControlHandlers.Add(controlHandler);
			}
		}

		/// <summary>
		/// start a task that Pushes objects from the receiving connection to the stream
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="stream"></param>
		private async Task startVideoFrameProviderAsync(DuplexConnectionAsync connection, MJPEG_Streamer stream) {
			CancellationToken streamCancellationToken = stream.getCancellationToken();
			//Run the provider
			while (!streamCancellationToken.IsCancellationRequested) {
				try {
					byte[] img = await connection.receiveBytesAsync();
					stream.Image = img;
				}
				catch (Exception) {
					//Stop provider
					stream.Dispose();
					throw;
				}
			}
		}

		/// <summary>
		/// Get a list of sub device
		/// </summary>
		/// <returns></returns>
		public List<SubConnection> getListOfSubConnections() {
			lock (listOfSubconnections) {
				return listOfSubconnections;
			}
		}

		public bool getControlTokenForDevice(Guid guid) {
			throw new NotImplementedException();
		}
	}
}