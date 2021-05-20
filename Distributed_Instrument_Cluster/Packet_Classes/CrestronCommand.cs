namespace Packet_Classes {
	/// <summary>
	/// Class used when serializing commands to send from the frontend, and deserialize on the remote server
	/// </summary>
	public class CrestronCommand {

		public string msg { get; set; }

		public CrestronCommand() {
			
		}

		public CrestronCommand(string msg) {
			this.msg = msg;
		}

	}
}
