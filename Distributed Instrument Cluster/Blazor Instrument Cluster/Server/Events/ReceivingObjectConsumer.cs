using System;
using System.Collections.Concurrent;

namespace Blazor_Instrument_Cluster.Server.Events {

	/// <summary>
	/// A class that can subscribe the a video frame queue and receive incoming video frames
	/// <author>Mikael Nilssen</author>
	/// Copied from Microsoft docs and modified
	/// </summary>
	public class ReceivingObjectConsumer<T> : IObserver<T> {

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
		private ConcurrentQueue<T> frameConcurrentQueue;

		/// <summary>
		/// Constructor, sets name and initializes queue
		/// </summary>
		/// <param name="name"></param>
		public ReceivingObjectConsumer(string name) {
			this.name = name;
			this.frameConcurrentQueue = new ConcurrentQueue<T>();
		}

		/// <summary>
		/// Adds this consumer to the providers list. and sets unsubscribe object
		///
		/// </summary>
		/// <param name="provider">ReceivingObjectProvider</param>
		public void Subscribe(IObservable<T> provider) {
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
		public void OnNext(T value) {
			frameConcurrentQueue.Enqueue(value);
		}

		/// <summary>
		/// Get the concurrent queue
		/// </summary>
		/// <returns></returns>
		public ConcurrentQueue<T> GetConcurrentQueue() {
			return this.frameConcurrentQueue;
		}
	}
}