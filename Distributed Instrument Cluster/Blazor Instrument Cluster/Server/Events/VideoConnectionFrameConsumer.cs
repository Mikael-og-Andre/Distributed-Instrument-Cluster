using System;
using System.Collections.Concurrent;
using Server_Library;

namespace Blazor_Instrument_Cluster.Server.Events {
	/// <summary>
	/// A class that can subscribe the a video frame queue and receive incoming video frames
	/// <author>Mikael Nilssen</author>
	/// Copied from Microsoft docs and modified
	/// </summary>
	public class VideoConnectionFrameConsumer :IObserver<VideoFrame> {

		/// <summary>
		/// Object used to unsubscribe from provider
		/// </summary>
		private IDisposable unsubscriber;
		/// <summary>
		/// Name of the device it wants a queue from
		/// </summary>
		private string name;
		/// <summary>
		/// Concurrent queue of incoming frames
		/// </summary>
		private ConcurrentQueue<VideoFrame> frameConcurrentQueue;
		/// <summary>
		/// Constructor, sets name and initializes queue
		/// </summary>
		/// <param name="name"></param>
		public VideoConnectionFrameConsumer(string name) {
			this.name = name;
			this.frameConcurrentQueue = new ConcurrentQueue<VideoFrame>();
		}

		/// <summary>
		/// Adds this consumer to the providers list. and sets unsubscribe object
		///
		/// </summary>
		/// <param name="provider">VideoConnectionFrameProvider</param>
		public void Subscribe(IObservable<VideoFrame> provider) {
			if (provider != null) {
				unsubscriber = provider.Subscribe(this);
			}
		}
		/// <summary>
		/// Unsubscribes this consumer from the provider
		/// </summary>
		public void Unsubscribe() {
			unsubscriber.Dispose();
		}
		/// <summary>
		/// What to do when the provider is done
		/// </summary>
		public void OnCompleted() {
			this.Unsubscribe();
		}
		/// <summary>
		/// What to do when an error occurs
		/// </summary>
		/// <param name="error"></param>
		public void OnError(Exception error) {
			throw new Exception("Observer error");
		}
		/// <summary>
		/// Pushes a VideoFrame from the provider to the queue
		/// </summary>
		/// <param name="value"></param>
		public void OnNext(VideoFrame value) {
			frameConcurrentQueue.Enqueue(value);
		}
		/// <summary>
		/// Get the concurrent queue
		/// </summary>
		/// <returns></returns>
		public ConcurrentQueue<VideoFrame> GetConcurrentQueue() {
			return this.frameConcurrentQueue;
		}
	}
}