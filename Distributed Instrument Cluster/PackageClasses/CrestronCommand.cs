using System;
using System.Collections.Generic;
using System.Text;

namespace PackageClasses {
	public class CrestronCommand {

		public string msg { get; set; }

		public CrestronCommand() {
			
		}

		public CrestronCommand(string msg) {
			this.msg = msg;
		}

	}
}
