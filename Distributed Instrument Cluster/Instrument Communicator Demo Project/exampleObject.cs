using System;
using System.Collections.Generic;
using System.Text;

namespace serverDemo {
	public class exampleObject {
		public string name { get; set; }
		public int age { get; set; }

		/// <summary>
		/// Default constructor for json
		/// (JsonSerializer wants an empty constructor)
		/// </summary>
		public exampleObject() {
			
		}

		/// <summary>
		/// Constructor with input
		/// </summary>
		/// <param name="name"></param>
		/// <param name="age"></param>
		public exampleObject(string name, int age) {
			this.name = name;
			this.age = age;
		}
	}
}
