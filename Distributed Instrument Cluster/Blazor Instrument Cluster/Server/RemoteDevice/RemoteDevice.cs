using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.Events;
using Blazor_Instrument_Cluster.Server.ProviderAndConsumer;
using Blazor_Instrument_Cluster.Shared;
using OpenCvSharp;
using PackageClasses;
using Server_Library;
using Server_Library.Connection_Types;
using Video_Library;

namespace Blazor_Instrument_Cluster.Server.RemoteDevice {
	/// <summary>
	/// A remote device connected to the server
	/// Stores data about connections belonging to each device, and the providers
	/// </summary>
	/// <typeparam name="Jpeg"></typeparam>
	/// <typeparam name="U"></typeparam>
	public class RemoteDevice<U> {

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
		private List<SendingConnection<U>> listOfSendingConnections;

		/// <summary>
		/// List of Receiving connections for the device
		/// </summary>
		private List<ReceivingConnection<Jpeg>> listOfReceivingConnections;

		/// <summary>
		/// List of video object providers
		/// </summary>
		private List<VideoObjectProvider> listOfReceivingConnectionProviders;

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
			this.listOfSendingConnections = new List<SendingConnection<U>>();
			this.listOfReceivingConnections = new List<ReceivingConnection<Jpeg>>();
			this.listOfReceivingConnectionProviders = new List<VideoObjectProvider>();
            this.listOfSubDevices = new List<SubDevice>();
        }

		/// <summary>
		/// Adds a receiving connection to the list of receiving connections for this remote device and start its corresponding device
		/// Starts a provider for the incoming connection
		/// </summary>
		/// <param name="receivingConnection"></param>
		/// <param name="streamer"></param>
		public void addReceivingConnection(ReceivingConnection<Jpeg> receivingConnection, MJPEG_Streamer streamer) {
			lock (listOfReceivingConnections) {
				listOfReceivingConnections.Add(receivingConnection);
			}
			//Add sub device
			addVideoSubdevice(receivingConnection, streamer);

			////Start a provider
			startVideoFrameProvider(receivingConnection, streamer);
		}

		private void addVideoSubdevice(ReceivingConnection<Jpeg> receivingConnection, MJPEG_Streamer streamer) {
			
			string streamtype = "Mjpeg";

			while (!streamer.isPortSet) {
				Thread.Sleep(50);
			}

			lock (listOfSubDevices) {
				listOfSubDevices.Add(new SubDevice(true,receivingConnection.getInstrumentInformation().SubName,streamer.portNumber,streamtype));
			}
		}

		private void addControlDevice(SendingConnection<U> sendingConnection) {
			lock (listOfSubDevices) {
				listOfSubDevices.Add(new SubDevice(false,sendingConnection.getInstrumentInformation().SubName,0,""));
			}
		}

		/// <summary>
		/// Adds a sending connection tot he list of sending connections for this remote device
		/// </summary>
		/// <param name="sendingConnection"></param>
		public void addSendingConnection(SendingConnection<U> sendingConnection) {
			lock (listOfSendingConnections) {
				listOfSendingConnections.Add(sendingConnection);	
			}
			//Add subdevice
			addControlDevice(sendingConnection);
		}


		private void startVideoFrameProvider(ReceivingConnection<Jpeg> receivingConnection, MJPEG_Streamer stream) {
			//Info about client
			ClientInformation info = receivingConnection.getInstrumentInformation();

			CancellationToken streamCancellationToken = stream.getCancellationToken();

			//Run the provider
			Task.Run(() => {
				while (!streamCancellationToken.IsCancellationRequested) {
					try { 
						//Try to get an object and broadcast it to subscribers
						if (receivingConnection.getObjectFromConnection(out Jpeg output)) {
							stream.image = output.jpeg.ToArray();
						}
						else {
							Thread.Sleep(10);
						}
					}
					catch (Exception ex) {
						//Stop provider
						stream.Dispose();
						throw;
					}
				}
			});
		}

        /// <summary>
		/// Checks all providers for a matching subname and subscribes the consumer to it
		/// </summary>
		/// <param name="subname">subname of the wanted connection</param>
		/// <param name="consumer">Consumer that will be subscribed</param>
		/// <returns>True or false</returns>
		public bool subscribeToProvider(VideoObjectConsumer consumer) {

			string consumerSubname = consumer.subname;

			lock (listOfReceivingConnectionProviders) {
				//Check all providers
				foreach (var provider in listOfReceivingConnectionProviders) {
					string providerSubname = provider.subname;
					//Check if subnames match
					if (providerSubname.ToLower().Equals(consumerSubname.ToLower())) {
						//Subscribe the consumer and return true
						provider.Subscribe(consumer);
						return true;
					}
				}
				//No match found return false
				return false;
			}
		}

		/// <summary>
		/// Get a sending connection with the matching subname
		/// </summary>
		/// <param name="subname"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		public bool getSendingConnectionWithSubname(string subname, out SendingConnection<U> output) {

			lock (listOfSendingConnections) {
				foreach (var connection in listOfSendingConnections) {

					ClientInformation info = connection.getInstrumentInformation();

					//If they match return it
					if (info.SubName.ToLower().Equals(subname.ToLower())) {
						output = connection;
						return true;
					}
				}
			}

			output = default;
			return false;
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
	}
}