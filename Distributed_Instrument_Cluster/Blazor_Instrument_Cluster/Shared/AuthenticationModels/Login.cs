using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Shared.AuthenticationModels {
	public class Login {
		public string email { get; set; }
		public string passwordHash { get; set; }
	}
}
