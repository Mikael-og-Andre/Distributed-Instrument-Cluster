using Server_Library;
using System;
using System.Collections.Generic;

namespace Blazor_Instrument_Cluster.Server.Events {

	/// <summary>
	/// Class for sending a frame to all subscribed listeners
	/// <author>Mikael Nilssen</author>
	/// Copied from Microsoft docs and modified
	/// </summary>
	public class VideoConnectionFrameProvider : IObservable<VideoFrame> {

		/// <summary>
		/// name of the device
		/// </summary>
		public string name { get; private set; }

		/// <summary>
		/// //observers of this provider
		/// </summary>
		private List<IObserver<VideoFrame>> observers;

		/// <summary>
		/// Constructor, sets name and initializes list of observers
		/// </summary>
		/// <param name="name"></param>
		public VideoConnectionFrameProvider(string name) {
			this.name = name;
			observers = new List<IObserver<VideoFrame>>();
		}

		/// <summary>
		/// Add observer to observer list
		/// </summary>
		/// <param name="observer"> VideoConnectionFrameConsumer</param>
		/// <returns>Unsubscribe implementation of IDisposable</returns>
		public IDisposable Subscribe(IObserver<VideoFrame> observer) {
			lock (observers) {
				if (!observers.Contains(observer)) {
					observers.Add(observer);
				}

				return new Unsubscriber<VideoFrame>(observers, observer);
			}
		}

		/// <summary>
		/// Sends a frame to all observers
		/// </summary>
		/// <param name="frameResult"></param>
		public void PushFrame(VideoFrame frameResult) {
			lock (observers) {
				foreach (var observer in observers) {
					observer.OnNext(frameResult);
				}
			}
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