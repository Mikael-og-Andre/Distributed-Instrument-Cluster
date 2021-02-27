using System;
using System.Collections.Generic;

namespace Blazor_Instrument_Cluster.Server.Events {
	/// <summary>
	/// Class for sending 
	/// </summary>
	public class VideoConnectionFrameProvider : IObservable<VideoFrame> {
		private string name;                            //
		private List<IObserver<VideoFrame>> observers;      //

		public VideoConnectionFrameProvider() {
			observers = new List<IObserver<VideoFrame>>();
		}

		/// <summary>
		/// Microsoft
		/// </summary>
		private class Unsubscriber : IDisposable {
			private List<IObserver<VideoFrame>> observers;
			private IObserver<VideoFrame> observer;

			public Unsubscriber(List<IObserver<VideoFrame>> observers, IObserver<VideoFrame> observer) {
				this.observers = observers;
				this.observer = observer;
			}

			public void Dispose() {
				if (observer != null) observers.Remove(observer);
			}
		}

		public IDisposable Subscribe(IObserver<VideoFrame> observer) {
			if (!observers.Contains(observer))
				observers.Add(observer);

			return new Unsubscriber(observers, observer);
		}
	}
}