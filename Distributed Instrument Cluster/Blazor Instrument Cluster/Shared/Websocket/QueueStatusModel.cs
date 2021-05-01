namespace Blazor_Instrument_Cluster.Shared.Websocket {
	/// <summary>
	/// Class for containing data about queue status when trying to control a device
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class QueueStatusModel {
		public int position { get; set; }
		
		public QueueStatusModel() {
			
		}

		public QueueStatusModel(int position) {
			this.position = position;
		}

	}
}
