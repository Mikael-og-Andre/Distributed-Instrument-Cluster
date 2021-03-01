using System;
using System.Collections.Concurrent;
using Instrument_Communicator_Library.Information_Classes;

namespace Blazor_Instrument_Cluster.Server.Events {
	/// <summary>
	/// Get video frames from the connected devices
	/// </summary>
	public class VideoConnectionFrameConsumer :IObserver<VideoFrame> {

		private IDisposable unsubscriber;
		private string name;
		private ConcurrentQueue<VideoFrame> frameConcurrentQueue;
		public VideoConnectionFrameConsumer(string name) {
			this.name = name;
			this.frameConcurrentQueue = new ConcurrentQueue<VideoFrame>();
		}

		/// <summary>
		/// Microsoft docs
		/// </summary>
		/// <param name="provider">VideoConnectionFrameProvider</param>
		public void Subscribe(IObservable<VideoFrame> provider) {
			if (provider != null) {
				unsubscriber = provider.Subscribe(this);
			}
		}

		public void Unsubscribe() {
			unsubscriber.Dispose();
		}

		public void OnCompleted() {
			this.Unsubscribe();
		}

		public void OnError(Exception error) {
			throw new Exception("Observer error");
		}

		public void OnNext(VideoFrame value) {
			frameConcurrentQueue.Enqueue(value);
		}

		public ConcurrentQueue<VideoFrame> GetConcurrentQueue() {
			return this.frameConcurrentQueue;
		}
	}
}