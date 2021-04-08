using System;
using System.Collections.Generic;
using System.Text;

namespace PackageClasses {
	public class Jpeg {
		public string jpeg;

		public Jpeg() { }

		public Jpeg(byte[] jpeg) {
			this.Set(jpeg);
		}

		public void Set(byte[] bytes) {
			jpeg = Encoding.UTF8.GetString(bytes);
		}

		public byte[] Get() {
			return Encoding.UTF8.GetBytes(jpeg);
		}
	}
}
