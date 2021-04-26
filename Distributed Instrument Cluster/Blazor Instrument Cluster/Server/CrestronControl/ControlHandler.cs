using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor_Instrument_Cluster.Server.RemoteDeviceManagement;

namespace Blazor_Instrument_Cluster.Server.CrestronControl {
	public class ControlHandler {

		private Guid id { get; set; }

		private SubConnection subConnection { get; set; }

		private List<ControllerInstance> controllerInstances { get; set; }

		public ControlHandler(SubConnection subConnection) {
			this.id = subConnection.id;
			this.controllerInstances = new List<ControllerInstance>();
		}
	}
}
