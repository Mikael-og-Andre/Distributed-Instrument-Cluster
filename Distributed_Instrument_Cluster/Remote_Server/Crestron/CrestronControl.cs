using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Crestron_Library;
using Packet_Classes;

namespace Remote_Server.Crestron {
	/// <summary>
	/// Interface used to send data from incoming connections to the crestron listener to the crestron device.
	/// <author>Mikael Nilssen, Andre Helland</author>
	/// </summary>
	class CrestronControl : ICrestronControl {
		/// <summary>
		/// Parses commands
		/// </summary>
		private CommandParser parser;

		public CrestronControl(Crestron_Library.CommandParser parser) {
			this.parser = parser;
		}

		/// <summary>
		/// Send a command to the crestron
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
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
