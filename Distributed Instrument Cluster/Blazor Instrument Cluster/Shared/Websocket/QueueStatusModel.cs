using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Shared {
	/// <summary>
	/// Class for containing data about queue status when trying to control a device
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class QueueStatusModel {
		/// <summary>
		/// Does the target have control
		/// </summary>
		public bool hasControl { get; set; }
		

		public QueueStatusModel() {
			
		}

		public QueueStatusModel(bool hasControl) {
			this.hasControl = hasControl;
		}

	}
}
