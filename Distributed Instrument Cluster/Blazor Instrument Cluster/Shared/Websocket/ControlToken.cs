using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Shared.Websocket {
	/// <summary>
	/// Control token, with id for identifying who is in control
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class ControlToken {

		public Guid tokenId { get; set; }

		public ControlToken() {
			this.tokenId = new Guid();
		}
	}
}
