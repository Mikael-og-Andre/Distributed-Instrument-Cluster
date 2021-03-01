using System;
using System.Collections.Generic;
using Instrument_Communicator_Library.Information_Classes;

namespace Blazor_Instrument_Cluster.Server.Events {
	/// <summary>
	/// Class for sending a frame to all subscribed listeners
	/// </summary>
	public class VideoConnectionFrameProvider : IObservable<VideoFrame> {
		public string name { get; private set; }			//name of the device
		private List<IObserver<VideoFrame>> observers;				//observers of this provider

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
	/// Copied from Microsoft Event docs
	/// </summary>
	public class Unsubscriber<U> : IDisposable {
		private List<IObserver<U>> observers;
		private IObserver<U> observer;

		public Unsubscriber(List<IObserver<U>> observers, IObserver<U> observer) {
			this.observers = observers;
			this.observer = observer;
		}

		public void Dispose() {
			if (observer != null) observers.Remove(observer);
		}
	}
}