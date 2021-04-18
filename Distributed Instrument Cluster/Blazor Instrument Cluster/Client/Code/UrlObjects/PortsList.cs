using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Client.Code.UrlObjects {
	/// <summary>
	/// Object for storing a list of ports for serialization
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class PortsList {

		public List<int> portsList { get; set; }

		public PortsList() {
			
		}

	}
}
