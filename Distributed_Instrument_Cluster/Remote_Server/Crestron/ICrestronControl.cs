using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remote_Server.Crestron {
	/// <summary>
	/// Interface for sending commands to crestron
	/// <Author>Mikael Nilssen</Author>
	/// </summary>
	public interface ICrestronControl {

		public Task sendCommandToCrestron(string msg);


	}
}
