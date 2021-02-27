using System;

namespace Blazor_Instrument_Cluster.Server.Events {
	/// <summary>
	/// Get video frames from the connected devices
	/// </summary>
	public class VideoConnectionFrameConsumer :IObserver<string> {
		public void OnCompleted() {
			throw new NotImplementedException();
		}

		public void OnError(Exception error) {
			throw new NotImplementedException();
		}

		public void OnNext(string value) {
			throw new NotImplementedException();
		}
	}
}