using System;
using System.Collections.Concurrent;

namespace Blazor_Instrument_Cluster.Server.Events {
	/// <summary>
	/// Get video frames from the connected devices
	/// </summary>
	public class VideoConnectionFrameConsumer<T> :IObserver<T> {

		private IDisposable unsubscriber;
		private string name;
		private ConcurrentQueue<T> frameConcurrentQueue;
		public VideoConnectionFrameConsumer(string name) {
			this.name = name;
			this.frameConcurrentQueue = new ConcurrentQueue<T>();
		}

		/// <summary>
		/// Microsoft docs
		/// </summary>
		/// <param name="provider">VideoConnectionFrameProvider</param>
		public virtual void Subscribe(IObservable<T> provider) {
			if (provider != null) {
				unsubscriber = provider.Subscribe(this);
			}
		}

		public virtual void Unsubscribe() {
			unsubscriber.Dispose();
		}

		public void OnCompleted() {
			this.Unsubscribe();
		}

		public void OnError(Exception error) {
			//do nothing
		}

		public void OnNext(T value) {
			frameConcurrentQueue.Enqueue(value);
		}

		public ConcurrentQueue<T> GetConcurrentQueue() {
			return this.frameConcurrentQueue;
		}
	}
}