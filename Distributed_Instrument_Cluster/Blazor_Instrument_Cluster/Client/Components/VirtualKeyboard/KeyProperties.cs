
namespace Blazor_Instrument_Cluster.Client.Code.UrlObjects {

	/// <summary>
	/// Class for storing key properties.
	/// </summary>
	/// <author>Andre Helland</author>
	public class KeyProperties {
		/// <summary>
		/// Backend value off key (what will be sent to remote device).
		/// Used as display string if displayString is not set.
		/// </summary>
		public string keyValue { get; set; }
		/// <summary>
		/// String key will display if no image is set.
		/// If not set value will default to keyValue.
		/// </summary>
		public string displayString { get; set; }
		/// <summary>
		/// Path to image file key will display instead of displayString (for special keys e.g. windows, backspace).
		/// </summary>
		public string imageFile { get; set; }
		/// <summary>
		/// Height of key.
		/// </summary>
		public string height { get; set; }
		/// <summary>
		/// Width of key.
		/// </summary>
		public string width { get; set; }
		/// <summary>
		/// Width margin between keys.
		/// </summary>
		public string margin { get; set; }

		public KeyProperties(string keyValue) {
			this.keyValue = keyValue;
			displayString = keyValue;
			imageFile = null;
			height = "0em";
			width = "0em";
			margin = "0.2em";
		}
	}
}