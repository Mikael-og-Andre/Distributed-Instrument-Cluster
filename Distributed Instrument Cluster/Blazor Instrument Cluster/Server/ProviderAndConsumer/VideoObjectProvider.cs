using Server_Library;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using PackageClasses;
using Video_Library;

namespace Blazor_Instrument_Cluster.Server.Events {

	/// <summary>
	/// Class for sending an object to all subscribed listeners
	/// <author>Mikael Nilssen</author>
	/// Copied from Microsoft docs and modified
	/// </summary>
	public class VideoObjectProvider : IObservable<Jpeg> {

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
		private List<IObserver<Jpeg>> observers;

		/// <summary>
		/// Cancellation token source
		/// </summary>
		private CancellationTokenSource cancellationTokenSource;

		public MJPEG_Streamer videoStreamer;

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
			
			observers = new List<IObserver<Jpeg>>();
			cancellationTokenSource = new CancellationTokenSource();
			videoStreamer = new MJPEG_Streamer(30, 8080);
			Console.WriteLine("videoStreamer.portNumber");
			Console.WriteLine(videoStreamer.portNumber);
		}

		/// <summary>
		/// Add observer to observer list
		/// </summary>
		/// <param name="observer"> VideoObjectConsumer</param>
		/// <returns>Unsubscribe implementation of IDisposable</returns>
		public IDisposable Subscribe(IObserver<Jpeg> observer) {
			lock (observers) {
				if (!observers.Contains(observer)) {
					observers.Add(observer);
				}

				return new Unsubscriber<Jpeg>(observers, observer);
			}
		}

		/// <summary>
		/// Sends a frame to all observers
		/// </summary>
		/// <param name="frameResult"></param>
		public void pushObject(Jpeg frameResult) {

			//videoStreamer.image = <Jpeg>frameResult.Get();
			//videoStreamer.image = JsonSerializer.Deserialize<T>(frameResult);
			//videoStreamer.image = JsonConvert.DeserializeObject<T>(frameResult);
			Console.WriteLine(videoStreamer.portNumber);

			videoStreamer.image = frameResult.jpeg.ToArray();

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