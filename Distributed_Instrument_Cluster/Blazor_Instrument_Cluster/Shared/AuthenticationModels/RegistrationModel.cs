using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blazor_Instrument_Cluster.Shared.AuthenticationModels {
	/// <summary>
	/// Model used when registering a new account
	/// <author>Mikael Nilssen</author>
	/// </summary>
	public class RegistrationModel : AccountModel {

		[Required]
		[Compare("password")]
		public string ConfirmPassword { get; set; }

	}
}
