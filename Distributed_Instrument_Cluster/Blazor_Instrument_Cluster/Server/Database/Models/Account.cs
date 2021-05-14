using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Server.Database.Models {
	public class Account {

		public int AccountID { get; set; }

		public string email { get; set; }

		public string passwordHash { get; set; }

	}
}
