using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Client.Auth.Models {
	public class AuthenticationAccountModel {
		[Required]
		public string email { get; set; }

		public string password { get; set; }
	}
}
