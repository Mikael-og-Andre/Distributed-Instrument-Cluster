using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remote_Server.Crestron {
	internal class TestCrestron : ICrestronControl {
		public Task sendCommandToCrestron(string msg) {
			Console.WriteLine("Crestron received: {0}",msg);
			return Task.CompletedTask;
		}
	}
}
