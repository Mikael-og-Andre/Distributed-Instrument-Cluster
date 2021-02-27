using System;
using System.Collections.Generic;

namespace Blazor_Instrument_Cluster.Server.Events {
	/// <summary>
	/// Class for sending a frame to all subscribed listeners
	/// </summary>
	public class VideoConnectionFrameProvider<T> : IObservable<T> {
		public string name { get; private set; }			//name of the device
		private List<IObserver<T>> observers;				//observers of this provider

		public VideoConnectionFrameProvider(string name) {
			this.name = name;
			observers = new List<IObserver<T>>();
		}

		/// <summary>
		/// Add observer to observer list
		/// </summary>
		/// <param name="observer"> VideoConnectionFrameConsumer</param>
		/// <returns>Unsubscribe implementation of IDisposable</returns>
		public IDisposable Subscribe(IObserver<T> observer) {
			if (!observers.Contains(observer)) {
				observers.Add(observer);
			}

			return new Unsubscriber<T>(observers, observer);
		}

		public void PushFrame(T frameResult) {
			throw new NotImplementedException();
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