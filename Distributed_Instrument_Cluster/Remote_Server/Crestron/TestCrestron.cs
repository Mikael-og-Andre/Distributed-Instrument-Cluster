using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remote_Server.Crestron {
	/// <summary>
	/// Testing interface for the crestron, just prints the output to console
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class TestCrestron : ICrestronControl {
		public Task sendCommandToCrestron(string msg) {
			Console.WriteLine("Crestron received: {0}",msg);
			return Task.CompletedTask;
		}
	}
}
