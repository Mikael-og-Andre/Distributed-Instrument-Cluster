using System;
using System.Collections.Generic;
using System.Text;

namespace PackageClasses {
	public class ExampleVideoObject {

		public string imgbase64 { get; set; }

		public ExampleVideoObject() {
			
		}

		public ExampleVideoObject(string imgbase64) {
			this.imgbase64 = imgbase64;
		}
	}
}
