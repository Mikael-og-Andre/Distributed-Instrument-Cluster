
using System.Collections.Generic;
using System.Text;

namespace Networking_Library_Test {
	public class TestJsonObject {

		public string name { get; set; }
		public int age { get; set; }
		public string address { get; set; }

		public TestJsonObject() {
			
		}

		public TestJsonObject(string name, int age, string address) {
			this.name = name;
			this.age = age;
			this.address = address;	
		}

	}
}
