using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Client.Auth.Models {
	public class AuthenticatedAccountModel {

		public string accessToken { get; set; }

		public string email { get; set; }
	}
}
