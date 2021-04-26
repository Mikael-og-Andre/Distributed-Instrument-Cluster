using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server.CrestronControl {
	public class ControllerInstance {
		private Guid id { get; set; }
		private ControlHandler controlHandler { get; set; }

		public ControllerInstance(Guid id, ControlHandler controlHandler) {
			this.id = id;
			this.controlHandler = controlHandler;
		}
	}
}
