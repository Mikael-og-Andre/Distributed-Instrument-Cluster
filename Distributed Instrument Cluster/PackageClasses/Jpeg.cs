using System;
using System.Collections.Generic;
using System.Text;

namespace PackageClasses {
	public class Jpeg {
		public List<byte> jpeg { get; set; }

		public Jpeg() { }

		public Jpeg(List<byte> jpeg) {
			this.jpeg = jpeg;
		}
	}
}
