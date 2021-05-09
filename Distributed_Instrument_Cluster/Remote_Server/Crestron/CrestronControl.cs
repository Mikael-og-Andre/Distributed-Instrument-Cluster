using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Crestron_Library;
using Packet_Classes;

namespace Remote_Server.Crestron {
	class CrestronControl : ICrestronControl {
		private CommandParser parser;

		public CrestronControl(Crestron_Library.CommandParser parser) {
			this.parser = parser;
		}

		public Task sendCommandToCrestron(string msg) {
			try {
				var obj = JsonSerializer.Deserialize<CrestronCommand>(msg);
				parser?.pars(obj?.msg);
			}
			catch (Exception e) {
				Console.WriteLine(e);
				//return Task.FromException(e);
			}

			return Task.CompletedTask;
		}
	}
}
