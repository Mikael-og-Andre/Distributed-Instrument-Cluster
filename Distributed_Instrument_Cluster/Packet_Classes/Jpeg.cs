using System.Collections.Generic;

namespace Packet_Classes {
	public class Jpeg {
		public List<byte> jpeg { get; set; }

		public Jpeg() { }

		public Jpeg(List<byte> jpeg) {
			this.jpeg = jpeg;
		}
	}
}
