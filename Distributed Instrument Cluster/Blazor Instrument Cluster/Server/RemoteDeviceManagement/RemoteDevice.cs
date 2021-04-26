using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.CrestronControl;
using Server_Library;
using Server_Library.Connection_Types;
using Video_Library;

namespace Blazor_Instrument_Cluster.Server.RemoteDeviceManagement {
	/// <summary>
	/// A remote device connected to the server
	/// Stores data about connections belonging to each device, and the providers
	/// </summary>
	public class RemoteDevice {

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
		/// List of sending connections for the device
		/// </summary>
		private List<SendingConnection> listOfSendingConnections;

		/// <summary>
		/// List of Receiving connections for the device
		/// </summary>
		private List<ReceivingConnection> listOfReceivingConnections;

		/// <summary>
		/// List of Sending connection handlers
		/// </summary>
		private List<ControlHandler> listOfSendingControlHandlers;

		/// <summary>
		/// List of sub devices
		/// </summary>
		private List<SubDevice> listOfSubDevices;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name"></param>
		/// <param name="location"></param>
		/// <param name="type"></param>
		public RemoteDevice(string name, string location, string type) {
			this.name = name;
			this.location = location;
			this.type = type;
			this.listOfSendingConnections = new List<SendingConnection>();
			this.listOfSendingControlHandlers = new List<ControlHandler>();
			this.listOfReceivingConnections = new List<ReceivingConnection>();
			this.listOfSubDevices = new List<SubDevice>();
		}

		/// <summary>
		/// Adds a receiving connection to the list of receiving connections for this remote device and start its corresponding device
		/// Starts a provider for the incoming connection
		/// </summary>
		/// <param name="receivingConnection"></param>
		/// <param name="streamer"></param>
		public void addReceivingConnection(ReceivingConnection receivingConnection, MJPEG_Streamer streamer) {
			lock (listOfReceivingConnections) {
				listOfReceivingConnections.Add(receivingConnection);
			}

			//Add sub device
			addVideoSubdevice(receivingConnection, streamer);

			////Start a provider
			startVideoFrameProvider(receivingConnection, streamer);
		}

		/// <summary>
		/// Add a Video Subdevice to the remote device
		/// </summary>
		/// <param name="receivingConnection"></param>
		/// <param name="streamer"></param>
		private void addVideoSubdevice(ReceivingConnection receivingConnection, MJPEG_Streamer streamer) {

			string streamtype = "Mjpeg";
			//Wait for a port to be assigned in the streamer
			while (!streamer.isPortSet) {
				Thread.Sleep(10);
			}

			//Add a sub device
			lock (listOfSubDevices) {
				listOfSubDevices.Add(new SubDevice(true, receivingConnection.getClientInformation().SubName,
					streamer.portNumber, streamtype));
			}
		}

		/// <summary>
		/// Add a Control Subdevice to the remote device
		/// </summary>
		/// <param name="sendingConnection"></param>
		private void addControlDevice(SendingConnection sendingConnection) {
			lock (listOfSubDevices) {
				listOfSubDevices.Add(new SubDevice(false, sendingConnection.getClientInformation().SubName, 0, ""));
			}
		}

		/// <summary>
		/// Adds a sending connection tot he list of sending connections for this remote device
		/// </summary>
		/// <param name="sendingConnection"></param>
		public void addSendingConnection(SendingConnection sendingConnection,bool allowMultiUser, double allowedInactiveMinutes) {
			lock (listOfSendingConnections) {
				listOfSendingConnections.Add(sendingConnection);
			}

			//create handler
			startControlHandler(allowedInactiveMinutes, sendingConnection, allowMultiUser);
			//Add subdevice
			addControlDevice(sendingConnection);
		}
		

		/// <summary>
		/// start a task that Pushes objects from the receiving connection to the stream
		/// </summary>
		/// <param name="receivingConnection"></param>
		/// <param name="stream"></param>
		private void startVideoFrameProvider(ReceivingConnection receivingConnection, MJPEG_Streamer stream) {
			//Info about client
			ClientInformation info = receivingConnection.getClientInformation();

			CancellationToken streamCancellationToken = stream.getCancellationToken();

			//Run the provider
			Task.Run(() => {
				while (!streamCancellationToken.IsCancellationRequested) {
					try {
						//Try to get an object and broadcast it to subscribers
						if (receivingConnection.getDataFromConnection(out byte[] output)) {
							stream.Image = output;
						}
						else {
							Thread.Sleep(5);
						}
					}
					catch (Exception) {
						//Stop provider
						stream.Dispose();
						throw;
					}
				}
			});
		}

		/// <summary>
		/// Get a list of sub device
		/// </summary>
		/// <returns></returns>
		public List<SubDevice> getSubDeviceList() {
			lock (listOfSubDevices) {
				return listOfSubDevices;
			}
		}

		/// <summary>
		/// Adds a handler for a sending connection
		/// </summary>
		/// <param name="allowedInactiveMinutes"></param>
		/// <param name="sendingConnection"></param>
		/// <param name="allowMultiUser"></param>
		private void startControlHandler(double allowedInactiveMinutes, SendingConnection sendingConnection,
			bool allowMultiUser) {
			//Create and add a handler
			ControlHandler handler =
				new ControlHandler(allowedInactiveMinutes, sendingConnection, allowMultiUser);
			lock (listOfSendingControlHandlers) {
				listOfSendingControlHandlers.Add(handler);
			}

		}

		/// <summary>
		/// Get a control token for a sendingConnection Controlhandler
		/// </summary>
		/// <param name="subname">Subname of the wanted device</param>
		/// <param name="output">Output SendingControlHandler</param>
		/// <returns>True if found, False if not</returns>
		public bool getControlTokenForDevice(string subname, out ControlToken output) {
			lock (listOfSendingControlHandlers) {
				//Loop handlers and check
				foreach (var handler in listOfSendingControlHandlers) {
					ClientInformation info = handler.getClientInformation();

					//If subnames match return it a token
					if (info.SubName.ToLower().Equals(subname.ToLower())) {
						ControlToken controlToken = handler.generateToken();
						output = controlToken;
						return true;
					}
				}
			}
			//Not found
			output = default;
			return false;
		}
	}
}