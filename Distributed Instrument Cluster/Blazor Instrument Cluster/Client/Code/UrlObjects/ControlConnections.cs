using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Client.Code.UrlObjects {
	/// <summary>
	/// Class for storing list of subnames
	/// <Author>Mikael Nilssen</Author>
	/// </summary>
	public class ControlConnections {

		public List<Guid> controllerIdList { get; set; }

		public ControlConnections() {
			
		}
	}
}
