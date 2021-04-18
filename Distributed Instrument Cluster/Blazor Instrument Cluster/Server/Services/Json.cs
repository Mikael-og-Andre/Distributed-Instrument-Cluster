namespace Blazor_Instrument_Cluster.Server.Services {
	/// <summary>
	/// Json class for storing config info for crestron and video services.
	/// </summary>
	/// <author>Andre Helland</author>
	internal class Json {
		public string serverIP { get; set; }
		public int crestronPort { get; set; }
		public int videoPort { get; set; }
	}
}
