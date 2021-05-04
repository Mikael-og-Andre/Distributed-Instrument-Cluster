using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Client.Code.UrlObjects;

namespace Blazor_Instrument_Cluster.Client.Components.VirtualKeyboard {
	/// <summary>
	/// Class for storing keyboard key configuration.
	/// </summary>
	public class KeyboardJson {
		public List<List<KeyProperties>> keyList { get; set; }
	}
}
