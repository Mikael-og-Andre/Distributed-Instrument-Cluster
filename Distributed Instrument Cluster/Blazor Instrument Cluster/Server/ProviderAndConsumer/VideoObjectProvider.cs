using Server_Library;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace Blazor_Instrument_Cluster.Server.Events {

	/// <summary>
	/// Class for sending an object to all subscribed listeners
	/// <author>Mikael Nilssen</author>
	/// Copied from Microsoft docs and modified
	/// </summary>
	public class VideoObjectProvider<T> : IObservable<T> {

		/// <summary>
		/// name of the device
		/// </summary>
		public string name { get; set; }
		/// <summary>
		/// Location of device
		/// </summary>
		public string location { get; set; }
		/// <summary>
		/// type of device
		/// </summary>
		public string type { get; set; }
		/// <summary>
		/// subname value of the connection
		/// </summary>
		public string subname { get; set; }

		/// <summary>
		/// //observers of this provider
		/// </summary>
		private List<IObserver<T>> observers;

		/// <summary>
		/// Cancellation token source
		/// </summary>
		private CancellationTokenSource cancellationTokenSource;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name"></param>
		/// <param name="location"></param>
		/// <param name="type"></param>
		public VideoObjectProvider(string name, string location, string type, string subname) {
			this.name = name;
			this.location = location;
			this.type = type;
			this.subname = subname;
			
			observers = new List<IObserver<T>>();
			cancellationTokenSource = new CancellationTokenSource();
		}

		/// <summary>
		/// Add observer to observer list
		/// </summary>
		/// <param name="observer"> VideoObjectConsumer</param>
		/// <returns>Unsubscribe implementation of IDisposable</returns>
		public IDisposable Subscribe(IObserver<T> observer) {
			lock (observers) {
				if (!observers.Contains(observer)) {
					observers.Add(observer);
				}

				return new Unsubscriber<T>(observers, observer);
			}
		}

		/// <summary>
		/// Sends a frame to all observers
		/// </summary>
		/// <param name="frameResult"></param>
		public void pushObject(T frameResult) {
			lock (observers) {
				foreach (var observer in observers) {
					observer.OnNext(frameResult);
				}
			}
		}

		/// <summary>
		/// Gets a cancellation token for this provider
		/// </summary>
		/// <returns></returns>
		public CancellationToken getCancellationToken() {
			return cancellationTokenSource.Token;
		}

		/// <summary>
		/// Signals the cancellation token to cancel
		/// </summary>
		public void stop() {
			cancellationTokenSource.Cancel();
		}
	}

	/// <summary>
	/// Class that lets you unsubscribe from the provider
	/// Copied from Microsoft Event docs
	/// </summary>
	public class Unsubscriber<U> : IDisposable {

		/// <summary>
		/// List of observers
		/// </summary>
		private List<IObserver<U>> observers;

		/// <summary>
		/// The specific observer for this unsubscribe
		/// </summary>
		private IObserver<U> observer;

		/// <summary>
		/// Constructor for unsubscribe
		/// </summary>
		/// <param name="observers"></param>
		/// <param name="observer"></param>
		public Unsubscriber(List<IObserver<U>> observers, IObserver<U> observer) {
			this.observers = observers;
			this.observer = observer;
		}

		/// <summary>
		/// Unsubscribes
		/// </summary>
		public void Dispose() {
			if (observer != null) observers.Remove(observer);
		}
	}
}