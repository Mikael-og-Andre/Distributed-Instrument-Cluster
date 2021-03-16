using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server.Object {
	public class dummyJsonObject {

		public int number { get; set; }
		public string text { get; set; }

		public dummyJsonObject() {
			
		}

		public dummyJsonObject(int number, string text) {
			this.number = number;
			this.text = text;
		}

	}
}
