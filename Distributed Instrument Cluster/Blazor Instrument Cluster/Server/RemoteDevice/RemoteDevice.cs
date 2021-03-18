using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.Events;
using Server_Library;
using Server_Library.Connection_Types;

namespace Blazor_Instrument_Cluster.Server.RemoteDevice {
	/// <summary>
	/// A remote device connected to the server
	/// Stores data about connections belonging to each device, and the providers
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="U"></typeparam>
	public class RemoteDevice<T, U> {

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
		private List<ReceivingConnection<T>> listOfReceivingConnections;

		/// <summary>
		/// List of video object providers
		/// </summary>
		private List<VideoObjectProvider<T>> listOfReceivingConnectionProviders;

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
			this.listOfReceivingConnections = new List<ReceivingConnection<T>>();
			this.listOfReceivingConnectionProviders = new List<VideoObjectProvider<T>>();
		}

		/// <summary>
		/// Adds a receiving connection to the list of receiving connections for this remote device and start its corresponding device
		/// Starts a provider for the incoming connection
		/// </summary>
		/// <param name="receivingConnection"></param>
		public void addReceivingConnection(ReceivingConnection<T> receivingConnection) {
			lock (listOfReceivingConnections) {
				listOfReceivingConnections.Add(receivingConnection);
			}
			//Start a provider
			startProvider(receivingConnection);
		}

		/// <summary>
		/// Adds a sending connection tot he list of sending connections for this remote device
		/// </summary>
		/// <param name="sendingConnection"></param>
		public void addSendingConnection(SendingConnection<U> sendingConnection) {
			lock (listOfSendingConnections) {
				listOfSendingConnections.Add(sendingConnection);	
			}
		}


		private void startProvider(ReceivingConnection<T> receivingConnection) {
			//Info about client
			ClientInformation info = receivingConnection.getInstrumentInformation();
			//Create new provider
			VideoObjectProvider<T> provider = new VideoObjectProvider<T>(info.Name,info.Location,info.Type,info.SubName);

			//Add to list of providers
			lock (listOfReceivingConnectionProviders) {
				listOfReceivingConnectionProviders.Add(provider);
			}

			//Get cancellation token
			CancellationToken providerCancellationToken = provider.getCancellationToken();
			//Run the provider
			Task.Run(() => {
				while (!providerCancellationToken.IsCancellationRequested) {
					try { 
						//Try to get an object and broadcast it to subscribers
						if (receivingConnection.getObjectFromConnection(out T output)) {
							provider.pushObject(output);
						}
						else {
							Thread.Sleep(300);
						}
					}
					catch (Exception ex) {
						//Stop provider
						provider.stop();
					}
				}
			});
		}

		/// <summary>
		/// Searches all connections for their sub names and returns a list fo them
		/// </summary>
		/// <returns></returns>
		public List<string> getSubNamesList() {
			List<string> newList = new List<string>();

			lock (listOfReceivingConnections) {
				foreach (var receiving in listOfReceivingConnections) {
					newList.Add(receiving.getInstrumentInformation().SubName);
				}
			}

			lock (listOfSendingConnections) {
				foreach (var sending in listOfSendingConnections) {
					newList.Add(sending.getInstrumentInformation().SubName);
				}
			}

			return newList;
		}

		/// <summary>
		/// Checks all providers for a matching subname and subscribes the consumer to it
		/// </summary>
		/// <param name="subname">subname of the wanted connection</param>
		/// <param name="consumer">Consumer that will be subscribed</param>
		/// <returns>True or false</returns>
		public bool subscribeToProvider(VideoObjectConsumer<T> consumer) {

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
	}
}